namespace Prexonite.Compiler.Ast
{
    public abstract class AstLoop : AstNode,
                                    IAstHasBlocks

    {
        protected AstLoop(string file, int line, int column)
            : base(file, line, column)
        {
            _block = new AstLoopBlock(file, line, column,parentNode:this);
        }

        internal AstLoop(Parser p)
            : base(p)
        {
            _block = new AstLoopBlock(p,parentNode:this);
        }

        private readonly AstLoopBlock _block;

        #region IAstHasBlocks Members

        public virtual AstBlock[] Blocks
        {
            get { return new[] {Block}; }
        }

        #endregion

        #region IAstHasExpressions Members

        public abstract IAstExpression[] Expressions { get; }

        public AstLoopBlock Block
        {
            get { return _block; }
        }

        #endregion
    }
}