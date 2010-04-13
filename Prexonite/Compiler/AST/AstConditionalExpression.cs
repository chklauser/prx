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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstConditionalExpression : AstNode,
                                            IAstExpression,
                                            IAstHasExpressions
    {
        public AstConditionalExpression(
            string file, int line, int column, IAstExpression condition, bool isNegative)
            : base(file, line, column)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            Condition = condition;
            IsNegative = isNegative;
        }

        public AstConditionalExpression(string file, int line, int column, IAstExpression condition)
            : this(file, line, column, condition, false)
        {
        }

        internal AstConditionalExpression(Parser p, IAstExpression condition, bool isNegative)
            : this(p.scanner.File, p.t.line, p.t.col, condition, isNegative)
        {
        }

        internal AstConditionalExpression(Parser p, IAstExpression condition)
            : this(p, condition, false)
        {
        }

        public IAstExpression IfExpression;
        public IAstExpression ElseExpression;
        public IAstExpression Condition;
        public bool IsNegative;
        private static int depth;

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {Condition, IfExpression, ElseExpression}; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Optimize condition
            OptimizeNode(target, ref Condition);
            var unaryCond = Condition as AstUnaryOperator;
            while (unaryCond != null && unaryCond.Operator == UnaryOperator.LogicalNot)
            {
                Condition = unaryCond.Operand;
                IsNegative = !IsNegative;
                unaryCond = Condition as AstUnaryOperator;
            }

            //Constant conditions
            if (Condition is AstConstant)
            {
                var constCond = (AstConstant) Condition;
                PValue condValue;
                if (
                    !constCond.ToPValue(target).TryConvertTo(
                         target.Loader, PType.Bool, out condValue))
                    expr = null;
                else if ((bool) condValue.Value)
                    expr = IfExpression;
                else
                    expr = ElseExpression;
                return expr != null;
            }

            expr = null;
            return false;
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            //Optimize condition
            OptimizeNode(target, ref Condition);
            OptimizeNode(target, ref IfExpression);
            OptimizeNode(target, ref ElseExpression);

            var elseLabel = "elsei\\" + depth + "\\assembler";
            var endLabel = "endifi\\" + depth + "\\assembler";
            depth++;

            //Emit
            //if => block / else => block
            AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
            IfExpression.EmitCode(target);
            target.EmitJump(endLabel);
            target.EmitLabel(elseLabel);
            ElseExpression.EmitCode(target);
            target.EmitLabel(endLabel);

            target.FreeLabel(elseLabel);
            target.FreeLabel(endLabel);
        }
    }
}