

using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class ReferenceSymbol : Symbol, IEquatable<ReferenceSymbol>
{
    public override string ToString()
    {
        return $"->{Entity}";
    }

    [DebuggerStepThrough]
    ReferenceSymbol(ISourcePosition position, EntityRef entity)
    {
        Entity = entity;
        Position = position;
    }

    internal static ReferenceSymbol _Create(EntityRef entity, ISourcePosition position)
    {
        return new(position, entity);
    }

    public EntityRef Entity { get; }

    public override ISourcePosition Position { get; }

    #region Overrides of Symbol

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleReference(this, argument);
    }

    public override bool TryGetReferenceSymbol([NotNullWhen(true)] out ReferenceSymbol? referenceSymbol)
    {
        referenceSymbol = this;
        return true;
    }

    #endregion

    public bool Equals(ReferenceSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Entity, other.Entity);
    }

    public override bool Equals(Symbol? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is ReferenceSymbol && Equals((ReferenceSymbol)obj);
    }

    public override int GetHashCode()
    {
        return Entity != null ? Entity.GetHashCode() : 0;
    }
}