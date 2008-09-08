/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prexonite
{
    [DebuggerNonUserCode]
    public class SymbolTable<TValue> : IDictionary<string, TValue>
    {
        private readonly Dictionary<string, TValue> table;
        private TValue _defaultValue;

        public TValue DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public SymbolTable()
        {
            table = new Dictionary<string, TValue>(Engine.DefaultStringComparer);
        }

        public SymbolTable(int capacity)
        {
            table = new Dictionary<string, TValue>(capacity, Engine.DefaultStringComparer);
        }

        #region IDictionary<string,TValue> Members

        public virtual void Add(string key, TValue value)
        {
            if (table.ContainsKey(key) && value.Equals(table[key]))
                return;
            table.Add(key, value);
        }

        public virtual bool ContainsKey(string key)
        {
            return table.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return table.Keys; }
        }

        public bool Remove(string key)
        {
            return table.Remove(key);
        }

        public virtual bool TryGetValue(string key, out TValue value)
        {
            value = DefaultValue;
            if (!table.ContainsKey(key))
                return false;
            value = table[key];
            return true;
        }

        public TValue GetDefault(string key, TValue defaultValue)
        {
            if (table.ContainsKey(key))
                return table[key];
            else
                return defaultValue;
        }

        public ICollection<TValue> Values
        {
            get { return table.Values; }
        }

        public virtual TValue this[string key]
        {
            get { return GetDefault(key, _defaultValue); }
            set
            {
                if (table.ContainsKey(key))
                    table.Remove(key);
                table[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,TValue>> Members

        public virtual void Add(KeyValuePair<string, TValue> item)
        {
            table.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            table.Clear();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return ((ICollection<KeyValuePair<string, TValue>>) table).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, TValue>>) table).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return table.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<string, TValue>>) table).IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return ((ICollection<KeyValuePair<string, TValue>>) table).Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,TValue>> Members

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return table.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, TValue>>) table).GetEnumerator();
        }

        #endregion

        protected void CloneFrom(SymbolTable<TValue> source)
        {
            Clear();
            foreach (KeyValuePair<string, TValue> pair in source)
                Add(pair);
            DefaultValue = source.DefaultValue;
        }
    }
}