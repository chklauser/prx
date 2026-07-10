namespace Prexonite.Compiler.Ast;

public abstract class AstLoop : AstNode, IAstHasBlocks
{
    internal AstLoop(ISourcePosition p, AstBlock parentBlock)
        : base(p)
    {
        Block = new(p, parentBlock, prefix: "body");
    }

    #region IAstHasBlocks Members

    public virtual AstBlock[] Blocks
    {
        get { return [Block]; }
    }

    #endregion

    #region IAstHasExpressions Members

    public abstract AstExpr[] Expressions { get; }

    public AstLoopBlock Block { get; }

    #endregion
}
