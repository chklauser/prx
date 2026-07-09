

namespace Prexonite.Compiler.Ast;

public class AstGetException : AstExpr
{
    internal AstGetException(Parser p)
        : base(p)
    {
    }

    public AstGetException(string file, int line, int column)
        : base(file, line, column)
    {
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Effect)
            return;

        target.Emit(Position,OpCode.exc);
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    #endregion
}