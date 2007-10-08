namespace Prexonite.Compiler.Ast
{
    public abstract class AstLoop : AstNode
    {
        protected AstLoop(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstLoop(Parser p)
            : base(p)
        {
        }

        public AstBlock Block;
        public BlockLabels Labels;

        public AstLoop GetCopy()
        {
            AstLoop copy = (AstLoop) MemberwiseClone();
            copy.Block = new AstBlock(File, Line, Column);
            copy.Block.Statements.AddRange(Block.Statements);
            copy.Labels = BlockLabels.CreateExistingLabels(Labels.Prefix, Labels.Uid);

            return copy;
        }

        #region IAstHasBlocks Members

        public virtual AstBlock[] Blocks
        {
            get { return new AstBlock[] {Block}; }
        }

        #endregion
    }
}