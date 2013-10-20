using System.Collections;
using System.Collections.Generic;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class Namespace : ISymbolView<Symbol>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator();
        public abstract bool IsEmpty { get; }
        public abstract bool TryGet(string id, out Symbol value);
    }
}