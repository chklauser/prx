using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prexonite.Modular
{
    public interface IResourceDescriptor
    {
        Stream Open();
        Task ExtractAsync(string destinationPath);
        void Extract(string destinationPath);
    }
}
