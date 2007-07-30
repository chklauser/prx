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

using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForeachLoop : AstLoop, IAstHasBlocks
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
        public bool IsPositive = true;

        public bool IsInitialized
        {
            [NoDebug]
            get { return List != null && Element != null; }
        }

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
                new AstGetSetSymbol(File, Line, Column, enumVar, SymbolInterpretations.LocalObjectVariable);
            AstGetSetMemberAccess getCurrent =
                new AstGetSetMemberAccess(File, Line, Column, ldEnumVar, "Current");
            element.Arguments.Add(getCurrent);
            element.Call = PCall.Set;

            //Actual Code Generation

            //Get the enumerator
            List.EmitCode(target);
            target.EmitGetCall(0, "GetEnumerator");
            target.Emit(new Instruction(OpCode.cast_const, "Object(\"System.Collections.IEnumerator\")"));
            target.EmitStoreLocal(enumVar);

            AstTryCatchFinally _try = new AstTryCatchFinally(File, Line, Column);
            _try.TryBlock = new AstActionBlock(this, 
                delegate
                {
                    target.EmitJump(Labels.ContinueLabel);

                    //Assignment (begin)
                    target.EmitLabel(Labels.BeginLabel);
                    element.EmitCode(target);

                    //Code block
                    Block.EmitCode(target);

                    //Condition (continue)
                    target.EmitLabel(Labels.ContinueLabel);
                    target.EmitLoadLocal(enumVar);
                    target.EmitGetCall(0, "MoveNext");
                    target.EmitJumpIfTrue(Labels.BeginLabel);

                    //Break
                    target.EmitLabel(Labels.BreakLabel);
                });

            _try.FinallyBlock = new AstActionBlock(this,
                delegate
                {
                    target.EmitLoadLocal(enumVar);
                    target.EmitCommandCall(1,Engine.DisposeCommand,true);
                });

            _try.EmitCode(target);
        }

    }
}