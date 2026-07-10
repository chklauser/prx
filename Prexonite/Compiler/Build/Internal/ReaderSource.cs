namespace Prexonite.Compiler.Build.Internal;

class ReaderSource : ISource, IDisposable
{
    TextReader? _reader;

    public ReaderSource(TextReader? reader)
    {
        _reader = reader;
    }

    #region Implementation of ISource

    public bool TryOpen([NotNullWhen(true)] out TextReader? reader)
    {
        var r = _reader;
        if (r == null)
        {
            reader = null;
            return false;
        }
        else if (Interlocked.CompareExchange(ref _reader, null, r) == r)
        {
            reader = r;
            return true;
        }
        else
        {
            reader = null;
            return false;
        }
    }

    #endregion

    #region Implementation of IDisposable

    public void Dispose()
    {
        if (TryOpen(out _reader))
            _reader.Dispose();
    }

    #endregion

    #region Implementation of ISource

    public bool CanOpen => _reader != null;

    public bool IsSingleUse => true;

    #endregion
}
