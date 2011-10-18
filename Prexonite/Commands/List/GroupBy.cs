// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class GroupBy : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");

            if (args.Length < 1)
                throw new PrexoniteException("GroupBy requires at least one argument.");

            var f = args[0];

            var sctx = sctxCarrier.StackContext;

            var groups =
                new Dictionary<PValue, List<PValue>>();

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                var xs = Map._ToEnumerable(sctx, arg);
                if (xs == null)
                    continue;
                foreach (var x in xs)
                {
                    var fx = f.IndirectCall(sctx, new[] {x});
                    if (!groups.ContainsKey(fx))
                    {
                        var lst = new List<PValue>();
                        lst.Add(x);
                        groups.Add(fx, lst);
                    }
                    else
                    {
                        groups[fx].Add(x);
                    }
                }
            }
            // DO NO CONVERT TO LINQ, dereferencing of sctx MUST be delayed!
            // ReSharper disable LoopCanBeConvertedToQuery 
            foreach (var pair in groups)
            {
                yield return new PValueKeyValuePair(pair.Key, (PValue) pair.Value);
            }
            // ReSharper restore LoopCanBeConvertedToQuery
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