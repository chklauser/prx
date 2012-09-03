namespace Prexonite.Compiler.Symbolic
{
    public interface ISymbolHandler<in TArg, out TResult>
    {
        TResult HandleCall(CallSymbol symbol, TArg argument);
        TResult HandleExpand(ExpandSymbol symbol, TArg argument);
        TResult HandleMessage(MessageSymbol symbol, TArg argument);
        TResult HandleDereference(DereferenceSymbol symbol, TArg argument);
        TResult HandleReferenceTo(ReferenceToSymbol symbol, TArg argument);
        TResult HandleMacroInstance(MacroInstanceSymbol symbol, TArg argument);
    }
}