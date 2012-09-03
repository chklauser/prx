namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolHandler<TArg, TResult> : ISymbolHandler<TArg, TResult>
    {
        protected abstract TResult HandleSymbolDefault(Symbol symbol, TArg argument);

        #region Implementation of ISymbolHandler<in TArg,out TResult>

        public virtual TResult HandleCall(CallSymbol symbol, TArg argument)
        {
            return HandleSymbolDefault(symbol, argument);
        }

        public virtual TResult HandleExpand(ExpandSymbol symbol, TArg argument)
        {
            return HandleSymbolDefault(symbol, argument);
        }

        public virtual TResult HandleMessage(MessageSymbol symbol, TArg argument)
        {
            if (symbol.Symbol == null)
                return HandleSymbolDefault(symbol, argument);
            else
                return symbol.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleDereference(DereferenceSymbol symbol, TArg argument)
        {
            return symbol.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleReferenceTo(ReferenceToSymbol symbol, TArg argument)
        {
            return symbol.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleMacroInstance(MacroInstanceSymbol symbol, TArg argument)
        {
            return HandleSymbolDefault(symbol, argument);
        }

        #endregion
    }
}