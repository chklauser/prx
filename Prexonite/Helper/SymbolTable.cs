// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prexonite
{
    public interface ISymbolTable<TValue> : IDictionary<string, TValue>
    {
        TValue DefaultValue { get; set; }
        TValue GetDefault(string key, TValue defaultValue);
        void AddRange(IEnumerable<KeyValuePair<string, TValue>> entries);
    }

    [DebuggerNonUserCode]
    public class SymbolTable<TValue> : ISymbolTable<TValue>
    {
        private readonly Dictionary<string, TValue> _table;
        private TValue _defaultValue;

        public TValue DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public SymbolTable()
        {
            _table = new Dictionary<string, TValue>(Engine.DefaultStringComparer);
        }

        public SymbolTable(int capacity)
        {
            _table = new Dictionary<string, TValue>(capacity, Engine.DefaultStringComparer);
        }

        #region IDictionary<string,TValue> Members

        public virtual void Add(string key, TValue value)
        {
            TValue existingValue;
            if(_table.TryGetValue(key, out existingValue) && Equals(existingValue,value))
                return;

            _table.Add(key, value);
        }

        public virtual bool ContainsKey(string key)
        {
            return _table.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _table.Keys; }
        }

        public bool Remove(string key)
        {
            return _table.Remove(key);
        }

        public virtual bool TryGetValue(string key, out TValue value)
        {
            var cont = _table.TryGetValue(key, out value);
            if (!cont)
                value = DefaultValue;
            return cont;
        }

        public TValue GetDefault(string key, TValue defaultValue)
        {
            if (_table.ContainsKey(key))
                return _table[key];
            else
                return defaultValue;
        }

        public void AddRange(IEnumerable<KeyValuePair<string, TValue>> entries)
        {
            _table.AddRange(entries);
        }

        public ICollection<TValue> Values
        {
            get { return _table.Values; }
        }

        public virtual TValue this[string key]
        {
            get { return GetDefault(key, _defaultValue); }
            set
            {
                if (!_table.ContainsKey(key))
                    _table.Add(key, value);
                else
                    _table[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,TValue>> Members

        public virtual void Add(KeyValuePair<string, TValue> item)
        {
            _table.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return ((ICollection<KeyValuePair<string, TValue>>) _table).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, TValue>>) _table).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _table.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, TValue>>) _table).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return ((ICollection<KeyValuePair<string, TValue>>) _table).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,TValue>> Members

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, TValue>>) _table).GetEnumerator();
        }

        #endregion

        protected void CloneFrom(SymbolTable<TValue> source)
        {
            Clear();
            foreach (var pair in source)
                Add(pair);
            DefaultValue = source.DefaultValue;
        }
    }
}