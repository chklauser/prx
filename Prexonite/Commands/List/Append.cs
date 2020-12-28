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
using System.Diagnostics.CodeAnalysis;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List
{
    public class Append : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Append()
        {
        }

        public static Append Instance { get; } = new();

        #endregion

        [Obsolete]
        public override bool IsPure => false;

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Coroutine")]
        protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            if (args == null)
                yield break;
            if (sctxCarrier == null)
                throw new ArgumentNullException(nameof(sctxCarrier));

            var sctx = sctxCarrier.StackContext;

            foreach (var arg in args)
            {
                if (arg == null)
                    throw new ArgumentException("No element in args must be null.", nameof(args));
                var xs = Map._ToEnumerable(sctx, arg);
                if (xs == null)
                    continue;
                foreach (var x in xs)
                    yield return x;
            }
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var carrier = new ContextCarrier();
            var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
            carrier.StackContext = corctx;
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }
    }
}