using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class ReferenceSymbol : Symbol, IEquatable<ReferenceSymbol>
    {
        public override string ToString()
        {
            return string.Format("->{0}", Entity);
        }

        private readonly EntityRef _entity;

        [DebuggerStepThrough]
        private ReferenceSymbol([NotNull] ISourcePosition position, [NotNull] EntityRef entity)
        {
            _entity = entity;
            _position = position;
        }

        [NotNull]
        internal static ReferenceSymbol _Create([NotNull] EntityRef entity, [NotNull] ISourcePosition position)
        {
            return new ReferenceSymbol(position, entity);
        }

        public EntityRef Entity
        {
            get { return _entity; }
        }

        [NotNull]
        private readonly ISourcePosition _position;

        public override ISourcePosition Position
        {
            get { return _position; }
        }

        #region Overrides of Symbol

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleReference(this, argument);
        }

        public override bool TryGetReferenceSymbol(out ReferenceSymbol referenceSymbol)
        {
            referenceSymbol = this;
            return true;
        }

        #endregion

        public bool Equals(ReferenceSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_entity, other._entity);
        }

        public override bool Equals(Symbol obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ReferenceSymbol && Equals((ReferenceSymbol)obj);
        }

        public override int GetHashCode()
        {
            return (_entity != null ? _entity.GetHashCode() : 0);
        }
    }
}