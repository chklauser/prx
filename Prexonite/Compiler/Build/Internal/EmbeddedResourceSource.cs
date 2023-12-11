using System.Reflection;
using System.Text;

namespace Prexonite.Compiler.Build.Internal;

public class EmbeddedResourceSource : ISource
{
    readonly Assembly _assembly;
    readonly string _name;
    readonly Encoding _encoding;

    public EmbeddedResourceSource(Assembly assembly, string name, Encoding? encoding = null)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _encoding = encoding ?? Encoding.UTF8;
    }

    public bool CanOpen => true;
    public bool IsSingleUse => false;
    public bool TryOpen([NotNullWhen(true)] out TextReader? reader)
    {
        var stream = _assembly.GetManifestResourceStream(_name);
        if(stream == null)
        {
            reader = null;
            return false;
        }

        reader = new StreamReader(stream, _encoding, false, 4096, false);
        return true;
    }
}