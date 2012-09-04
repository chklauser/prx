namespace Prexonite.Compiler.Symbolic
{
    public interface ISymbolHandler<in TArg, out TResult>
    {
        TResult HandleCall(CallSymbol self, TArg argument);
        TResult HandleExpand(ExpandSymbol self, TArg argument);
        TResult HandleMessage(MessageSymbol self, TArg argument);
        TResult HandleDereference(DereferenceSymbol self, TArg argument);
        TResult HandleReferenceTo(ReferenceToSymbol self, TArg argument);
        TResult HandleMacroInstance(MacroInstanceSymbol self, TArg argument);
    }
}