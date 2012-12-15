using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Properties;

namespace Prexonite
{
    public interface IObjectCache<T>
    {
        T GetCached(T name);
    }

    public class LastAccessCache<T> : IObjectCache<T>
    {
        private readonly LinkedList<T> _accessOrder = new LinkedList<T>();

        /// <summary>
        /// Also acts as a synch root.
        /// </summary>
        private readonly Dictionary<T, LinkedListNode<T>> _pointerTable =
            new Dictionary<T, LinkedListNode<T>>();

        public LastAccessCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException(Resources.LastAccessCache_CapacityMustBePositive, "capacity");

            Capacity = capacity;
        }

        public int Capacity { get; set; }

        public T GetCached(T name)
        {
            lock (_pointerTable)
            {
                LinkedListNode<T> node;
                if (_pointerTable.TryGetValue(name, out node))
                {
                    _accessOrder.Remove(node);
                    _accessOrder.AddFirst(node);
                    return node.Value;
                }

                _insert(name);
            }
            return name;
        }

        public int EstimateSize()
        {
            return _accessOrder.Count;
        }

        private void _insert(T name)
        {
            if (_accessOrder.Count > Capacity*2)
                _truncate();
            var node = _accessOrder.AddFirst(name);
            _pointerTable.Add(name,node);
        }

        private void _truncate()
        {
            Debug.Assert(_accessOrder.Count >= Capacity,
                "Access order linked list of last access cache should be truncated but has less than $Capacity entries.");
            
            var buf = new T[Capacity];
            var i = 0;
            foreach (var n in _accessOrder.Take(Capacity))
                buf[i++] = n;

            _accessOrder.Clear();
            _accessOrder.AddRange(buf);
            _pointerTable.Clear();
            foreach (var node in _accessOrder.ToNodeSequence())
                _pointerTable.Add(node.Value, node);
        }

        protected IEnumerable<T> Contents()
        {
            lock (_pointerTable)
                foreach (var item in _accessOrder.InReverse())
                    yield return item;
        }

        protected int Count { get { return _accessOrder.Count; } }

    }
}