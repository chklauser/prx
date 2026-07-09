

using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstLogicalAnd : AstLazyLogical, IAstPartiallyApplicable
{
    public AstLogicalAnd(
        string file,
        int line,
        int col,
        AstExpr leftCondition,
        AstExpr rightCondition)
        : base(file, line, col, leftCondition, rightCondition)
    {
    }

    internal AstLogicalAnd(
        Parser p, AstExpr leftCondition, AstExpr rightCondition)
        : base(p, leftCondition, rightCondition)
    {
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var labelNs = $@"And\{Guid.NewGuid():N}";
        var trueLabel = $@"True\{labelNs}";
        var falseLabel = $@"False\{labelNs}";
        var evalLabel = $@"Eval\{labelNs}";

        EmitCode(target, trueLabel, falseLabel);

        // When the AND node gets used as a value (as opposed to a part of control flow),
        // we have to convert the control flow into true/false values.
        if (stackSemantics == StackSemantics.Value)
        {
            target.EmitLabel(Position, trueLabel);
            target.EmitConstant(Position, true);
            target.EmitJump(Position, evalLabel);
            target.EmitLabel(Position, falseLabel);
            target.EmitConstant(Position, false);
            target.EmitLabel(Position, evalLabel);
        }
        else
        {
            Debug.Assert(stackSemantics == StackSemantics.Effect);
            target.EmitLabel(Position, trueLabel);
            target.EmitLabel(Position, falseLabel);
        }
    }

    //Called by either AstLogicalAnd or AstLogicalOr
    protected override void DoEmitCode(CompilerTarget target, string trueLabel,
        string falseLabel)
    {
        var labelNs = @"And\" + Guid.NewGuid().ToString("N");
        var nextLabel = @"Next\" + labelNs;
        foreach (var expr in Conditions)
        {
            if (expr is AstLogicalOr or)
            {
                or.EmitCode(target, nextLabel, falseLabel);
                //ResolveOperator pending jumps to Next
                target.EmitLabel(Position, nextLabel);
                target.FreeLabel(nextLabel);
                //Future references of to nextLabel will be resolved in the next iteration
            }
            else
            {
                expr.EmitValueCode(target);
                target.EmitJumpIfFalse(Position, falseLabel);
            }
        }
        target.EmitJump(Position, trueLabel);
    }

    #region Partial application

    protected override AstExpr CreatePrefix(ISourcePosition position,
        IEnumerable<AstExpr> clauses)
    {
        return CreateConjunction(position, clauses);
    }

    protected override bool ShortcircuitValue => false;

    #endregion
}