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
using System.Collections.Generic;
using System.IO;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    [NoDebug()]
    public class MetaTable : SymbolTable<MetaEntry>,
                             IMetaFilter,
                             ICloneable
    {
        #region Constructors

        public MetaTable()
        {
        }

        public MetaTable(IMetaFilter filter)
        {
            Filter = filter;
        }

        public MetaTable(int capacity)
            : base(capacity)
        {
        }

        public MetaTable(IMetaFilter filter, int capacity)
            : base(capacity)
        {
            Filter = filter;
        }

        #endregion

        #region Filter

        private IMetaFilter filter;

        public IMetaFilter Filter
        {
            get { return filter; }
            set
            {
                if (filter == this)
                    throw new ArgumentException(
                        "You cannot use a Metatable as its own filter. (Recursion!)");
                filter = value;
            }
        }

        #endregion

        #region Table

        public override void Add(string key, MetaEntry value)
        {
            Add(new KeyValuePair<string, MetaEntry>(key, value));
        }

        public override void Add(KeyValuePair<string, MetaEntry> item)
        {
            KeyValuePair<string, MetaEntry>? nentry = Filter.SetTransform(item);
            if (nentry.HasValue)
                base.Add(nentry.Value);
        }

        public void AddTo(string key, MetaEntry entry)
        {
            if (ContainsKey(key))
                this[key] = this[key].AddToList(entry);
            else
                Add(key, (MetaEntry) new MetaEntry[] {entry});
        }

        public override bool ContainsKey(string key)
        {
            return base.ContainsKey(Filter.GetTransform(key));
        }

        public override MetaEntry this[string key]
        {
            get
            {
                key = Filter.GetTransform(key);
                MetaEntry ret;
                if (key == null)
                    ret = new MetaEntry("");
                else
                    ret = base[key] ?? new MetaEntry("");

                if (!base.ContainsKey(key))
                    base.Add(key, ret);

                return ret;
            }
            set
            {
                KeyValuePair<string, MetaEntry>? item = Transform(key, value);
                if (item == null)
                    return;
                else
                    base[item.Value.Key] = item.Value.Value;
            }
        }

        public void SetDirect(KeyValuePair<string, MetaEntry> item)
        {
            if (ContainsKey(item.Key))
                Remove(item.Key);
            if (item.Value != null)
                base[item.Key] = item.Value;
        }

        public void SetDirect(string key, MetaEntry entry)
        {
            SetDirect(new KeyValuePair<string, MetaEntry>(key, entry));
        }

        #endregion

        #region Storage

        public void Store(TextWriter writer)
        {
            foreach (KeyValuePair<string, MetaEntry> kvp in this)
            {
                MetaEntry entry = kvp.Value;
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
            MetaTable clone = new MetaTable(Filter, Count);
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

    #region MetaEntry

    #endregion

    public interface IMetaFilter
    {
        string GetTransform(string key);
        KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item);
    }

    public interface IHasMetaTable
    {
        MetaTable Meta
        {
            get;
        }
    }
}