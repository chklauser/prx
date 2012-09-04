namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolHandler<TArg, TResult> : ISymbolHandler<TArg, TResult>
    {
        protected abstract TResult HandleSymbolDefault(Symbol symbol, TArg argument);

        #region Implementation of ISymbolHandler<in TArg,out TResult>

        public virtual TResult HandleCall(CallSymbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        public virtual TResult HandleExpand(ExpandSymbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        public virtual TResult HandleMessage(MessageSymbol self, TArg argument)
        {
            if (self.Symbol == null)
                return HandleSymbolDefault(self, argument);
            else
                return self.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleDereference(DereferenceSymbol self, TArg argument)
        {
            return self.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleReferenceTo(ReferenceToSymbol self, TArg argument)
        {
            return self.Symbol.HandleWith(this, argument);
        }

        public virtual TResult HandleMacroInstance(MacroInstanceSymbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        #endregion
    }
}