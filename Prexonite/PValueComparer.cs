using System.Diagnostics;

namespace Prexonite;

/// <summary>
///     Compares PValues with respect to a stack context.
/// </summary>
[DebuggerNonUserCode]
public class PValueComparer : IComparer<PValue>
{
    readonly StackContext _sctx;

    public PValueComparer(StackContext sctx)
    {
        _sctx = sctx ?? throw new ArgumentNullException(nameof(sctx));
    }

    #region IComparer<PValue> Members

    ///<summary>
    ///    Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    ///</summary>
    ///<returns>
    ///     Value less than x means x is less than y. Value equal to zero means x equals y.
    ///     Value greater than zero means x is greater than y.
    ///</returns>
    ///<param name = "y">The second object to compare.</param>
    ///<param name = "x">The first object to compare.</param>
    public int Compare(PValue? x, PValue? y)
    {
        if (x == null && y == null)
            return 0;
        else if (x == null)
            return -1;
        else if (y == null)
            return 1;
        else
        {
            if (x.TryDynamicCall(_sctx, Array.Empty<PValue>(), PCall.Get, "CompareTo", out var pr))
            {
                if (pr.Type is not IntPType)
                    pr.ConvertTo(_sctx, PType.Int);
                return (int) pr.Value!;
            }
            else if (y.TryDynamicCall(_sctx, Array.Empty<PValue>(), PCall.Get, "CompareTo", out pr))
            {
                if (pr.Type is not IntPType)
                    pr.ConvertTo(_sctx, PType.Int);
                return (int) pr.Value!;
            }
            else
            {
                return 0;
            }
        }
    }

    #endregion
}