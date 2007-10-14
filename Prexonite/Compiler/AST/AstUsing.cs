using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstUsing : AstNode,
                            IAstHasBlocks,
                            IAstHasExpressions
    {
        internal AstUsing(Parser p)
            : base(p)
        {
            Container = null;
            Block = new AstBlock(File, Line, Column);
        }

        public AstUsing(string file, int line, int column)
            : base(file, line, column)
        {
            Container = null;
            Block = new AstBlock(File, Line, Column);
        }

        internal AstUsing(Parser p, AstGetSet container)
            : base(p)
        {
            Container = container;
            Block = new AstBlock(File, Line, Column);
        }

        public AstUsing(string file, int line, int column, AstGetSet container)
            : base(file, line, column)
        {
            Container = container;
            Block = new AstBlock(File, Line, Column);
        }

        public AstGetSet Container;
        public IAstExpression Expression;
        public AstBlock Block;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new AstBlock[] {Block}; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {Expression}; }
        }

        #endregion

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (Container == null)
                throw new PrexoniteException("AstUsing requires Container to be initialized.");
            if (Expression == null)
                throw new PrexoniteException("AstUsing requires Expression to be initialized.");

            AstTryCatchFinally _try = new AstTryCatchFinally(File, Line, Column);
            //Try block => {Container} = {Expression}; {Block};
            AstGetSet setContainer = Container.GetCopy();
            setContainer.Call = PCall.Set;
            setContainer.Arguments.Add(Expression);

            AstBlock _tryBlock = _try.TryBlock;
            _tryBlock.Add(setContainer);
            _tryBlock.AddRange(Block);

            //Finally block => dispose( {Container} );
            AstGetSetSymbol dispose =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    Engine.DisposeCommand,
                    SymbolInterpretations.Command);
            dispose.Arguments.Add(Container);

            _try.FinallyBlock.Add(dispose);

            //Emit code!
            _try.EmitCode(target);
        }
    }
}