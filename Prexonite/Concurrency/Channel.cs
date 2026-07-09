

using System.Diagnostics;

namespace Prexonite.Concurrency;

public sealed class Channel : IObject, IDisposable
{
    #region Implementation of IObject

    public bool TryDynamicCall(
        StackContext sctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)]
        out PValue? result
    )
    {
        result = null;
        switch (id.ToUpperInvariant())
        {
            case "RECEIVE":
                result = Receive();
                break;
            case "SEND":
                Send((args.Length > 0 ? args[0] : null) ?? PType.Null);
                result = PType.Null;
                break;
            case "TOSTRING":
                result = ToString() ?? nameof(Channel);
                break;
            case "TRYRECEIVE":
                var refVar = (args.Length > 0 ? args[0] : null) ?? PType.Null;
                if (TryReceive(out var datum))
                {
                    refVar.IndirectCall(sctx, datum);
                    result = true;
                }
                else
                {
                    refVar.IndirectCall(sctx, PType.Null);
                    result = false;
                }
                break;
            default:
                return false;
        }

        return true;
    }

    #endregion

    #region State

    PValue? _datum;

    #endregion

    #region Synchornization

    readonly object _syncRoot = new();
    readonly ManualResetEvent _dataAvailable = new(false);
    readonly ManualResetEvent _channelEmpty = new(true);

    public WaitHandle DataAvailable
    {
        [DebuggerStepThrough]
        get => _dataAvailable;
    }

    #endregion

    public void Send(PValue obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        lock (_syncRoot)
        {
            while (_datum != null)
            {
                try
                {
                    Monitor.Exit(_syncRoot);
                    _channelEmpty.WaitOne(Timeout.Infinite, true);
                    //Wait for the channelEmpty event (lock released)
                }
                finally
                {
                    Monitor.Enter(_syncRoot);
                }
            }

            //We have the lock and _datum == null
            //  -> Update state and signals
            _datum = obj;
            _channelEmpty.Reset();
            _dataAvailable.Set();
        }
    }

    public PValue Receive()
    {
        lock (_syncRoot)
        {
            while (_datum == null)
            {
                try
                {
                    Monitor.Exit(_syncRoot);
                    _dataAvailable.WaitOne(Timeout.Infinite, true);
                    //Wait for the dataAvailable event (lock released)
                }
                finally
                {
                    Monitor.Enter(_syncRoot);
                }
            }

            //We have the lock and _datum != null
            var d = _datum;
            _datum = null;
            _dataAvailable.Reset();
            _channelEmpty.Set();
            return d;
        }
    }

    public bool TryReceive([NotNullWhen(true)] out PValue? datum)
    {
        lock (_syncRoot)
        {
            if (_datum == null)
            {
                datum = null;
                return false;
            }
            else
            {
                datum = Receive();
                return true;
            }
        }
    }

    #region Implementation of IDisposable

    bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;
        _channelEmpty.Dispose();
        _dataAvailable.Dispose();
        _disposed = true;
    }

    #endregion
}