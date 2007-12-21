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

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the caller command. Returns the stack context of the caller.
    /// </summary>
    public class Caller : PCommand
    {
        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack contetx that wishes to find out, who called him.</param>
        /// <param name="args">Ignored</param>
        /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return sctx.CreateNativePValue(GetCaller(sctx));
        }

        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack context that wishes tp find out, who called him.</param>
        /// <returns>Either the stack context of the caller or null.</returns>
        public static StackContext GetCaller(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            LinkedList<StackContext> stack = sctx.ParentEngine.Stack;
            if (!stack.Contains(sctx))
                return null;
            else
            {
                LinkedListNode<StackContext> callee = stack.FindLast(sctx);
                if (callee.Previous == null)
                    return null;
                else
                    return callee.Previous.Value;
            }
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }
    }
}