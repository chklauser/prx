namespace Prexonite;

/// <summary>
///     Integrates suspendable .NET managed code into the Prexonite stack via the IEnumerator interface.
/// </summary>
[SuppressMessage(
    "Microsoft.Naming",
    "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = nameof(Coroutine)
)]
public sealed class CoroutineContext : StackContext, IDisposable
{
    public override string ToString()
    {
        return $"Managed Coroutine({_coroutine})";
    }

    public CoroutineContext(StackContext sctx, IEnumerator<PValue> coroutine)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

        ParentEngine = sctx.ParentEngine;
        ParentApplication = sctx.ParentApplication;
        ImportedNamespaces = sctx.ImportedNamespaces;
    }

    public CoroutineContext(StackContext sctx, IEnumerable<PValue> coroutine)
        : this(sctx, coroutine.GetEnumerator()) { }

    readonly IEnumerator<PValue> _coroutine;

    PValue _returnValue = PType.Null;

    /// <summary>
    ///     Represents the engine this context is part of.
    /// </summary>
    public override Engine ParentEngine { get; }

    /// <summary>
    ///     The parent application.
    /// </summary>
    public override Application ParentApplication { get; }

    public override SymbolCollection ImportedNamespaces { get; }

    /// <summary>
    ///     Indicates whether the context still has code/work to do.
    /// </summary>
    /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
    protected override bool PerformNextCycle(StackContext? lastContext)
    {
        var moved = _coroutine.MoveNext();
        if (moved)
        {
            _returnValue = _coroutine.Current;
            ReturnMode = ReturnMode.Continue;
        }
        else
        {
            ReturnMode = ReturnMode.Break;
        }
        return false; //remove the context from the stack (for now)
    }

    /// <summary>
    ///     Tries to handle the supplied exception.
    /// </summary>
    /// <param name = "exc">The exception to be handled.</param>
    /// <returns>True if the exception has been handled, false otherwise.</returns>
    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    /// <summary>
    ///     Represents the return value of the context.
    ///     Just providing a value here does not mean that it gets consumed by the caller.
    ///     If the context does not provide a return value, this property should return null (not NullPType).
    /// </summary>
    public override PValue ReturnValue => _returnValue;

    #region IDisposable

    bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _coroutine.Dispose();
            _disposed = true;
        }
    }

    #endregion
}
