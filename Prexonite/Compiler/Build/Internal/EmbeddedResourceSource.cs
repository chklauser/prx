using System;
using System.IO;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Build.Internal
{
    public class EmbeddedResourceSource : ISource
    {
        [NotNull] private readonly Assembly _assembly;
        [NotNull] private readonly string _name;
        [NotNull] private readonly Encoding _encoding;

        public EmbeddedResourceSource([NotNull] Assembly assembly, [NotNull] string name, [CanBeNull] Encoding encoding = null)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _encoding = encoding ?? Encoding.UTF8;
        }

        public bool CanOpen => true;
        public bool IsSingleUse => false;
        public bool TryOpen(out TextReader reader)
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
}