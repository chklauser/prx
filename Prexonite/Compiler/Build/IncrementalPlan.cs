using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public class IncrementalPlan : ManualPlan
{
    readonly TaskMap<ModuleName, ITarget> _taskMap = new();

    protected override TaskMap<ModuleName, ITarget> CreateTaskMap()
    {
        return _taskMap;
    }

    protected override Task<ITarget> BuildTargetAsync(
        Task<IBuildEnvironment> buildEnvironment,
        ITargetDescription description,
        Dictionary<ModuleName, Task<ITarget>> dependencies,
        CancellationToken token
    )
    {
        return base.BuildTargetAsync(buildEnvironment, description, dependencies, token)
            .ContinueWith(
                t =>
                {
                    var actualTarget = t.Result;
                    if (actualTarget is ProvidedTarget)
                        return actualTarget;

                    var providedTarget = new ProvidedTarget(description, actualTarget);
                    TargetDescriptions.Replace(description, providedTarget);
                    return providedTarget;
                },
                token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current
            );
    }
}
