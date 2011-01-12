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
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    /// <summary>
    /// Implementation of takewhile
    /// </summary>
    public class TakeWhile : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton

        private TakeWhile()
        {
        }

        private static readonly TakeWhile _instance = new TakeWhile();

        public static TakeWhile Instance
        {
            get { return _instance; }
        }

        #endregion

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }

        protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier, PValue[] args)
        {
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");
            if (args == null)
                throw new ArgumentNullException("args");
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
                    if ((bool) f.IndirectCall(sctx, new[] {value, i++}).ConvertTo(sctx, PType.Bool, true).Value)
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
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}