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
using System.Collections.Generic;
using System.Diagnostics;

namespace Prexonite.Compiler.Ast
{
    public class AstLogicalOr : AstLazyLogical, IAstPartiallyApplicable
    {
        public AstLogicalOr(
            string file,
            int line,
            int column,
            AstExpr leftCondition,
            AstExpr rightCondition)
            : base(file, line, column, leftCondition, rightCondition)
        {
        }

        internal AstLogicalOr(Parser p, AstExpr leftCondition, AstExpr rightCondition)
            : base(p, leftCondition, rightCondition)
        {
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            var trueLabel = @"True\" + labelNs;
            var falseLabel = @"False\" + labelNs;
            var evalLabel = @"Eval\" + labelNs;

            EmitCode(target, trueLabel, falseLabel);

            if (stackSemantics == StackSemantics.Value)
            {
                target.EmitLabel(Position, falseLabel);
                target.EmitConstant(Position, false);
                target.EmitJump(Position, evalLabel);
                target.EmitLabel(Position, trueLabel);
                target.EmitConstant(Position, true);
                target.EmitLabel(Position, evalLabel);
            }
            else
            {
                Debug.Assert(stackSemantics == StackSemantics.Effect);
                target.EmitLabel(Position, falseLabel);
                target.EmitLabel(Position, trueLabel);
            }
        }

        protected override void DoEmitCode(CompilerTarget target, string trueLabel,
            string falseLabel)
        {
            var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            var nextLabel = @"Next\" + labelNs;
            foreach (var expr in Conditions)
            {
                if (expr is AstLogicalAnd and)
                {
                    and.EmitCode(target, trueLabel, nextLabel);
                    //ResolveOperator pending jumps to Next
                    target.EmitLabel(Position, nextLabel);
                    target.FreeLabel(nextLabel);
                    //Future references of to nextLabel will be resolved in the next iteration
                }
                else
                {
                    expr.EmitValueCode(target);
                    target.EmitJumpIfTrue(Position, trueLabel);
                }
            }
            target.EmitJump(Position, falseLabel);
        }

        #region Partial application

        protected override bool ShortcircuitValue => true;

        protected override AstExpr CreatePrefix(ISourcePosition position,
            IEnumerable<AstExpr> clauses)
        {
            return CreateDisjunction(position, clauses);
        }

        #endregion
    }
}