
#region Namespace Imports

using System.Collections;
using System.Reflection;

#endregion

namespace Prexonite.Compiler.Cil;

public class ForeachHint : ICilHint
{
    public ForeachHint
    (
        string enumVar, int castAddress, int getCurrentAddress, int moveNextAddress,
        int disposeAddress)
    {
        EnumVar = enumVar;
        DisposeAddress = disposeAddress;
        MoveNextAddress = moveNextAddress;
        GetCurrentAddress = getCurrentAddress;
        CastAddress = castAddress;
    }

    #region Meta format

    public const int CastAddressIndex = 1;
    public const int DisposeAddressIndex = 4;
    public const int EntryLength = 5;
    public const int EnumVarIndex = 0;
    public const int GetCurrentAddressIndex = 2;
    public const int MoveNextAddressIndex = 3;
    public const string Key = "foreach";

    public ForeachHint(MetaEntry[] hint)
    {
        if (hint == null)
            throw new ArgumentNullException(nameof(hint));
        if (hint.Length < EntryLength)
            throw new ArgumentException($"Hint must have at least {EntryLength} entries.");
        EnumVar = hint[EnumVarIndex + 1].Text;
        CastAddress = int.Parse(hint[CastAddressIndex + 1].Text);
        GetCurrentAddress = int.Parse(hint[GetCurrentAddressIndex + 1].Text);
        MoveNextAddress = int.Parse(hint[MoveNextAddressIndex + 1].Text);
        DisposeAddress = int.Parse(hint[DisposeAddressIndex + 1].Text);
    }

    public static ForeachHint FromMetaEntry(MetaEntry[] entry)
    {
        return new(entry);
    }

    public MetaEntry[] GetFields()
    {
        var fields = new MetaEntry[EntryLength];
        fields[EnumVarIndex] = EnumVar;
        fields[CastAddressIndex] = CastAddress.ToString();
        fields[GetCurrentAddressIndex] = GetCurrentAddress.ToString();
        fields[MoveNextAddressIndex] = MoveNextAddress.ToString();
        fields[DisposeAddressIndex] = DisposeAddress.ToString();
        return fields;
    }

    public string CilKey => Key;

    #endregion

    public string EnumVar { get; }

    public int CastAddress { get; }

    public int GetCurrentAddress { get; }

    public int MoveNextAddress { get; }

    public int DisposeAddress { get; }

    #region Methodinfos

    internal static readonly MethodInfo MoveNextMethod =
        typeof(IEnumerator).GetMethod("MoveNext")
        ?? throw new InvalidOperationException(
            $"{nameof(IEnumerator)}.{nameof(IEnumerator.MoveNext)} method not found.");

    internal static readonly MethodInfo GetCurrentMethod =
        typeof(IEnumerator<PValue>).GetProperty(nameof(IEnumerator<PValue>.Current))!.GetGetMethod()
        ?? throw new InvalidOperationException(
            $"{nameof(IEnumerator)}.{nameof(IEnumerator.Current)} property getter not found.");

    internal static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose))
        ?? throw new InvalidOperationException(
            $"{nameof(IDisposable)}.{nameof(IDisposable.Dispose)} method not found.");

    #endregion
}