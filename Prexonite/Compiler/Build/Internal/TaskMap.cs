

using System.Collections.Concurrent;

namespace Prexonite.Compiler.Build.Internal;

public class TaskMap<TKey,TValue> : ConcurrentDictionary<TKey,Lazy<Task<TValue>>>
    where TKey : notnull
{
    public TaskMap()
    {
    }

    public TaskMap(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
    {
    }

    public TaskMap(IEnumerable<KeyValuePair<TKey, Lazy<Task<TValue>>>> collection) : base(collection)
    {
    }

    public TaskMap(IEqualityComparer<TKey> comparer) : base(comparer)
    {
    }

    public TaskMap(IEnumerable<KeyValuePair<TKey, Lazy<Task<TValue>>>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer)
    {
    }

    public TaskMap(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, Lazy<Task<TValue>>>> collection, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, collection, comparer)
    {
    }

    public TaskMap(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : base(concurrencyLevel, capacity, comparer)
    {
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out Task<TValue>? result)
    {
        if(TryGetValue(key, out Lazy<Task<TValue>>? lazyTask))
        {
            result = lazyTask.Value;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public Task<TValue> Get(TKey key)
    {
        return this[key].Value;
    }

    public Task<TValue> GetOrAdd(TKey key, Func<TKey,Task<TValue>> taskFactory)
    {
        var someThunk = GetOrAdd(key, 
            actualKey => new(() => taskFactory(actualKey))
        );

        // not necessarily our thunk, but ensures that we never invoke a taskFactory more than once
        return someThunk.Value; 
    }
}