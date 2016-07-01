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
using System.IO;
using Prexonite.Internal;
using Prexonite.Types;

namespace Prexonite
{
    public abstract class MetaTable : ISymbolTable<MetaEntry>, IMetaFilter, ICloneable
    {
        public static MetaTable Create(IMetaFilter filter)
        {
            return new MetaTableImpl(filter);
        }

        public static MetaTable Create()
        {
            return Create(null);
        }

        /// <summary>
        ///     Returns a reference to the object that filters requests to this meta table. Can be null.
        /// </summary>
        protected abstract IMetaFilter Filter { get; }

        /// <summary>
        ///     Adds a new entry to the meta table.
        /// </summary>
        /// <param name = "key">The key under which the entry is stored.</param>
        /// <param name = "value">The value to be stored in the meta table.</param>
        public void Add(string key, MetaEntry value)
        {
            Add(new KeyValuePair<string, MetaEntry>(key, value));
        }

        public bool Remove(string key)
        {
            return RemoveTransformed(GetTransform(key));
        }

        protected abstract bool RemoveTransformed(string key);

        /// <summary>
        ///     Adds a new entry to the meta table.
        /// </summary>
        /// <param name = "item">The item to store in the meta table.</param>
        public void Add(KeyValuePair<string, MetaEntry> item)
        {
            var nentry = SetTransform(item);
            if (nentry.HasValue)
                AddTransformed(nentry.Value.Key, nentry.Value.Value);
        }

        public abstract void Clear();

        public virtual bool Contains(KeyValuePair<string, MetaEntry> item)
        {
            var key = GetTransform(item.Key);
            MetaEntry currentEntry;
            return TryGetValueTransformed(key, out currentEntry) && Equals(currentEntry, item.Value);
        }

        public abstract void CopyTo(KeyValuePair<string, MetaEntry>[] array, int arrayIndex);

        public virtual bool Remove(KeyValuePair<string, MetaEntry> item)
        {
            var key = GetTransform(item.Key);
            MetaEntry currentEntry;
            if (TryGetValueTransformed(key, out currentEntry) && Equals(currentEntry, item.Value))
            {
                Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public abstract int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public abstract bool IsReadOnly { get; }

        /// <summary>
        ///     Adds an already transformed entry to the table's internal storage.
        /// </summary>
        /// <param name="key">The key of the meta entry.</param>
        /// <param name="item">The meta entry to store.</param>
        protected abstract void AddTransformed(string key, MetaEntry item);

        /// <summary>
        ///     Adds the meta <paramref name = "entry" /> to the list stored under the supplied <paramref name = "key" />.
        /// </summary>
        /// <param name = "key">The key of the list to which to add the entry.</param>
        /// <param name = "entry">The entry to add to the list.</param>
        /// <remarks>
        ///     <para>If the entry doesn't already exist, it is created as 
        ///         a list with the supplied entry as its only element, </para>
        ///     <para>The entry is converted to a list if necessary.</para>
        /// </remarks>
        public void AddTo(string key, MetaEntry entry)
        {
            if (ContainsKey(key))
                this[key] = this[key].AddToList(entry);
            else
                Add(
                    key, (MetaEntry) new[]
                        {
                            entry
                        });
        }

        /// <summary>
        ///     Checks if an entry for a given <paramref name = "key" /> exists in the meta table.
        /// </summary>
        /// <param name = "key">The key to search for.</param>
        /// <returns>True if the the meta table contains an entry for the given key. False otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return ContainsTransformedKey(GetTransform(key));
        }

        protected abstract bool ContainsTransformedKey(string key);

        /// <summary>
        ///     Provides key based access to the entries in the meta table.
        /// </summary>
        /// <param name = "key">The key of an entry in the table.</param>
        /// <returns>The entry stored under the supplied key or a default element if no entry is found.</returns>
        /// <remarks>
        ///     <para>Using this property it is impossible to tell whether a default entry has 
        ///         explicitly been added to the meta table.</para>
        ///     <para>Use the <see cref = "SymbolTable{TValue}.TryGetValue" /> method for this purpose.</para>
        /// </remarks>
        public MetaEntry this[string key]
        {
            get
            {
                key = GetTransform(key);
                MetaEntry ret;
                if (key == null)
                    ret = new MetaEntry("");
                else
                    ret = GetDefault(key, MetaEntry.CreateDefaultEntry());

                //if (!base.ContainsKey(key))
                //    base.Add(key, ret);

                return ret;
            }
            set
            {
                var item = SetTransform(key, value);
                if (item == null)
                    return;
                SetTransformed(item.Value.Key, item.Value.Value);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public abstract ICollection<string> Keys { get; }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public abstract ICollection<MetaEntry> Values { get; }

        /// <summary>
        ///     Sets an already transformed meta entry.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="entry">The meta entry to set.</param>
        protected abstract void SetTransformed(string key, MetaEntry entry);

        /// <summary>
        ///     Checks if the meta table contains an entry for a given key and returns it.
        /// </summary>
        /// <param name = "key">The key of the entry to look up.</param>
        /// <param name = "value">Variable to store the meta entry in.</param>
        /// <returns>True if the entry can be found in the table. False otherwise.</returns>
        /// <remarks>
        ///     <paramref name = "value" /> contains the default element if the table does 
        ///     not contain an entry for <paramref name = "key" />.
        /// </remarks>
        public bool TryGetValue(string key, out MetaEntry value)
        {
            key = GetTransform(key);
            if (TryGetValueTransformed(key, out value))
                return true;
            value = DefaultValue;
            return false;
        }

        protected abstract bool TryGetValueTransformed(string key, out MetaEntry entry);

        /// <summary>
        ///     Writes a machine- and human-readable representation to the supplied <see cref = "TextWriter" />.
        /// </summary>
        /// <param name = "writer">The <see cref = "TextWriter" /> used to store the representation.</param>
        /// <exception cref = "ArgumentNullException"><paramref name = "writer" /> is null.</exception>
        public virtual void Store(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            foreach (var kvp in this)
            {
                var entry = kvp.Value;
                if (entry.EntryType == MetaEntry.Type.Switch)
                {
                    if (kvp.Key.EndsWith("ation"))
                    {
                        writer.Write("{0} {1};", StringPType.ToIdLiteral(kvp.Key),
                            entry.Switch ? "enabled" : "disabled");
                    }
                    else
                    {
                        if (!entry.Switch)
                        {
                            writer.Write("is not ");
                        }
                        writer.Write(StringPType.ToIdLiteral(kvp.Key));
                        writer.Write(";");
                    }
                }
                else
                {
                    writer.Write("{0} {1};", StringPType.ToIdLiteral(kvp.Key), entry);
                }
#if DEBUG || Verbose
                writer.WriteLine();
#endif
            }
        }

        /// <summary>
        ///     Applies the set transformation of the associated filter to the supplied meta entry.
        /// </summary>
        /// <param name = "key">The key of the meta entry to transform.</param>
        /// <param name = "value">The value of the meta entry to transform.</param>
        /// <returns></returns>
        public KeyValuePair<string, MetaEntry>? SetTransform(string key, MetaEntry value)
        {
            return SetTransform(new KeyValuePair<string, MetaEntry>(key, value));
        }

        /// <summary>
        ///     Applies the set transformation of the associated filter to the supplied meta entry.
        /// </summary>
        /// <param name = "item">The new entry to transform.</param>
        /// <returns>A transformed entry or null if the filter completly blocks the entry.</returns>
        public virtual KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            if (Filter == null)
                return item;
            return Filter.SetTransform(item);
        }

        ///<summary>
        ///    Applies the "get" transformation defined by the current <see cref = "MetaTable.Filter" />, if it is not null.
        ///</summary>
        ///<param name = "key">The key to transform by the filter.</param>
        ///<returns>The transformed key.</returns>
        public virtual string GetTransform(string key)
        {
            if (Filter == null)
                return key;
            return Filter.GetTransform(key);
        }

        ///<summary>
        ///    Creates a metatable that is a copy of the current instance.
        ///</summary>
        ///<returns>
        ///    A new metatable that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public abstract MetaTable Clone();

        ///<summary>
        ///    Creates a metatable that is a copy of the current instance.
        ///</summary>
        ///<returns>
        ///    A new metatable that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract IEnumerator<KeyValuePair<string, MetaEntry>> GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract MetaEntry DefaultValue { get; set; }

        public MetaEntry GetDefault(string key, MetaEntry defaultValue)
        {
            key = GetTransform(key);
            MetaEntry entry;
            if (TryGetValueTransformed(key, out entry))
                return entry;
            else
                return defaultValue;
        }

        public abstract void AddRange(IEnumerable<KeyValuePair<string, MetaEntry>> entries);
    }
}