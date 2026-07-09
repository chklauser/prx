

using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("Nil")]
public sealed class NilSymbol : Symbol, IEquatable<NilSymbol>
{
    #region Overrides of Symbol

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleNil(this,argument);
    }

    public override bool TryGetNilSymbol([NotNullWhen(true)] out NilSymbol? nilSymbol)
    {
        nilSymbol = this;
        return true;
    }

    public override bool Equals(Symbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return other is NilSymbol otherRef && Equals(otherRef);
    }

    public override ISourcePosition Position { get; }

    #endregion

    NilSymbol(ISourcePosition position)
    {
        Position = position;
    }

    internal static NilSymbol _Create(ISourcePosition position)
    {
        return new(position);
    }

    public override string ToString()
    {
        return "Nil";
    }

    public bool Equals(NilSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
    }

    const int NilSymbolHashCode = 384950146;

    public override int GetHashCode()
    {
        return NilSymbolHashCode;
    }
}