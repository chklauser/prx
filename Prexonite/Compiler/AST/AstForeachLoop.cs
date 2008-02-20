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

using Prexonite.Compiler.Cil;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForeachLoop : AstLoop,
                                  IAstHasExpressions
    {
        [NoDebug]
        public AstForeachLoop(string file, int line, int column)
            : base(file, line, column)
        {
            Block = new AstBlock(file, line, column);
            Labels = CreateBlockLabels();
        }

        [NoDebug]
        public static BlockLabels CreateBlockLabels()
        {
            return new BlockLabels("foreach");
        }

        [NoDebug]
        internal AstForeachLoop(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public IAstExpression List;
        public AstGetSet Element;

        public bool IsInitialized
        {
            [NoDebug]
            get { return List != null && Element != null; }
        }

        #region IAstHasExpressions Members

        public override IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {List}; }
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (!IsInitialized)
                throw new PrexoniteException("AstForeachLoop requires List and Element to be set.");

            //Optimize expression
            OptimizeNode(target, ref List);

            //Create the enumerator variable
            string enumVar = Labels.CreateLabel("enumerator");
            target.Function.Variables.Add(enumVar);

            //Create the element assignment statement
            AstGetSet element = Element.GetCopy();
            AstGetSetSymbol ldEnumVar =
                new AstGetSetSymbol(
                    File, Line, Column, enumVar, SymbolInterpretations.LocalObjectVariable);
            AstGetSetMemberAccess getCurrent =
                new AstGetSetMemberAccess(File, Line, Column, ldEnumVar, "Current");
            element.Arguments.Add(getCurrent);
            element.Call = PCall.Set;

            //Actual Code Generation
            int castAddr, moveNextAddr = -1, getCurrentAddr = -1, disposeAddr = -1;

            //Get the enumerator
            List.EmitCode(target);
            target.EmitGetCall(0, "GetEnumerator");
            castAddr = target.Code.Count;
            target.Emit(OpCode.cast_const, "Object(\"System.Collections.IEnumerator\")");
            target.EmitStoreLocal(enumVar);

            //check whether an enhanced CIL implementation is possible
            bool emitHint;
            if (element.DefaultAdditionalArguments + element.Arguments.Count > 1) //has additional arguments
                emitHint = false;
            else
                emitHint = true;

            AstTryCatchFinally _try = new AstTryCatchFinally(File, Line, Column);
            _try.TryBlock = new AstActionBlock(
                this,
                delegate
                {
                    target.EmitJump(Labels.ContinueLabel);

                    //Assignment (begin)
                    target.EmitLabel(Labels.BeginLabel);
                    getCurrentAddr = target.Code.Count;
                    element.EmitEffectCode(target);

                    //Code block
                    Block.EmitCode(target);

                    //Condition (continue)
                    target.EmitLabel(Labels.ContinueLabel);
                    moveNextAddr = target.Code.Count;
                    target.EmitLoadLocal(enumVar);
                    target.EmitGetCall(0, "MoveNext");
                    target.EmitJumpIfTrue(Labels.BeginLabel);

                    //Break
                    target.EmitLabel(Labels.BreakLabel);
                });

            _try.FinallyBlock = new AstActionBlock(
                this,
                delegate
                {
                    disposeAddr = target.Code.Count;
                    target.EmitLoadLocal(enumVar);
                    target.EmitCommandCall(1, Engine.DisposeCommand, true);
                });

            _try.EmitCode(target);

            if(getCurrentAddr < 0 || moveNextAddr < 0 || disposeAddr < 0)
                throw new PrexoniteException("Could not capture addresses within foreach construct for CIL compiler hint.");
            else if (emitHint)
            {
                ForeachHint hint = new ForeachHint(enumVar, castAddr, getCurrentAddr, moveNextAddr, disposeAddr);
                if(target.Meta.ContainsKey(Loader.CilHintsKey))
                    target.Meta.AddTo(Loader.CilHintsKey, hint.ToMetaEntry());
                else
                    target.Meta[Loader.CilHintsKey] = (MetaEntry) new MetaEntry[] {hint.ToMetaEntry()};

                Action<int, int> mkHook =
                    delegate(int index, int original)
                    {
                        target.AddressChangeHooks.Add(
                            original,
                            delegate(int newAddr)
                            {
                                foreach(MetaEntry hintEntry in target.Meta[Loader.CilHintsKey].List)
                                {
                                    MetaEntry[] entry = hintEntry.List;
                                    if(entry[0] == ForeachHint.Key && entry[index].Text == original.ToString())
                                        entry[index] = newAddr.ToString();
                                }
                            });
                    };

                mkHook(ForeachHint.CastAddressIndex, castAddr);
                mkHook(ForeachHint.GetCurrentAddressIndex, getCurrentAddr);
                mkHook(ForeachHint.MoveNextAddressIndex, moveNextAddr);
                mkHook(ForeachHint.DisposeAddressIndex, disposeAddr);
            } // else nothing
        }

        private delegate void Action<Ta1, Ta2>(Ta1 a1, Ta2 a2);
    }
}