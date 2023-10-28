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

namespace Prexonite.Compiler.Build.Internal;

[DebuggerDisplay("Target({Name}) success={IsSuccessful}")]
class DefaultModuleTarget : ITarget
{
    [NotNull]
    static readonly IReadOnlyCollection<IResourceDescriptor> _emptyResourceCollection =
        Array.Empty<IResourceDescriptor>();

    [CanBeNull]
    readonly List<Message> _messages;

    public DefaultModuleTarget(Module module, SymbolStore symbols, IEnumerable<Message> messages = null, Exception exception = null)
    {
        Module = module ?? throw new ArgumentNullException(nameof(module));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Exception = exception;
        if(messages != null)
            _messages = new List<Message>(messages);
        IsSuccessful = exception == null && (_messages == null || _messages.All(m => m.Severity != MessageSeverity.Error));
    }

    [CanBeNull]
    static Exception _createAggregateException(Exception[] aggregateExceptions)
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

    [NotNull]
    public Module Module { get; }

    public IReadOnlyCollection<IResourceDescriptor> Resources => _emptyResourceCollection;

    [NotNull]
    public SymbolStore Symbols { get; }

    public ModuleName Name => Module.Name;

    internal static readonly Message[] NoMessages = Array.Empty<Message>();

    [NotNull]
    public IReadOnlyCollection<Message> Messages => (IReadOnlyCollection<Message>)_messages ?? NoMessages;

    [CanBeNull]
    public Exception Exception { get; }

    public bool IsSuccessful { get; }
}