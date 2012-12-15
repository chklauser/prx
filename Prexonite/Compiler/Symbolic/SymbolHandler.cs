using System;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolHandler<TArg, TResult> : ISymbolHandler<TArg, TResult>
    {
        protected virtual TResult HandleSymbolDefault(Symbol self, TArg argument)
        {
            throw new NotSupportedException(
                string.Format(
                    Resources.SymbolHandler_CannotHandleSymbolOfType, GetType().Name,
                    self.GetType().Name));
        }

        protected virtual TResult HandleWrappingSymbol(WrappingSymbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        protected virtual TResult HandleLeafSymbol(Symbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        #region Implementation of ISymbolHandler<in TArg,out TResult>

        public virtual TResult HandleReference(ReferenceSymbol self, TArg argument)
        {
            return HandleLeafSymbol(self, argument);
        }

        public virtual TResult HandleNil(NilSymbol self, TArg argument)
        {
            return HandleLeafSymbol(self, argument);
        }

        public virtual TResult HandleExpand(ExpandSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        public virtual TResult HandleDereference(DereferenceSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        public virtual TResult HandleMessage(MessageSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        #endregion
    }
}