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
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    [DebuggerStepThrough]
    public class SymbolCollection : ICollection<string>
    {
        private readonly Hashtable _set;

        public SymbolCollection()
        {
            _set = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }

        public SymbolCollection(int capacity)
        {
            _set = new Hashtable(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public SymbolCollection(IEnumerable<string> items)
            : this()
        {
            foreach (string item in items)
                Add(item);
        }

        #region ICollection<string> Members

        public void Add(string item)
        {
            if (!_set.ContainsKey(item))
                _set.Add(item, null);
        }

        public void Clear()
        {
            _set.Clear();
        }

        public bool Contains(string item)
        {
            return _set.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _set.Keys.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _set.Count; }
        }

        public bool IsReadOnly
        {
            get { return _set.IsReadOnly; }
        }

        public bool Remove(string item)
        {
            int cnt = _set.Count;
            _set.Remove(item);
            return cnt != _set.Count;
        }

        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            foreach (DictionaryEntry entry in _set)
                yield return entry.Key as string;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        #endregion
    }
}