using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{Name}: ({Symbol}, {Origin})")]
public sealed class SymbolInfo
{
    public SymbolInfo(Symbol symbol, SymbolOrigin origin, string name)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Symbol Symbol { get; }

    public SymbolOrigin Origin { get; }

    public string Name { get; }
}
