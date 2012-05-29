using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Configurations
{
    public static class ModuleCache
    {
        [ThreadStatic]
        private static DateTime _lastAccess;
        [ThreadStatic]
        private static Dictionary<string, Tuple<Module, IEnumerable<SymbolInfo>>> _cache;

// ReSharper disable InconsistentNaming
        private static Dictionary<string, Tuple<Module, IEnumerable<SymbolInfo>>> Cache
// ReSharper restore InconsistentNaming
        {
            get
            {
                _lastAccess = DateTime.Now;
                return _cache ?? (_cache = new Dictionary<string, Tuple<Module, IEnumerable<SymbolInfo>>>());
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

        public static bool TryGetModule(string path, out Module module, out IEnumerable<SymbolInfo> symbols)
        {
            EnsureFresh();
            Tuple<Module, IEnumerable<SymbolInfo>> tuple;
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

        public static IEnumerable<SymbolInfo> Provide(string path, Loader ldr)
        {
            EnsureFresh();
            var origin = new SymbolOrigin.ModuleTopLevel(ldr.ParentApplication.Module.Name,
                                                         new SourcePosition(path, -1, -1));
            var symbols =
                ldr.Symbols.LocalDeclarations
                .Select(decl => new SymbolInfo(decl.Value, origin, decl.Key))
                .ToArray();
            
            Cache[path] =
                Tuple.Create<Module, IEnumerable<SymbolInfo>>(
                    ldr.ParentApplication.Module, symbols);
            LastAccess = DateTime.Now;

            return symbols;
        }
    }
}
