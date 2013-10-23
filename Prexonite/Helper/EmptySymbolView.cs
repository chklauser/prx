using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prexonite
{
    /// <summary>
    /// An empty scope. Will never return elements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EmptySymbolView<T> : ISymbolView<T>
    {
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return Enumerable.Empty<KeyValuePair<string, T>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGet(string id, out T value)
        {
            value = default (T);
            return false;
        }

        public bool IsEmpty
        {
            get { return true; }
        }
    }
}