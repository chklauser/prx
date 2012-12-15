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
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstWhileLoop : AstLoop
    {
        [DebuggerStepThrough]
        public AstWhileLoop(ISourcePosition position, AstBlock parentBlock, bool isPrecondition = true,
            bool isPositive = true)
            : base(position,parentBlock)
        {
            IsPrecondition = isPrecondition;
            IsPositive = isPositive;
        }

        public AstExpr Condition;
        public bool IsPrecondition { get; set; }
        public bool IsPositive { get; set; }

        public override AstExpr[] Expressions
        {
            get { return new[] {Condition}; }
        }

        public bool IsInitialized
        {
            [DebuggerStepThrough]
            get { return Condition != null; }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics == StackSemantics.Value)
                throw new NotSupportedException("While loops do not produce values and can thus not be used as expressions.");
            if (!IsInitialized)
                throw new PrexoniteException("AstWhileLoop requires Condition to be set.");

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
                    if (!IsPrecondition) //If do-while, emit the body without loop code
                    {
                        target.BeginBlock(Block);
                        Block.EmitEffectCode(target);
                        target.EndBlock();
                    }
                    return;
                }
            }
            continueFull:

            target.BeginBlock(Block);
            if (!Block.IsEmpty) //Body exists -> complete loop code?
            {
                if (conditionIsConstant) //Infinite, hopefully user managed, loop ->
                {
                    target.EmitLabel(Position, Block.ContinueLabel);
                    target.EmitLabel(Position, Block.BeginLabel);
                    Block.EmitEffectCode(target);
                    target.EmitJump(Position, Block.ContinueLabel);
                }
                else
                {
                    if (IsPrecondition)
                        target.EmitJump(Position, Block.ContinueLabel);

                    target.EmitLabel(Position, Block.BeginLabel);
                    Block.EmitEffectCode(target);

                    _emitCondition(target);
                }
            }
            else //Body does not exist -> Condition loop
            {
                target.EmitLabel(Position, Block.BeginLabel);
                _emitCondition(target);
            }

            target.EmitLabel(Position, Block.BreakLabel);
            target.EndBlock();
        }

        private void _emitCondition(CompilerTarget target)
        {
            target.EmitLabel(Position, Block.ContinueLabel);
            AstLazyLogical.EmitJumpCondition(target, Condition, Block.BeginLabel, IsPositive);
        }
    }
}