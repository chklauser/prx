

using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstLogicalOr : AstLazyLogical, IAstPartiallyApplicable
{
    public AstLogicalOr(
        string file,
        int line,
        int column,
        AstExpr leftCondition,
        AstExpr rightCondition)
        : base(file, line, column, leftCondition, rightCondition)
    {
    }

    internal AstLogicalOr(Parser p, AstExpr leftCondition, AstExpr rightCondition)
        : base(p, leftCondition, rightCondition)
    {
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
        var trueLabel = @"True\" + labelNs;
        var falseLabel = @"False\" + labelNs;
        var evalLabel = @"Eval\" + labelNs;

        EmitCode(target, trueLabel, falseLabel);

        if (stackSemantics == StackSemantics.Value)
        {
            target.EmitLabel(Position, falseLabel);
            target.EmitConstant(Position, false);
            target.EmitJump(Position, evalLabel);
            target.EmitLabel(Position, trueLabel);
            target.EmitConstant(Position, true);
            target.EmitLabel(Position, evalLabel);
        }
        else
        {
            Debug.Assert(stackSemantics == StackSemantics.Effect);
            target.EmitLabel(Position, falseLabel);
            target.EmitLabel(Position, trueLabel);
        }
    }

    protected override void DoEmitCode(CompilerTarget target, string trueLabel,
        string falseLabel)
    {
        var labelNs = @"Or\" + Guid.NewGuid().ToString("N");
        var nextLabel = @"Next\" + labelNs;
        foreach (var expr in Conditions)
        {
            if (expr is AstLogicalAnd and)
            {
                and.EmitCode(target, trueLabel, nextLabel);
                //ResolveOperator pending jumps to Next
                target.EmitLabel(Position, nextLabel);
                target.FreeLabel(nextLabel);
                //Future references of to nextLabel will be resolved in the next iteration
            }
            else
            {
                expr.EmitValueCode(target);
                target.EmitJumpIfTrue(Position, trueLabel);
            }
        }
        target.EmitJump(Position, falseLabel);
    }

    #region Partial application

    protected override bool ShortcircuitValue => true;

    protected override AstExpr CreatePrefix(ISourcePosition position,
        IEnumerable<AstExpr> clauses)
    {
        return CreateDisjunction(position, clauses);
    }

    #endregion
}