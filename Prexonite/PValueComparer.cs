// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    ///     Compares PValueswith respect to a stack context.
    /// </summary>
    [DebuggerNonUserCode]
    public class PValueComparer : IComparer<PValue>
    {
        private StackContext sctx;

        public PValueComparer(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            this.sctx = sctx;
        }

        #region IComparer<PValue> Members

        ///<summary>
        ///    Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        ///</summary>
        ///<returns>
        ///    Value Condition Less than zerox is less than y.Zerox equals y.Greater than zerox is greater than y.
        ///</returns>
        ///<param name = "y">The second object to compare.</param>
        ///<param name = "x">The first object to compare.</param>
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