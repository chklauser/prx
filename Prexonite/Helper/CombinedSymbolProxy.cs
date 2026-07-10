namespace Prexonite;

public class CombinedSymbolProxy<T>
{
    CombinedSymbolProxy(ISymbolTable<T>[] tables)
    {
        var copy = new ISymbolTable<T>[tables.Length];
        Array.Copy(tables, copy, tables.Length);
        _tables = tables;
    }

    public static CombinedSymbolProxy<T> CreateHierarchy(params ISymbolTable<T>[] tableHierarchy)
    {
        return new(tableHierarchy);
    }

    readonly ISymbolTable<T>[] _tables;
    protected ISymbolTable<T> TargetTable => _tables[^1];
}
