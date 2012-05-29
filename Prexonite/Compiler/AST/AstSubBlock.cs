using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast
{
    public class AstSubBlock : AstBlock
    {
        [NotNull]
        private readonly AstBlock _lexicalScope;

        public AstSubBlock([NotNull] ISourcePosition p, [NotNull] AstBlock lexicalScope, string uid = null, string prefix = null)
            : base(p,lexicalScope, uid:uid, prefix:prefix)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException("lexicalScope");
            _lexicalScope = lexicalScope;
        }

        /// <summary>
        ///     The node this block is a part of. Can be null.
        /// </summary>
        [NotNull]
        public AstNode LexicalScope
        {
            [DebuggerStepThrough]
            get { return _lexicalScope; }
        }
    }
}