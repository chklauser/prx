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

using System;

namespace Prexonite.Compiler.Ast
{
    //[System.Diagnostics.DebuggerNonUserCode()]
    public class AstAsmInstruction : AstNode
    {
        public Instruction Instruction;

        public AstAsmInstruction(string file, int line, int column, Instruction instruction)
            : base(file, line, column)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");
            Instruction = instruction;
        }

        internal AstAsmInstruction(Parser p, Instruction instruction)
            : this(p.scanner.File, p.t.line, p.t.col, instruction)
        {
        }

        public override void EmitCode(CompilerTarget target)
        {
            //Jumps need special treatment for label resolution

            if (Instruction.Arguments == -1)
            {
                switch (Instruction.OpCode)
                {
                    case OpCode.jump:
                        target.EmitJump(Instruction.Id);
                        break;
                    case OpCode.jump_t:
                        target.EmitJumpIfTrue(Instruction.Id);
                        break;
                    case OpCode.jump_f:
                        target.EmitJumpIfFalse(Instruction.Id);
                        break;
                    case OpCode.leave:
                        target.EmitLeave(Instruction.Id);
                        break;
                    default:
                        goto emitNormally;
                }
            }
            else
                goto emitNormally;

            return;
            emitNormally:
            target.Emit(Instruction);
        }

        public override string ToString()
        {
            return Instruction.ToString();
        }
    }
}