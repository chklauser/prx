using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public interface IPlan
    {
        /// <summary>
        /// Set of build watchers that will get access to events raised during building.
        /// </summary>
        ISet<IBuildWatcher> BuildWatchers
        {
            get;
        }

        TargetDescriptionSet TargetDescriptions
        {
            get;
        }

        Task<ITarget> BuildAsync(ModuleName name, CancellationToken token);
        Task<Application> LoadAsync(ModuleName name, CancellationToken token);
    }
}
