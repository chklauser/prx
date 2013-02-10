using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    [DebuggerDisplay("ManualTargetDescription({Name} from {_fileName})")]
    internal class ManualTargetDescription : ITargetDescription
    {
        [NotNull]
        private readonly ModuleName _moduleName;
        [NotNull]
        private readonly ISource _source;
        /// <summary>
        /// The file name for symbols derived from the supplied reader. Can be null.
        /// </summary>
        [CanBeNull]
        private readonly string _fileName;
        [NotNull]
        private readonly DependencySet _dependencies;

        internal ManualTargetDescription([NotNull] ModuleName moduleName, [NotNull] ISource source, [CanBeNull] string fileName, [NotNull] IEnumerable<ModuleName> dependencies)
        {
            if (moduleName == null)
                throw new ArgumentNullException("moduleName");
            if (source == null)
                throw new ArgumentNullException("source");
            if (dependencies == null)
                throw new ArgumentNullException("dependencies");
            _moduleName = moduleName;
            _source = source;
            _fileName = fileName;
            _dependencies = new DependencySet(moduleName);
            _dependencies.AddRange(dependencies);
        }

        public ISet<ModuleName> Dependencies
        {
            get { return _dependencies; }
        }

        public ModuleName Name
        {
            get { return _moduleName; }
        }

        public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            return Task.Factory.StartNew(
                () =>
                    {
                        var ldr = build.CreateLoader(new LoaderOptions(null, null));

                        var aggregateMessages = dependencies.Values
                            .SelectMany(t => t.Result.Messages);
                        var aggregateExceptions = dependencies.Values
                            .Select(t => t.Result.Exception)
                            .Where(e => e != null);
                        if (dependencies.Values.All(t => t.Result.IsSuccessful))
                        {
                            try
                            {
                                TextReader reader;
                                if (!_source.TryOpen(out reader))
                                    throw new BuildFailureException(this,
                                        string.Format("The source for target {0} could not be opened.", Name),
                                        Enumerable.Empty<Message>());
                                using (reader)
                                {
                                    token.ThrowIfCancellationRequested();
                                    Plan.Trace.TraceEvent(TraceEventType.Information, 0, "Building {0}.", this);
                                    Trace.CorrelationManager.StartLogicalOperation("Build");
                                    ldr.LoadFromReader(reader, _fileName);
                                    Trace.CorrelationManager.StopLogicalOperation();
                                    Plan.Trace.TraceEvent(TraceEventType.Verbose, 0, "Done with building {0}, wrapping result in target.", this);
                                }

                                // ReSharper disable PossibleMultipleEnumeration
                                return DefaultModuleTarget._FromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                            }
                            catch (Exception e)
                            {
                                return DefaultModuleTarget._FromLoader(ldr,aggregateExceptions.Append(e).ToArray(),
                                    aggregateMessages);
                                // ReSharper restore PossibleMultipleEnumeration
                            }
                        }
                        else
                        {
                            Plan.Trace.TraceEvent(TraceEventType.Error, 0,
                                "Not all dependencies of {0} were built successfully. Waiting for other dependencies to finish and then return a failed target.",
                                this);
                            Task.WaitAll(dependencies.Values.ToArray<Task>());
                            return DefaultModuleTarget._FromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                        }
                    }, token);
        }

        public override string ToString()
        {
            return string.Format("{{{0} located in {1}}}", _moduleName, _fileName);
        }
    }
}
