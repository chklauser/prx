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
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the <c>concat</c> command.
    /// </summary>
    public sealed class Concat : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Concat()
        {
        }

        private static readonly Concat _instance = new Concat();

        public static Concat Instance
        {
            get { return _instance; }
        }

        #endregion

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return true; }
        }

        /// <summary>
        /// Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name="sctx">The context in which to convert the arguments to strings.</param>
        /// <param name="args">The list of fragments to concatenate.</param>
        /// <returns>The concatenated string.</returns>
        /// <remarks>Please note that this method uses a string builder. The addition operator is faster for only two fragments.</remarks>
        public static string ConcatenateString(StackContext sctx, PValue[] args)
        {
            var elements = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var element = arg.Type is StringPType ? (string) arg.Value : arg.CallToString(sctx);
                elements[i] = element;
            }

            return String.Concat(elements);
        }

        /// <summary>
        /// Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name="sctx">The context in which to convert the arguments to strings.</param>
        /// <param name="args">The list of fragments to concatenate.</param>
        /// <returns>The concatenated string.</returns>
        /// <remarks>Please note that this method uses a string builder. The addition operator is faster for only two fragments.</remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            return ConcatenateString(sctx, args);
        }

        /// <summary>
        /// Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name="sctx">The context in which to convert the arguments to strings.</param>
        /// <param name="args">The list of fragments to concatenate.</param>
        /// <returns>A PValue containing the concatenated string.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}