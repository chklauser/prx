using System;
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    /// Compares PValueswith respect to a stack context.
    /// </summary>
    [DebuggerNonUserCode]
    public class PValueComparer : IComparer<PValue>
    {
        private StackContext sctx;

        public PValueComparer(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            this.sctx = sctx;
        }

        #region IComparer<PValue> Members

        ///<summary>
        ///Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        ///</summary>
        ///
        ///<returns>
        ///Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
        ///</returns>
        ///
        ///<param name="y">The second object to compare.</param>
        ///<param name="x">The first object to compare.</param>
        public int Compare(PValue x, PValue y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;
            else
            {
                PValue pr;
                if (x.TryDynamicCall(sctx, new PValue[] {}, PCall.Get, "CompareTo", out pr))
                {
                    if (!(pr.Type is IntPType))
                        pr.ConvertTo(sctx, PType.Int);
                    return (int) pr.Value;
                }
                else if (y.TryDynamicCall(sctx, new PValue[] {}, PCall.Get, "CompareTo", out pr))
                {
                    if (!(pr.Type is IntPType))
                        pr.ConvertTo(sctx, PType.Int);
                    return (int) pr.Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }
}