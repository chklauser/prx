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

using System.Collections.Immutable;
using System.Diagnostics;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

[DebuggerDisplay("{" + nameof(debuggerDisplay) + "}")]
public class ProvidedTarget : ITargetDescription, ITarget
{
    readonly DependencySet dependencies;

    readonly List<IResourceDescriptor>? resources;

    readonly List<Message>? messages;

    readonly IReadOnlyList<Message>? buildMessages;

    string debuggerDisplay =>
        $"ProvidedTarget({Name}) {(IsSuccessful ? "successful" : "errors detected")}";

    public ProvidedTarget(Module module,
        IEnumerable<ModuleName>? dependencies = null,
        IEnumerable<KeyValuePair<string, Symbol>>? symbols = null,
        IEnumerable<IResourceDescriptor>? resources = null,
        IEnumerable<Message>? messages = null,
        IEnumerable<Message>? buildMessages = null,
        Exception? exception = null)
    {
        Module = module;
        this.dependencies = new(module.Name);
        if (dependencies != null)
            this.dependencies.AddRange(dependencies);
        Symbols = SymbolStore.Create();
        if (symbols != null)
            foreach (var entry in symbols)
                Symbols.Declare(entry.Key, entry.Value);
        this.resources = new();
        if (resources != null)
            this.resources.AddRange(resources);

        Exception = exception;

        if (messages != null)
            this.messages = new(messages);

        if (buildMessages != null)
        {
            this.buildMessages = new List<Message>(buildMessages);

            if (this.messages == null)
                this.messages = new(this.buildMessages);
            else
                this.messages.AddRange(this.buildMessages);
        }

        IsSuccessful = exception == null &&
            (this.messages == null || this.messages.All(m => m.Severity != MessageSeverity.Error));
    }

    public ProvidedTarget(ITargetDescription description, ITarget result)
        : this(result.Module, description.Dependencies, result.Symbols, result.Resources, result.Messages, description.BuildMessages, result.Exception)
    {
    }

    #region Implementation of ITargetDescription

    public IReadOnlyCollection<ModuleName> Dependencies => dependencies;

    public Module Module { get; }

    public IReadOnlyCollection<IResourceDescriptor> Resources => resources ??
        (IReadOnlyCollection<IResourceDescriptor>)ImmutableArray<IResourceDescriptor>.Empty;

    public SymbolStore Symbols { get; }

    public ModuleName Name => Module.Name;

    public IReadOnlyList<Message> BuildMessages => buildMessages ?? DefaultModuleTarget.NoMessages;

    public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<ITarget>();
        tcs.SetResult(this);
        Plan.Trace.TraceEvent(TraceEventType.Information, 0, "Used provided target {0}.", this);
        return tcs.Task;
    }

    #region Implementation of ITarget

    public IReadOnlyCollection<Message> Messages => (IReadOnlyCollection<Message>?)messages ?? DefaultModuleTarget.NoMessages;

    public Exception? Exception { get; }

    public bool IsSuccessful { get; }

    #endregion

    public override string ToString()
    {
        return debuggerDisplay;
    }

    #endregion
}