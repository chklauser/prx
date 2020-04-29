#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Prexonite
{
    public sealed class ThreadSafeList<T> : IList<T>
    {
        private readonly IList<T> _inner = new List<T>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        
        public IEnumerator<T> GetEnumerator()
        {
            return asWeaklyConsistentEnumerable().GetEnumerator();
        }

        private IEnumerable<T> asWeaklyConsistentEnumerable()
        {
            var enumerator = WithReadLock(inner => inner.GetEnumerator());
            while (true)
            {
                var (hasValue, value) = WithReadLock(_ => 
                    enumerator.MoveNext() ? (true, enumerator.Current) : (false, default(T)!)
                );
                if (!hasValue)
                {
                    yield break;
                }

                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WithWriteLock(Action<IList<T>> action)
        {
            _lock.EnterWriteLock();
            try
            {
                action(_inner);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult WithWriteLock<TResult>(Func<IList<T>, TResult> action)
        {
            _lock.EnterWriteLock();
            try
            {
                return action(_inner);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult WithReadLock<TResult>(Func<IList<T>, TResult> action)
        {
            _lock.EnterReadLock();
            try
            {
                return action(_inner);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WithReadLock(Action<IList<T>> action)
        {
            _lock.EnterReadLock();
            try
            {
                action(_inner);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        
        public void Add(T item)
        {
            WithWriteLock(inner => inner.Add(item));
        }

        public void Clear()
        {
            WithWriteLock(inner => inner.Clear());
        }

        public bool Contains(T item)
        {
            return WithReadLock(inner => inner.Contains(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            WithReadLock(inner => inner.CopyTo(array, arrayIndex));
        }

        public bool Remove(T item)
        {
            return WithWriteLock(inner => inner.Remove(item));
        }

        public int Count => WithReadLock(inner => inner.Count);

        public bool IsReadOnly => _inner.IsReadOnly;

        public int IndexOf(T item)
        {
            return WithReadLock(inner => inner.IndexOf(item));
        }

        public void Insert(int index, T item)
        {
            WithWriteLock(inner => inner.Insert(index, item));
        }

        public void RemoveAt(int index)
        {
            WithWriteLock(inner => inner.RemoveAt(index));
        }

        public T this[int index]
        {
            get => WithReadLock(inner => inner[index]);
            set => WithWriteLock(inner => inner[index] = value);
        }
    }
}