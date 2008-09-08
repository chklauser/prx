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
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    /// <summary>
    /// Restricts sequences  to a given range. <br />
    /// <code>range(index, count, xs) = xs >> skip(index) >> limit(count);</code>
    /// </summary>
    public class Range : CoroutineCommand, ICilCompilerAware
    {
        private static readonly Range _instance = new Range();

        private Range()
        {
        }

        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            return CoroutineRunStatically(sctx, args);
        }

        //function range(index, count, xs) = xs >> skip(index) >> limit(count);
        private static IEnumerable<PValue> CoroutineRunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");
            int skipCount, returnCount;
            if(args.Length < 3)
                throw new  PrexoniteException("The command range requires at least 3 arguments: [index], [count] and the [list].");

            skipCount = (int) args[0].ConvertTo(sctx, PType.Int, true).Value;
            returnCount = (int) args[1].ConvertTo(sctx, PType.Int, true).Value;

            int index = 0;

            for (int i = 2; i < args.Length; i++)
            {
                PValue arg = args[i];

                IEnumerable<PValue> xs = Map._ToEnumerable(sctx, arg);

                foreach (PValue x in xs)
                {
                    if (index >= skipCount)
                    {
                        if (index == skipCount + returnCount)
                        {
                            goto breakAll; //stop processing
                        }
                        else
                        {
                            yield return x;
                        }
                    }
                    index += 1;
                }

            breakAll:
                ;
            }
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            CoroutineContext corctx = new CoroutineContext(sctx, CoroutineRunStatically(sctx, args));
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        public static Range Instance
        {
            get
            {
                return _instance;
            }
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