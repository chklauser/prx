using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstScopedExpr : AstExpr
    {
        [NotNull]
        private readonly AstBlock _lexicalScope;

        protected AstScopedExpr([NotNull] ISourcePosition position, [NotNull] AstBlock lexicalScope)
            : base(position)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException("lexicalScope");
            _lexicalScope = lexicalScope;
        }

        internal AstScopedExpr([NotNull] Parser p, [NotNull]  AstBlock lexicalScope)
            : base(p)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException("lexicalScope");
            _lexicalScope = lexicalScope;
        }

        protected AstScopedExpr([NotNull] string file, int line, int column, [NotNull] AstBlock lexicalScope)
            : base(file, line, column)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException("lexicalScope");
            _lexicalScope = lexicalScope;
        }

        /// <summary>
        ///     The node this block is a part of.
        /// </summary>
        [NotNull]
        public AstBlock LexicalScope
        {
            [DebuggerStepThrough]
            get { return _lexicalScope; }
        }
    }
}