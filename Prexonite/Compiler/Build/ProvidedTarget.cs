using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    [DebuggerDisplay("{_debuggerDisplay}")]
    public class ProvidedTarget : ITargetDescription, ITarget
    {
        [NotNull]
        private readonly DependencySet _dependencies;

        [NotNull]
        private readonly Module _module;

        [CanBeNull]
        private readonly SymbolStore _symbols;

        [CanBeNull]
        private readonly List<IResourceDescriptor> _resources;

        [CanBeNull]
        private readonly List<Message> _messages;

        [CanBeNull]
        private readonly IReadOnlyList<Message> _buildMessages;

        [CanBeNull]
        private readonly Exception _exception;

        private readonly bool _isSuccessful;

        private string _debuggerDisplay
        {
            get { return String.Format("ProvidedTarget({0}) {1}", Name, IsSuccessful ? "successful" : "errors detected"); }
        }

        public ProvidedTarget(Module module,
            IEnumerable<ModuleName> dependencies = null,
            IEnumerable<KeyValuePair<string, Symbol>> symbols = null,
            IEnumerable<IResourceDescriptor> resources = null,
            IEnumerable<Message> messages = null,
            IEnumerable<Message> buildMessages = null,
            Exception exception = null)
        {
            _module = module;
            _dependencies = new DependencySet(module.Name);
            if (dependencies != null)
                _dependencies.AddRange(dependencies);
            _symbols = SymbolStore.Create();
            if (symbols != null)
                foreach (var entry in symbols)
                    _symbols.Declare(entry.Key, entry.Value);
            _resources = new List<IResourceDescriptor>();
            if (resources != null)
                _resources.AddRange(resources);

            _exception = exception;

            if (messages != null)
                _messages = new List<Message>(messages);

            if (buildMessages != null)
            {
                _buildMessages = new List<Message>(buildMessages);

                if (_messages == null)
                    _messages = new List<Message>(_buildMessages);
                else
                    _messages.AddRange(_buildMessages);
                
            }

            _isSuccessful = exception == null &&
                            (_messages == null || _messages.All(m => m.Severity != MessageSeverity.Error));
        }

        public ProvidedTarget(ITargetDescription description, ITarget result)
            : this(result.Module, description.Dependencies, result.Symbols, result.Resources, result.Messages, description.BuildMessages, result.Exception)
        {
        }

        #region Implementation of ITargetDescription

        public IReadOnlyCollection<ModuleName> Dependencies
        {
            get { return _dependencies; }
        }

        [NotNull]
        public Module Module
        {
            get { return _module; }
        }

        [CanBeNull]
        public IReadOnlyCollection<IResourceDescriptor> Resources
        {
            get { return _resources; }
        }

        [CanBeNull]
        public SymbolStore Symbols
        {
            get { return _symbols; }
        }

        public ModuleName Name
        {
            get { return _module.Name; }
        }

        public IReadOnlyList<Message> BuildMessages
        {
            get { return _buildMessages ?? DefaultModuleTarget.NoMessages; }
        }

        [NotNull]
        public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<ITarget>();
            tcs.SetResult(this);
            Plan.Trace.TraceEvent(TraceEventType.Information, 0, "Used provided target {0}.", this);
            return tcs.Task;
        }

        #region Implementation of ITarget

        [NotNull]
        public IReadOnlyList<Message> Messages
        {
            get { return (IReadOnlyList<Message>)_messages ?? DefaultModuleTarget.NoMessages; }
        }

        [CanBeNull]
        public Exception Exception
        {
            get { return _exception; }
        }

        public bool IsSuccessful
        {
            get { return _isSuccessful; }
        }

        #endregion

        public override string ToString()
        {
            return _debuggerDisplay;
        }

        #endregion
    }
}
