
#region

using System.Collections;

#endregion

namespace Prexonite.Types;

/// <summary>
///     An enumerator proxy that returns the values instead of PValue objects of an <see cref = "IEnumerable{T}" />
/// </summary>
public sealed class PValueEnumeratorWrapper(IEnumerator<PValue> baseEnumerator) : PValueEnumerator
{
    #region Class

    /// <summary>
    ///     Creates a new proxy for the IEnumerator of the supplied <paramref name = "enumerable" />.
    /// </summary>
    /// <param name = "enumerable">An IEnumerable.</param>
    public PValueEnumeratorWrapper(IEnumerable<PValue> enumerable)
        : this(enumerable.GetEnumerator())
    {
    }

    #endregion

    #region IEnumerator<PValue> Members

    /// <summary>
    ///     Returns the current element
    /// </summary>
    public override PValue Current => baseEnumerator.Current;

    #endregion

    #region IDisposable Members

    // Dispose() calls Dispose(true)

    // The bulk of the clean-up code is implemented in Dispose(bool)
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
            return;
        // free managed resources 
        baseEnumerator.Dispose();
    }

    #endregion

    #region IEnumerator Members

    /// <summary>
    ///     Moves on to the next value.
    /// </summary>
    /// <returns>True if that next value exists; false otherwise.</returns>
    public override bool MoveNext()
    {
        return baseEnumerator.MoveNext();
    }

    /// <summary>
    ///     Resets the base enumerator.
    /// </summary>
    /// <remarks>
    ///     Some enumerators may not support the <see cref = "IEnumerator.Reset" /> method.
    /// </remarks>
    /// <exception cref = "NotSupportedException">The base enumerator does not support resetting.</exception>
    public override void Reset()
    {
        baseEnumerator.Reset();
    }

    #endregion
}