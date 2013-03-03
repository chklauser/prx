using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Build
{
    /// <summary>
    /// A build plan that can read self-describing Prexonite Script module files.
    /// </summary>
    public interface ISelfAssemblingPlan : IPlan
    {
        /// <summary>
        /// A mutable list of path prefixes to use for searching mentioned modules. 
        /// This object is updated concurrently, use <see cref="SearchPathsLock"/> 
        /// to coordinate access to it.
        /// </summary>
        [NotNull] IList<string> SearchPaths { get; }

        /// <summary>
        /// The lock used to coordinate access to <see cref="SearchPaths"/>.
        /// </summary>
        [NotNull] SemaphoreSlim SearchPathsLock { get; }

        /// <summary>
        /// Reads self-assembly instructions in the header of the provided source 
        /// file and creates the corresponding build target descriptions. 
        /// Does not build any targets.
        /// </summary>
        /// <param name="source">The source from which to read the self-assembly 
        /// instructions</param>
        /// <param name="token">The cancellation token for this asynchronous operation.</param>
        /// <returns>A task that represents the build plan assembly in progress.</returns>
        [NotNull] Task<ITargetDescription> AssembleAsync(ISource source, CancellationToken token);
    }
}