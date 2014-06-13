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
using Prexonite.Compiler.Internal;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForLoop : AstLoop
    {
        public AstForLoop(ISourcePosition position, AstBlock parentBlock)
            : this(position, new AstScopedBlock(
                position,
                new AstScopedBlock(
                    position, 
                    parentBlock, prefix: "init"),
                prefix:"next"))
        {
        }

        /// <summary>
        /// This constructor should only be called from the public constructor.
        /// It is just here to wire up the loop block to be a sub block of the 
        /// initialization and next iteration blocks. (So that symbols declared in 
        /// initialization are available in the loop body)
        /// </summary>
        /// <param name="position">The source position for this node and all block nodes.</param>
        /// <param name="nextBlock">The block reserved for the "next iteration" code. 
        /// It's parent block must be the initialization block.</param>
        private AstForLoop(ISourcePosition position, AstScopedBlock nextBlock)
            : base(position, nextBlock)
        {
            _initialize = (AstScopedBlock)nextBlock.LexicalScope;
            _nextIteration = nextBlock;
        }

        public AstExpr Condition { get; set; }
        private readonly AstScopedBlock _initialize;
        public AstScopedBlock Initialize
        {
            get { return _initialize; }
        }

        private readonly AstScopedBlock _nextIteration;
        public AstScopedBlock NextIteration
        {
            get { return _nextIteration; }
        }

        private bool _isPositive = true;
        private bool _isPrecondition = true;

        public bool IsPositive
        {
            [DebuggerStepThrough]
            get { return _isPositive; }
            [DebuggerStepThrough]
            set { _isPositive = value; }
        }

        public bool IsPrecondition
        {
            [DebuggerStepThrough]
            get { return _isPrecondition; }
            [DebuggerStepThrough]
            set { _isPrecondition = value; }
        }

        public bool IsInitialized
        {
            [DebuggerStepThrough]
            get { return Condition != null; }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
             if(stackSemantics == StackSemantics.Value)
                throw new NotSupportedException("For loops don't produce values and can thus not be emitted with value semantics.");

            if (!IsInitialized)
                throw new PrexoniteException("AstForLoop requires Condition to be set.");

            //Optimize unary not condition
            var condition = Condition;

            _OptimizeNode(target, ref condition);
            // Invert condition when unary logical not
            AstIndirectCall unaryCond;
            while (Condition.IsCommandCall(Commands.Core.Operators.LogicalNot.DefaultAlias, out unaryCond))
            {
                Condition = unaryCond.Arguments[0];
                IsPositive = !IsPositive;
            }

            //Constant conditions
            var conditionIsConstant = false;
            var constCond = condition as AstConstant;
            if (constCond != null)
            {
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
                    target.BeginBlock(Initialize);
                    Initialize.EmitValueCode(target);
                    if (!IsPrecondition) //start with nextIteration
                        target.EmitJump(Position, Block.ContinueLabel);
                    target.EmitLabel(Position, Block.BeginLabel);
                    target.BeginBlock(NextIteration);
                    target.BeginBlock(Block);
                    Block.EmitEffectCode(target);
                    target.EndBlock();
                    target.EmitLabel(Position, Block.ContinueLabel);
                    NextIteration.EmitValueCode(target);
                    target.EndBlock();
                    target.EmitJump(Position, Block.BeginLabel);
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
                    target.BeginBlock(Initialize);
                    Initialize.EmitValueCode(target);
                    target.BeginBlock(NextIteration);
                    if (IsPrecondition)
                        target.EmitJump(Position, conditionLabel);
                    else
                        target.EmitJump(Position, Block.ContinueLabel);
                    target.EmitLabel(Position, Block.BeginLabel);
                    target.BeginBlock(Block);
                    Block.EmitEffectCode(target);
                    target.EndBlock();
                    target.EmitLabel(Position, Block.ContinueLabel);
                    NextIteration.EmitValueCode(target);
                    target.EndBlock();
                    target.EmitLabel(Position, conditionLabel);
                    AstLazyLogical.EmitJumpCondition(
                        target, condition, Block.BeginLabel, IsPositive);
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
                Initialize.EmitValueCode(target);
                if (!IsPrecondition)
                    target.EmitJump(Position, Block.ContinueLabel);
                target.EmitLabel(Position, Block.BeginLabel);
                AstLazyLogical.EmitJumpCondition(target, condition, Block.BreakLabel, !IsPositive);
                if (IsPrecondition)
                    target.EmitLabel(Position, Block.ContinueLabel);
                NextIteration.EmitValueCode(target);
                target.EmitJump(Position, Block.BeginLabel);
                target.EndBlock();
            }

            target.EmitLabel(Position, Block.BreakLabel);
        }

        public override AstBlock[] Blocks
        {
            get
            {
                var blocks = new List<AstBlock>(base.Blocks)
                    {
                        Initialize,
                        NextIteration,
                        Block
                    };
                return blocks.ToArray();
            }
        }

        #region IAstHasExpressions Members

        public override AstExpr[] Expressions
        {
            get { return new[] {Condition}; }
        }

        #endregion
    }
}