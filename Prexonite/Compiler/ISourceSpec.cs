#nullable enable
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Build;

namespace Prexonite.Compiler
{
    /// <summary>
    /// A retro-fitted precursor to <see cref="ISource"/> (<see cref="Source"/>). It is used to support including
    /// Prexonite Script files embedded as resources. Modern code should prefer using modules and the build system
    /// (with one module per embedded resource). 
    /// </summary>
    /// <seealso cref="Plan"/>
    public interface ISourceSpec
    {
            public string FullName { get; }
            public Stream OpenStream();
            public string? LoadPath { get; }

            public ISource ToSource();
    }

    public sealed record FileSpec(FileInfo File) : ISourceSpec
    {
        public string FullName => File.FullName;
        public string? LoadPath => File.DirectoryName;

        public Stream OpenStream() => new FileStream(
            File.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4 * 1024,
            FileOptions.SequentialScan);

        public ISource ToSource() => Source.FromFile(File, Encoding.UTF8);
    }
    
    public sealed record ResourceSpec(Assembly ResourceAssembly, string Name) : ISourceSpec
    {
        public string FullName => $"resource:{ResourceAssembly.FullName ?? "<unknown assembly>"}/{Name}";
        public Stream OpenStream() => ResourceAssembly.GetManifestResourceStream(Name) 
            ?? throw new InvalidOperationException($"Assembly {ResourceAssembly.FullName} does not " +
                                                   $"contain a resource '{Name}' (or it is not accessible).");

        public string? LoadPath => null;

        public ISource ToSource() => Source.FromStream(OpenStream(), Encoding.UTF8);
    }
}