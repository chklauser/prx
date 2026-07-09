
namespace Prexonite.Compiler.Ast;

public class AstNull : AstExpr
{
    public AstNull(string file, int line, int column)
        : base(file, line, column)
    {
    }

    internal AstNull(Parser p)
        : base(p)
    {
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Effect)
            return;

        target.EmitNull(Position);
    }
}