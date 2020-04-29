// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    /// <summary>
    /// A build plan that can read self-describing Prexonite Script module files.
    /// </summary>
    [PublicAPI]
    public interface ISelfAssemblingPlan : IPlan
    {
        /// <summary>
        /// A mutable list of path prefixes to use for searching mentioned modules. Implementations is guaranteed to be
        /// thread-safe, but enumeration may not be consistent.
        /// </summary>
        /// <para>Behavior is not defined if the search path is being modified while this build plan is
        /// assembling modules.</para>
        [NotNull, PublicAPI]
        IList<string> SearchPaths { get; }

        /// <summary>
        /// Reads self-assembly instructions in the header of the provided source 
        /// file and creates the corresponding build target descriptions. 
        /// Does not build any targets.
        /// </summary>
        /// <param name="source">The source from which to read the self-assembly 
        /// instructions</param>
        /// <param name="token">The cancellation token for this asynchronous operation.</param>
        /// <returns>A task that represents the build plan assembly in progress.</returns>
        [NotNull, PublicAPI]
        Task<ITargetDescription> AssembleAsync(ISource source, CancellationToken token = default);

        /// <summary>
        /// The set of standard library modules to implicitly link against. Can be suppressed on a per-module basis via the <see cref="Module.NoStandardLibraryKey"/> tag.
        /// </summary>
        [NotNull, PublicAPI]
        ISet<ModuleName> StandardLibrary { get; }

        /// <summary>
        /// <para>Offers a module in source form to the self-assembling build plan.</para>
        /// <para>Unlike <see cref="SelfAssemblingPlan.AssembleAsync"/>, this method does <em>not</em> search the file system for dependencies. It simply takes note of 
        /// them, expecting the user of the build plan to make sure that all dependencies are met in the end.</para>
        /// </summary>
        /// <param name="source">The source text to read. Must be a module.</param>
        /// <param name="token"></param>
        /// <returns>A description of the supplied module. Its dependencies might not be satisfied at this point.</returns>
        [NotNull]
        Task<ITargetDescription> RegisterModule(ISource source, CancellationToken token = default);
    }
}