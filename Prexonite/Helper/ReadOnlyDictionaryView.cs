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

namespace Prexonite
{
    /// <summary>
    ///     Provides a read-only view onto a symbol table. Does not protect the contents of the symbol table.
    /// </summary>
    /// <typeparam name = "TValue">The type of values stored in the table.</typeparam>
    /// <typeparam name="TKey">The type of keys used to access values in the table. </typeparam>
    public class ReadOnlyDictionaryView<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _table;

        public ReadOnlyDictionaryView(IDictionary<TKey, TValue> table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        #region Throwing exceptions

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw _notSupportedException();
        }

        private static NotSupportedException _notSupportedException()
        {
            return new("Cannot modify read-only symbol view.");
        }

        public void Clear()
        {
            throw _notSupportedException();
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw _notSupportedException();
        }

        public bool Remove(string key)
        {
            throw _notSupportedException();
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => _table[key];
            set => throw _notSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw _notSupportedException();
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw _notSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw _notSupportedException();
        }

        #endregion

        #region Delegated from table

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _table.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _table.CopyTo(array, arrayIndex);
        }

        public int Count => _table.Count;

        public bool IsReadOnly => _table.IsReadOnly;

        public bool ContainsKey(TKey key)
        {
            return _table.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _table.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _table[key];
            set => _table[key] = value;
        }

        public ICollection<TKey> Keys => _table.Keys;

        public ICollection<TValue> Values => _table.Values;

        #endregion
    }
}