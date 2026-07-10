using System.Diagnostics;

namespace Prexonite;

/// <summary>
///     Integrates suspendable .NET managed code into the Prexonite stack via the IEnumerator interface.
/// </summary>
public class CooperativeContext : StackContext, IDisposable
{
    public override string ToString()
    {
        return $"Cooperative managed method({_existingMethod})";
    }

    public CooperativeContext(StackContext sctx, Func<Action<PValue>, IEnumerable<bool>> methodCtor)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        _methodCtor = methodCtor ?? throw new ArgumentNullException(nameof(methodCtor));
        ParentEngine = sctx.ParentEngine;
        ParentApplication = sctx.ParentApplication;
        ImportedNamespaces = sctx.ImportedNamespaces;
    }

    IEnumerator<bool> method
    {
        [DebuggerStepThrough]
        get
        {
            if (_existingMethod != null)
            {
                return _existingMethod;
            }
            else if (_methodCtor != null)
            {
                _existingMethod = _methodCtor(v => _returnValue = v).GetEnumerator();
                _methodCtor = null;
                return _existingMethod;
            }
            else
            {
                throw new PrexoniteException("Can only execute the method once.");
            }
        }
    }

    Func<Action<PValue>, IEnumerable<bool>>? _methodCtor;
    IEnumerator<bool>? _existingMethod;

    PValue? _returnValue;

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
        return method.MoveNext() && method.Current;
    }

    /// <summary>
    ///     Tries to handle the supplied exception.
    /// </summary>
    /// <param name = "exc">The exception to be handled.</param>
    /// <returns>True if the exception has been handled, false otherwise.</returns>
    public override bool TryHandleException(Exception exc)
    {
        if (ExceptionHandler != null)
            return ExceptionHandler(exc);
        else
            return false;
    }

    public Func<Exception, bool>? ExceptionHandler { get; set; }

    /// <summary>
    ///     Represents the return value of the context.
    ///     Just providing a value here does not mean that it gets consumed by the caller.
    ///     If the context does not provide a return value, this property should return null (not NullPType).
    /// </summary>
    public override PValue ReturnValue => _returnValue ?? PType.Null.CreatePValue();

    #region IDisposable

    bool disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _dispose(true);
    }

    void _dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _existingMethod?.Dispose();
            }
        }
        disposed = true;
    }

    ~CooperativeContext()
    {
        _dispose(false);
    }

    #endregion
}
