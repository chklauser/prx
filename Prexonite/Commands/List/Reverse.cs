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
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List
{
    public class Reverse : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private static readonly Reverse _instance = new Reverse();

        public static Reverse Instance
        {
            get { return _instance; }
        }

        private Reverse()
        {
        }

        #endregion

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }

        private static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier, IEnumerable<PValue> args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");

            var sctx = sctxCarrier.StackContext;

            var lst = new List<PValue>();

            foreach (var arg in args)
                lst.AddRange(Map._ToEnumerable(sctx, arg));

            for (var i = lst.Count - 1; i >= 0; i--)
                yield return lst[i];
        }

// Bound statically by CIL compiler
// ReSharper disable UnusedMember.Global
        public static PValue RunStatically(StackContext sctx, PValue[] args)
// ReSharper restore UnusedMember.Global
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