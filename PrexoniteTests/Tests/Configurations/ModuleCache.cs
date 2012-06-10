using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;

namespace PrexoniteTests.Tests.Configurations
{
    public static class ModuleCache
    {
        [ThreadStatic]
        private static DateTime _lastAccess;

        [ThreadStatic] 
        private static IncrementalPlan _plan;

        [ThreadStatic] private static ITargetDescription _sysDescription;

// ReSharper disable InconsistentNaming
        private static ManualPlan Cache
// ReSharper restore InconsistentNaming
        {
            get
            {
                _lastAccess = DateTime.Now;
                return _plan ?? (_plan = new IncrementalPlan());
            }
        }

        // ReSharper disable InconsistentNaming
        private static ITargetDescription SysDescription
        // ReSharper restore InconsistentNaming
        {
            get { return _sysDescription ?? (_sysDescription = _loadSys()); }
        }

        private static ITargetDescription _loadSys()
        {
            var sysName = new ModuleName("sys", new Version(0, 0));
            var desc = Cache.CreateDescription(sysName,
                                               Source.FromString(Resources.sys),
                                               "sys.pxs",
                                               Enumerable.Empty<ModuleName>());
            Cache.TargetDescriptions.Add(desc);
            Cache.Build(sysName);
            // Important: lookup the target description in order to get the cached description
            return Cache.TargetDescriptions[sysName];
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
            if (IsStale)
            {
                _plan = null;
            }
        }



        public static void Describe(Loader environment, TestDependency script)
        {
            EnsureFresh();

            var path = script.ScriptName;
            var dependencies = script.Dependencies ?? Enumerable.Empty<string>();

            var file = environment.ApplyLoadPaths(path);
            if (file == null || !file.Exists)
                throw new PrexoniteException(string.Format("Cannot find script {0}.", path));

            var moduleName = new ModuleName(Path.GetFileNameWithoutExtension(path), new Version(0, 0));

            if (Cache.TargetDescriptions.Contains(moduleName))
                return;
            
            var dependencyNames =
                dependencies.Select(dep => 
                    new ModuleName(Path.GetFileNameWithoutExtension(dep), new Version(0, 0))).ToArray();

            var desc = Cache.CreateDescription(moduleName, Source.FromFile(file,Encoding.UTF8), path, dependencyNames.Append(SysDescription.Name));
            Cache.TargetDescriptions.Add(desc);

        }

        private static IEnumerable<SymbolInfo> _addOriginInfo(ProvidedTarget p)
        {
            if (p.Symbols == null)
                return Enumerable.Empty<SymbolInfo>();
            else
                return p.Symbols.Select(s => 
                    new SymbolInfo(
                        s.Value, 
                        new SymbolOrigin.ModuleTopLevel(p.Name, NoSourcePosition.Instance), s.Key)
                    );
        }

        public static Tuple<Application,ITarget> Load(string path)
        {
            EnsureFresh();

            return Cache.Load(_toModuleName(path));
        }

        private static ModuleName _toModuleName(string path)
        {
            return new ModuleName(Path.GetFileNameWithoutExtension(path), new Version(0, 0));
        }

        public static ITarget Build(string path)
        {
            EnsureFresh();
            return Cache.Build(_toModuleName(path));
        }

        public static Task<ITarget> BuildAsync(ModuleName name)
        {
            return Cache.BuildAsync(name, CancellationToken.None);
        }
    }
}
