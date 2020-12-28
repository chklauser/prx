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
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    /// <summary>
    ///     Implementation of takewhile
    /// </summary>
    public class TakeWhile : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton

        private TakeWhile()
        {
        }

        public static TakeWhile Instance { get; } = new();

        #endregion

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
            if (sctxCarrier == null)
                throw new ArgumentNullException(nameof(sctxCarrier));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 2)
                throw new PrexoniteException("TakeWhile requires at least two arguments.");

            var sctx = sctxCarrier.StackContext;

            var f = args[0];

            var i = 0;
            for (var k = 1; k < args.Length; k++)
            {
                var arg = args[k];
                var set = Map._ToEnumerable(sctx, arg);
                if (set == null)
                    continue;
                foreach (var value in set)
                    if (
                        (bool)
                            f.IndirectCall(sctx, new[] {value, i++}).ConvertTo(sctx, PType.Bool,
                                true).Value)
                        yield return value;
            }
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var carrier = new ContextCarrier();
            var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
            carrier.StackContext = corctx;
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        /// <summary>
        ///     A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>
        ///     Pure commands can be applied at compile time.
        /// </remarks>
        [Obsolete]
        public override bool IsPure => false;

        #region ICilCompilerAware Members

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}