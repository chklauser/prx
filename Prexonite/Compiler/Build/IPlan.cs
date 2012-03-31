using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prexonite.Compiler.Build
{
    public interface IPlan
    {
        /// <summary>
        /// List of Resolvers to try in that order to resolve unfulfilled dependencies.
        /// </summary>
        IList<IResolver> Resolvers
        {
            get;
        }

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

        Task Resolve();

        Task<ITarget> Build(ITargetDescription targetDescription);
    }
}
