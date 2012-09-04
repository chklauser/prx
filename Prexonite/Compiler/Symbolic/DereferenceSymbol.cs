using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("ref {Symbol}")]
    public sealed class DereferenceSymbol : Symbol
    {
        private class WrapInDereferenceHandler : WrapTransparentlyHandler
        {
            #region Overrides of WrapTransparentlyHandler

            protected override Symbol Wrap(Symbol inner)
            {
                return new DereferenceSymbol(inner);
            }

            public override Symbol HandleReferenceTo(ReferenceToSymbol self, object argument)
            {
                return self.Symbol.HandleWith(this, argument);
            }

            #endregion
        }
        private static readonly WrapTransparentlyHandler _wrapInReference = new WrapInDereferenceHandler();

        [NotNull]
        public static Symbol Create([NotNull] Symbol symbol)
        {
            if (symbol == null)
                throw new System.ArgumentNullException("symbol");
            return symbol.HandleWith(_wrapInReference, null);
        }

        [NotNull]
        private readonly Symbol _symbol;

        private DereferenceSymbol(Symbol symbol)
        {
            _symbol = symbol;
        }

        [NotNull]
        public Symbol Symbol
        {
            get { return _symbol; }
        }

        #region Equality members

        private bool _equals(DereferenceSymbol other)
        {
            return _symbol.Equals(other._symbol);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DereferenceSymbol && _equals((DereferenceSymbol) obj);
        }

        public override int GetHashCode()
        {
            return 111 ^ _symbol.GetHashCode();
        }

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

        #endregion
    }
}