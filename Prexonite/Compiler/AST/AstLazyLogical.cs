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
    public abstract class AstLazyLogical : AstNode,
                                           IAstExpression
    {
        public LinkedList<IAstExpression> Conditions = new LinkedList<IAstExpression>();

        internal AstLazyLogical(Parser p, IAstExpression leftExpression, IAstExpression rightExpression)
            : this(p.scanner.File, p.t.line, p.t.col, leftExpression, rightExpression)
        {
        }

        protected AstLazyLogical(string file, int line, int column, IAstExpression leftExpression,
                                 IAstExpression rightExpression)
            : base(file, line, column)
        {
            AddExpression(leftExpression);
            AddExpression(rightExpression);
        }

        public void AddExpression(IAstExpression expr)
        {
            //Flatten hierarchy
            AstLazyLogical lazy = expr as AstLazyLogical;
            if (lazy != null && lazy.GetType() == GetType())
            {
                foreach (IAstExpression cond in lazy.Conditions)
                    AddExpression(cond);
            }
            else
            {
                Conditions.AddLast(expr);
            }
        }

        public abstract void EmitCode(CompilerTarget target, string trueLabel, string falseLabel);
        public abstract bool TryOptimize(CompilerTarget target, out IAstExpression expr);

        public static void EmitJumpIfCondition(CompilerTarget target, IAstExpression cond, string targetLabel)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException("targetLabel may neither be null nor empty.", "targetLabel");
            AstLazyLogical logical = cond as AstLazyLogical;
            if (logical != null)
            {
                string continueLabel = "Continue\\Lazy\\" + Guid.NewGuid().ToString("N");
                logical.EmitCode(target, targetLabel, continueLabel);
                target.EmitLabel(continueLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfTrue(targetLabel);
            }
        }

        public static void EmitJumpCondition(CompilerTarget target, IAstExpression cond, string targetLabel,
                                             bool isPositive)
        {
            if (isPositive)
                EmitJumpIfCondition(target, cond, targetLabel);
            else
                EmitJumpUnlessCondition(target, cond, targetLabel);
        }

        public static void EmitJumpCondition(CompilerTarget target, IAstExpression cond, string targetLabel,
                                             string alternativeLabel, bool isPositive)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException("targetLabel may neither be null nor empty.", "targetLabel");
            if (String.IsNullOrEmpty(alternativeLabel))
                throw new ArgumentException("alternativeLabel may neither be null nor empty.", "alternativeLabel");
            AstLazyLogical logical = cond as AstLazyLogical;
            if (!isPositive)
            {
                //Invert if needed
                string tmpLabel = alternativeLabel;
                alternativeLabel = targetLabel;
                targetLabel = tmpLabel;
            }
            if (logical != null)
            {
                logical.EmitCode(target, targetLabel, alternativeLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfTrue(targetLabel);
                target.EmitJump(alternativeLabel);
            }
        }

        public static void EmitJumpCondition(CompilerTarget target, IAstExpression cond, string targetLabel,
                                             string alternativeLabel)
        {
            EmitJumpCondition(target, cond, targetLabel, alternativeLabel, true);
        }

        public static void EmitJumpUnlessCondition(CompilerTarget target, IAstExpression cond, string targetLabel)
        {
            if (cond == null)
                throw new ArgumentNullException("cond", "Condition may not be null.");
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target may not be null.");
            if (String.IsNullOrEmpty(targetLabel))
                throw new ArgumentException("targetLabel may neither be null nor empty.", "targetLabel");
            AstLazyLogical logical = cond as AstLazyLogical;
            if (logical != null)
            {
                string continueLabel = "Continue\\Lazy\\" + Guid.NewGuid().ToString("N");
                logical.EmitCode(target, continueLabel, targetLabel); //inverted
                target.EmitLabel(continueLabel);
            }
            else
            {
                cond.EmitCode(target);
                target.EmitJumpIfFalse(targetLabel);
            }
        }
    }
}