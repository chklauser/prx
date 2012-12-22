using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class ExpandSymbol : WrappingSymbol
    {
        public override string ToString()
        {
            return string.Format("expand {0}", InnerSymbol);
        }

        [NotNull]
        internal static ExpandSymbol _Create([NotNull] Symbol inner, [CanBeNull] ISourcePosition position)
        {
            return new ExpandSymbol(position ?? inner.Position, inner);
        }

        private ExpandSymbol([NotNull] ISourcePosition position, [NotNull] Symbol inner) : base(position, inner)
        {
        }

        protected override int HashCodeXorFactor
        {
            get { return 588697; }
        }

        public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition newPosition = null)
        {
            return new ExpandSymbol(newPosition ??  Position,newInnerSymbol);
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

        public override bool Equals(Symbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is ExpandSymbol && Equals((ExpandSymbol)other);
        }
    }
}