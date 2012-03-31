using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Prexonite.Modular
{
    public interface IResourceDescriptor
    {
        Stream Open();
        void Extract(string destinationPath);
    }
}
