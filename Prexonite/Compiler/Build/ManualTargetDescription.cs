using System;
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
    internal class ManualTargetDescription : ITargetDescription
    {
        private readonly ModuleName _moduleName;
        private readonly ISource _source;
        /// <summary>
        /// The file name for symbols derived from the supplied reader. Can be null.
        /// </summary>
        private readonly string _fileName;
        private readonly DependencySet _dependencies;

        internal ManualTargetDescription(ModuleName moduleName, ISource source, string fileName, IEnumerable<ModuleName> dependencies)
        {
            if ((object) moduleName == null)
                throw new ArgumentNullException("moduleName");
            if ((object) source == null)
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
            return Task.Factory.StartNew(() =>
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
                                                                string.Format("The source for target {0} could not be opened.", this),
                                                                Enumerable.Empty<Message>());
                            using (reader)
                            {
                                Trace.WriteLine("Building module " + Name);
                                token.ThrowIfCancellationRequested();
                                ldr.LoadFromReader(reader, _fileName);
                            }

                            return _createTargetFromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                        }
                        catch (Exception e)
                        {
                            return DefaultModuleTarget._FromLoader(ldr,
                                                                   _createAggregateException(aggregateExceptions.Append(e).ToArray()),
                                                                   aggregateMessages);
                        }
                    }
                    else
                    {
                        return _createTargetFromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                    }
                },token);
        }

        private static ITarget _createTargetFromLoader(Loader ldr, Exception[] aggregateExceptions, IEnumerable<Message> aggregateMessages)
        {
            return DefaultModuleTarget._FromLoader(
                ldr, 
                _createAggregateException(aggregateExceptions),
                aggregateMessages);
        }

        private static Exception _createAggregateException(Exception[] aggregateExceptions)
        {
            Exception aggregateException;
            if (aggregateExceptions.Length == 1)
                aggregateException = aggregateExceptions[0];
            else if (aggregateExceptions.Length > 0)
                aggregateException = new AggregateException(aggregateExceptions);
            else
                aggregateException = null;
            return aggregateException;
        }
    }
}
