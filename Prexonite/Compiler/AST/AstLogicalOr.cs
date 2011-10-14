// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstLogicalOr : AstLazyLogical, IAstPartiallyApplicable
    {
        public AstLogicalOr(
            string file,
            int line,
            int column,
            IAstExpression leftCondition,
            IAstExpression rightCondition)
            : base(file, line, column, leftCondition, rightCondition)
        {
        }

        internal AstLogicalOr(Parser p, IAstExpression leftCondition, IAstExpression rightCondition)
            : base(p, leftCondition, rightCondition)
        {
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            var trueLabel = @"True\" + labelNs;
            var falseLabel = @"False\" + labelNs;
            var evalLabel = @"Eval\" + labelNs;

            EmitCode(target, trueLabel, falseLabel);

            target.EmitLabel(this, falseLabel);
            target.EmitConstant(this, false);
            target.EmitJump(this, evalLabel);
            target.EmitLabel(this, trueLabel);
            target.EmitConstant(this, true);
            target.EmitLabel(this, evalLabel);
        }

        protected override void DoEmitCode(CompilerTarget target, string trueLabel,
            string falseLabel)
        {
            var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            var nextLabel = @"Next\" + labelNs;
            foreach (var expr in Conditions)
            {
                var and = expr as AstLogicalAnd;
                if (and != null)
                {
                    and.EmitCode(target, trueLabel, nextLabel);
                    //Resolve pending jumps to Next
                    target.EmitLabel(this, nextLabel);
                    target.FreeLabel(nextLabel);
                    //Future references of to nextLabel will be resolved in the next iteration
                }
                else
                {
                    expr.EmitCode(target);
                    target.EmitJumpIfTrue(this, trueLabel);
                }
            }
            target.EmitJump(this, falseLabel);
        }

        #region Partial application

        protected override bool ShortcircuitValue
        {
            get { return true; }
        }

        protected override IAstExpression CreatePrefix(ISourcePosition position,
            IEnumerable<IAstExpression> clauses)
        {
            return CreateDisjunction(position, clauses);
        }

        #endregion
    }
}