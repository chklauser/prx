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

using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForLoop : AstLoop
    {
        [DebuggerStepThrough]
        public AstForLoop(string file, int line, int column)
            : base(file, line, column)
        {
            Initialize = new AstBlock(file, line, column);
            NextIteration = new AstBlock(file, line, column);
        }

        [DebuggerStepThrough]
        internal AstForLoop(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public IAstExpression Condition;
        public AstBlock Initialize;
        public AstBlock NextIteration;
        public bool IsPositive = true;
        public bool IsPrecondition = true;

        public bool IsInitialized
        {
            [DebuggerStepThrough]
            get { return Condition != null; }
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            if (!IsInitialized)
                throw new PrexoniteException("AstForLoop requires Condition to be set.");

            //Optimize unary not condition
            _OptimizeNode(target, ref Condition);
            var unaryCond = Condition as AstUnaryOperator;
            while (unaryCond != null && unaryCond.Operator == UnaryOperator.LogicalNot)
            {
                Condition = unaryCond.Operand;
                IsPositive = !IsPositive;
                unaryCond = Condition as AstUnaryOperator;
            }

            //Constant conditions
            var conditionIsConstant = false;
            if (Condition is AstConstant)
            {
                var constCond = (AstConstant) Condition;
                PValue condValue;
                if (
                    !constCond.ToPValue(target).TryConvertTo(
                        target.Loader, PType.Bool, out condValue))
                    goto continueFull;
                else if ((bool) condValue.Value == IsPositive)
                    conditionIsConstant = true;
                else
                {
                    //Condition is always false
                    return;
                }
            }
            continueFull:

            var conditionLabel = Block.CreateLabel("condition");

            if (!Block.IsEmpty) //Body exists -> complete loop code?
            {
                if (conditionIsConstant) //Infinite, hopefully user managed, loop ->
                {
                    /*  {init}
                     *  begin:
                     *  {block}
                     *  continue:
                     *  {next}
                     *  jump -> begin
                     */
                    target.BeginBlock(Block);
                    Initialize.EmitCode(target);
                    if (!IsPrecondition) //start with nextIteration
                        target.EmitJump(this, Block.ContinueLabel);
                    target.EmitLabel(this, Block.BeginLabel);
                    Block.EmitCode(target);
                    target.EmitLabel(this, Block.ContinueLabel);
                    NextIteration.EmitCode(target);
                    target.EmitJump(this, Block.BeginLabel);
                    target.EndBlock();
                }
                else //Variable condition and body -> full loop code
                {
                    /*  {init}
                     *  jump -> condition
                     *  begin:
                     *  {block}
                     *  continue:
                     *  {next}
                     *  condition:
                     *  {condition}
                     *  jump if true -> begin
                     */
                    target.BeginBlock(Block);
                    Initialize.EmitCode(target);
                    if (IsPrecondition)
                        target.EmitJump(this, conditionLabel);
                    else
                        target.EmitJump(this, Block.ContinueLabel);
                    target.EmitLabel(this, Block.BeginLabel);
                    Block.EmitCode(target);
                    target.EmitLabel(this, Block.ContinueLabel);
                    NextIteration.EmitCode(target);
                    target.EmitLabel(this, conditionLabel);
                    AstLazyLogical.EmitJumpCondition(
                        target, Condition, Block.BeginLabel, IsPositive);
                    target.EndBlock();
                }
            }
            else //Body does not exist -> Condition loop
            {
                /*  {init}
                 *  begin:
                 *  {cond}
                 *  jump if false -> break
                 *  continue:
                 *  {next}
                 *  jump -> begin
                 */
                target.BeginBlock(Block);
                Initialize.EmitCode(target);
                if (!IsPrecondition)
                    target.EmitJump(this, Block.ContinueLabel);
                target.EmitLabel(this, Block.BeginLabel);
                AstLazyLogical.EmitJumpCondition(target, Condition, Block.BreakLabel, !IsPositive);
                if (IsPrecondition)
                    target.EmitLabel(this, Block.ContinueLabel);
                NextIteration.EmitCode(target);
                target.EmitJump(this, Block.BeginLabel);
                target.EndBlock();
            }

            target.EmitLabel(this, Block.BreakLabel);
        }

        public override AstBlock[] Blocks
        {
            get
            {
                var blocks = new List<AstBlock>(base.Blocks)
                    {
                        Initialize,
                        NextIteration
                    };
                return blocks.ToArray();
            }
        }

        #region IAstHasExpressions Members

        public override IAstExpression[] Expressions
        {
            get { return new[] {Condition}; }
        }

        #endregion
    }
}