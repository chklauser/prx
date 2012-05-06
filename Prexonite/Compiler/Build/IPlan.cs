using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        Task<ITarget> BuildAsync(ITargetDescription targetDescription, CancellationToken token);
    }
}
