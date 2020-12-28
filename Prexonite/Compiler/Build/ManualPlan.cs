#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;
using Debug = System.Diagnostics.Debug;

namespace Prexonite.Compiler.Build
{
    public class ManualPlan : IPlan
    {
        private readonly DefaultObjectPool<Engine> _enginePool;
        private class EnginePoolPolicy : IPooledObjectPolicy<Engine>
        {
            private readonly ManualPlan _inner;

            public EnginePoolPolicy(ManualPlan inner)
            {
                _inner = inner;
            }

            public Engine Create() =>
                _inner.Options?.ParentEngine switch
                {
                    { } x => new Engine(x),
                    _ => new Engine()
                };

            public bool Return(Engine obj) => true;
        }

        public ISet<IBuildWatcher> BuildWatchers { get; } = new HashSet<IBuildWatcher>();

        public TargetDescriptionSet TargetDescriptions { get; } = TargetDescriptionSet.Create();

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
        public ITargetDescription CreateDescription(ModuleName moduleName, ISource source, string? fileName, IEnumerable<ModuleName> dependencies, IEnumerable<Message>? buildMessages = null)
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
                    $"Not all dependencies of target named {description.Name} have been resolved. The following modules are missing: {missing}", description);
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
            return new(5, TargetDescriptions.Count);
        }

        public IDictionary<ModuleName, Task<Tuple<Application, ITarget>>> LoadAsync(IEnumerable<ModuleName> names, CancellationToken token)
        {
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            var taskMap = CreateTaskMap();
            return names.ToDictionary(name => name, name =>
            {
                var description = _prepareBuild(name);
                return BuildWithMapAsync(description, taskMap, token).ContinueWith(buildTask =>
                {
                    var target = buildTask.Result;
                    var app = new Application(target.Module);
                    _linkDependencies(taskMap, app, description, token);
                    return Tuple.Create(app, target);
                }, token);
            });
        }

        protected ManualPlan()
        {
            _enginePool = new DefaultObjectPool<Engine>(new EnginePoolPolicy(this), Environment.ProcessorCount);
        }

        public LoaderOptions? Options { get; set; }

        internal Engine LeaseBuildEngine() => _enginePool.Get();
        internal void ReturnBuildEngine(Engine buildEngine) => _enginePool.Return(buildEngine);

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
            {
                // Note how the lambda expression doesn't actually depend on the result (directly)
                //  the continue all makes sure that we're not wasting a thread that blocks on Task.Result
                // GetBuildEnvironment can access the results via the taskMap.
                Plan.Trace.TraceEvent(TraceEventType.Verbose, 0,"{0} is waiting for its dependencies to be built: {1}", 
                    description.Name, description.Dependencies.ToListString());
                return Task.Factory.ContinueWhenAll(description.Dependencies.Select(d => taskMap[d].Value).ToArray(),
                    _ => GetBuildEnvironment(taskMap, description, token));
            }
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
                .ContinueWith(async bet =>
                {
                    try
                    {
                        var instance = new Application(bet.Result.Module);
                        Plan.Trace.TraceEvent(TraceEventType.Verbose, 0,
                            "Linking compile-time dependencies for module {0}.", bet.Result.Module.Name);
                        _linkDependencies(taskMap, instance, targetDescription, token);
                        token.ThrowIfCancellationRequested();
                        var target = await BuildTargetAsync(bet, desc, depMap, token);
                        Plan.Trace.TraceEvent(TraceEventType.Verbose, 0, "Finished BuildTargetAsync({0})", desc.Name);
                        return target;
                    }
                    finally
                    {
                        bet.Dispose();
                    }
                }, token);
            return buildTask.Unwrap();
        }

        protected virtual Task<ITarget> BuildTargetAsync(Task<IBuildEnvironment> buildEnvironment, ITargetDescription description, Dictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            var buildTask = description.BuildAsync(buildEnvironment.Result, dependencies, token);
            Debug.Assert(buildTask != null, "Task for building target is null.",
                $"{description.GetType().Name}.BuildAsync returned null instead of a Task.");
            return buildTask;
        }
    }
}