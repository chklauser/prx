

using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public abstract class AstGetSetImplBase : AstGetSet
{
    public override ArgumentsProxy Arguments
    {
        [DebuggerNonUserCode]
        get;
    }

    #region IAstHasExpressions Members

    /// <summary>
    ///     <para>Indicates whether this node uses get or set syntax</para>
    ///     <para>(set syntax involves an equal sign (=); get syntax does not)</para>
    /// </summary>
    public sealed override PCall Call { get; set; }

    #endregion

    protected AstGetSetImplBase(string file, int line, int column, PCall call)
        : this(new SourcePosition(file,line,column), call)
    {
    }

    protected AstGetSetImplBase(ISourcePosition position, PCall call)
        : base(position)
    {
        Call = call;
        Arguments = new(new());
    }

    internal AstGetSetImplBase(Parser p, PCall call)
        : this(p.scanner.File, p.t.line, p.t.col, call)
    {
    }
}