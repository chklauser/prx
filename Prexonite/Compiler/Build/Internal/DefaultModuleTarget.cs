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
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    [DebuggerDisplay("Target({Name}) success={IsSuccessful}")]
    internal class DefaultModuleTarget : ITarget
    {
        [NotNull]
        private static readonly IReadOnlyCollection<IResourceDescriptor> _emptyResourceCollection =
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
                throw new ArgumentNullException(nameof(module));
            if (symbols == null)
                throw new ArgumentNullException(nameof(symbols));
            _module = module;
            _symbols = symbols;
            _exception = exception;
            if(messages != null)
                _messages = new List<Message>(messages);
            _isSuccessful = exception == null && (_messages == null || _messages.All(m => m.Severity != MessageSeverity.Error));
        }

        [CanBeNull]
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

        internal static ITarget _FromLoader(Loader loader, Exception[] exceptions = null, IEnumerable<Message> additionalMessages = null)
        {
            return _FromLoader(loader, 
                exceptions == null ? null : _createAggregateException(exceptions),
                additionalMessages);
        }

        internal static ITarget _FromLoader(Loader loader, Exception exception = null, IEnumerable<Message> additionalMessages = null)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader));
            Debug.Assert(loader.TopLevelSymbols != null,"Loader.TopLevelSymbols must not be null.");
            var exported = SymbolStore.Create();
            foreach (var (id, symbol) in loader.TopLevelSymbols.LocalDeclarations)
                exported.Declare(id, symbol);
            var messages = loader.Errors.Append(loader.Warnings).Append(loader.Infos);
            if (additionalMessages != null)
                messages = additionalMessages.Append(messages);
            return new DefaultModuleTarget(loader.ParentApplication.Module,exported,messages, exception);
        }

        public Module Module
        {
            get { return _module; }
        }

        public IReadOnlyCollection<IResourceDescriptor> Resources
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
        public IReadOnlyCollection<Message> Messages
        {
            get { return (IReadOnlyCollection<Message>)_messages ?? NoMessages; }
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
