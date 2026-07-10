namespace Prexonite.Compiler.Symbolic.Internal;

public class ReplaceCoreNilHandler : TransformHandler<Symbol>
{
    #region Singleton Pattern

    public static ReplaceCoreNilHandler Instance { get; } = new();

    ReplaceCoreNilHandler() { }

    #endregion

    public override Symbol HandleNil(NilSymbol self, Symbol argument)
    {
        return argument;
    }
}
