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
        private readonly LinkedList<T> _accessOrder = new();

        /// <summary>
        /// Also acts as a synch root.
        /// </summary>
        private readonly Dictionary<T, LinkedListNode<T>> _pointerTable =
            new();

        public LastAccessCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException(Resources.LastAccessCache_CapacityMustBePositive, nameof(capacity));

            Capacity = capacity;
        }

        public int Capacity { get; set; }

        public T GetCached(T name)
        {
            lock (_pointerTable)
            {
                if (_pointerTable.TryGetValue(name, out var node))
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

        protected int Count => _accessOrder.Count;
    }
}