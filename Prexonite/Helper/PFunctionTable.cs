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
using System.IO;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// Wraps a SymbolicTable&lt;PFunction&gt; to ensure that a function is stored with it's Id as the key.
    /// </summary>
    [NoDebug]
    public class PFunctionTable : ICollection<PFunction>,
                                  IEnumerable<PFunction>
    {
        #region table

        private SymbolTable<PFunction> _table;

        public PFunctionTable()
        {
            _table = new SymbolTable<PFunction>();
        }

        public PFunctionTable(int capacity)
        {
            _table = new SymbolTable<PFunction>(capacity);
        }

        public bool Contains(string id)
        {
            return _table.ContainsKey(id);
        }

        public bool TryGetValue(string id, out PFunction func)
        {
            return _table.TryGetValue(id, out func);
        }

        public PFunction this[string id]
        {
            get { return _table.GetDefault(id, null); }
            set { _table[id] = value; }
        }

        #endregion

        #region Storage

        public void Store(TextWriter writer)
        {
            foreach (KeyValuePair<string, PFunction> kvp in _table)
                kvp.Value.Store(writer);
        }

        #endregion

        #region ICollection<PFunction> Members

        public void Add(PFunction item)
        {
            if (_table.ContainsKey(item.Id))
                throw new ArgumentException("The function table already contains a function named " + item.Id);
            _table.Add(item.Id, item);
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Contains(PFunction item)
        {
            return _table.ContainsKey(item.Id);
        }

        public void CopyTo(PFunction[] array, int arrayIndex)
        {
            if (_table.Count + arrayIndex > array.Length)
                throw new ArgumentException("Array to copy functions into is not long enough.");

            int i = arrayIndex;
            foreach (KeyValuePair<string, PFunction> kvp in _table)
            {
                if (i >= array.Length)
                    break;
                array[i++] = kvp.Value;
            }
        }

        public int Count
        {
            get { return _table.Count; }
        }

        public bool IsReadOnly
        {
            get { return _table.IsReadOnly; }
        }

        public bool Remove(PFunction item)
        {
            if (_table.ContainsKey(item.Id))
            {
                _table.Remove(item.Id);
                return true;
            }
            else
                return false;
        }

        public bool  Remove(string id)
        {
            if(_table.ContainsKey(id))
            {
                return _table.Remove(id);
            }
            else
                return false;
        }

        #endregion

        #region IEnumerable<PFunction> Members

        public IEnumerator<PFunction> GetEnumerator()
        {
            foreach (KeyValuePair<string, PFunction> kvp in _table)
                yield return kvp.Value;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<string, PFunction> kvp in _table)
                yield return kvp.Value;
        }

        #endregion
    }
}