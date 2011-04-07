using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Helper
{
    /// <summary>
    /// Provides a read-only view onto a symbol table. Does not protect the contents of the symbol table.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the symbol table.</typeparam>
    public class ReadOnlyDictionaryView<TKey, TValue> : IDictionary<TKey,TValue>
    {
        private readonly IDictionary<TKey,TValue> _table;

        public ReadOnlyDictionaryView(IDictionary<TKey, TValue> table)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            _table = table;
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
            return new NotSupportedException("Cannot modify read-only symbol view.");
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
            get { return _table[key]; }
            set { throw _notSupportedException(); }
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

        public int Count
        {
            get { return _table.Count; }
        }

        public bool IsReadOnly
        {
            get { return _table.IsReadOnly; }
        }

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
            get { return _table[key]; }
            set { _table[key] = value; }
        }

        public ICollection<TKey> Keys
        {
            get { return _table.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _table.Values; }
        }

        #endregion
    }
}