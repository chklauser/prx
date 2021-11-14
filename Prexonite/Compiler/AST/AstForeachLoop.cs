// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Diagnostics;
using System.Globalization;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast;

public class AstForeachLoop : AstLoop
{
    public AstForeachLoop(ISourcePosition position, AstBlock parentBlock)
        : base(position, parentBlock)
    {
    }

    public AstExpr List;
    public AstGetSet Element;

    public bool IsInitialized
    {
        [DebuggerStepThrough]
        get => List != null && Element != null;
    }

    #region IAstHasExpressions Members

    public override AstExpr[] Expressions
    {
        get { return new[] {List}; }
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Value)
            throw new NotSupportedException("Foreach loops don't produce values and can thus not be emitted with value semantics.");

        if (!IsInitialized)
            throw new PrexoniteException("AstForeachLoop requires List and Element to be set.");

        //Optimize expression
        _OptimizeNode(target, ref List);

        //Create the enumerator variable
        var enumVar = Block.CreateLabel("enumerator");
        target.Function.Variables.Add(enumVar);

        //Create the element assignment statement
        var element = Element.GetCopy();
        if (element.TryOptimize(target, out var optElem))
        {
            element = optElem as AstGetSet;
            if (element == null)
            {
                target.Loader.ReportMessage(Message.Error(Resources.AstForeachLoop_DoEmitCode_ElementTooComplicated,Position,MessageClasses.ForeachElementTooComplicated));
                return;
            }
        }
        var ldEnumVar = target.Factory.Call(Position, EntityRef.Variable.Local.Create(enumVar));
        var getCurrent =
            new AstGetSetMemberAccess(File, Line, Column, ldEnumVar, "Current");
        element.Arguments.Add(getCurrent);
        element.Call = PCall.Set;

        //Actual Code Generation
        var moveNextAddr = -1;
        var getCurrentAddr = -1;
        var disposeAddr = -1;

        //Get the enumerator
        target.BeginBlock(Block);

        List.EmitValueCode(target);
        target.EmitGetCall(List.Position, 0, "GetEnumerator");
        var castAddr = target.Code.Count;
        target.Emit(List.Position, OpCode.cast_const, "Object(\"System.Collections.IEnumerator\")");
        target.EmitStoreLocal(List.Position, enumVar);

        //check whether an enhanced CIL implementation is possible
        var emitHint = element.DefaultAdditionalArguments + element.Arguments.Count <= 1;

        var @try = new AstTryCatchFinally(Position, Block);

        @try.TryBlock = new AstActionBlock
        (
            Position, @try,
            delegate
            {
                target.EmitJump(Position, Block.ContinueLabel);

                //Assignment (begin)
                target.EmitLabel(Position, Block.BeginLabel);
                getCurrentAddr = target.Code.Count;
                element.EmitEffectCode(target);

                //Code block
                Block.EmitEffectCode(target);

                //Condition (continue)
                target.EmitLabel(Position, Block.ContinueLabel);
                moveNextAddr = target.Code.Count;
                target.EmitLoadLocal(List.Position, enumVar);
                target.EmitGetCall(List.Position, 0, "MoveNext");
                target.EmitJumpIfTrue(Position, Block.BeginLabel);

                //Break
                target.EmitLabel(Position, Block.BreakLabel);
            });
        @try.FinallyBlock = new AstActionBlock
        (
            Position, @try,
            delegate
            {
                disposeAddr = target.Code.Count;
                target.EmitLoadLocal(List.Position, enumVar);
                target.EmitCommandCall(List.Position, 1, Engine.DisposeAlias, true);
            });
                

        @try.EmitEffectCode(target);

        target.EndBlock();

        if (getCurrentAddr < 0 || moveNextAddr < 0 || disposeAddr < 0)
            throw new PrexoniteException(
                "Could not capture addresses within foreach construct for CIL compiler hint.");
        else if (emitHint)
        {
            var hint = new ForeachHint(enumVar, castAddr, getCurrentAddr, moveNextAddr,
                disposeAddr);
            Cil.Compiler.AddCilHint(target, hint);

            Action<int, int> mkHook =
                (index, original) =>
                {
                    AddressChangeHook hook = null;
                    hook = new AddressChangeHook(
                        original,
                        newAddr =>
                        {
                            foreach (
                                var hintEntry in target.Meta[Loader.CilHintsKey].List)
                            {
                                var entry = hintEntry.List;
                                if (entry[0] == ForeachHint.Key &&
                                    entry[index].Text == original.ToString(CultureInfo.InvariantCulture))
                                {
                                    entry[index] = newAddr.ToString(CultureInfo.InvariantCulture);
                                    // AddressChangeHook.ctor can be trusted not to call the closure.
                                    // ReSharper disable PossibleNullReferenceException
                                    // ReSharper disable AccessToModifiedClosure
                                    hook.InstructionIndex = newAddr;
                                    // ReSharper restore AccessToModifiedClosure
                                    // ReSharper restore PossibleNullReferenceException
                                    original = newAddr;
                                }
                            }
                        });
                    target.AddressChangeHooks.Add(hook);
                };

            mkHook(ForeachHint.CastAddressIndex + 1, castAddr);
            mkHook(ForeachHint.GetCurrentAddressIndex + 1, getCurrentAddr);
            mkHook(ForeachHint.MoveNextAddressIndex + 1, moveNextAddr);
            mkHook(ForeachHint.DisposeAddressIndex + 1, disposeAddr);
        } // else nothing
    }
}