

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
        this.resources = [];
        if (resources != null)
            this.resources.AddRange(resources);

        Exception = exception;

        if (messages != null)
            this.messages = [..messages];

        if (buildMessages != null)
        {
            this.buildMessages = new List<Message>(buildMessages);

            if (this.messages == null)
                this.messages = [..this.buildMessages];
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