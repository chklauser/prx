/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

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