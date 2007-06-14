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
    public class AstCondition : AstNode
    {
        public AstCondition(string file, int line, int column, IAstExpression condition, bool isNegative)
            : base(file, line, column)
        {
            IfBlock = new AstBlock(file, line, column);
            ElseBlock = new AstBlock(file, line, column);
            if (condition == null)
                throw new ArgumentNullException("condition");
            Condition = condition;
            IsNegative = isNegative;
        }

        public AstCondition(string file, int line, int column, IAstExpression condition)
            : this(file, line, column, condition, false)
        {
        }

        internal AstCondition(Parser p, IAstExpression condition, bool isNegative)
            : this(p.scanner.File, p.t.line, p.t.col, condition, isNegative)
        {
        }

        internal AstCondition(Parser p, IAstExpression condition)
            : this(p, condition, false)
        {
        }

        public AstBlock IfBlock;
        public AstBlock ElseBlock;
        public IAstExpression Condition;
        public bool IsNegative;
        private static int depth = 0;

        public override void EmitCode(CompilerTarget target)
        {
            //Optimize condition
            OptimizeNode(target, ref Condition);
            AstUnaryOperator unaryCond = Condition as AstUnaryOperator;
            while (unaryCond != null && unaryCond.Operator == UnaryOperator.LogicalNot)
            {
                Condition = unaryCond.Operand;
                IsNegative = !IsNegative;
                unaryCond = Condition as AstUnaryOperator;
            }

            //Constant conditions
            if (Condition is AstConstant)
            {
                AstConstant constCond = (AstConstant) Condition;
                PValue condValue;
                if (!constCond.ToPValue(target).TryConvertTo(target.Loader, PType.Bool, out condValue))
                    goto continueFull;
                else if ((bool) condValue.Value)
                    IfBlock.EmitCode(target);
                else
                    ElseBlock.EmitCode(target);
                return;
            }
            //Conditions with empty blocks
            if (IfBlock.IsEmpty && ElseBlock.IsEmpty)
            {
                IAstEffect effect = Condition as IAstEffect;
                if (effect != null)
                    effect.EmitEffectCode(target);
                else
                {
                    Condition.EmitCode(target);
                    target.EmitPop();
                }
                return;
            }
            continueFull:
            ;

            //Switch If and Else block in case the if-block is empty
            if (IfBlock.IsEmpty)
            {
                IsNegative = !IsNegative;
                AstBlock tmp = IfBlock;
                IfBlock = ElseBlock;
                ElseBlock = tmp;
            }

            string elseLabel = "else\\" + depth + "\\assembler";
            string endLabel = "endif\\" + depth + "\\assembler";
            depth++;

            //Emit
            AstExplicitGoTo ifGoto = IfBlock.IsSingleStatement? IfBlock[0] as AstExplicitGoTo : null;
            AstExplicitGoTo elseGoto = ElseBlock.IsSingleStatement ? ElseBlock[0] as AstExplicitGoTo : null;
            ;

            bool ifIsGoto = ifGoto != null;
            bool elseIsGoto = elseGoto != null;

            if (ifIsGoto && elseIsGoto)
            {
                //only jumps
                AstLazyLogical.EmitJumpCondition(target, Condition, ifGoto.Destination, elseGoto.Destination,
                                                 !IsNegative);
            }
            else if (ifIsGoto)
            {
                //if => jump / else => block
                AstLazyLogical.EmitJumpCondition(target, Condition, ifGoto.Destination, !IsNegative);
                ElseBlock.EmitCode(target);
            }
            else if (elseIsGoto)
            {
                //if => block / else => jump
                AstLazyLogical.EmitJumpCondition(target, Condition, elseGoto.Destination, IsNegative); //inverted
                IfBlock.EmitCode(target);
            }
            else
            {
                //if => block / else => block
                AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
                IfBlock.EmitCode(target);
                target.EmitJump(endLabel);
                target.EmitLabel(elseLabel);
                ElseBlock.EmitCode(target);
                target.EmitLabel(endLabel);
            }

            target.FreeLabel(elseLabel);
            target.FreeLabel(endLabel);
        }
    }
}