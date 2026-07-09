
namespace Prexonite.Compiler.Ast;

public abstract class AstTypeExpr : AstExpr
{
    protected AstTypeExpr(ISourcePosition position) : base(position)
    {
    }

    internal AstTypeExpr(Parser p) : base(p)
    {
    }

    protected AstTypeExpr(string file, int line, int column) : base(file, line, column)
    {
    }
}