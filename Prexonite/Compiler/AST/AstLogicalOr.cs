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

        protected override void DoEmitCode(CompilerTarget target, string trueLabel, string falseLabel)
        {
            string labelNs = @"Or\" + Guid.NewGuid().ToString("N");
            string nextLabel = @"Next\" + labelNs;
            foreach (IAstExpression expr in Conditions)
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

        protected override IAstExpression CreatePrefix(ISourcePosition position, IEnumerable<IAstExpression> clauses)
        {
            return CreateDisjunction(position, clauses);
        }

        #endregion
    }
}