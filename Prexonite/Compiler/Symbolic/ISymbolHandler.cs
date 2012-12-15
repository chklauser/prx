using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    public interface ISymbolHandler<in TArg, out TResult>
    {
        TResult HandleReference([NotNull] ReferenceSymbol self, TArg argument);
        TResult HandleNil([NotNull] NilSymbol self, TArg argument);

        TResult HandleExpand([NotNull] ExpandSymbol self, TArg argument);
        TResult HandleDereference([NotNull] DereferenceSymbol self, TArg argument);

        TResult HandleMessage([NotNull] MessageSymbol self, TArg argument);
    }
}