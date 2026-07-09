

namespace Prexonite;

public interface ISymbolView<T> : IEnumerable<KeyValuePair<string,T>> where T: class
{
    bool TryGet(string id, [NotNullWhen(true)] out T? value);
    bool IsEmpty { get; }
}

public static class SymbolViewExtensions
{
    extension<T>(ISymbolView<T> view) where T: class
    {
        public T GetOrDefault(string key, T defaultValue)
        {
            return view.TryGet(key, out var result) ? result : defaultValue;
        }

        public bool Contains(string key)
        {
            return view.TryGet(key, out var dummy);
        }
    }
}