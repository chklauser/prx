namespace Prexonite.Compiler.Ast
{
    public abstract class AstLoop : AstNode,
                                    IAstHasBlocks

    {
        protected AstLoop(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstLoop(Parser p)
            : base(p)
        {
        }

        public AstLoopBlock Block;

        #region IAstHasBlocks Members

        public virtual AstBlock[] Blocks
        {
            get { return new[] {Block}; }
        }

        #endregion

        #region IAstHasExpressions Members

        public abstract IAstExpression[] Expressions { get; }

        #endregion
    }
}