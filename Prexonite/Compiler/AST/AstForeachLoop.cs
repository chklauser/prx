/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System.Diagnostics;
using Prexonite.Compiler.Cil;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForeachLoop : AstLoop
    {
        [DebuggerStepThrough]
        public AstForeachLoop(string file, int line, int column)
            : base(file, line, column)
        {
            Block = new AstLoopBlock(file, line, column);
        }

        [DebuggerStepThrough]
        internal AstForeachLoop(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public IAstExpression List;
        public AstGetSet Element;

        public bool IsInitialized
        {
            [DebuggerStepThrough]
            get { return List != null && Element != null; }
        }

        #region IAstHasExpressions Members

        public override IAstExpression[] Expressions
        {
            get { return new[] {List}; }
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (!IsInitialized)
                throw new PrexoniteException("AstForeachLoop requires List and Element to be set.");

            //Optimize expression
            OptimizeNode(target, ref List);

            //Create the enumerator variable
            var enumVar = Block.CreateLabel("enumerator");
            target.Function.Variables.Add(enumVar);

            //Create the element assignment statement
            var element = Element.GetCopy();
            IAstExpression optElem;
            if (element.TryOptimize(target, out optElem))
            {
                element = optElem as AstGetSet;
                if (element == null)
                {
                    target.Loader.ReportSemanticError
                        (
                        Element.Line,
                        Element.Column,
                        "Optimization of the element expression in the foreach head " +
                        "resulted in a non-GetSet expression. Try to use a simpler expression.");
                    return;
                }
            }
            var ldEnumVar =
                new AstGetSetSymbol(
                    File, Line, Column, enumVar, SymbolInterpretations.LocalObjectVariable);
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
            List.EmitCode(target);
            target.EmitGetCall(List, 0, "GetEnumerator");
            var castAddr = target.Code.Count;
            target.Emit(List, OpCode.cast_const, "Object(\"System.Collections.IEnumerator\")");
            target.EmitStoreLocal(List, enumVar);

            //check whether an enhanced CIL implementation is possible
            bool emitHint;
            if (element.DefaultAdditionalArguments + element.Arguments.Count > 1) //has additional arguments
                emitHint = false;
            else
                emitHint = true;

            var _try = new AstTryCatchFinally(File, Line, Column)
            {
                TryBlock = new AstActionBlock
                    (
                    this,
                    delegate
                    {
                        target.EmitJump(this, Block.ContinueLabel);

                        //Assignment (begin)
                        target.EmitLabel(this, Block.BeginLabel);
                        getCurrentAddr = target.Code.Count;
                        element.EmitEffectCode(target);

                        //Code block
                        Block.EmitCode(target);

                        //Condition (continue)
                        target.EmitLabel(this, Block.ContinueLabel);
                        moveNextAddr = target.Code.Count;
                        target.EmitLoadLocal(List, enumVar);
                        target.EmitGetCall(List, 0, "MoveNext");
                        target.EmitJumpIfTrue(this, Block.BeginLabel);

                        //Break
                        target.EmitLabel(this, Block.BreakLabel);
                    }),
                FinallyBlock = new AstActionBlock
                    (
                    this,
                    delegate
                    {
                        disposeAddr = target.Code.Count;
                        target.EmitLoadLocal(List, enumVar);
                        target.EmitCommandCall(List, 1, Engine.DisposeAlias, true);
                    })
            };

            _try.EmitCode(target);

            target.EndBlock();

            if (getCurrentAddr < 0 || moveNextAddr < 0 || disposeAddr < 0)
                throw new PrexoniteException("Could not capture addresses within foreach construct for CIL compiler hint.");
            else if (emitHint)
            {
                var hint = new ForeachHint(enumVar, castAddr, getCurrentAddr, moveNextAddr, disposeAddr);
                Cil.Compiler.AddCilHint(target, hint);

                Action<int, int> mkHook =
                    (index, original) =>
                    {
                        AddressChangeHook hook = null;
                        hook = new AddressChangeHook(
                            original,
                            newAddr =>
                            {
                                foreach (var hintEntry in target.Meta[Loader.CilHintsKey].List)
                                {
                                    var entry = hintEntry.List;
                                    if (entry[0] == ForeachHint.Key &&
                                        entry[index].Text == original.ToString())
                                    {
                                        entry[index] = newAddr.ToString();
                                        // AddressChangeHook.ctor can be trusted not to call the closure.
// ReSharper disable PossibleNullReferenceException
                                        hook.InstructionIndex = newAddr;
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

        private delegate void Action<in TA1, in TA2>(TA1 a1, TA2 a2);
    }
}