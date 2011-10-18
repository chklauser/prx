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
using System.Diagnostics;
using System.IO;

namespace Prexonite
{
    /// <summary>
    ///     Wraps a SymbolicTable&lt;PFunction&gt; to ensure that a function is stored with it's Id as the key.
    /// </summary>
    [DebuggerStepThrough]
    public class PFunctionTable : ICollection<PFunction>
    {
        #region table

        private readonly SymbolTable<PFunction> _table;

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
            foreach (var kvp in _table)
                kvp.Value.Store(writer);
        }

        #endregion

        #region ICollection<PFunction> Members

        public void Add(PFunction item)
        {
            if (_table.ContainsKey(item.Id))
                throw new ArgumentException(
                    "The function table already contains a function named " + item.Id);
            _table.Add(item.Id, item);
        }

        public void AddOverride(PFunction item)
        {
            if (_table.ContainsKey(item.Id))
                _table.Remove(item.Id);
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

            var i = arrayIndex;
            foreach (var kvp in _table)
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

        public bool Remove(string id)
        {
            if (_table.ContainsKey(id))
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
            foreach (var kvp in _table)
                yield return kvp.Value;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var kvp in _table)
                yield return kvp.Value;
        }

        #endregion
    }
}