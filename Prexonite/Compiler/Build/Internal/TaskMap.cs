// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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