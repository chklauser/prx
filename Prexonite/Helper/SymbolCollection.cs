

using System.Collections;
using System.Diagnostics;

namespace Prexonite;

[DebuggerStepThrough]
public class SymbolCollection(int capacity) : ICollection<string>
{
    readonly HashSet<string> _set = new(capacity, StringComparer.OrdinalIgnoreCase);

    public SymbolCollection() : this(0)
    {
        _set = new(0, StringComparer.OrdinalIgnoreCase);
    }

    public SymbolCollection(IEnumerable<string> items)
        : this()
    {
        foreach (var item in items)
            Add(item);
    }

    #region ICollection<string> Members

    public void Add(string item)
    {
        _set.Add(item);
    }

    public void Clear()
    {
        _set.Clear();
    }

    public bool Contains(string? item)
    {
        return item != null && _set.Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        _set.CopyTo(array, arrayIndex);
    }

    public int Count => _set.Count;

    public bool IsReadOnly => false;

    public bool Remove(string? item)
    {
        if (item == null)
        {
            return false;
        }
        
        return _set.Remove(item);
    }

    #endregion

    #region IEnumerable<string> Members

    public IEnumerator<string> GetEnumerator() => _set.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    #endregion
}