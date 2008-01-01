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
using System.Diagnostics;
using Prexonite.Commands.List;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Returns null if no callable object is passed.
    /// </para>
    /// <para>
    ///     Uses the <see cref="IIndirectCall"/> interface.
    /// </para>
    /// </remarks>
    /// <seealso cref="IIndirectCall"/>
    public class Call : StackAwareCommand
    {

        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Returns null if no callable object is passed.
        /// </para>
        /// <para>
        ///     Uses the <see cref="IIndirectCall"/> interface.
        /// </para>
        /// <para>
        ///     Wrap Lists in other lists, if you want to pass them without being unfolded: 
        /// <code>
        /// function main()
        /// {   var myList = [1, 2, 3];
        ///     var f = xs => xs.Count;
        ///     print( call(f, [ myList ]) );
        /// }
        /// 
        /// //Prints "3"
        /// </code>
        /// </para>
        /// </remarks>
        /// <seealso cref="IIndirectCall"/>
        /// <param name="sctx">The stack context in which to call the callable argument.</param>
        /// <param name="args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by <see cref="IIndirectCall.IndirectCall"/> or PValue Null if no callable object has been passed.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                return PType.Null.CreatePValue();

            List<PValue> iargs = FlattenArguments(sctx, args, 1);

            return args[0].IndirectCall(sctx, iargs.ToArray());
        }

        [DebuggerStepThrough]
        public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args)
        {
            return FlattenArguments(sctx, args, 0);
        }

        public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args, int index)
        {
            if (args == null)
                args = new PValue[] { };
            List<PValue> iargs = new List<PValue>();
            for (int i = index; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> folded = Map._ToEnumerable(sctx, arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }
            return iargs;
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
                return new NullContext(sctx);

            List<PValue> iargs = FlattenArguments(sctx, args, 1);

            PValue callable = args[0];
            return CreateStackContext(sctx, callable, iargs.ToArray());
        }

        public static StackContext CreateStackContext(StackContext sctx, PValue callable, PValue[] args)
        {
            IStackAware sa = callable.Value as IStackAware;
            if (callable.Type is ObjectPType && sa != null)
                return sa.CreateStackContext(sctx, args);
            else
                return new IndirectCallContext(sctx, callable, args);
        }
    }
}