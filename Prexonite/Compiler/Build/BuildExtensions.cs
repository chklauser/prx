using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public static class BuildExtensions
    {
        public static ITarget Build(this IPlan plan, ModuleName name)
        {
            return plan.BuildAsync(name).Result;
        }

        public static Task<ITarget> BuildAsync(this IPlan plan, ModuleName name)
        {
            return BuildAsync(plan, name, CancellationToken.None);
        }

        public static Task<ITarget> BuildAsync(this IPlan plan, ModuleName name, CancellationToken token)
        {
            var buildTasks = plan.BuildAsync(name.Singleton(), token);
            Debug.Assert(buildTasks.Count == 1,"Expected build task dictionary for a single module to only contain a single task.");
            return buildTasks[name];
        }

        public static Tuple<Application,ITarget> Load(this IPlan plan, ModuleName name)
        {
            var loadTask = plan.LoadAsync(name, CancellationToken.None);
            return loadTask.Result;
        }

        public static Application LoadApplication(this IPlan plan, ModuleName name)
        {
            var desc = plan.TargetDescriptions[name];
            var t = plan.LoadAsync(name, CancellationToken.None).Result;
            t.Item2.ThrowIfFailed(desc);
            return t.Item1;
        }

        public static Task<Application> LoadAsync(this IPlan plan, ModuleName name)
        {
            return plan.LoadAsync(name, CancellationToken.None).ContinueWith(tt =>
                {
                    var result = tt.Result;
                    result.Item2.ThrowIfFailed(plan.TargetDescriptions[name]);
                    return result.Item1;
                });
        }
    }
}
