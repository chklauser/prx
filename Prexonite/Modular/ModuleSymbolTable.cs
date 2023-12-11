using System.Collections;

namespace Prexonite.Modular;

public interface IModuleSymbolTable<T> : IDictionary<(ModuleName ModuleName, string Id), T>
{
    T this[ModuleName moduleName, string id] { get; set; }
    void Add(ModuleName moduleName, string id, T value);
    bool Remove(ModuleName moduleName, string id);
    bool TryGetValue(ModuleName moduleName, string id, [MaybeNullWhen(false)] out T value);
}

public class ModuleSymbolTable<T> : IModuleSymbolTable<T>
{
    readonly Dictionary<(ModuleName ModuleName, string Id), T> _inner;

    public ModuleSymbolTable()
    {
        _inner = new(new ModuleSymbolTableEqualityComparer());
    }

    public ModuleSymbolTable(int capacity)
    {
        _inner = new(capacity, new ModuleSymbolTableEqualityComparer());
    }

    class ModuleSymbolTableEqualityComparer : IEqualityComparer<(ModuleName ModuleName, string Id)>
    {
        public bool Equals((ModuleName ModuleName, string Id) x, (ModuleName ModuleName, string Id) y)
        {
            return x.ModuleName == y.ModuleName && Engine.DefaultStringComparer.Compare(x.Id, y.Id) == 0;
        }

        public int GetHashCode((ModuleName, string) obj)
        {
            return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
        }
    }

    public T this[ModuleName moduleName, string id]
    {
        get => _inner[(moduleName, id)];
        set => _inner[(moduleName, id)] = value;
    }

    public void Add(ModuleName moduleName, string id, T value)
    {
        _inner.Add((moduleName, id), value);
    }

    public bool Remove(ModuleName moduleName, string id)
    {
        return _inner.Remove((moduleName, id));
    }

    public bool TryGetValue(ModuleName moduleName, string id, [MaybeNullWhen(false)] out T value)
    {
        return _inner.TryGetValue((moduleName, id), out value);
    }

    # region IDictionary`2 implemetation

    public IEnumerator<KeyValuePair<(ModuleName ModuleName, string Id), T>> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_inner).GetEnumerator();
    }

    public void Add(KeyValuePair<(ModuleName ModuleName, string Id), T> item)
    {
        ((IDictionary<(ModuleName ModuleName, string Id), T>)_inner).Add(item);
    }

    public void Clear()
    {
        _inner.Clear();
    }

    public bool Contains(KeyValuePair<(ModuleName ModuleName, string Id), T> item)
    {
        return ((IDictionary<(ModuleName ModuleName, string Id), T>)_inner).Contains(item);
    }

    public void CopyTo(KeyValuePair<(ModuleName ModuleName, string Id), T>[] array, int arrayIndex)
    {
        ((IDictionary<(ModuleName ModuleName, string Id), T>)_inner).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<(ModuleName ModuleName, string Id), T> item)
    {
        return ((IDictionary<(ModuleName ModuleName, string Id), T>)_inner).Remove(item);
    }

    public int Count => _inner.Count;

    public bool IsReadOnly => ((IDictionary<(ModuleName ModuleName, string Id), T>)_inner).IsReadOnly;

    public void Add((ModuleName ModuleName, string Id) key, T value)
    {
        _inner.Add(key, value);
    }

    public bool ContainsKey((ModuleName ModuleName, string Id) key)
    {
        return _inner.ContainsKey(key);
    }

    public bool Remove((ModuleName ModuleName, string Id) key)
    {
        return _inner.Remove(key);
    }

    public bool TryGetValue((ModuleName ModuleName, string Id) key, [MaybeNullWhen(false)] out T value)
    {
        return _inner.TryGetValue(key, out value);
    }

    public T this[(ModuleName ModuleName, string Id) key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<(ModuleName ModuleName, string Id)> Keys => _inner.Keys;

    public ICollection<T> Values => _inner.Values;

    #endregion
}