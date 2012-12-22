using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class DereferenceSymbol : WrappingSymbol
    {
        public override string ToString()
        {
            var rsym = InnerSymbol as ReferenceSymbol;

            if (rsym != null)
            {
                return rsym.Entity.ToString();
            }
            else
            {
                return string.Format("ref {0}", InnerSymbol);
            }
        }

        [NotNull]
        internal static Symbol _Create([NotNull] Symbol symbol, [CanBeNull] ISourcePosition position)
        {
            if (symbol == null)
                throw new System.ArgumentNullException("symbol");
            return new DereferenceSymbol(position ?? symbol.Position, symbol);
        }

        private DereferenceSymbol([NotNull] ISourcePosition position, Symbol inner)
            : base(position, inner)
        {
        }

        #region Equality members

        #region Overrides of WrappingSymbol

        protected override int HashCodeXorFactor
        {
            get { return 5557; }
        }

        public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition newPosition = null)
        {
            return new DereferenceSymbol(newPosition ?? Position, newInnerSymbol);
        }

        #endregion

        #endregion

        #region Overrides of Symbol

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleDereference(this, argument);
        }

        public override bool TryGetDereferenceSymbol(out DereferenceSymbol dereferenceSymbol)
        {
            dereferenceSymbol = this;
            return true;
        }

        public override bool Equals(Symbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is DereferenceSymbol && Equals((DereferenceSymbol)other);
        }

        #endregion
    }
}