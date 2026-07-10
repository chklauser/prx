namespace Prexonite.Compiler.Symbolic;

public interface ISymbolHandler<in TArg, out TResult>
{
    TResult HandleReference(ReferenceSymbol self, TArg argument);

    TResult HandleNil(NilSymbol self, TArg argument);

    TResult HandleExpand(ExpandSymbol self, TArg argument);

    TResult HandleDereference(DereferenceSymbol self, TArg argument);

    TResult HandleMessage(MessageSymbol self, TArg argument);

    TResult HandleNamespace(NamespaceSymbol self, TArg argument);
}
