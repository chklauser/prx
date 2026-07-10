using System.Diagnostics;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal;

[DebuggerDisplay("Target({Name}) success={IsSuccessful}")]
class DefaultModuleTarget : ITarget
{
    static readonly IReadOnlyCollection<IResourceDescriptor> _emptyResourceCollection = [];

    readonly List<Message>? _messages;

    public DefaultModuleTarget(
        Module module,
        SymbolStore symbols,
        IEnumerable<Message>? messages = null,
        Exception? exception = null
    )
    {
        Module = module ?? throw new ArgumentNullException(nameof(module));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Exception = exception;
        if (messages != null)
            _messages = [.. messages];
        IsSuccessful =
            exception == null
            && (_messages == null || _messages.All(m => m.Severity != MessageSeverity.Error));
    }

    static Exception? _createAggregateException(Exception[] aggregateExceptions)
    {
        Exception? aggregateException;
        if (aggregateExceptions.Length == 1)
            aggregateException = aggregateExceptions[0];
        else if (aggregateExceptions.Length > 0)
            aggregateException = new AggregateException(aggregateExceptions);
        else
            aggregateException = null;
        return aggregateException;
    }

    internal static ITarget _FromLoader(
        Loader loader,
        Exception[]? exceptions = null,
        IEnumerable<Message>? additionalMessages = null
    )
    {
        return _FromLoader(
            loader,
            exceptions == null ? null : _createAggregateException(exceptions),
            additionalMessages
        );
    }

    internal static ITarget _FromLoader(
        Loader loader,
        Exception? exception = null,
        IEnumerable<Message>? additionalMessages = null
    )
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));
        Debug.Assert(loader.TopLevelSymbols != null, "Loader.TopLevelSymbols must not be null.");
        var exported = SymbolStore.Create();
        foreach (var (id, symbol) in loader.TopLevelSymbols.LocalDeclarations)
            exported.Declare(id, symbol);
        var messages = loader.Errors.Append(loader.Warnings).Append(loader.Infos);
        if (additionalMessages != null)
            messages = additionalMessages.Append(messages);
        return new DefaultModuleTarget(
            loader.ParentApplication.Module,
            exported,
            messages,
            exception
        );
    }

    public Module Module { get; }

    public IReadOnlyCollection<IResourceDescriptor> Resources => _emptyResourceCollection;

    public SymbolStore Symbols { get; }

    public ModuleName Name => Module.Name;

    internal static readonly Message[] NoMessages = [];

    public IReadOnlyCollection<Message> Messages =>
        (IReadOnlyCollection<Message>?)_messages ?? NoMessages;

    public Exception? Exception { get; }

    public bool IsSuccessful { get; }
}
