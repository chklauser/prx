using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Configurations
{
    public static class ModuleCache
    {
        [ThreadStatic]
        private static DateTime _lastAccess;
        [ThreadStatic]
        private static Dictionary<string, Tuple<Module,IDictionary<string,SymbolEntry>>> _cache;

// ReSharper disable InconsistentNaming
        private static Dictionary<string, Tuple<Module, IDictionary<string, SymbolEntry>>> Cache
// ReSharper restore InconsistentNaming
        {
            get
            {
                _lastAccess = DateTime.Now;
                return _cache ?? (_cache = new Dictionary<string, Tuple<Module, IDictionary<string, SymbolEntry>>>());
            }
        }

        public static DateTime LastAccess
        {
            get
            {
                return _lastAccess;
            }
            set { _lastAccess = value; }
        }

        public static TimeSpan StaleTimeout
        {
            get
            {
                return new TimeSpan(0,0,0,20);
            }
        }

        public static bool IsStale
        {
            //get { return (DateTime.Now - LastAccess) > StaleTimeout; }
            get { return false; }
        }

// ReSharper disable InconsistentNaming
        private static void EnsureFresh()
// ReSharper restore InconsistentNaming
        {
            if(IsStale)
                _cache = null;
        }

        public static bool TryGetModule(string path, out Module module, out IDictionary<string,SymbolEntry> symbols )
        {
            EnsureFresh();
            Tuple<Module, IDictionary<string, SymbolEntry>> tuple;
            if (Cache.TryGetValue(path, out tuple))
            {
                LastAccess = DateTime.Now;
                module =  tuple.Item1;
                symbols = tuple.Item2;
                return true;
            }
            else
            {
                module = null;
                symbols = null;
                return false;
            }
        }

        public static IDictionary<string, SymbolEntry> Provide(string path, Loader ldr)
        {
            EnsureFresh();
            var symbols = 
                ldr.Symbols
                .Where(kvp =>
                {
                    var e = kvp.Value;
                    return e.Module == null || e.Module == ldr.ParentApplication.Module.Name;
                })
                .ToDictionary(sym => sym.Key, sym => sym.Value);
            Cache[path] =
                Tuple.Create<Module, IDictionary<string, SymbolEntry>>(
                    ldr.ParentApplication.Module, symbols);
            LastAccess = DateTime.Now;

            return symbols;
        }

        public static void ImportSymbols(this Loader ldr, IEnumerable<KeyValuePair<string, SymbolEntry>> symbols )
        {
            foreach (var sym in symbols)
                ldr.Symbols[sym.Key] = sym.Value;
        }
    }
}
