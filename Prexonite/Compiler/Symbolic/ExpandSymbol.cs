

using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class ExpandSymbol : WrappingSymbol
{
    public override string ToString()
    {
        return $"expand {InnerSymbol}";
    }

    internal static ExpandSymbol _Create(Symbol inner, ISourcePosition? position)
    {
        return new(position ?? inner.Position, inner);
    }

    ExpandSymbol(ISourcePosition position, Symbol inner) : base(position, inner)
    {
    }

    protected override int HashCodeXorFactor => 588697;

    public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition? newPosition = null)
    {
        return new ExpandSymbol(newPosition ??  Position,newInnerSymbol);
    }

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleExpand(this, argument);
    }

    public override bool TryGetExpandSymbol([NotNullWhen(true)] out ExpandSymbol? expandSymbol)
    {
        expandSymbol = this;
        return true;
    }

    public override bool Equals(Symbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return other is ExpandSymbol otherExpand && Equals(otherExpand);
    }
}