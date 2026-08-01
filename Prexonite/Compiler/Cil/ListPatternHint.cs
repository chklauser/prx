namespace Prexonite.Compiler.Cil;

/// <summary>
///     Associates a list-shape helper and its element extractors so static CIL can unwrap the
///     underlying <see cref="List{PValue}"/> once and reuse the typed list.
/// </summary>
public sealed class ListPatternHint : ICilHint
{
    public const string Key = "list-pattern";

    public ListPatternHint(int shapeAddress, IEnumerable<int> extractionAddresses)
    {
        ShapeAddress = shapeAddress;
        ExtractionAddresses = extractionAddresses.ToArray();
    }

    public ListPatternHint(MetaEntry[] hint)
    {
        if (hint == null)
            throw new ArgumentNullException(nameof(hint));
        if (hint.Length < 2)
            throw new ArgumentException("List-pattern hint must include a shape address.");

        ShapeAddress = int.Parse(hint[1].Text);
        ExtractionAddresses = hint.Skip(2).Select(entry => int.Parse(entry.Text)).ToArray();
    }

    public int ShapeAddress { get; }

    public IReadOnlyList<int> ExtractionAddresses { get; }

    public string CilKey => Key;

    public MetaEntry[] GetFields() =>
        [
            ShapeAddress.ToString(),
            .. ExtractionAddresses.Select(address => (MetaEntry)address.ToString()),
        ];
}
