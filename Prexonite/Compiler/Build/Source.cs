

using System.Reflection;
using System.Text;
using Prexonite.Compiler.Build.Internal;

namespace Prexonite.Compiler.Build;

public static class Source
{
    public static ISource FromReader(TextReader reader)
    {
        return new ReaderSource(reader);
    }

    public static ISource FromString(string source)
    {
        return new StringSource(source);
    }

    public static ISource FromStream(Stream stream, Encoding encoding)
    {
        return FromStream(stream, encoding, true);
    }

    public static ISource FromStream(Stream stream, Encoding encoding, bool forceSingleUse)
    {
        return new StreamSource(stream, encoding, forceSingleUse);
    }

    public static ISource FromFile(FileInfo file, Encoding encoding)
    {
        return new FileSource(file, encoding);
    }

    public static ISource FromFile(string path, Encoding encoding)
    {
        return FromFile(new FileInfo(path), encoding);
    }

    public static ISource FromBytes(byte[] data, Encoding encoding)
    {
        return FromStream(new MemoryStream(data, false), encoding, false);
    }

    public static ISource FromEmbeddedPrexoniteResource(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new EmbeddedResourceSource(Assembly.GetExecutingAssembly(), "Prexonite." + name);
    }
    
    public static ISource FromEmbeddedResource(Assembly assembly, string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new EmbeddedResourceSource(assembly, name);
    }

    extension(ISource source)
    {
        public async Task<ISource> CacheInMemoryAsync()
        {
            if(!source.TryOpen(out var reader))
                throw new InvalidOperationException("Unable to open source " + source + " for reading.");
            var contents = await reader.ReadToEndAsync();
            return FromString(contents);
        }
    }
}