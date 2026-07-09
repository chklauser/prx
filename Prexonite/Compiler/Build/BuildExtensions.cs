

using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public static class BuildExtensions
{
    extension(IPlan plan)
    {
        public ITarget Build(ModuleName name)
        {
            return plan.BuildAsync(name).GetAwaiter().GetResult();
        }

        public Task<ITarget> BuildAsync(ModuleName name)
        {
            return plan.BuildAsync(name, CancellationToken.None);
        }

        public (Application Application, ITarget Target) Load(ModuleName name)
        {
            var loadTask = plan.LoadAsync(name, CancellationToken.None);
            return loadTask.GetAwaiter().GetResult();
        }

        public Application LoadApplication(ModuleName name)
        {
            var desc = plan.TargetDescriptions[name];
            var t = plan.LoadAsync(name, CancellationToken.None).Result;
            t.Item2.ThrowIfFailed(desc);
            return t.Item1;
        }

        public Task<Application> LoadAsync(ModuleName name)
        {
            return plan.LoadAsync(name, CancellationToken.None).ContinueWith(tt =>
            {
                var result = tt.Result;
                result.Item2.ThrowIfFailed(plan.TargetDescriptions[name]);
                return result.Item1;
            });
        }
    }

    extension(ISelfAssemblingPlan plan)
    {
        public ITargetDescription Assemble(ISource source)
        {
            return plan.AssembleAsync(source, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}