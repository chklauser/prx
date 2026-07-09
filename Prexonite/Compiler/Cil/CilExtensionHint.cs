

using System.Diagnostics;

namespace Prexonite.Compiler.Cil;

/// <summary>
///     Wraps a cil extension hint. Should replace existing hints of the same kind.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = nameof(Cil))]
public class CilExtensionHint : ICilHint
{
    /// <summary>
    ///     The CIL hint key for CIL extensions.
    /// </summary>
    public const string Key = "ext";

    /// <summary>
    ///     The offsets at which CIL extension code begins.
    /// </summary>
    public IList<int> Offsets { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Creates a new CIL extension hint.
    /// </summary>
    /// <param name = "offsets">The offsets at which CIL extension code begins</param>
    public CilExtensionHint(IList<int> offsets)
    {
        Offsets = offsets ?? throw new ArgumentNullException(nameof(offsets));
    }

    #region Implementation of ICilHint

    /// <summary>
    ///     The key under which this CIL hint is stored. This key is used to deserialize the hint into the correct format.
    /// </summary>
    public string CilKey => Key;

    /// <summary>
    ///     Get the list of fields to be serialized. Does not include the key.
    /// </summary>
    /// <returns>The list of fields to be serialized.</returns>
    public MetaEntry[] GetFields()
    {
        return (from address in Offsets
            select (MetaEntry) address.ToString()).ToArray();
    }

    #endregion

    /// <summary>
    ///     Parses CIL extension hint from a meta entry.
    /// </summary>
    /// <param name = "hint"></param>
    /// <returns></returns>
    public static CilExtensionHint FromMetaEntry(MetaEntry[] hint)
    {
        var offsets = new List<int>(hint.Length);
        foreach (var metaEntry in hint.Skip(1))
        {
            if (metaEntry == null)
                continue;
            if (int.TryParse(metaEntry.Text, out var offset))
                offsets.Add(offset);
        }

        return new(offsets);
    }
}