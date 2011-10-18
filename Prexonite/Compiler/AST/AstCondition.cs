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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstCondition : AstNode,
                                IAstHasBlocks,
                                IAstHasExpressions
    {
        public AstCondition(
            string file, int line, int column, IAstExpression condition, bool isNegative)
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
        private static int _depth;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new[] {IfBlock, ElseBlock}; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {Condition}; }
        }

        #endregion

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            //Optimize condition
            _OptimizeNode(target, ref Condition);
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
                    goto continueFull;
                else if (((bool) condValue.Value) ^ IsNegative)
                    IfBlock.EmitCode(target);
                else
                    ElseBlock.EmitCode(target);
                return;
            }
            //Conditions with empty blocks
            if (IfBlock.IsEmpty && ElseBlock.IsEmpty)
            {
                var effect = Condition as IAstEffect;
                if (effect != null)
                    effect.EmitEffectCode(target);
                else
                {
                    Condition.EmitCode(target);
                    target.EmitPop(this);
                }
                return;
            }
            continueFull:
            ;

            //Switch If and Else block in case the if-block is empty
            if (IfBlock.IsEmpty)
            {
                IsNegative = !IsNegative;
                var tmp = IfBlock;
                IfBlock = ElseBlock;
                ElseBlock = tmp;
            }

            var elseLabel = "else\\" + _depth + "\\assembler";
            var endLabel = "endif\\" + _depth + "\\assembler";
            _depth++;

            //Emit
            var ifGoto = IfBlock.IsSingleStatement
                ? IfBlock[0] as AstExplicitGoTo
                : null;
            var elseGoto = ElseBlock.IsSingleStatement
                ? ElseBlock[0] as AstExplicitGoTo
                : null;
            ;

            var ifIsGoto = ifGoto != null;
            var elseIsGoto = elseGoto != null;

            if (ifIsGoto && elseIsGoto)
            {
                //only jumps
                AstLazyLogical.EmitJumpCondition(
                    target,
                    Condition,
                    ifGoto.Destination,
                    elseGoto.Destination,
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
                AstLazyLogical.EmitJumpCondition(
                    target, Condition, elseGoto.Destination, IsNegative); //inverted
                IfBlock.EmitCode(target);
            }
            else
            {
                //if => block / else => block
                AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
                IfBlock.EmitCode(target);
                target.EmitJump(this, endLabel);
                target.EmitLabel(this, elseLabel);
                ElseBlock.EmitCode(target);
                target.EmitLabel(this, endLabel);
            }

            target.FreeLabel(elseLabel);
            target.FreeLabel(endLabel);
        }
    }
}