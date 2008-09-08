using System;
using System.Collections.Generic;
using System.IO;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// The Prexonite meta table is used to store information about <see cref="Application"/>s, 
    /// Functions (<see cref="PFunction"/>) and global variables (<see cref="PVariable"/>).
    /// </summary>
    [NoDebug]
    public class MetaTable : SymbolTable<MetaEntry>,
                             IMetaFilter,
                             ICloneable
    {
        #region Constructors

        /// <summary>
        /// Creates a new meta table.
        /// </summary>
        public MetaTable()
        {
        }

        /// <summary>
        /// Creates a new meta table.
        /// </summary>
        /// <param name="filter">An object that filters requests to the meta table.</param>
        public MetaTable(IMetaFilter filter)
        {
            _filter = filter;
        }

        /// <summary>
        /// Creates a new meta table.
        /// </summary>
        /// <param name="filter">An object that filters request to the meta table.</param>
        /// <param name="capacity">The initial capacity for the underlying data structure.</param>
        protected MetaTable(IMetaFilter filter, int capacity)
            : base(capacity)
        {
            _filter = filter;
        }

        #endregion

        #region Filter

        private IMetaFilter _filter;

        /// <summary>
        /// Returns a reference ti the object that filters requests to this mtea table.
        /// </summary>
        public virtual IMetaFilter Filter
        {
            get { return _filter; }
            protected set
            {
                if (_filter == this)
                    throw new ArgumentException(
                        "You cannot use a Metatable as its own _filter. (Recursion!)");
                _filter = value;
            }
        }

        #endregion

        #region Table

        /// <summary>
        /// Adds a new entry to the meta table.
        /// </summary>
        /// <param name="key">The key under which the entry is stored.</param>
        /// <param name="value">The value to be stored in the meta table.</param>
        public override void Add(string key, MetaEntry value)
        {
            Add(new KeyValuePair<string, MetaEntry>(key, value));
        }

        /// <summary>
        /// Adds a new entry to the meta table.
        /// </summary>
        /// <param name="item">The item to store in the meta table.</param>
        public override void Add(KeyValuePair<string, MetaEntry> item)
        {
            var nentry = Filter.SetTransform(item);
            if (nentry.HasValue)
                base.Add(nentry.Value);
        }

        /// <summary>
        /// Adds the meta <paramref name="entry"/> to the list stored under the supplied <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the list to which to add the entry.</param>
        /// <param name="entry">The entry to add to the list.</param>
        /// <remarks>
        ///     <para>If the entry doesn't already exist, it is created as 
        /// a list with the supplied entry as its only element, </para>
        ///     <para>The entry is converted to a list if necessary.</para>
        /// </remarks>
        public void AddTo(string key, MetaEntry entry)
        {
            if (ContainsKey(key))
                this[key] = this[key].AddToList(entry);
            else
                Add(key, (MetaEntry) new[]
                                     {
                                         entry
                                     });
        }

        /// <summary>
        /// Checks if an entry for a given <paramref name="key"/> exists in the meta table.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>True if the the meta table contains an entry for the given key. False otherwise.</returns>
        public override bool ContainsKey(string key)
        {
            return base.ContainsKey(Filter.GetTransform(key));
        }

        /// <summary>
        /// Provides key based access to the entries in the meta table.
        /// </summary>
        /// <param name="key">The key of an entry in the table.</param>
        /// <returns>The entry stored under the supplied key or a default element if no entry is found.</returns>
        /// <remarks><para>Using this property it is impossible to tell whether a default entry has 
        /// explicitly been added to the meta table.</para>
        /// <para>Use the <see cref="SymbolTable{TValue}.TryGetValue"/> method for this purpose.</para></remarks>
        public override MetaEntry this[string key]
        {
            get
            {
                key = Filter.GetTransform(key);
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
                var item = Transform(key, value);
                if (item == null)
                    return;
                else
                    base[item.Value.Key] = item.Value.Value;
            }
        }

        /// <summary>
        /// Checks if the meta table contains an entry for a given key and returns it.
        /// </summary>
        /// <param name="key">The key of the entry to look up.</param>
        /// <param name="value">Variable to store the meta entry in.</param>
        /// <returns>True if the entry can be found in the table. False otherwise.</returns>
        /// <remarks><paramref name="value"/> contains the default element if the table does 
        /// not contain an entry for <paramref name="key"/>.</remarks>
        public override bool TryGetValue(string key, out MetaEntry value)
        {
            key = Filter.GetTransform(key);
            if(base.ContainsKey(key))
            {
                value = base[key];
                return true;
            }
            else
            {
                value = MetaEntry.CreateDefaultEntry();
                return false;
            }
        }

        /// <summary>
        /// Sets a key directly, bypassing the filter.
        /// </summary>
        /// <param name="item">The item to store in the meta table.</param>
        internal void _SetDirect(KeyValuePair<string, MetaEntry> item)
        {
            if (ContainsKey(item.Key))
                Remove(item.Key);
            if (item.Value != null)
                base[item.Key] = item.Value;
        }

        /// <summary>
        /// Sets a key directly, bypassing the filter.
        /// </summary>
        /// <param name="entry">The entry to store in the meta table.</param>
        /// <param name="key">The key of the meta table entry.</param>
        internal void _SetDirect(string key, MetaEntry entry)
        {
            _SetDirect(new KeyValuePair<string, MetaEntry>(key, entry));
        }

        #endregion

        #region Storage

        /// <summary>
        /// Writes a machine- and human-readable representation to the supplied <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to store the representation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public void Store(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            foreach (var kvp in this)
            {
                var entry = kvp.Value;
                if (entry.EntryType == MetaEntry.Type.Switch)
                {
                    if (kvp.Key.EndsWith("ation"))
                        writer.Write("{0} {1};", kvp.Key, entry.Switch ? "enabled" : "disabled");
                    else
                        writer.Write("is {1} {0};", kvp.Key, entry.Switch ? "" : "not");
                }
                else
                {
                    writer.Write("{0} {1};", kvp.Key, entry);
                }
#if DEBUG || Verbose
                writer.WriteLine();
#endif
            }
        }

        #endregion

        #region IMetaFilter Members

        /// <summary>
        /// Applies the set transformation of the associated filter to the supplied meta entry.
        /// </summary>
        /// <param name="key">The key of the meta entry to transform.</param>
        /// <param name="value">The value of the meta entry to transform.</param>
        /// <returns></returns>
        public KeyValuePair<string, MetaEntry>? Transform(string key, MetaEntry value)
        {
            return SetTransform(new KeyValuePair<string, MetaEntry>(key, value));
        }


        public KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            if (Filter == null)
                return item;
            else
                return Filter.SetTransform(item);
        }

        public string GetTransform(string key)
        {
            if (Filter == null)
                return key;
            else
                return Filter.GetTransform(key);
        }

        #endregion

        #region ICloneable Members

        ///<summary>
        ///Creates a metatable that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new metatable that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public MetaTable Clone()
        {
            var clone = new MetaTable(Filter, Count);
            clone.CloneFrom(this);
            return clone;
        }

        ///<summary>
        ///Creates a metatable that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new metatable that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }

    /// <summary>
    /// Defines transformations of get and set requests to the associated meta table.
    /// </summary>
    public interface IMetaFilter
    {
        string GetTransform(string key);
        KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item);
    }

    /// <summary>
    /// An object that is associated with meta information.
    /// </summary>
    public interface IHasMetaTable
    {
        /// <summary>
        /// Returns a reference to the meta table associated with the object.
        /// </summary>
        MetaTable Meta { get; }
    }
}