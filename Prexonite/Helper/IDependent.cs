using System.Collections.Generic;

namespace Prexonite
{
    public interface IDependent<T> : INamed<T>
    {
        IEnumerable<T> GetDependencies();
    }
}