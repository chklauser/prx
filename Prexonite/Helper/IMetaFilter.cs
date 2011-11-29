using System.Collections.Generic;

namespace Prexonite
{
    /// <summary>
    ///     Defines transformations of get and set requests to the associated meta table.
    /// </summary>
    public interface IMetaFilter
    {
        string GetTransform(string key);
        KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item);
    }
}