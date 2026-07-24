using System.Collections;

namespace Prexonite.Types;

/// <summary>
///     Exposes a sequence of <see cref="PValue" /> objects to both .NET and Prexonite callers.
/// </summary>
public sealed class PValueEnumerable(Func<IEnumerator<PValue>> getEnumerator)
    : IEnumerable<PValue>,
        IObject
{
    readonly Func<IEnumerator<PValue>> _getEnumerator =
        getEnumerator ?? throw new ArgumentNullException(nameof(getEnumerator));

    public PValueEnumerable(IEnumerable<PValue> enumerable)
        : this((enumerable ?? throw new ArgumentNullException(nameof(enumerable))).GetEnumerator)
    { }

    public PValueEnumerator GetEnumerator()
    {
        var enumerator = _getEnumerator();
        return enumerator as PValueEnumerator ?? new PValueEnumeratorWrapper(enumerator);
    }

    IEnumerator<PValue> IEnumerable<PValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryDynamicCall(
        StackContext sctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        if (args.Length == 0 && Engine.StringsAreEqual(id, nameof(GetEnumerator)))
        {
            result = sctx.CreateNativePValue(GetEnumerator());
            return true;
        }

        result = null;
        return false;
    }
}
