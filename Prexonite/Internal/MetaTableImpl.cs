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
using System.Collections.Generic;

namespace Prexonite.Internal
{
    ///<summary>
    ///    The Prexonite meta table is used to store information about <see cref = "Application" />s, 
    ///    Functions (<see cref = "PFunction" />) and global variables (<see cref = "PVariable" />).
    ///</summary>
    ///[DebuggerStepThrough]
    internal class MetaTableImpl : MetaTable
    {
        #region Constructors

        /// <summary>
        ///     Creates a new meta table.
        /// </summary>
        /// <param name = "filter">An object that filters requests to the meta table.</param>
        internal MetaTableImpl(IMetaFilter filter) : this(filter, 7)
        {
        }

        /// <summary>
        ///     Creates a new meta table.
        /// </summary>
        /// <param name = "filter">An object that filters request to the meta table.</param>
        /// <param name = "capacity">The initial capacity for the underlying data structure.</param>
        protected MetaTableImpl(IMetaFilter filter, int capacity)
        {
            _filter = filter;
            _table = new SymbolTable<MetaEntry>(capacity);
        }

        #endregion

        #region Filter

        private IMetaFilter _filter;

        /// <summary>
        ///     Returns a reference to the object that filters requests to this meta table. Can be null.
        /// </summary>
        protected override IMetaFilter Filter
        {
            get { return _filter; }
        }

        protected override bool RemoveTransformed(string key)
        {
            return _table.Remove(key);
        }

        public override void Clear()
        {
            _table.Clear();
        }

        public override void CopyTo(KeyValuePair<string, MetaEntry>[] array, int arrayIndex)
        {
            _table.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get { return _table.Count; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override void AddTransformed(string key, MetaEntry item)
        {
            _table.Add(key,item);
        }

        protected override bool ContainsTransformedKey(string key)
        {
            return _table.ContainsKey(key);
        }

        public override ICollection<string> Keys
        {
            get { return _table.Keys; }
        }

        public override ICollection<MetaEntry> Values
        {
            get { return _table.Values; }
        }

        protected override void SetTransformed(string key, MetaEntry entry)
        {
            _table[key] = entry;
        }

        protected override bool TryGetValueTransformed(string key, out MetaEntry entry)
        {
            return _table.TryGetValue(key, out entry);
        }

        #endregion

        #region ICloneable Members

        ///<summary>
        ///    Creates a metatable that is a copy of the current instance.
        ///</summary>
        ///<returns>
        ///    A new metatable that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override MetaTable Clone()
        {
            var clone = new MetaTableImpl(Filter, Count);
            clone.CloneFrom(this);
            return clone;
        }

        public override IEnumerator<KeyValuePair<string, MetaEntry>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public override MetaEntry DefaultValue { get; set; }
        public override void AddRange(IEnumerable<KeyValuePair<string, MetaEntry>> entries)
        {
            _table.AddRange(entries);
        }

        private static SymbolTable<MetaEntry> _createInternalStorage(int capacity)
        {
            return new SymbolTable<MetaEntry>(capacity);
        }

        private SymbolTable<MetaEntry> _table;

        protected virtual void CloneFrom(MetaTableImpl metaTable)
        {
            _table = _createInternalStorage(metaTable.Count);
            _table.AddRange(metaTable._table);
            _filter = metaTable._filter;
        }

        #endregion
    }
}