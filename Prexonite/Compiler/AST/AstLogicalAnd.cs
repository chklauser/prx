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
using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstLogicalAnd : AstLazyLogical,
                                 IAstExpression
    {
        public AstLogicalAnd(
            string file,
            int line,
            int col,
            IAstExpression leftCondition,
            IAstExpression rightCondition)
            : base(file, line, col, leftCondition, rightCondition)
        {
        }

        internal AstLogicalAnd(
            Parser p, IAstExpression leftCondition, IAstExpression rightCondition)
            : base(p, leftCondition, rightCondition)
        {
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            var labelNs = @"And\" + Guid.NewGuid().ToString("N");
            var trueLabel = @"True\" + labelNs;
            var falseLabel = @"False\" + labelNs;
            var evalLabel = @"Eval\" + labelNs;

            EmitCode(target, trueLabel, falseLabel);

            target.EmitLabel(this, trueLabel);
            target.EmitConstant(this, true);
            target.EmitJump(this, evalLabel);
            target.EmitLabel(this, falseLabel);
            target.EmitConstant(this, false);
            target.EmitLabel(this, evalLabel);
        }

        //Called by either AstLogicalAnd or AstLogicalOr
        public override void EmitCode(CompilerTarget target, string trueLabel, string falseLabel)
        {
            var labelNs = @"And\" + Guid.NewGuid().ToString("N");
            var nextLabel = @"Next\" + labelNs;
            foreach (var expr in Conditions)
            {
                var or = expr as AstLogicalOr;
                if (or != null)
                {
                    or.EmitCode(target, nextLabel, falseLabel);
                    //Resolve pending jumps to Next
                    target.EmitLabel(this, nextLabel);
                    target.FreeLabel(nextLabel);
                    //Future references of to nextLabel will be resolved in the next iteration
                }
                else
                {
                    expr.EmitCode(target);
                    target.EmitJumpIfFalse(this, falseLabel);
                }
            }
            target.EmitJump(this, trueLabel);
        }

        #region IAstExpression Members

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            if (Conditions.Count <= 0)
                return false;
            var node = Conditions.First;
            do
            {
                var condition = node.Value;
                OptimizeNode(target, ref condition);
                node.Value = condition; //Update list of conditions with optimized condition

                if (condition is AstConstant && ((AstConstant) condition).Constant is bool)
                {
                    var result = (bool) (condition as AstConstant).Constant;
                    if (result) // Expr1 And True And Expr2 = Expr And Expr
                        Conditions.Remove(node);
                    else
                    {
                        // Expr1 And False And Expr2 = False
                        expr = condition;
                        return true;
                    }
                }
            } while ((node = node.Next) != null);

            if (Conditions.Count == 0)
                expr = new AstConstant(File, Line, Column, true);
            else if (Conditions.Count == 1)
                expr = Conditions.First.Value;
            else
                return false;

            return true;
        }

        #endregion
    }
}