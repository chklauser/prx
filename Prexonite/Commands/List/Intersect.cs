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

namespace Prexonite.Commands.List
{
    public class Intersect : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");

            var sctx = sctxCarrier.StackContext;

            var xss = new List<IEnumerable<PValue>>();
            foreach (var arg in args)
            {
                var xs = Map._ToEnumerable(sctx, arg);
                if (xs != null)
                    xss.Add(xs);
            }

            var n = xss.Count;
            if (n < 2)
                throw new PrexoniteException("Intersect requires at least two sources.");

            var t = new Dictionary<PValue, int>();
            //All elements of the first source are considered candidates
            foreach (var x in xss[0])
                if (!t.ContainsKey(x))
                    t.Add(x, 1);

            var d = new Dictionary<PValue, object>();
            for (var i = 1; i < n - 1; i++)
            {
                foreach (var x in xss[i])
                    if ((!d.ContainsKey(x)) && t.ContainsKey(x))
                    {
                        d.Add(x, null); //only current source
                        t[x]++;
                    }
                d.Clear();
            }

            foreach (var x in xss[n - 1])
                if ((!d.ContainsKey(x)) && t.ContainsKey(x))
                {
                    d.Add(x, null); //only current source
                    var k = t[x] + 1;
                    if (k == n)
                        yield return x;
                }
        }

        /// <summary>
        ///     A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>
        ///     Pure commands can be applied at compile time.
        /// </remarks>
        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }
    }
}