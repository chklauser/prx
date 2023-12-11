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

using System.Diagnostics;
using Prexonite.Commands.Core.Operators;

namespace Prexonite.Compiler.Ast;

public class AstForLoop : AstLoop
{
    public AstForLoop(ISourcePosition position, AstBlock parentBlock)
        : this(position, new(
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
    AstForLoop(ISourcePosition position, AstScopedBlock nextBlock)
        : base(position, nextBlock)
    {
        Initialize = (AstScopedBlock)nextBlock.LexicalScope;
        NextIteration = nextBlock;
    }

    public AstExpr? Condition { get; set; }
    public AstScopedBlock Initialize { get; }

    public AstScopedBlock NextIteration { get; }

    public bool IsPositive { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = true;

    public bool IsPrecondition { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = true;

    [MemberNotNullWhen(true, nameof(Condition))]
    public bool IsInitialized => Condition != null;

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
        while (Condition.IsCommandCall(LogicalNot.DefaultAlias, out var unaryCond))
        {
            Condition = unaryCond.Arguments[0];
            IsPositive = !IsPositive;
        }

        //Constant conditions
        var conditionIsConstant = false;
        if (condition is AstConstant constCond)
        {
            if (
                !constCond.ToPValue(target).TryConvertTo(
                    target.Loader,
                    PType.Bool,
                    out var condValue))
            {
            }
            else if ((bool) condValue.Value! == IsPositive)
                conditionIsConstant = true;
            else
            {
                //Condition is always false
                return;
            }
        }

        var conditionLabel = Block.CreateLabel(nameof(condition));

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
                Block,
            };
            return blocks.ToArray();
        }
    }

    #region IAstHasExpressions Members

    public override AstExpr[] Expressions
    {
        get { return Condition != null ? new[] {Condition} : Array.Empty<AstExpr>(); }
    }

    #endregion
}