

using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstScopedBlock : AstBlock
{
    readonly AstBlock _lexicalScope;

    public AstScopedBlock(ISourcePosition p, AstBlock lexicalScope, string? uid = null, string? prefix = null)
        : base(p,lexicalScope, uid:uid, prefix:prefix)
    {
        _lexicalScope = lexicalScope ?? throw new ArgumentNullException(nameof(lexicalScope));
    }

    /// <summary>
    ///     The node this block is a part of. Can be null.
    /// </summary>
    public AstNode LexicalScope
    {
        [DebuggerStepThrough]
        get => _lexicalScope;
    }
}