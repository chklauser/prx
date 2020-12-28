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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prexonite
{
    /// <summary>
    ///     Custom implementation of a queue that allows random access.
    /// </summary>
    /// <typeparam name = "T">The type of the elements the queue is supposed to manage.</typeparam>
    public class RandomAccessQueue<T> : IList<T>
    {
        #region Constructors

        private const int DEFAULT_INITIAL_CAPACITY = 10;

        /// <summary>
        ///     Creates a new RandomAccessQueue
        /// </summary>
        /// <remarks>
        ///     This overload uses a default value for the capacity of it's data store.
        /// </remarks>
        [DebuggerStepThrough]
        public RandomAccessQueue()
        {
            _store = new List<T>();
        }

        /// <summary>
        ///     Creates a new RandomAccessQueue.
        /// </summary>
        /// <param name = "collection">Elements to copy to the queue upon creation.</param>
        [DebuggerStepThrough]
        public RandomAccessQueue(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            _store = new List<T>(collection);
        }

        /// <summary>
        ///     Creates a new RandomAccessQueue
        /// </summary>
        /// <param name = "capacity">The initial capacity of the queue.</param>
        /// <remarks>
        ///     Although the queue increases the size of it's data store as required, setting an 
        ///     initial capacity can reduce the number of resize operations, when filling the queue.
        /// </remarks>
        [DebuggerStepThrough]
        public RandomAccessQueue(int capacity)
        {
            _store = new List<T>(capacity);
        }

        #endregion

        #region Core

        private readonly List<T> _store;
        private int _front;
        private int _rear = -1;

        private void unwrap()
        {
            var nstore = new T[_store.Count];
            var count = normalCount();
            _store.CopyTo(_front, nstore, 0, count);

            //Copy the wrapped part, if necessary
            if (isWrapped())
            {
                var wrapped = wrappedCount();
                _store.CopyTo(0, nstore, count, wrapped);
            }

            //Write queue back to the store
            _store.Clear();
            _store.Capacity = nstore.Length;
            _front = 0;
            _rear = -1;
            foreach (var t in nstore)
            {
                _store.Add(t);
                _rear++;
            }
        }

        private int normalCount()
        {
            if (_rear < 0)
                return 0;
            else if (isWrapped())
                return _store.Count - _front;
            else
                return _rear + 1 - _front;
        }

        private int wrappedCount()
        {
            return isWrapped() ? _rear + 1 - 0 : 0;
        }

        [DebuggerStepThrough]
        private bool isWrapped()
        {
            return _front > _rear;
        }

        private int toIndex(int qidx)
        {
            var idx = _front + qidx;
            if (idx >= _store.Count)
                idx -= _store.Count;
            return idx;
        }

        #endregion

        #region Queue Members

        /// <summary>
        ///     Adds an element to the end of the queue.
        /// </summary>
        /// <param name = "item">The element to be added to the end of the queue.</param>
        public void Enqueue(T item)
        {
            if (_rear == -1)
            {
                if (_store.Count >= 1)
                    _store[0] = item;
                else
                    _store.Add(item);
                _rear++;
            }
            else if (!isWrapped())
                if (_rear + 1 < _store.Count || _store.Count < DEFAULT_INITIAL_CAPACITY)
                {
                    //Stay unwrapped
                    if (++_rear == _store.Count)
                        _store.Add(item);
                    else
                        _store[_rear] = item;
                }
                else //Wrap!
                    if (_front == 0) //no space. resize instead.
                    {
                        _store.Add(item);
                        _rear++;
                    }
                    else
                    {
                        _rear = 0;
                        _store[0] = item;
                    }
            else //wrapped
                if (_rear + 1 == _front)
                {
                    var newRear = Count - 1 + 1;
                    unwrap();
                    _store.Add(item);
                    _rear = newRear;
                }
                else
                {
                    _rear++;
                    _store[_rear] = item;
                }
        }

        /// <summary>
        ///     Returns the element in front of the queue (to be dequeued next).
        /// </summary>
        /// <returns>The element in front of the queue (to be dequeued next).</returns>
        [DebuggerStepThrough]
        public T Peek()
        {
            return _store[_front];
        }

        /// <summary>
        ///     Removes and returns the element in front of the queue.
        /// </summary>
        /// <returns>The element in front of the queue.</returns>
        public T Dequeue()
        {
            var item = _store[_front];
            _store[_front] = default; //Make sure, item get's garbage collected
            if (_front == _rear) //just removed last element -> reset
            {
                _front = 0;
                _rear = -1;
            }
            else if (_front + 1 >= _store.Count)
                _front = 0;
            else
                _front++;
            return item;
        }

        #endregion

        #region IList<T> Members

        /// <summary>
        ///     Returns the index at which <paramref name = "item" /> is located.
        /// </summary>
        /// <param name = "item">The item to search for.</param>
        /// <returns>The index in the queue where the item is stored or -1 if the item cannot be found.</returns>
        [DebuggerStepThrough]
        public int IndexOf(T item)
        {
            var normal = _store.IndexOf(
                item,
                _front,
                normalCount());
            if (normal < 0 && isWrapped())
                return _store.IndexOf(item, 0, wrappedCount());
            else
                return normal;
        }

        /// <summary>
        ///     Inserts an item at a random position in the queue.
        /// </summary>
        /// <param name = "index">Where to insert <paramref name = "item" />.</param>
        /// <param name = "item">What to insert at <paramref name = "index" />.</param>
        [DebuggerStepThrough]
        public void Insert(int index, T item)
        {
            _store.Insert(toIndex(index), item);
        }

        /// <summary>
        ///     Removes the element at a supplied index.
        /// </summary>
        /// <param name = "index">The index of the element to remove.</param>
        [DebuggerStepThrough]
        public void RemoveAt(int index)
        {
            _store.RemoveAt(toIndex(index));
        }

        /// <summary>
        ///     Random access to the queue.
        /// </summary>
        /// <param name = "index">The index of the element to retrieve or set.</param>
        /// <returns>The element at the supplied index.</returns>
        public T this[int index]
        {
            [DebuggerStepThrough]
            get => _store[toIndex(index)];
            [DebuggerStepThrough]
            set => _store[toIndex(index)] = value;
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        ///     Adds an element to the queue. Synonym to <see cref = "Enqueue" />.
        /// </summary>
        /// <param name = "item">The item to add (enqueue).</param>
        [DebuggerStepThrough]
        public void Add(T item)
        {
            Enqueue(item);
        }

        /// <summary>
        ///     Removes all elements from the queue.
        /// </summary>
        [DebuggerStepThrough]
        public void Clear()
        {
            _store.Clear();
            _front = 0;
            _rear = 0;
        }

        /// <summary>
        ///     Indicates whether the queue contains a specific element.
        /// </summary>
        /// <param name = "item">The element to look for.</param>
        /// <returns>True, if the queue contains the element; false otherwise.</returns>
        [DebuggerStepThrough]
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        ///     Copies the contents of the queue to the specified array.
        /// </summary>
        /// <param name = "array">An array, that is big enough to hold all elements in the queue.</param>
        /// <param name = "arrayIndex">The index in the supplied array, that indicates where to start writing.</param>
        /// <exception cref = "ArgumentOutOfRangeException"><paramref name = "array" /> is not big enough.</exception>
        [DebuggerStepThrough]
        public void CopyTo(T[] array, int arrayIndex)
        {
            var idx = arrayIndex;
            if (array.Length < Count)
                throw new ArgumentOutOfRangeException(
                    "The supplied array is not big enough for " + Count + " elements.");

            foreach (var t in this)
                array[idx++] = t;
        }

        /// <summary>
        ///     Retuns the number of elements in the queue
        /// </summary>
        public int Count => normalCount() + wrappedCount();

        /// <summary>
        ///     Queues are never readonly. Always returns false.
        /// </summary>
        public bool IsReadOnly
        {
            [DebuggerStepThrough]
            get => false;
        }

        /// <summary>
        ///     Removes an element from the queue.
        /// </summary>
        /// <param name = "item">The element to remove.</param>
        /// <returns>True if an element has been removed; false otherwise.</returns>
        [DebuggerStepThrough]
        public bool Remove(T item)
        {
            var idx = IndexOf(item);
            if (idx < 0)
                return false;
            else
                _store.Remove(item);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        ///     Returns an IEnumerator that enumerates over all elements in the queue.
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public IEnumerator<T> GetEnumerator()
        {
            var count = normalCount();

            //Return normal part.
            for (var i = _front; i < count; i++)
                yield return _store[i];

            //Check if there exists a wrapped part
            if (isWrapped())
            {
                var wrapped = wrappedCount();

                //Return the wrapped part.
                for (var i = 0; i < wrapped; i++)
                    yield return _store[i];
            }
        }

        #endregion

        #region IEnumerable Members

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}