using System.Collections;

namespace Prexonite;

/// <summary>
/// An empty scope. Will never return elements.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EmptySymbolView<T> : ISymbolView<T> where T: class
{
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        return Enumerable.Empty<KeyValuePair<string, T>>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryGet(string id, [NotNullWhen(true)] out T? value)
    {
        value = default;
        return false;
    }

    public bool IsEmpty => true;
}