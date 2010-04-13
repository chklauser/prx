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

namespace Prexonite.Commands.List
{
    public class Except : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private static readonly Except _instance = new Except();

        public static Except Instance
        {
            get { return _instance; }
        }

        private Except()
        {
        }

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            var xss = new List<IEnumerable<PValue>>();
            foreach (var arg in args)
            {
                var xs = Map._ToEnumerable(sctx, arg);
                if (xs != null)
                    xss.Add(xs);
            }

            var n = xss.Count;
            if (n < 2)
                throw new PrexoniteException("Except requires at least two sources.");

            var t = new Dictionary<PValue, bool>();
            //All elements of the first source are considered candidates
            foreach (var x in xss[0])
                if (!t.ContainsKey(x))
                    t.Add(x, true);

            for (var i = 1; i < n; i++)
                foreach (var x in xss[i])
                    if (t.ContainsKey(x))
                        t.Remove(x);

            return sctx.CreateNativePValue(t.Keys);
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
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
            return CompilationFlags.PreferRunStatically;
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