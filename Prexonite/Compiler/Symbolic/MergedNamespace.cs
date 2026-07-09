

namespace Prexonite.Compiler.Symbolic;

public class MergedNamespace(SymbolStore exportScope) : Namespace
{
    public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
    {
        return exportScope.GetEnumerator();
    }

    public override bool IsEmpty => exportScope.IsEmpty;

    public override bool TryGet(string id, [NotNullWhen(true)] out Symbol? value)
    {
        return exportScope.TryGet(id, out value);
    }
}