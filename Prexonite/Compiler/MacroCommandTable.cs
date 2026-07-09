

using System.Collections.ObjectModel;
using Prexonite.Compiler.Macro;

namespace Prexonite.Compiler;

public class MacroCommandTable : KeyedCollection<string, MacroCommand>
{
    public MacroCommandTable()
        : base(Engine.DefaultStringComparer)
    {
    }

    protected override string GetKeyForItem(MacroCommand item)
    {
        return item.Id;
    }
}