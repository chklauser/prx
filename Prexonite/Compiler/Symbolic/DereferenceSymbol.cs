using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class DereferenceSymbol : WrappingSymbol
{
    public override string ToString()
    {
        if (InnerSymbol is ReferenceSymbol rsym)
        {
            return rsym.Entity.ToString();
        }
        else
        {
            return $"ref {InnerSymbol}";
        }
    }

    internal static Symbol _Create(Symbol symbol, ISourcePosition? position)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));
        return new DereferenceSymbol(position ?? symbol.Position, symbol);
    }

    DereferenceSymbol(ISourcePosition position, Symbol inner)
        : base(position, inner) { }

    #region Equality members

    #region Overrides of WrappingSymbol

    protected override int HashCodeXorFactor => 5557;

    public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition? newPosition = null)
    {
        return new DereferenceSymbol(newPosition ?? Position, newInnerSymbol);
    }

    #endregion

    #endregion

    #region Overrides of Symbol

    public override TResult HandleWith<TArg, TResult>(
        ISymbolHandler<TArg, TResult> handler,
        TArg argument
    )
    {
        return handler.HandleDereference(this, argument);
    }

    public override bool TryGetDereferenceSymbol(
        [NotNullWhen(true)] out DereferenceSymbol? dereferenceSymbol
    )
    {
        dereferenceSymbol = this;
        return true;
    }

    public override bool Equals(Symbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return other is DereferenceSymbol otherDeref && Equals(otherDeref);
    }

    #endregion
}
