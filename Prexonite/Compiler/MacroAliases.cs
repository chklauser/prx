

namespace Prexonite.Compiler;

public static class MacroAliases
{
    public const string ContextAlias = "context";

    public static IEnumerable<string> Aliases()
    {
        yield return ContextAlias;
    }
}