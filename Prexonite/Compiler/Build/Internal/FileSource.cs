using System.Text;

namespace Prexonite.Compiler.Build.Internal;

public class FileSource : ISource
{
    public FileSource(FileInfo file, Encoding encoding)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    #region Implementation of ISource

    public bool CanOpen => File.Exists;

    public bool IsSingleUse => false;

    public FileInfo File { get; }

    public Encoding Encoding { get; }

    public bool TryOpen([NotNullWhen(true)] out TextReader? reader)
    {
        if (!File.Exists)
        {
            reader = null;
            return false;
        }

        try
        {
            reader = new StreamReader(File.FullName, Encoding);
            return true;
        }
        catch (FileNotFoundException)
        {
            reader = null;
            return false;
        }
    }

    #endregion
}
