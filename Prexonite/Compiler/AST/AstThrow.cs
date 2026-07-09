
namespace Prexonite.Compiler.Ast;

public class AstThrow : AstExpr,
    IAstHasExpressions
{
    public required AstExpr Expression;

    public AstThrow(string file, int line, int column)
        : base(file, line, column)
    {
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return [Expression]; }
    }

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    #endregion


    public override string ToString()
    {
        return "throw " + (Expression.ToString() ?? "");
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (Expression == null)
            throw new PrexoniteException("Expression must be assigned.");

        Expression.EmitValueCode(target);
        target.Emit(Position,OpCode.@throw);

        if (stackSemantics == StackSemantics.Value)
            target.Emit(Position,OpCode.ldc_null);
    }
}