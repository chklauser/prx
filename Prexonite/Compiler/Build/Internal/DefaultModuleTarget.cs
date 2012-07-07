using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    [DebuggerDisplay("Target({Name}) success={IsSuccessful}")]
    internal class DefaultModuleTarget : ITarget
    {
        [NotNull]
        private static readonly ICollection<IResourceDescriptor> _emptyResourceCollection =
            new IResourceDescriptor[0];

        [NotNull]
        private readonly Module _module;
        [NotNull]
        private readonly SymbolStore _symbols;
        [CanBeNull]
        private readonly List<Message> _messages;
        [CanBeNull]
        private readonly Exception _exception;

        private readonly bool _isSuccessful;

        public DefaultModuleTarget(Module module, SymbolStore symbols, IEnumerable<Message> messages = null, Exception exception = null)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (symbols == null)
                throw new ArgumentNullException("symbols");
            _module = module;
            _symbols = symbols;
            _exception = exception;
            if(messages != null)
                _messages = new List<Message>(messages);
            _isSuccessful = exception == null && (_messages == null || _messages.All(m => m.Severity != MessageSeverity.Error));
        }

        internal static ITarget _FromLoader(Loader loader, Exception exception = null, IEnumerable<Message> additionalMessages = null)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");
            Debug.Assert(loader.Symbols != null,"Loader.Symbols must not be null.");
            var exported = SymbolStore.Create();
            foreach (var decl in loader.Symbols.LocalDeclarations)
                exported.Declare(decl.Key, decl.Value);
            var messages = loader.Errors.Append(loader.Warnings).Append(loader.Infos);
            if (additionalMessages != null)
                messages = additionalMessages.Append(messages);
            return new DefaultModuleTarget(loader.ParentApplication.Module,exported,messages, exception);
        }

        public Module Module
        {
            get { return _module; }
        }

        public ICollection<IResourceDescriptor> Resources
        {
            get { return _emptyResourceCollection; }
        }

        public SymbolStore Symbols
        {
            get { return _symbols; }
        }

        public ModuleName Name
        {
            get { return _module.Name; }
        }

        internal static readonly Message[] NoMessages = new Message[0];

        [NotNull]
        public ICollection<Message> Messages
        {
            get { return (ICollection<Message>)_messages ?? NoMessages;}
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool IsSuccessful
        {
            get { return _isSuccessful; }
        }
    }
}
