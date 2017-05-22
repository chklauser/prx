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
    public class AstLogicalAnd : AstLazyLogical, IAstPartiallyApplicable
    {
        public AstLogicalAnd(
            string file,
            int line,
            int col,
            AstExpr leftCondition,
            AstExpr rightCondition)
            : base(file, line, col, leftCondition, rightCondition)
        {
        }

        internal AstLogicalAnd(
            Parser p, AstExpr leftCondition, AstExpr rightCondition)
            : base(p, leftCondition, rightCondition)
        {
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var labelNs = $@"And\{Guid.NewGuid():N}";
            var trueLabel = $@"True\{labelNs}";
            var falseLabel = $@"False\{labelNs}";
            var evalLabel = $@"Eval\{labelNs}";

            EmitCode(target, trueLabel, falseLabel);

            // When the AND node gets used as a value (as opposed to a part of control flow),
            // we have to convert the control flow into true/false values.
            if (stackSemantics == StackSemantics.Value)
            {
                target.EmitLabel(Position, trueLabel);
                target.EmitConstant(Position, true);
                target.EmitJump(Position, evalLabel);
                target.EmitLabel(Position, falseLabel);
                target.EmitConstant(Position, false);
                target.EmitLabel(Position, evalLabel);
            }
            else
            {
                Debug.Assert(stackSemantics == StackSemantics.Effect);
                target.EmitLabel(Position, trueLabel);
                target.EmitLabel(Position, falseLabel);
            }
        }

        //Called by either AstLogicalAnd or AstLogicalOr
        protected override void DoEmitCode(CompilerTarget target, string trueLabel,
            string falseLabel)
        {
            var labelNs = @"And\" + Guid.NewGuid().ToString("N");
            var nextLabel = @"Next\" + labelNs;
            foreach (var expr in Conditions)
            {
                var or = expr as AstLogicalOr;
                if (or != null)
                {
                    or.EmitCode(target, nextLabel, falseLabel);
                    //ResolveOperator pending jumps to Next
                    target.EmitLabel(Position, nextLabel);
                    target.FreeLabel(nextLabel);
                    //Future references of to nextLabel will be resolved in the next iteration
                }
                else
                {
                    expr.EmitValueCode(target);
                    target.EmitJumpIfFalse(Position, falseLabel);
                }
            }
            target.EmitJump(Position, trueLabel);
        }

        #region AstExpr Members

        #endregion

        #region Partial application

        protected override AstExpr CreatePrefix(ISourcePosition position,
            IEnumerable<AstExpr> clauses)
        {
            return CreateConjunction(position, clauses);
        }

        protected override bool ShortcircuitValue
        {
            get { return false; }
        }

        #endregion
    }
}