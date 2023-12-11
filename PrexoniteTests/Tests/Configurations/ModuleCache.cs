using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Configurations;

public class ModuleCache
{
    public static ModuleCache LeaseFor(bool compileToCil, FunctionLinking functionLinking) => 
        poolFor(compileToCil, functionLinking).Get();
    
    public static void ReturnTo(bool compileToCil, FunctionLinking functionLinking, ModuleCache cache) =>
        poolFor(compileToCil, functionLinking).Return(cache);

    static ObjectPool<ModuleCache> poolFor(bool compileToCil, FunctionLinking functionLinking) =>
        (compileToCil, functionLinking) switch
        {
            (false, _) => InterpretedCache,
            (true, FunctionLinking.FullyIsolated) => IsolatedCilCache,
            (true, FunctionLinking.FullyStatic) => LinkedCilCache,
            _ => throw new ArgumentException(
                $"Test scenario compileToCil={compileToCil}, functionLinking={functionLinking} is not supported."),
        };

    static readonly ObjectPool<ModuleCache> InterpretedCache = mkCachePool();
    static readonly ObjectPool<ModuleCache> IsolatedCilCache = mkCachePool();
    static readonly ObjectPool<ModuleCache> LinkedCilCache = mkCachePool();

    static ObjectPool<ModuleCache> mkCachePool() => 
        new DefaultObjectPool<ModuleCache>(
            new DefaultPooledObjectPolicy<ModuleCache>(), 
            Environment.ProcessorCount
            );

    ITargetDescription? _legacySymbolsDescription;

    ITargetDescription? _stdlibDescription;

    static readonly TraceSource _trace =
        new("PrexoniteTests.Tests.Configurations.ModuleCache");

    // ReSharper disable InconsistentNaming
    ManualPlan Cache { get; } = new IncrementalPlan();

    // ReSharper disable InconsistentNaming

    ITargetDescription _loadLegacySymbols()
    {
        var moduleName = new ModuleName("prx.v1", new(0, 0));
        var desc = Cache.CreateDescription(moduleName,
            Source.FromEmbeddedPrexoniteResource("prxlib.prx.v1.prelude.pxs"),
            "prxlib/prx.v1.prelude.pxs",
            Enumerable.Empty<ModuleName>());
        Cache.TargetDescriptions.Add(desc);
        Cache.Build(moduleName);
        // Important: lookup the target description in order to get the cached description
        return Cache.TargetDescriptions[moduleName];

    }

    /// <summary>
    /// Description of the module containing legacy symbols.
    /// </summary>
    ITargetDescription LegacySymbolsDescription =>
        _legacySymbolsDescription ??= _loadLegacySymbols();
    // ReSharper restore InconsistentNaming

    ITargetDescription stdlibDescription => _stdlibDescription ??= _loadStdlib();

    ITargetDescription _loadStdlib()
    {
        try
        {
            var v1Name = new ModuleName("prx", new(1, 0));
            var v1Desc = Cache.CreateDescription(v1Name, Source.FromEmbeddedPrexoniteResource("prxlib.prx.v1.pxs"),
                "prxlib/prx.v1.pxs", Enumerable.Empty<ModuleName>());
            Cache.TargetDescriptions.Add(v1Desc);
                
            var primName = new ModuleName("prx.prim", new(0, 0));
            var primDesc = Cache.CreateDescription(primName, Source.FromEmbeddedPrexoniteResource("prxlib.prx.prim.pxs"),
                "prxlib/prx.prim.pxs", Enumerable.Empty<ModuleName>());
            Cache.TargetDescriptions.Add(primDesc);

            var coreName = new ModuleName("prx.core", new(0, 0));
            var coreDesc = Cache.CreateDescription(coreName, Source.FromEmbeddedPrexoniteResource("prxlib.prx.core.pxs"),
                "prxlib/prx.core.pxs", primName.Singleton());
            Cache.TargetDescriptions.Add(coreDesc);

            var sysName = new ModuleName("sys", new(0, 0));
            var desc = Cache.CreateDescription(sysName, Source.FromEmbeddedPrexoniteResource("prxlib.sys.pxs"), "prxlib/sys.pxs",
                new[]{ primName, coreName });
            Cache.TargetDescriptions.Add(desc);

            Cache.Build(sysName);
            return Cache.TargetDescriptions[sysName];
        }
        finally
        {
            _trace.Flush();
        }
    }
    
    public void Describe(Loader environment, TestDependency script)
    {
        var path = script.ScriptName;
        var dependencies = script.Dependencies ?? Enumerable.Empty<string>();

        var file = environment.ApplyLoadPaths(path);
        if (file == null)
            throw new PrexoniteException($"Cannot find script {path}.");

        var moduleName = new ModuleName(Path.GetFileNameWithoutExtension(path), new(0, 0));

        if (Cache.TargetDescriptions.Contains(moduleName))
        {
            _trace.TraceEvent(TraceEventType.Verbose, 0,
                "ModuleCache already contains a description of {0} on thread {1}, no action necessary.", moduleName,
                Environment.CurrentManagedThreadId);
            return;
        }

        var dependencyNames =
            dependencies.Select(dep => 
                new ModuleName(Path.GetFileNameWithoutExtension(dep), new(0, 0))).ToArray();

        // Manually add legacy symbol and stdlib dependencies
        var effectiveDependencies = dependencyNames.Append(LegacySymbolsDescription.Name).Append(stdlibDescription.Name);
        var desc = Cache.CreateDescription(moduleName, file.ToSource(), path, effectiveDependencies);
        _trace.TraceEvent(TraceEventType.Information, 0,
            "Adding new target description for cache on thread {0}: {1}.", Thread.CurrentThread.ManagedThreadId,
            desc);
        Cache.TargetDescriptions.Add(desc);
    }

    public (Application Application, ITarget Target) Load(string path)
    {
        var targetModuleName = _toModuleName(path);
        (Application Application, ITarget Target) result;
        try
        {
            result = Cache.Load(targetModuleName);
        }
        finally
        {
            _trace.Flush();
        }
            
        return result;
    }

    static ModuleName _toModuleName(string path)
    {
        return new(Path.GetFileNameWithoutExtension(path), new(0, 0));
    }

    public Task<ITarget> BuildAsync(ModuleName name)
    {
        _trace.TraceEvent(TraceEventType.Information, 0, "Requested asynchronous build of module {0}.", name);
        return Cache.BuildAsync(name, CancellationToken.None);
    }
}