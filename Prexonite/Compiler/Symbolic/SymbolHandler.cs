

using JetBrains.Annotations;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic;

[PublicAPI]
public abstract class SymbolHandler<TArg, TResult> : ISymbolHandler<TArg, TResult>
{
    [PublicAPI]
    protected virtual TResult HandleSymbolDefault(Symbol self, TArg argument)
    {
        throw new NotSupportedException(
            string.Format(
                Resources.SymbolHandler_CannotHandleSymbolOfType, GetType().Name,
                self.GetType().Name));
    }

    [PublicAPI]
    protected virtual TResult HandleWrappingSymbol(WrappingSymbol self, TArg argument)
    {
        return HandleSymbolDefault(self, argument);
    }

    [PublicAPI]
    protected virtual TResult HandleLeafSymbol(Symbol self, TArg argument)
    {
        return HandleSymbolDefault(self, argument);
    }

    #region Implementation of ISymbolHandler<in TArg,out TResult>

    [PublicAPI]
    public virtual TResult HandleReference(ReferenceSymbol self, TArg argument)
    {
        return HandleLeafSymbol(self, argument);
    }

    [PublicAPI]
    public virtual TResult HandleNil(NilSymbol self, TArg argument)
    {
        return HandleLeafSymbol(self, argument);
    }

    [PublicAPI]
    public virtual TResult HandleExpand(ExpandSymbol self, TArg argument)
    {
        return HandleWrappingSymbol(self, argument);
    }

    [PublicAPI]
    public virtual TResult HandleDereference(DereferenceSymbol self, TArg argument)
    {
        return HandleWrappingSymbol(self, argument);
    }

    [PublicAPI]
    public virtual TResult HandleMessage(MessageSymbol self, TArg argument)
    {
        return HandleWrappingSymbol(self, argument);
    }

    [PublicAPI]
    public virtual TResult HandleNamespace(NamespaceSymbol self, TArg argument)
    {
        return HandleLeafSymbol(self, argument);
    }

    #endregion
}