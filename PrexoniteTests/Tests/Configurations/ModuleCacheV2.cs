#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Prexonite.Compiler.Ast;

using System.Threading.Tasks;
using Prexonite;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests.Configurations
{
    public static class ModuleCacheV2
    {
        [ThreadStatic] 
        private static ISelfAssemblingPlan? _sharedPlan;

        [ThreadStatic]
        private static Engine? _sharedEnginePrototype;

        private static readonly TraceSource _trace = new("PrexoniteTests.Tests.Configurations.ModuleCacheV2");
        
        private static readonly Lazy<string> _slnPath = new Lazy<string>(() =>
        {
            _trace.TraceEvent(TraceEventType.Information, 0, "Infer sln path");
            var slnCandidate = AppContext.BaseDirectory;
            while (Directory.Exists(slnCandidate) && !File.Exists(Path.Combine(slnCandidate, "prx.sln")))
                slnCandidate = Path.Combine(slnCandidate, @".." + Path.DirectorySeparatorChar);

            if (Directory.Exists(slnCandidate))
            {
                return slnCandidate;
            }
            else
            {
                throw new PrexoniteException("Failed to infer solution path.");
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static string SolutionPath => _slnPath.Value;

        private static ISelfAssemblingPlan sharedPlan
        {
            get
            {
                if (_sharedPlan != null)
                {
                    return _sharedPlan;
                }

                _trace.TraceEvent(TraceEventType.Information, 0, 
                    "Creating self-assembling build plan for test thread {0}", 
                    Thread.CurrentThread.ManagedThreadId);

                var psrPath = Path.GetFullPath(Path.Combine(SolutionPath, "Prx", "psr", "_2"));
                _trace.TraceEvent(TraceEventType.Information, 0, "inferred psr path {0}", psrPath);
                var plan = Plan.CreateSelfAssembling(StandardLibraryPreference.Default);
                plan.SearchPaths.Add(psrPath);
                _sharedPlan = plan;
                return plan;
            }
        }

        public static Engine CreateEngine() => new Engine(sharedEnginePrototype);

        private static Engine sharedEnginePrototype => _sharedEnginePrototype ??= new Engine();

        public static (Application, ITarget) Load(string path, bool compileToCil) => 
            loadAsync(path, compileToCil, sharedPlan).Result;

        private static async Task<(Application, ITarget)> loadAsync(string path, bool compileToCil, ISelfAssemblingPlan plan,
            CancellationToken ct = default)
        {
            var source = new FileSource(new FileInfo(path), Encoding.UTF8);
            var desc = await plan.AssembleAsync(source, ct);
            var loadedAppTask = compileToCil
                ? (await Compiler.CompileModulesAsync(plan, desc.Name.Singleton(), 
                    sharedEnginePrototype, FunctionLinking.FullyStatic, ct))[desc.Name]
                : await plan.LoadAsync(desc.Name, ct);
            return loadedAppTask.ToValueTuple();
        }
    }
}