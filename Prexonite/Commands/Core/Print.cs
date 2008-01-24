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
using System.IO;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the <c>print</c> command.
    /// </summary>
    public class Print : PCommand
    {
        private TextWriter _writer;

        /// <summary>
        /// Creates a new <c>println</c> command, that prints to the supplied <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The TextWriter to write to.</param>
        public Print(TextWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Creates a new <c>println</c> command that prints to <see cref="Console.Out"/>.
        /// </summary>
        public Print()
        {
            _writer = Console.Out;
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Prints all arguments.
        /// </summary>
        /// <param name="sctx">The context in which to convert the arguments to strings.</param>
        /// <param name="args">The list of arguments to print.</param>
        /// <returns></returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                PValue arg = args[i];
                buffer.Append(arg.Type is StringPType ? (string) arg.Value : arg.CallToString(sctx));
            }

            _writer.Write(buffer);

            return buffer.ToString();
        }
    }
}