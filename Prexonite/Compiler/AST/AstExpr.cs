namespace Prexonite.Compiler.Ast;

public abstract class AstExpr : AstNode
{
    protected AstExpr(ISourcePosition position)
        : base(position)
    {
    }

    internal AstExpr(Parser p)
        : base(p)
    {
    }

    protected AstExpr(string file, int line, int column)
        : base(file, line, column)
    {
    }

    #region Implementation of AstExpr

    /// <summary>
    /// Gives the node a chance to replace itself with a simpler node after inspecting its children. On returning
    /// <c>true</c>, the <c>out</c> parameter <paramref name="expr"/> holds the replacement node.
    /// </summary>
    /// <para>Note that the node may still have simplified its internals/children even when the method returns
    /// <c>false</c>.</para>
    /// <param name="target">The context in which to perform optimizations.</param>
    /// <param name="expr">If <c>true</c> is returned, holds the replacement expression.</param>
    /// <returns><c>true</c> if <c>this</c> node should be replaced by <paramref name="expr"/>;
    /// <c>false</c> otherwise</returns>
    public abstract bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr);

    #endregion

}