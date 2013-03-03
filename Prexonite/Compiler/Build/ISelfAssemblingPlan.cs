// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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