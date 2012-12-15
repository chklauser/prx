using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("->{Symbol}")]
    public sealed class ReferenceToSymbol : Symbol
    {

        [NotNull]
        private readonly ISourcePosition _position;

        [NotNull]
        public override ISourcePosition Position
        {
            get { return _position; }
        }

        public override bool Equals(Symbol other)
        {
            throw new System.NotImplementedException();
        }

        private class WrapInReferenceToHandler : WrapTransparentlyHandler
        {
            #region Overrides of WrapTransparentlyHandler

            protected override Symbol Wrap(Symbol inner)
            {
                return new ReferenceToSymbol(inner);
            }
             
            public override Symbol HandleDereference(DereferenceSymbol self, object argument)
            {
                return self.InnerSymbol.HandleWith(this,argument);
                
            }

            #endregion
        }
        private static readonly WrapTransparentlyHandler _wrapInReference = new WrapInReferenceToHandler(); 

        [NotNull]
        public static Symbol Create([NotNull] Symbol symbol)
        {
            if (symbol == null)
                throw new System.ArgumentNullException("symbol");
            return symbol.HandleWith(_wrapInReference, null);
        }

        [NotNull]
        private readonly Symbol _symbol;

        private ReferenceToSymbol([NotNull] Symbol symbol)
        {
            _symbol = symbol;
        }

        [NotNull]
        public Symbol Symbol
        {
            get { return _symbol; }
        }

        #region Equality members

        private bool _equals(ReferenceToSymbol other)
        {
            return _symbol.Equals(other._symbol);
        }

        public override int GetHashCode()
        {
            return 337 ^ _symbol.GetHashCode();
        }

        #endregion

        #region Overrides of Symbol

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return default(TResult);
        }

        #endregion
    }
}