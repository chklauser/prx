using System;
using System.Collections.Generic;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    internal class DefaultModuleTarget : ITarget
    {
        private static readonly ICollection<IResourceDescriptor> _emptyResourceCollection =
            new IResourceDescriptor[0];

        private readonly Module _module;
        private readonly SymbolStore _symbols;

        public DefaultModuleTarget(Module module, SymbolStore symbols)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (symbols == null)
                throw new ArgumentNullException("symbols");
            _module = module;
            _symbols = symbols;
        }

        internal static ITarget _FromLoader(Loader loader)
        {
            var exported = SymbolStore.Create();
            foreach (var decl in loader.Symbols.LocalDeclarations)
                exported.Declare(decl.Key, decl.Value);
            return new DefaultModuleTarget(loader.ParentApplication.Module,exported);
        }

        public Module Module
        {
            get { return _module; }
        }

        public ICollection<IResourceDescriptor> Resources
        {
            get { return _emptyResourceCollection; }
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
