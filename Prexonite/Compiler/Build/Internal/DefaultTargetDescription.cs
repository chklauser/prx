using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    internal class DefaultTargetDescription : ITargetDescription
    {
        private readonly ModuleName _moduleName;
        private readonly TextReader _reader;
        /// <summary>
        /// The file name for symbols derived from the supplied reader. Can be null.
        /// </summary>
        private readonly string _fileName;
        private readonly DependencySet _dependencies;

        internal DefaultTargetDescription(ModuleName moduleName, TextReader reader, string fileName, IEnumerable<ModuleName> dependencies)
        {
            if ((object) moduleName == null)
                throw new ArgumentNullException("moduleName");
            if ((object) reader == null)
                throw new ArgumentNullException("reader");
            if ((object) dependencies == null)
                throw new ArgumentNullException("dependencies");
            _moduleName = moduleName;
            _reader = reader;
            _fileName = fileName;
            _dependencies = new DependencySet(moduleName);
        }

        public ISet<ModuleName> Dependencies
        {
            get { return _dependencies; }
        }

        public ModuleName Name
        {
            get { return _moduleName; }
        }

        public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            return new Task<ITarget>(() =>
                {
                    var ldr = build.CreateLoader(new LoaderOptions(null, null));
                    ldr.LoadFromReader(_reader, _fileName);
                    if (ldr.ErrorCount > 0)
                        throw new BuildFailureException(this, "There were {0} {1} while translating " + Name + ".",
                                                        ldr.Errors.Append(ldr.Warnings).Append(ldr.Infos));
                    return DefaultModuleTarget.FromLoader(ldr);
                });
        }
    }
}
