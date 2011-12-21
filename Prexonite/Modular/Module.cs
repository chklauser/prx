using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Internal;

namespace Prexonite.Modular
{

    /// <summary>
    /// A Prexonite module is a named container for  Prexonite functions and variable declarations.
    /// </summary>
    public abstract class Module : IHasMetaTable, IMetaFilter
    {
        public abstract ModuleName Name { get; }

        public abstract MetaTable Meta { get; }

        public abstract PFunctionTable Functions { get; }

        public abstract VariableTable Variables { get; }

        public abstract CentralCache Cache { get; internal set; }

        #region IMetaFilter Members

        [DebuggerStepThrough]
        string IMetaFilter.GetTransform(string key)
        {
            return GetTransform(key);
        }

        protected virtual string GetTransform(string key)
        {
            if (Engine.StringsAreEqual(key, Application.NameKey))
                return Application.IdKey;
            else if (Engine.StringsAreEqual(key, "imports"))
                return Application.ImportKey;
            else
                return key;
        }

        [DebuggerStepThrough]
        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(
            KeyValuePair<string, MetaEntry> item)
        {
            return SetTransform(item);
        }

        protected virtual KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            if (Engine.StringsAreEqual(item.Key, Application.NameKey))
                item = new KeyValuePair<string, MetaEntry>(Application.IdKey, item.Value);
            else if (Engine.StringsAreEqual(item.Key, "imports"))
                item = new KeyValuePair<string, MetaEntry>(Application.ImportKey, item.Value);
            return item;
        }
        
        #endregion

        public static Module Create(ModuleName moduleName)
        {
            return new ModuleImpl(moduleName);
        }
    }

    class ModuleImpl : Module
    {
        private CentralCache _cache = CentralCache.Create();

        public ModuleImpl(ModuleName name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            
            _name = name;
            var m = MetaTable.Create(this);
            m[Application.IdKey] = name.ToMetaEntry();
            _meta = m;
            _meta[Application.EntryKey] = Application.DefaultEntryFunction;
            _meta[Application.ImportKey] = Application.DefaultImport;
        }

        public override CentralCache Cache
        {
            get { return _cache; }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                
                _cache = value;
            }
        }

        private readonly ModuleName _name;
        private readonly PFunctionTable _functions = new PFunctionTableImpl();
        private readonly VariableTable _variables = new VariableTable();
        private readonly MetaTable _meta; // must be assigned in the constructor

        public override ModuleName Name
        {
            get { return _name; }
        }

        public override MetaTable Meta
        {
            get { return _meta; }
        }

        public override PFunctionTable Functions
        {
            get { return _functions; }
        }

        public override VariableTable Variables
        {
            get { return _variables; }
        }

        #region IMetaFilter

        protected override string GetTransform(string key)
        {
            if (_meta == null)
                return key;
            else
                return base.GetTransform(key);
        }

        protected override KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            if (_meta == null)
                return item;
            else
                return base.SetTransform(item);
        }

        #endregion
    }
}
