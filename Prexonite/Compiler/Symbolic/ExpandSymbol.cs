using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("expand {Entity}")]
    public sealed class ExpandSymbol : Symbol
    {
        public static ExpandSymbol Create(EntityRef entity)
        {
            return new ExpandSymbol(entity);
        }

        private readonly EntityRef _entity;

        private ExpandSymbol(EntityRef entity)
        {
            _entity = entity;
        }

        public EntityRef Entity
        {
            get { return _entity; }
        }

        public bool Equals(ExpandSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._entity, _entity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ExpandSymbol)) return false;
            return Equals((ExpandSymbol)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_entity.GetHashCode() * 397);
            }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleExpand(this, argument);
        }

        public override bool TryGetExpandSymbol(out ExpandSymbol expandSymbol)
        {
            expandSymbol = this;
            return true;
        }
    }
}