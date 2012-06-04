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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public class ManualPlan : IPlan
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
        /// <param name="source">The source code of the module.</param>
        /// <param name="fileName">The file name to store in symbols derived from that reader.</param>
        /// <param name="dependencies">The set of modules that need to be linked with this target.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Dependencies must not contain multiple versions of the same module.</exception>
        /// <remarks>You will have to make sure that this plan contains a target description for each dependency before this target description can be built.</remarks>
        public ITargetDescription CreateDescription(ModuleName moduleName, ISource source, string fileName, IEnumerable<ModuleName> dependencies)
        {
            return new ManualTargetDescription(moduleName, source, fileName, dependencies);
        }

        protected void EnsureIsResolved(ITargetDescription description)
        {
            if (HasUnresolvedDependencies(description))
            {
                var missing = description.Dependencies.Where(d => !TargetDescriptions.Contains(d));
                throw new BuildException(
                    string.Format("Not all dependencies of target named {0} have been resolved. The following modules are missing: {1}",
                                  description.Name, missing.ToEnumerationString()), description);
            }
        }

        protected bool HasUnresolvedDependencies(ITargetDescription targetDescription)
        {
            return !targetDescription.Dependencies.All(TargetDescriptions.Contains);
        }

        public Task<ITarget> BuildAsync(ModuleName name, CancellationToken token)
        {
            ConcurrentDictionary<ModuleName, Task<ITarget>> taskMap;
            var description = _prepareBuild(name, out taskMap);
            return BuildWithMapAsync(description, taskMap, token);
        }

        private ITargetDescription _prepareBuild(ModuleName name, out ConcurrentDictionary<ModuleName, Task<ITarget>> taskMap)
        {
            var description = TargetDescriptions[name];
            EnsureIsResolved(description);
            taskMap = CreateTaskMap();
            return description;
        }

        protected virtual ConcurrentDictionary<ModuleName, Task<ITarget>> CreateTaskMap()
        {
            return new ConcurrentDictionary<ModuleName, Task<ITarget>>(5, TargetDescriptions.Count);
        }

        public Task<Application> LoadAsync(ModuleName name, CancellationToken token)
        {
            ConcurrentDictionary<ModuleName, Task<ITarget>> taskMap;
            var description = _prepareBuild(name, out taskMap);
            return BuildWithMapAsync(description, taskMap, token).ContinueWith(buildTask =>
                {
                    var target = buildTask.Result;
                    var app = new Application(target.Module);
                    _linkDependencies(taskMap, app, description);
                    return app;
                },token);
        }

        private void _linkDependencies(ConcurrentDictionary<ModuleName,Task<ITarget>>  taskMap, Application instance, ITargetDescription instanceDescription )
        {
            foreach (var dependency in instanceDescription.Dependencies)
            {
                if (instance.IsLinkedTo(dependency)) 
                    continue;
                var dependencyDescription = TargetDescriptions[dependency];
                var dependencyInstance = new Application(taskMap[dependency].Result.Module);
                Application.Link(instance, dependencyInstance);
                _linkDependencies(taskMap, dependencyInstance,dependencyDescription);
            }
        }

        protected Task<IBuildEnvironment> GetBuildEnvironment(Dictionary<ModuleName, Task<ITarget>> dependencies, ITargetDescription description, CancellationToken token)
        {
            if (dependencies.Count == 0)
                return Task.Factory.StartNew<IBuildEnvironment>(() => 
                    new DefaultBuildEnvironment(Enumerable.Empty<ITarget>(), description, token));
            else
                return Task.Factory.ContinueWhenAll(dependencies.Values.ToArray(),
                deps => (IBuildEnvironment)
                        new DefaultBuildEnvironment(deps.Select(d => d.Result), description, token));
        }

        protected Task<ITarget> BuildWithMapAsync(ITargetDescription targetDescription, ConcurrentDictionary<ModuleName,Task<ITarget>> taskMap, CancellationToken token)
        {
            return taskMap.GetOrAdd(targetDescription.Name, name =>
                {
                    var desc = TargetDescriptions[name];
                    var deps =
                        desc.Dependencies.Select(
                            depName => new KeyValuePair<ModuleName, Task<ITarget>>(
                                           depName,
                                           BuildWithMapAsync(
                                               TargetDescriptions[depName],
                                               taskMap, token)));

                    var depMap = new Dictionary<ModuleName, Task<ITarget>>();
                    depMap.AddRange(deps);

                    token.ThrowIfCancellationRequested();

                    var buildTask = GetBuildEnvironment(depMap, desc, token)
                        .ContinueWith(bet => BuildTargetAsync(bet, desc, depMap, token), token,
                                      TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
                    return buildTask.Unwrap();
                });
        }

        protected virtual Task<ITarget> BuildTargetAsync(Task<IBuildEnvironment> buildEnvironment, ITargetDescription description, Dictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            var buildTask = description.BuildAsync(buildEnvironment.Result, dependencies, token);
            Debug.Assert(buildTask != null, "Task for building target is null.",string.Format("{0}.BuildAsync returned null instead of a Task.", description.GetType().Name));
            return buildTask;
        }
    }
}