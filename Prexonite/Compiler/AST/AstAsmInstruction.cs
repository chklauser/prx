// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            //Jumps need special treatment for label resolution

            if (Instruction.Arguments == -1)
            {
                switch (Instruction.OpCode)
                {
                    case OpCode.jump:
                        target.EmitJump(Position, Instruction.Id);
                        break;
                    case OpCode.jump_t:
                        target.EmitJumpIfTrue(Position, Instruction.Id);
                        break;
                    case OpCode.jump_f:
                        target.EmitJumpIfFalse(Position, Instruction.Id);
                        break;
                    case OpCode.leave:
                        target.EmitLeave(Position, Instruction.Id);
                        break;
                    default:
                        goto emitNormally;
                }
            }
            else
                goto emitNormally;

            return;
            emitNormally:
            target.Emit(Position, Instruction);
        }

        public override string ToString()
        {
            return Instruction.ToString();
        }
    }
}