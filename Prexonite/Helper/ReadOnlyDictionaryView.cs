// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;

namespace Prexonite
{
    /// <summary>
    ///     Provides a read-only view onto a symbol table. Does not protect the contents of the symbol table.
    /// </summary>
    /// <typeparam name = "TValue">The type of values stored in the table.</typeparam>
    /// <typeparam name="TKey">The type of keys used to access values in the table. </typeparam>
    public abstract class ReadOnlyDictionaryView<TKey, TValue> : IDictionary<TKey, TValue>
    {
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
            return new NotSupportedException("Cannot modify read-only symbol view.");
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw _notSupportedException();
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

        TValue IDictionary<TKey, TValue>.this[TKey key] {
            get { return Get(key); }
// ReSharper disable ValueParameterNotUsed
            set { _notSupportedException(); }
// ReSharper restore ValueParameterNotUsed
        }

        [PublicAPI]
        TValue this[TKey key]
        {
            get { return Get(key); }
        }

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        #endregion

        #region Delegated

        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        public abstract bool Contains(KeyValuePair<TKey, TValue> item);

        public abstract void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex);

        public abstract int Count { get; }

        public abstract bool ContainsKey(TKey key);

        public abstract bool TryGetValue(TKey key, out TValue value);

        protected abstract TValue Get(TKey key);

        public abstract ICollection<TKey> Keys { get; }

        public abstract ICollection<TValue> Values { get; }

        #endregion
    }

    internal class SymbolStoreView : ReadOnlyDictionaryView<String, Symbol>
    {
        private readonly SymbolStore _store;

        public SymbolStoreView(SymbolStore store)
        {
            _store = store;
        }

        public override int Count
        {
            get { return _store.Count; }
        }

        public override ICollection<String> Keys
        {
            get { return _store.Select(x => x.Key).ToArray(); }
        }

        public override ICollection<Symbol> Values
        {
            get { return _store.Select(x => x.Value).ToArray(); }
        }

        public override IEnumerator<KeyValuePair<String, Symbol>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        public override bool Contains(KeyValuePair<String, Symbol> item)
        {
            return _store.Contains(item);
        }

        public override void CopyTo(KeyValuePair<String, Symbol>[] array, int arrayIndex)
        {
            _store.ToArray().CopyTo(array, arrayIndex);
        }

        public override bool ContainsKey(String key)
        {
            return _store.Contains(key);
        }

        public override bool TryGetValue(String key, out Symbol value)
        {
            return _store.TryGet(key, out value);
        }

        #region Overrides of ReadOnlyDictionaryView<string,Symbol>

        protected override Symbol Get(string key)
        {
            Symbol symbol;
            if(_store.TryGet(key, out symbol))
            {
                return symbol;
            }
            else
            {
                return SymbolStore._CreateSymbolNotFoundError(key, new SourcePosition("", -1, -1));
            }
        }

        #endregion
    }
}