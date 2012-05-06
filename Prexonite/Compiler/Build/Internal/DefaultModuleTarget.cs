using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    internal class DefaultModuleTarget : ITarget
    {
        private static readonly ICollection<IResourceDescriptor> EmptyResourceCollection =
            new List<IResourceDescriptor>(0);

        private readonly Module _module;
        private readonly SymbolStore _symbols;

        public DefaultModuleTarget(Module module, SymbolStore symbols)
        {
            if ((object) module == null)
                throw new ArgumentNullException("module");
            if ((object) symbols == null)
                throw new ArgumentNullException("symbols");
            _module = module;
            _symbols = symbols;
        }

        internal static ITarget FromLoader(Loader loader)
        {
            //TODO: extract symbol store from loader
            return new DefaultModuleTarget(loader.ParentApplication.Module,null);
        }

        public Module Module
        {
            get { return _module; }
        }

        public ICollection<IResourceDescriptor> Resources
        {
            get { return EmptyResourceCollection; }
        }

        public SymbolStore Symbols
        {
            get { return _symbols; }
        }

        public ModuleName Name
        {
            get { return _module.Name; }
        }
    }
}
