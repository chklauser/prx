namespace Prexonite.Internal;

/// <summary>
///     Wraps a SymbolicTable&lt;PFunction&gt; to ensure that a function is stored with it's Id as the key.
/// </summary>
public class PFunctionTableImpl : PFunctionTable
{
    #region table

    readonly SymbolTable<PFunction> _table = new();

    public override bool Contains(string id)
    {
        return _table.ContainsKey(id);
    }

    public override bool TryGetValue(string id, [NotNullWhen(true)] out PFunction? func)
    {
        return _table.TryGetValue(id, out func);
    }

    public override PFunction? this[string id] => _table.GetDefault(id, null);

    #endregion

    #region Storage

    public override void Store(TextWriter writer)
    {
        foreach (var kvp in _table)
            kvp.Value.Store(writer);
    }

    #endregion

    #region ICollection<PFunction> Members

    public override void Add(PFunction? item)
    {
        if (item == null)
        {
            return;
        }
        if (!_table.TryAdd(item.Id, item))
            throw new ArgumentException(
                "The function table already contains a function named " + item.Id
            );
    }

    public override void AddOverride(PFunction item)
    {
        if (_table.TryGetValue(item.Id, out var oldFunc))
        {
            _table.Remove(oldFunc.Id);
        }

        _table.Add(item.Id, item);
    }

    public override void Clear()
    {
        _table.Clear();
    }

    public override bool Contains(PFunction? item)
    {
        if (item == null)
        {
            return false;
        }

        return _table.ContainsKey(item.Id);
    }

    public override void CopyTo(PFunction[] array, int arrayIndex)
    {
        if (_table.Count + arrayIndex > array.Length)
            throw new ArgumentException("Array to copy functions into is not long enough.");

        var i = arrayIndex;
        foreach (var kvp in _table)
        {
            if (i >= array.Length)
                break;
            array[i++] = kvp.Value;
        }
    }

    public override int Count => _table.Count;

    public override bool IsReadOnly => _table.IsReadOnly;

    public override bool Remove(PFunction? item)
    {
        if (item == null)
        {
            return false;
        }
        else if (_table.TryGetValue(item.Id, out var f) && ReferenceEquals(f, item))
        {
            _table.Remove(item.Id);
            return true;
        }
        else
            return false;
    }

    public override bool Remove(string id)
    {
        if (_table.TryGetValue(id, out var oldFunc))
        {
            return _table.Remove(id);
        }
        else
            return false;
    }

    #endregion

    #region IEnumerable<PFunction> Members

    public override IEnumerator<PFunction> GetEnumerator()
    {
        foreach (var kvp in _table)
            yield return kvp.Value;
    }

    #endregion
}
