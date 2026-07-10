using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public abstract class AstScopedExpr : AstExpr
{
    protected AstScopedExpr(ISourcePosition position, AstBlock lexicalScope)
        : base(position)
    {
        LexicalScope = lexicalScope ?? throw new ArgumentNullException(nameof(lexicalScope));
    }

    internal AstScopedExpr(Parser p, AstBlock lexicalScope)
        : base(p)
    {
        LexicalScope = lexicalScope ?? throw new ArgumentNullException(nameof(lexicalScope));
    }

    protected AstScopedExpr(string file, int line, int column, AstBlock lexicalScope)
        : base(file, line, column)
    {
        LexicalScope = lexicalScope ?? throw new ArgumentNullException(nameof(lexicalScope));
    }

    /// <summary>
    ///     The node this block is a part of.
    /// </summary>
    public AstBlock LexicalScope { [DebuggerStepThrough] get; }
}
