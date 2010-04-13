/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
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
    /// Implementation of the foldr function.
    /// </summary>
    /// <remarks>
    /// <code>function foldr(ref f, right, source)
    /// {
    ///     var lst = [];
    ///     foreach(var e in source)
    ///         lst[] = e;
    ///     for(var i = lst.Count-1; i>=0; i--)
    ///         right = f(lst[i],right);
    ///     return right;
    /// }</code>
    /// </remarks>
    public class FoldR : PCommand, ICilCompilerAware
    {
        #region Singleton

        private FoldR()
        {
        }

        private static readonly FoldR _instance = new FoldR();

        public static FoldR Instance
        {
            get { return _instance; }
        }

        #endregion

        public static PValue Run(
            StackContext sctx, IIndirectCall f, PValue right, IEnumerable<PValue> source)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (f == null)
                throw new ArgumentNullException("f");
            if (right == null)
                right = PType.Null.CreatePValue();
            if (source == null)
                source = new PValue[] {};

            var lst = new List<PValue>(source);

            for (var i = lst.Count - 1; i >= 0; i--)
            {
                right = f.IndirectCall(sctx, new[] {lst[i], right});
            }
            return right;
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            //Get f
            IIndirectCall f;
            if (args.Length < 1)
                throw new PrexoniteException("The foldr command requires a function argument.");
            else
                f = args[0];

            //Get left
            PValue left;
            if (args.Length < 2)
                left = null;
            else
                left = args[1];

            //Get the source
            IEnumerable<PValue> source;
            if (args.Length == 3)
            {
                var psource = args[2];
                source = Map._ToEnumerable(sctx, psource) ?? new[] {psource};
            }
            else
            {
                var lstsource = new List<PValue>();
                for (var i = 1; i < args.Length; i++)
                {
                    var multiple = Map._ToEnumerable(sctx, args[i]);
                    if (multiple != null)
                        lstsource.AddRange(multiple);
                    else
                        lstsource.Add(args[i]);
                }
                source = lstsource;
            }

            return Run(sctx, f, left, source);
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; } //use of indirect call
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