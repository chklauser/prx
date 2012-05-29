// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    class ManualPlan : IPlan
    {
        private readonly HashSet<IBuildWatcher> _buildWatchers = new HashSet<IBuildWatcher>();
        private readonly TargetDescriptionSet _targetDescriptions = TargetDescriptionSet.Create();

        public ISet<IBuildWatcher> BuildWatchers
        {
            get { return _buildWatchers; }
        }

        public TargetDescriptionSet TargetDescriptions
        {
            get { return _targetDescriptions; }
        }

        /// <summary>
        /// Creates an <see cref="ITargetDescription"/> from a <see cref="TextReader"/> with manually specified dependencies.
        /// </summary>
        /// <param name="moduleName">The name of module to be compiled.6</param>
        /// <param name="reader">The text reader for reading the module contents.</param>
        /// <param name="fileName">The file name to store in symbols derived from that reader.</param>
        /// <param name="dependencies">The set of modules that need to be linked with this target.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Dependencies must not contain multiple versions of the same module.</exception>
        /// <remarks>You will have to make sure that this plan contains a target description for each dependency before this target description can be built.</remarks>
        public ITargetDescription CreateDescription(ModuleName moduleName, TextReader reader, string fileName, IEnumerable<ModuleName> dependencies)
        {
            return new DefaultTargetDescription(moduleName, reader, fileName, dependencies);
        }

        protected void EnsureIsResolved(ITargetDescription description)
        {
            if (HasUnresolvedDependencies(description))
                throw new BuildException(
                    string.Format("Not all dependencies of target named {0} have been resolved.",
                        description.Name), description);
        }

        protected bool HasUnresolvedDependencies(ITargetDescription targetDescription)
        {
            return targetDescription.Dependencies.All(TargetDescriptions.Contains);
        }

        public Task<ITarget> BuildAsync(ITargetDescription targetDescription, CancellationToken token)
        {
            EnsureIsResolved(targetDescription);
            var taskMap =
                new System.Collections.Concurrent.ConcurrentDictionary<ITargetDescription, Task<ITarget>>(5,TargetDescriptions.Count);
            return BuildWithMapAsync(targetDescription, taskMap, token);
        }

        protected Task<IBuildEnvironment> GetBuildEnvironment(Dictionary<ModuleName, Task<ITarget>> dependencies, ITargetDescription description, CancellationToken token)
        {
            return Task.Factory.ContinueWhenAll(dependencies.Values.ToArray(),
                deps => (IBuildEnvironment)
                        new DefaultBuildEnvironment(deps.Select(d => d.Result), description, token));
        }

        protected Task<ITarget> BuildWithMapAsync(ITargetDescription targetDescription, System.Collections.Concurrent.ConcurrentDictionary<ITargetDescription,Task<ITarget>> taskMap, CancellationToken token)
        {
            return taskMap.GetOrAdd(targetDescription, desc =>
                {
                    var deps =
                        desc.Dependencies.Select(
                            name =>
                                {
                                    var depDesc =
                                        TargetDescriptions[name];
                                    return new KeyValuePair<ModuleName, Task<ITarget>>(depDesc.Name,
                                        BuildWithMapAsync(depDesc, taskMap, token));
                                });

                    var depMap = new Dictionary<ModuleName, Task<ITarget>>();
                    depMap.AddRange(deps);

                    token.ThrowIfCancellationRequested();

                    return GetBuildEnvironment(depMap,desc, token)
                        .ContinueWith(bet => desc.BuildAsync(bet.Result, depMap, token),token)
                        .Unwrap();
                });
        }
    }
}