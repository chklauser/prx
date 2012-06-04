using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public class ProvidedTarget : ITargetDescription, ITarget
    {
        private readonly DependencySet _dependencies;
        private readonly Module _module;
        private readonly SymbolStore _symbols;
        private readonly List<IResourceDescriptor> _resources;

        public ProvidedTarget(Module module, 
            IEnumerable<ModuleName> dependencies = null, 
            IEnumerable<KeyValuePair<string,Symbol>> symbols = null, 
            IEnumerable<IResourceDescriptor> resources = null)
        {
            _module = module;
            _dependencies = new DependencySet(module.Name);
            if(dependencies != null)
                _dependencies.AddRange(dependencies);
            _symbols = SymbolStore.Create();
            if(symbols != null)
                foreach (var entry in symbols)
                    _symbols.Declare(entry.Key, entry.Value);
            _resources = new List<IResourceDescriptor>();
            if(resources != null)
                _resources.AddRange(resources);
        }

        public ProvidedTarget(ITargetDescription description, ITarget result)
            : this(result.Module,description.Dependencies,result.Symbols,result.Resources)
        {
        }

        #region Implementation of ITargetDescription

        public ISet<ModuleName> Dependencies
        {
            get { return _dependencies; }
        }

        public Module Module
        {
            get { return _module; }
        }

        public ICollection<IResourceDescriptor> Resources
        {
            get { return _resources; }
        }

        public SymbolStore Symbols
        {
            get { return _symbols; }
        }

        public ModuleName Name
        {
            get { return _module.Name; }
        }

        public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<ITarget>();
            tcs.SetResult(this);
            return tcs.Task;
        }

        #endregion
    }
}
