

using Prexonite.Commands.Core.Operators;

namespace Prexonite.Compiler.Ast;

public class AstConditionalExpression : AstExpr,
    IAstHasExpressions
{
    public AstConditionalExpression(
        string file, int line, int column, AstExpr condition, bool isNegative)
        : base(file, line, column)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        IsNegative = isNegative;
    }

    public AstConditionalExpression(string file, int line, int column, AstExpr condition)
        : this(file, line, column, condition, false)
    {
    }

    internal AstConditionalExpression(Parser p, AstExpr condition, bool isNegative)
        : this(p.scanner.File, p.t.line, p.t.col, condition, isNegative)
    {
    }

    internal AstConditionalExpression(Parser p, AstExpr condition)
        : this(p, condition, false)
    {
    }

    public required AstExpr IfExpression;
    public required AstExpr ElseExpression;
    public AstExpr Condition;
    public bool IsNegative;
    static int _depth;

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return new[] {Condition, IfExpression, ElseExpression}.ToArray(); }
    }

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        //Optimize condition
        _OptimizeNode(target, ref Condition);
        // Invert condition when unary logical not
        while (Condition.IsCommandCall(LogicalNot.DefaultAlias, out var unaryCond))
        {
            Condition = unaryCond.Arguments[0];
            IsNegative = !IsNegative;
        }

        //Constant conditions
        if (Condition is AstConstant constCond)
        {
            if (!constCond.ToPValue(target).TryConvertTo(target.Loader, PType.Bool, out var condValue))
                expr = null;
            else if ((bool) condValue.Value! ^ IsNegative)
                expr = IfExpression;
            else
                expr = ElseExpression;
            return expr != null;
        }

        expr = null;
        return false;
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //Optimize condition
        _OptimizeNode(target, ref Condition);
        _OptimizeNode(target, ref IfExpression);
        _OptimizeNode(target, ref ElseExpression);

        var elseLabel = "elsei\\" + _depth + "\\assembler";
        var endLabel = "endifi\\" + _depth + "\\assembler";
        _depth++;

        //Emit
        //if => block / else => block
        AstLazyLogical.EmitJumpCondition(target, Condition, elseLabel, IsNegative);
        IfExpression.EmitCode(target, stackSemantics);
        target.EmitJump(Position, endLabel);
        target.EmitLabel(Position, elseLabel);
        ElseExpression.EmitCode(target, stackSemantics);
        target.EmitLabel(Position, endLabel);

        target.FreeLabel(elseLabel);
        target.FreeLabel(endLabel);
    }
}