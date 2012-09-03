namespace Prexonite.Compiler.Symbolic.Internal
{
    internal abstract class WrapTransparentlyHandler : ISymbolHandler<object,Symbol>
    {
        #region Implementation of ISymbolHandler<in object,out Symbol>

        protected abstract Symbol Wrap(Symbol inner);

        public virtual Symbol HandleCall(CallSymbol symbol, object argument)
        {
            return Wrap(symbol);
        }

        public virtual Symbol HandleExpand(ExpandSymbol symbol, object argument)
        {
            return Wrap(symbol);
        }

        public virtual Symbol HandleMessage(MessageSymbol symbol, object argument)
        {
            // Keep message symbols on the outside, except when it doesn't wrap
            //  an inner symbol (pure error symbols)
            if (symbol.Symbol == null)
                return symbol;
            else
                return MessageSymbol.Create(symbol.Message, Wrap(symbol.Symbol));
        }

        public virtual Symbol HandleDereference(DereferenceSymbol symbol, object argument)
        {
            return Wrap(symbol);
        }

        public virtual Symbol HandleReferenceTo(ReferenceToSymbol symbol, object argument)
        {
            return Wrap(symbol);
        }

        public virtual Symbol HandleMacroInstance(MacroInstanceSymbol symbol, object argument)
        {
            return Wrap(symbol);
        }

        #endregion
    }
}