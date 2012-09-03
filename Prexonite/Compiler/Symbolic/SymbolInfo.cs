using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{Name}: ({Symbol}, {Origin})")]
    public sealed class SymbolInfo
    {
        private readonly Symbol _symbol;
        private readonly SymbolOrigin _origin;
        private readonly string _name;

        public SymbolInfo(Symbol symbol, SymbolOrigin origin, string name)
        {
            if (symbol == null)
                throw new System.ArgumentNullException("symbol");
            if (origin == null)
                throw new System.ArgumentNullException("origin");
            if (symbol == null)
                throw new System.ArgumentNullException("symbol");
            _symbol = symbol;
            _origin = origin;
            _name = name;
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public SymbolOrigin Origin
        {
            get { return _origin; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}