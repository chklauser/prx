using System;

namespace Prexonite.Compiler.Cil
{

    /// <summary>
    /// A cil hint. Can be serialized to a meta entry for storage.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cil")]
    public interface ICilHint
    {

        /// <summary>
        /// The key under which this CIL hint is stored. This key is used to deserialize the hint into the correct format.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cil")]
        string CilKey { get; }

        /// <summary>
        /// Get the list of fields to be serialized. Does not include the key.
        /// </summary>
        /// <returns>The list of fields to be serialized.</returns>
        MetaEntry[] GetFields();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cil")]
    public static class CilHint
    {
        /// <summary>
        /// Converts the supplied CIL hint to a meta entry (including the CIL hint key). 
        /// </summary>
        /// <param name="hint">The CIL hint to serialize</param>
        /// <returns>The serialized cil hint.</returns>
        public static MetaEntry ToMetaEntry(this ICilHint hint)
        {
            var key = hint.CilKey;
            var fields = hint.GetFields();

            var entry = new MetaEntry[fields.Length + 1];
            entry[0] = key;
            Array.Copy(fields, 0, entry,1, fields.Length);

            return (MetaEntry) entry;
        }
    }
}