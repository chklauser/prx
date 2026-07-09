
namespace Prexonite.Compiler.Symbolic.Internal;

public abstract class TransformHandler<TArg> : SymbolHandler<TArg,Symbol>
{
    #region Overrides of SymbolHandler<TArg,Symbol>

    protected override Symbol HandleWrappingSymbol(WrappingSymbol self, TArg argument)
    {
        var newInner = self.InnerSymbol.HandleWith(this, argument);
        return self.InnerSymbol.Equals(newInner) ? self : self.With(newInner);
    }

    protected override Symbol HandleLeafSymbol(Symbol self, TArg argument)
    {
        return self;
    }

    #endregion
}