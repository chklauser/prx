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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        [ThreadStatic] private static ITargetDescription _legacySymbolsDescription;

        [ThreadStatic] private static ITargetDescription _stdlibDescription;

        [NotNull] private static readonly TraceSource _trace =
            new TraceSource("PrexoniteTests.Tests.Configurations.ModuleCache");

// ReSharper disable InconsistentNaming
        private static ManualPlan Cache
// ReSharper restore InconsistentNaming
        {
            get
            {
                _lastAccess = DateTime.Now;
                if (_plan != null) return _plan;
                else
                {
                    _trace.TraceEvent(TraceEventType.Information, 0, "Creating empty build plan for thread {0}.", Thread.CurrentThread.ManagedThreadId);
                    return _plan = new IncrementalPlan();
                }
            }
        }

        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Description of the module containing legacy symbols.
        /// </summary>
        private static ITargetDescription LegacySymbolsDescription
        // ReSharper restore InconsistentNaming
        {
            get { return _legacySymbolsDescription ?? (_legacySymbolsDescription = _loadLegacySymbols()); }
        }

        private static ITargetDescription _loadLegacySymbols()
        {
            Trace.CorrelationManager.StartLogicalOperation("Load legacy symbols (_loadLegacySymbols)");
            try
            {
                var moduleName = new ModuleName("prx.v1", new Version(0, 0));
                var desc = Cache.CreateDescription(moduleName,
                                                   Source.FromString(Resources.legacy_symbols),
                                                   "prxlib/legacy_symbols.pxs",
                                                   Enumerable.Empty<ModuleName>());
                Cache.TargetDescriptions.Add(desc);
                Cache.Build(moduleName);
                // Important: lookup the target description in order to get the cached description
                return Cache.TargetDescriptions[moduleName];
            }
            finally
            {
                Trace.CorrelationManager.StopLogicalOperation();
            }
            
        }

        private static ITargetDescription StdlibDescription
        {
            get { return _stdlibDescription ?? (_stdlibDescription = _loadStdlib());  }
        }

        private static ITargetDescription _loadStdlib()
        {
            Trace.CorrelationManager.StartLogicalOperation("Load stdlib");
            try
            {
                var sysName = new ModuleName("sys", new Version(0, 0));
                var desc = Cache.CreateDescription(sysName, Source.FromString(Resources.sys), "prxlib/sys.pxs",
                    Enumerable.Empty<ModuleName>());
                Cache.TargetDescriptions.Add(desc);
                Cache.Build(sysName);
                return Cache.TargetDescriptions[sysName];
            }
            finally
            {
                Trace.CorrelationManager.StopLogicalOperation();
                _trace.Flush();
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
            if (IsStale)
            {
                _trace.TraceEvent(TraceEventType.Information, 0, "Delete cached build plan for thread {0}.",
                    Thread.CurrentThread.ManagedThreadId);
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
            {
                _trace.TraceEvent(TraceEventType.Verbose, 0,
                    "ModuleCache already contains a description of {0} on thread {1}, no action necessary.", moduleName,
                    Thread.CurrentThread.ManagedThreadId);
                return;
            }

            var dependencyNames =
                dependencies.Select(dep => 
                    new ModuleName(Path.GetFileNameWithoutExtension(dep), new Version(0, 0))).ToArray();

            // Manually add legacy symbol and stdlib dependencies
            var effectiveDependencies = dependencyNames.Append(LegacySymbolsDescription.Name).Append(StdlibDescription.Name);
            var desc = Cache.CreateDescription(moduleName, Source.FromFile(file,Encoding.UTF8), path, effectiveDependencies);
            _trace.TraceEvent(TraceEventType.Information, 0,
                "Adding new target description for cache on thread {0}: {1}.", Thread.CurrentThread.ManagedThreadId,
                desc);
            Cache.TargetDescriptions.Add(desc);
        }

        public static Tuple<Application,ITarget> Load(string path)
        {
            EnsureFresh();

            var targetModuleName = _toModuleName(path);
            Trace.CorrelationManager.StartLogicalOperation("ModuleCache.Load(" + targetModuleName + ")");
            Tuple<Application, ITarget> result;
            try
            {
                result = Cache.Load(targetModuleName);
            }
            finally
            {
                Trace.CorrelationManager.StopLogicalOperation();
                _trace.Flush();
            }
            
            return result;
        }

        private static ModuleName _toModuleName(string path)
        {
            return new ModuleName(Path.GetFileNameWithoutExtension(path), new Version(0, 0));
        }

        public static Task<ITarget> BuildAsync(ModuleName name)
        {
            EnsureFresh();
            _trace.TraceEvent(TraceEventType.Information, 0, "Requested asynchronous build of module {0}.", name);
            return Cache.BuildAsync(name, CancellationToken.None);
        }
    }
}
