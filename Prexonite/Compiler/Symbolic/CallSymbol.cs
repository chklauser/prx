using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("call({Entity})")]
    public sealed class CallSymbol : Symbol
    {
        public static CallSymbol Create(EntityRef entity)
        {
            return new CallSymbol(entity);
        }

        private readonly EntityRef _entity;

        private CallSymbol(EntityRef entity)
        {
            _entity = entity;
        }

        public EntityRef Entity
        {
            get { return _entity; }
        }

        public bool Equals(CallSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._entity, _entity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (CallSymbol)) return false;
            return Equals((CallSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_entity.GetHashCode()*397);
            }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleCall(this, argument);
        }

        public override bool TryGetCallSymbol(out CallSymbol callSymbol)
        {
            callSymbol = this;
            return true;
        }
    }
}