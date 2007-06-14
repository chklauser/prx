using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite
{
    public interface IStackAware
    {
        StackContext CreateStackContext(Engine eng, PValue[] args);
    }
}
