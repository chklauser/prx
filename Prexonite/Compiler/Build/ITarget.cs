using System.Collections.Generic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public interface ITarget
    {
        Module Module
        {
            get;
        }

        ICollection<Prexonite.Modular.IResourceDescriptor> Resources
        {
            get;
        }

        SymbolStore SymbolStore
        {
            get;
        }

        ModuleName Name
        {
            get;
        }
    }
}
