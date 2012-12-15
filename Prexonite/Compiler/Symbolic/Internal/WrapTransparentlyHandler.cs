namespace Prexonite.Compiler.Symbolic.Internal
{
    internal abstract class WrapTransparentlyHandler : ISymbolHandler<object,Symbol>
    {
        #region Implementation of ISymbolHandler<in object,out Symbol>

        protected abstract Symbol Wrap(Symbol inner);

        public virtual Symbol HandleExpand(ExpandSymbol self, object argument)
        {
            return Wrap(self);
        }

        public virtual Symbol HandleMessage(MessageSymbol self, object argument)
        {
            return self.With(Wrap(self.InnerSymbol));
        }

        public virtual Symbol HandleDereference(DereferenceSymbol self, object argument)
        {
            return Wrap(self);
        }

        public Symbol HandleReference(ReferenceSymbol self, object argument)
        {
            return Wrap(self);
        }

        public Symbol HandleNil(NilSymbol self, object argument)
        {
            return Wrap(self);
        }

        #endregion
    }
}