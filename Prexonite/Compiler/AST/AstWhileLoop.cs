

using System.Diagnostics;
using Prexonite.Commands.Core.Operators;

namespace Prexonite.Compiler.Ast;

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

    public AstExpr? Condition;
    public bool IsPrecondition { get; set; }
    public bool IsPositive { get; set; }

    public override AstExpr[] Expressions
    {
        get { return Condition != null ? [Condition] : []; }
    }

    [MemberNotNullWhen(true, nameof(Condition))]
    public bool IsInitialized
    {
        [DebuggerStepThrough]
        get => Condition != null;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Value)
            throw new NotSupportedException("While loops do not produce values and can thus not be used as expressions.");
        if (!IsInitialized)
            throw new PrexoniteException("AstWhileLoop requires Condition to be set.");

        //Optimize unary not condition
        _OptimizeNode(target, ref Condition);
        // Invert condition when unary logical not
        while (Condition.IsCommandCall(LogicalNot.DefaultAlias, out var unaryCond))
        {
            Condition = unaryCond.Arguments[0];
            IsPositive = !IsPositive;
        }

        //Constant conditions
        var conditionIsConstant = false;
        if (Condition is AstConstant constCond)
        {
            if (
                !constCond.ToPValue(target).TryConvertTo(
                    target.Loader, PType.Bool, out var condValue))
            {}
            else if ((bool) condValue.Value! == IsPositive)
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

    void _emitCondition(CompilerTarget target)
    {
        target.EmitLabel(Position, Block.ContinueLabel);
        AstLazyLogical.EmitJumpCondition(target, Condition!, Block.BeginLabel, IsPositive);
    }
}