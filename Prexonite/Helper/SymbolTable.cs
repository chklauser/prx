#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Prexonite;

public interface ISymbolTable<TValue> : IDictionary<string, TValue>
{
    TValue? GetDefault(string key, TValue defaultValue);
    void AddRange(IEnumerable<KeyValuePair<string, TValue>> entries);
}

[DebuggerNonUserCode]
public class SymbolTable<TValue> : ISymbolTable<TValue> where TValue : notnull
{
    private readonly Dictionary<string, TValue> _table;

    public SymbolTable()
    {
        _table = new(Engine.DefaultStringComparer);
    }

    public SymbolTable(int capacity)
    {
        _table = new(capacity, Engine.DefaultStringComparer);
    }

    #region IDictionary<string,TValue> Members

    public virtual void Add(string key, TValue value)
    {
        if(_table.TryGetValue(key, out var existingValue) && Equals(existingValue,value))
            return;

        _table.Add(key, value);
    }

    public virtual bool ContainsKey(string key)
    {
        return _table.ContainsKey(key);
    }

    public ICollection<string> Keys => _table.Keys;

    public bool Remove(string key)
    {
        return _table.Remove(key);
    }

    public virtual bool TryGetValue(string key, [NotNullWhen(true)] out TValue? value)
    {
        var cont = _table.TryGetValue(key, out value);
        if (!cont)
            value = default;
        return cont;
    }

    public TValue? GetDefault(string key, TValue? defaultValue)
    {
        if (_table.ContainsKey(key))
            return _table[key];
        else
            return defaultValue;
    }

    public void AddRange(IEnumerable<KeyValuePair<string, TValue>> entries)
    {
        _table.AddRange(entries);
    }

    public ICollection<TValue> Values => _table.Values;

    public virtual TValue this[string key]
    {
        get => GetDefault(key, default) 
            ?? throw new PrexoniteException($"Lookup of {key} in symbol table failed.");
        set
        {
            if (!_table.ContainsKey(key))
                _table.Add(key, value);
            else
                _table[key] = value;
        }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> Members

    public virtual void Add(KeyValuePair<string, TValue> item)
    {
        _table.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _table.Clear();
    }

    public bool Contains(KeyValuePair<string, TValue> item)
    {
        return ((ICollection<KeyValuePair<string, TValue>>) _table).Contains(item);
    }

    public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, TValue>>) _table).CopyTo(array, arrayIndex);
    }

    public int Count => _table.Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, TValue>>) _table).IsReadOnly;

    public bool Remove(KeyValuePair<string, TValue> item)
    {
        return ((ICollection<KeyValuePair<string, TValue>>) _table).Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> Members

    public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
    {
        return _table.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((ICollection<KeyValuePair<string, TValue>>) _table).GetEnumerator();
    }

    #endregion
}