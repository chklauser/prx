using System.Diagnostics;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal;

class DefaultBuildEnvironment : IBuildEnvironment
{
    readonly CancellationToken _token;
    readonly TaskMap<ModuleName, ITarget> _taskMap;
    readonly ManualPlan _plan;
    readonly ITargetDescription _description;
    readonly Engine _compilationEngine;

    public bool TryGetModule(ModuleName moduleName, [NotNullWhen(true)] out Module? module)
    {
        if(_taskMap.TryGetValue(moduleName, out var target))
        {
            module = target.Result.Module;
            return true;
        }
        else
        {
            module = null;
            return false;
        }
    }

    public DefaultBuildEnvironment(ManualPlan plan, ITargetDescription description, TaskMap<ModuleName, ITarget> taskMap, CancellationToken token)
    {
        _token = token;
        _plan = plan ?? throw new ArgumentNullException(nameof(plan));
        _taskMap = taskMap ?? throw new ArgumentNullException(nameof(taskMap));
        _description = description ?? throw new ArgumentNullException(nameof(description));
        var externals = new List<SymbolInfo>();
        foreach (var name in description.Dependencies)
        {
            var d = taskMap[name].Value.Result;
            if (d.Symbols == null) continue;
            var origin = new SymbolOrigin.ModuleTopLevel(name, NoSourcePosition.Instance);
            externals.AddRange(from decl in d.Symbols
                select new SymbolInfo(decl.Value, origin, decl.Key));
        }
        ExternalSymbols = SymbolStore.Create(conflictUnionSource: externals);
        _compilationEngine = plan.LeaseBuildEngine();
        Module = Module.Create(description.Name);
    }

    public SymbolStore ExternalSymbols { get; }

    public Module Module { get; }

    public Application InstantiateForBuild()
    {
        var instance = new Application(Module);
        ManualPlan._LinkDependenciesImpl(_plan,_taskMap, instance,_description, _token);
        return instance;
    }

    public Loader CreateLoader(LoaderOptions? defaults = null, Application? compilationTarget = null)
    {
        defaults ??= new(null, null);
        var planOptions = _plan.Options;
        if(planOptions != null)
            defaults.InheritFrom(planOptions);
        compilationTarget ??= InstantiateForBuild();
        Debug.Assert(compilationTarget.Module.Name == Module.Name);
        var lowPrioritySymbols = defaults.ExternalSymbols;
        SymbolStore predef;
        if(lowPrioritySymbols.IsEmpty)
        {
            predef = SymbolStore.Create(ExternalSymbols);
        }
        else
        {
            // First create an intermediate symbol store as a copy
            //  of ExternalSymbols, inheriting from lowPrioritySymbols
            predef = SymbolStore.Create(lowPrioritySymbols);
            foreach (var externalSymbol in ExternalSymbols)
                predef.Declare(externalSymbol.Key, externalSymbol.Value);
            // Then create the final SymbolStore inheriting from the intermediate one.
            //  that way we can later extract the declarations made by this module.
            predef = SymbolStore.Create(predef); 
        }
        var finalOptions = new LoaderOptions(_compilationEngine, compilationTarget, predef)
            {ReconstructSymbols = false, RegisterCommands = false};
        finalOptions.InheritFrom(defaults);
        return new(finalOptions);
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_compilationEngine != null)
            {
                _plan?.ReturnBuildEngine(_compilationEngine);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}