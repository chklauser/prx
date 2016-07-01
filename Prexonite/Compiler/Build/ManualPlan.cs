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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        /// Creates an <see cref="ITargetDescription"/> from an <see cref="ISource"/> with manually specified dependencies.
        /// </summary>
        /// <param name="moduleName">The name of module to be compiled.6</param>
        /// <param name="source">The source code of the module.</param>
        /// <param name="fileName">The file name to store in symbols derived from that reader.</param>
        /// <param name="dependencies">The set of modules that need to be linked with this target.</param>
        /// <param name="buildMessages">Messages associated with this target description. Will automatically be part of the resulting target.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Dependencies must not contain multiple versions of the same module.</exception>
        /// <remarks>You will have to make sure that this plan contains a target description for each dependency before this target description can be built.</remarks>
        public ITargetDescription CreateDescription([NotNull] ModuleName moduleName, [NotNull] ISource source, [CanBeNull] string fileName, [NotNull] IEnumerable<ModuleName> dependencies, [CanBeNull] IEnumerable<Message> buildMessages = null)
        {
            return new ManualTargetDescription(moduleName, source, fileName, dependencies, buildMessages);
        }

        protected void EnsureIsResolved(ITargetDescription description)
        {
            if (HasUnresolvedDependencies(description))
            {
                var missing = description.Dependencies.Where(d => !TargetDescriptions.Contains(d)).ToEnumerationString();
                Plan.Trace.TraceEvent(TraceEventType.Error, 0,
                    "Failed to resolve the following dependencies of target {0}: {1}", description, missing);
                throw new BuildException(
                    string.Format("Not all dependencies of target named {0} have been resolved. The following modules are missing: {1}",
                                  description.Name, missing), description);
            }
        }

        protected bool HasUnresolvedDependencies(ITargetDescription targetDescription)
        {
            return !targetDescription.Dependencies.All(TargetDescriptions.Contains);
        }

        public Task<ITarget> BuildAsync(ModuleName name, CancellationToken token)
        {
            var taskMap = CreateTaskMap();
            var description = _prepareBuild(name);
            return BuildWithMapAsync(description, taskMap, token);
        }

        public IDictionary<ModuleName, Task<ITarget>> BuildAsync(IEnumerable<ModuleName> names, CancellationToken token)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            var taskMap = CreateTaskMap();
            return
                names
                    .ToDictionary(name => name, name => BuildWithMapAsync(_prepareBuild(name), taskMap, token));
        }

        private ITargetDescription _prepareBuild(ModuleName name)
        {
            var description = TargetDescriptions[name];
            EnsureIsResolved(description);
            return description;
        }

        protected virtual TaskMap<ModuleName, ITarget> CreateTaskMap()
        {
            return new TaskMap<ModuleName, ITarget>(5, TargetDescriptions.Count);
        }

        public Task<Tuple<Application, ITarget>> LoadAsync(ModuleName name, CancellationToken token)
        {
            var taskMap = CreateTaskMap();
            var description = _prepareBuild(name);
            return BuildWithMapAsync(description, taskMap, token).ContinueWith(buildTask =>
                {
                    var target = buildTask.Result;
                    var app = new Application(target.Module);
                    _linkDependencies(taskMap, app, description, token);
                    return Tuple.Create(app, target);
                }, token);
        }

        [CanBeNull] private LoaderOptions _options;
        public LoaderOptions Options
        {
            get { return _options; }
            set { _options = value; }
        }

        private void _linkDependencies(TaskMap<ModuleName, ITarget> taskMap, Application instance, ITargetDescription instanceDescription, CancellationToken token)
        {
            _LinkDependenciesImpl(this, taskMap, instance, instanceDescription, token);
        }

        internal static void _LinkDependenciesImpl(IPlan plan, TaskMap<ModuleName, ITarget> taskMap, Application instance,
                                            ITargetDescription instanceDescription, CancellationToken token)
        {
            foreach (var dependency in instanceDescription.Dependencies)
            {
                if (instance.IsLinkedTo(dependency))
                    continue;
                var dependencyDescription = plan.TargetDescriptions[dependency];
                token.ThrowIfCancellationRequested();
                var dependencyInstance = new Application(taskMap[dependency].Value.Result.Module);
                Application.Link(instance, dependencyInstance);
                _LinkDependenciesImpl(plan, taskMap, dependencyInstance, dependencyDescription, token);
            }
        }

        protected Task<IBuildEnvironment> GetBuildEnvironmentAsync(TaskMap<ModuleName, ITarget> taskMap, ITargetDescription description, CancellationToken token)
        {
            if (description.Dependencies.Count == 0)
            {
                var tcs = new TaskCompletionSource<IBuildEnvironment>();
                tcs.SetResult(GetBuildEnvironment(taskMap, description, token));
                return tcs.Task;
            }
            else
                // Note how the lambda expression doesn't actually depend on the result (directly)
                //  the continue all makes sure that we're not wasting a thread that blocks on Task.Result
                // GetBuildEnvironment can access the results via the taskMap.
                return Task.Factory.ContinueWhenAll(description.Dependencies.Select(d => taskMap[d].Value).ToArray(),
                _ => GetBuildEnvironment(taskMap, description, token));
        }

        protected virtual IBuildEnvironment GetBuildEnvironment(TaskMap<ModuleName, ITarget> taskMap, ITargetDescription description, CancellationToken token)
        {
            Plan.Trace.TraceEvent(TraceEventType.Verbose, 0, "Get build environment for {0}.", description);
            var buildEnvironment = new DefaultBuildEnvironment(this, description, taskMap, token);
            return buildEnvironment;
        }

        protected Task<ITarget> BuildWithMapAsync(ITargetDescription targetDescription, TaskMap<ModuleName, ITarget> taskMap, CancellationToken token)
        {
            return taskMap.GetOrAdd(targetDescription.Name,
                name =>
                    {
                        Plan.Trace.TraceEvent(TraceEventType.Verbose, 0, "Request build of {0} and its dependencies.",
                            targetDescription);
                        return
                            Task.Factory.StartNew(() => _buildTaskImpl(targetDescription, taskMap, token, name), token)
                                .Unwrap();
                    });
        }

        private Task<ITarget> _buildTaskImpl(ITargetDescription targetDescription, TaskMap<ModuleName, ITarget> taskMap, CancellationToken token,
            ModuleName name)
        {
            var desc = TargetDescriptions[name];
            var deps =
                desc.Dependencies.Select(
                    depName =>
                        new KeyValuePair<ModuleName, Task<ITarget>>(
                            depName,
                            BuildWithMapAsync(
                                TargetDescriptions[depName],
                                taskMap, token)));

            var depMap = new Dictionary<ModuleName, Task<ITarget>>();
            depMap.AddRange(deps);

            token.ThrowIfCancellationRequested();

            var buildTask = GetBuildEnvironmentAsync(taskMap, desc,
                token)
                .ContinueWith(bet =>
                                  {
                                      var instance =
                                          new Application(
                                              bet.Result.Module);
                                      Plan.Trace.TraceEvent(
                                          TraceEventType.Verbose, 0,
                                          "Linking compile-time dependencies for module {0}.",
                                          bet.Result.Module.Name);
                                      _linkDependencies(taskMap,
                                          instance, targetDescription,
                                          token);
                                      token
                                          .ThrowIfCancellationRequested
                                          ();
                                      return BuildTargetAsync(bet, desc,
                                          depMap, token);
                                  }, token);
            return buildTask.Unwrap();
        }

        protected virtual Task<ITarget> BuildTargetAsync(Task<IBuildEnvironment> buildEnvironment, ITargetDescription description, Dictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            var buildTask = description.BuildAsync(buildEnvironment.Result, dependencies, token);
            Debug.Assert(buildTask != null, "Task for building target is null.", string.Format("{0}.BuildAsync returned null instead of a Task.", description.GetType().Name));
            return buildTask;
        }
    }
}