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
using Prexonite.Compiler;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// A command that aids in generating debug output. Best used in conjunction with the <see cref="DebugHook"/>.
    /// </summary>
    public class Debug : PCommand
    {
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            var fctx = sctx as FunctionContext;
            if (fctx == null)
                return false;
            var debugging = DebugHook.IsDebuggingEnabled(fctx.Implementation);
            var println = sctx.ParentEngine.Commands[Engine.PrintLineAlias];
            if (debugging)
                foreach (var arg in args)
                {
                    println.Run(
                        sctx, new PValue[] {String.Concat("DEBUG ??? = ", arg.CallToString(sctx))});
                }
            return debugging;
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public virtual bool IsPure
        {
            get { return false; }
        }
    }
}