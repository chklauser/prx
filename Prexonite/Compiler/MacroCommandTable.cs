using Prexonite.Compiler.Macro;

namespace Prexonite.Compiler
{
    public class MacroCommandTable : System.Collections.ObjectModel.KeyedCollection<string, MacroCommand>
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
}