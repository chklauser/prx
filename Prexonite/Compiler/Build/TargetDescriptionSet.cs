

using System.Collections;
using System.Collections.Concurrent;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public sealed class TargetDescriptionSet 
    : ICollection<ITargetDescription>
{
    readonly ConcurrentDictionary<ModuleName, ITargetDescription> _table = new();

    TargetDescriptionSet()
    {
    }

    public static TargetDescriptionSet Create()
    {
        return new();
    }

    public bool TryGetValue(ModuleName name, out ITargetDescription? description)
    {
        if ((object) name == null)
            throw new ArgumentNullException(nameof(name));
        return _table.TryGetValue(name, out description);
    }

    public void Replace(ITargetDescription oldDescription, ITargetDescription newDescription)
    {
        if (oldDescription == null)
            throw new ArgumentNullException(nameof(oldDescription));
        if (newDescription == null)
            throw new ArgumentNullException(nameof(newDescription));
        if(oldDescription.Name != newDescription.Name)
            throw new ArgumentException(
                $"Cannot replace description for {oldDescription.Name} with a description for a different module ({newDescription.Name}).");
        if (!_table.TryUpdate(oldDescription.Name, newDescription, oldDescription))
        {
            throw new PrexoniteException(
                "Failed to update target description set. Probably due to concurrent modification.");
        }
    }

    public ITargetDescription GetOrAdd(ModuleName name,
        Func<ModuleName, ITargetDescription> factory)
    {
        return _table.GetOrAdd(name, factory);
    }

    public ITargetDescription this[ModuleName name] =>
        _table.TryGetValue(name, out var description)
            ? description
            : throw new KeyNotFoundException($"Cannot find target description for module {name}.");

    public bool Contains(ModuleName name)
    {
        return _table.ContainsKey(name);
    }

    #region Implementation of IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Implementation of IEnumerable<out ITargetDescription>

    public IEnumerator<ITargetDescription> GetEnumerator()
    {
        return _table.Values.GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<ITargetDescription>

    public void Add(ITargetDescription? item)
    {
        if(item == null)
            throw new ArgumentNullException(nameof(item));
        if(!_table.TryAdd(item.Name,item))
            throw new ArgumentException($"A target description for this module name already exists: {item.Name}");
    }

    public void Clear()
    {
        _table.Clear();
    }

    public bool Contains(ITargetDescription? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        return _table.TryGetValue(item.Name, out var value) && item.Equals(value);
    }

    public void CopyTo(ITargetDescription[] array, int arrayIndex)
    {
        _table.Values.CopyTo(array, arrayIndex);
    }

    public bool Remove(ITargetDescription? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        return _table.TryRemove(item.Name, out _);
    }

    public int Count => _table.Count;

    public bool IsReadOnly => false;

    #endregion
}