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
    public class AstLogicalOr : AstLazyLogical,
                                IAstExpression
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

        #region IAstExpression Members

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            if (Conditions.Count <= 0)
                return false;
            LinkedListNode<IAstExpression> node = Conditions.First;
            do
            {
                IAstExpression condition = node.Value;
                OptimizeNode(target, ref condition);
                node.Value = condition; //Update list of conditions with optimized condition

                if (condition is AstConstant && ((AstConstant) condition).Constant is bool)
                {
                    bool result = (bool) (condition as AstConstant).Constant;
                    if (!result) // Expr1 Or False Or Expr2 = Expr Or Expr
                        Conditions.Remove(node);
                    else
                    {
                        // Expr1 Or True Or Expr2 = True
                        expr = condition;
                        return true;
                    }
                }
            } while ((node = node.Next) != null);

            if (Conditions.Count == 0)
                //All conditions have been reduced because they evaluated to false
                expr = new AstConstant(File, Line, Column, false);
            else if (Conditions.Count == 1)
                //There is no need for a LazyOr structure with only one condition
                expr = Conditions.First.Value;
            else
                return false;

            return true;
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            string labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            string trueLabel = @"True\" + labelNs;
            string falseLabel = @"False\" + labelNs;
            string evalLabel = @"Eval\" + labelNs;

            EmitCode(target, trueLabel, falseLabel);

            target.EmitLabel(falseLabel);
            target.EmitConstant(false);
            target.EmitJump(evalLabel);
            target.EmitLabel(trueLabel);
            target.EmitConstant(true);
            target.EmitLabel(evalLabel);
        }

        public override void EmitCode(CompilerTarget target, string trueLabel, string falseLabel)
        {
            string labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            string nextLabel = @"Next\" + labelNs;
            foreach (IAstExpression expr in Conditions)
            {
                AstLogicalAnd and = expr as AstLogicalAnd;
                if (and != null)
                {
                    and.EmitCode(target, trueLabel, nextLabel);
                    //Resolve pending jumps to Next
                    target.EmitLabel(nextLabel);
                    target.FreeLabel(nextLabel);
                    //Future references of to nextLabel will be resolved in the next iteration
                }
                else
                {
                    expr.EmitCode(target);
                    target.EmitJumpIfTrue(trueLabel);
                }
            }
            target.EmitJump(falseLabel);
        }
    }
}