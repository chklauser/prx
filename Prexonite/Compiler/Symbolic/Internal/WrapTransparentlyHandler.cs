namespace Prexonite.Compiler.Symbolic.Internal
{
    internal abstract class WrapTransparentlyHandler : ISymbolHandler<object,Symbol>
    {
        #region Implementation of ISymbolHandler<in object,out Symbol>

        protected abstract Symbol Wrap(Symbol inner);

        public virtual Symbol HandleCall(CallSymbol self, object argument)
        {
            return Wrap(self);
        }

        public virtual Symbol HandleExpand(ExpandSymbol self, object argument)
        {
            return Wrap(self);
        }

        public virtual Symbol HandleMessage(MessageSymbol self, object argument)
        {
            // Keep message symbols on the outside, except when it doesn't wrap
            //  an inner symbol (pure error symbols)
            if (self.Symbol == null)
                return self;
            else
                return MessageSymbol.Create(self.Message, Wrap(self.Symbol));
        }

        public virtual Symbol HandleDereference(DereferenceSymbol self, object argument)
        {
            return Wrap(self);
        }

        public virtual Symbol HandleReferenceTo(ReferenceToSymbol self, object argument)
        {
            return Wrap(self);
        }

        public virtual Symbol HandleMacroInstance(MacroInstanceSymbol self, object argument)
        {
            return Wrap(self);
        }

        #endregion
    }
}