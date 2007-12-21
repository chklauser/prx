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
using Prexonite.Commands.List;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
    /// </summary>
    public class CallMember : PCommand
    {
        /// <summary>
        /// Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Wrap Lists in other lists, if you want to pass them without being unfolded: 
        /// <code>
        /// function main()
        /// {   var myList = [1, 2];
        ///     var obj = "{1}hell{0}";
        ///     print( callmember(obj, "format",  [ myList ]) );
        /// }
        /// 
        /// //Prints "2hell1"
        /// </code>
        /// </para>
        /// </remarks>
        /// <param name="sctx">The stack context in which to call the callable argument.</param>
        /// <param name="args">A list of the form [ obj, id, arg1, arg2, arg3, ..., argn].<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length < 2 || args[0] == null)
                throw new ArgumentException(
                    "The command callmember has the signature(obj, [isSet,] id [, arg1, arg2,...,argn]).");

            bool isSet = false;
            string id;
            int i = 2;

            if (args[1].Type == PType.Bool && args.Length > 2)
            {
                isSet = (bool) args[1].Value;
                id = args[i++].CallToString(sctx);
            }
            else
            {
                id = args[1].CallToString(sctx);
            }


            PValue[] iargs = new PValue[args.Length - i];
            Array.Copy(args,i,iargs,0,iargs.Length);

            return Run(sctx, args[0], isSet, id, iargs);
        }

        /// <summary>
        /// Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <param name="sctx">The stack context in which to call the member of <paramref name="obj"/>.</param>
        /// <param name="obj">The obj to call.</param>
        /// <param name="id">The id of the member to call.</param>
        /// <param name="args">The array of arguments to pass to the member call.<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null.</exception>
        public PValue Run(StackContext sctx, PValue obj, string id, params PValue[] args)
        {
            return Run(sctx, obj, false, id, args);
        }

        /// <summary>
        /// Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <param name="sctx">The stack context in which to call the member of <paramref name="obj"/>.</param>
        /// <param name="obj">The obj to call.</param>
        /// <param name="isSet">Indicates whether to perform a Set-call.</param>
        /// <param name="id">The id of the member to call.</param>
        /// <param name="args">The array of arguments to pass to the member call.<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by the member call.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null.</exception>
        public PValue Run(StackContext sctx, PValue obj, bool isSet, string id, params PValue[] args)
        {
            if (obj == null)
                return PType.Null.CreatePValue();
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            List<PValue> iargs = new List<PValue>();
            for (int i = 0; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> folded = Map._ToEnumerable(sctx, arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }

            return obj.DynamicCall(sctx, iargs.ToArray(), isSet ? PCall.Set : PCall.Get, id);
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