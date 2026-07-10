using System.Text;
using Prexonite.Properties;

namespace Prexonite.Compiler.Build.Internal;

class StreamSource : ISource, IDisposable
{
    readonly Encoding _encoding;
    readonly bool _forceSingleUse;
    Stream? _stream;

    public StreamSource(Stream stream, Encoding encoding, bool forceSingleUse)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead)
            throw new ArgumentException(
                Resources.Exception_StreamSource_CannotUseWriteOnlyStream,
                nameof(stream)
            );

        _stream = stream;
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _forceSingleUse = forceSingleUse;
    }

    #region Implementation of ISource

    public bool CanOpen
    {
        get
        {
            var stream = _stream;
            return stream != null && stream.CanRead;
        }
    }

    public bool IsSingleUse => _forceSingleUse || _stream == null || !_stream.CanSeek;

    public bool TryOpen([NotNullWhen(true)] out TextReader? reader)
    {
        object? streamObject = _stream;
        if (streamObject == null)
        {
            reader = null;
            return false;
        }
        else
        {
            lock (streamObject)
            {
                if (_stream == null)
                {
                    reader = null;
                    return false;
                }
                else
                {
                    reader = new StreamReader(_stream, _encoding);
                    if (IsSingleUse)
                        _stream = null;
                    return true;
                }
            }
        }
    }

    #endregion

    #region Implementation of IDisposable

    public void Dispose()
    {
        var d = _stream;
        if (d != null)
        {
            lock (d)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }
        }
    }

    #endregion
}
