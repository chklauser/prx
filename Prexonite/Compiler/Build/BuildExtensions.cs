using System;
using System.Collections.Generic;
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
            var buildTask = plan.BuildAsync(name, CancellationToken.None);
            Console.WriteLine();
            Console.WriteLine();
            return buildTask.Result;
        }

        public static Application Load(this IPlan plan, ModuleName name)
        {
            var loadTask = plan.LoadAsync(name, CancellationToken.None);
            return loadTask.Result;
        }
    }
}
