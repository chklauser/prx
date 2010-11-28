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
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class Call_Tail : StackAwareCommand
    {
        private Call_Tail()
        {
        }

        private static Call_Tail _instance = new Call_Tail();

        public static Call_Tail Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (args == null || args.Length < 1 || args[0] == null || args[0].IsNull)
                return PType.Null;

            var iargs = make_tailcall(sctx, args);

            return args[0].IndirectCall(sctx, iargs.ToArray());
        }

        public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (args == null || args.Length < 1 || args[0] == null || args[0].IsNull)
                return new NullContext(sctx);

            var iargs = make_tailcall(sctx, args);

            return Call.CreateStackContext(sctx, args[0], iargs.ToArray());
        }

        private static List<PValue> make_tailcall(StackContext sctx, PValue[] args)
        {
            var iargs = Call.FlattenArguments(sctx, args, 1);

            //remove caller from stack
            var stack = sctx.ParentEngine.Stack;
            stack.Remove(stack.FindLast(sctx));
            return iargs;
        }
    }
}