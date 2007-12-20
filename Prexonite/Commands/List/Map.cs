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
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the map function. Applies a supplied function (#1) to every 
    /// value in the supplied list (#2) and returns a list with the result values.
    /// </summary>
    /// <remarks>
    /// <code>function map(ref f, var lst)
    /// {
    ///     var nlst = [];
    ///     foreach(var x in lst)
    ///         nlst[] = f(x);
    ///     return nlst;
    /// }</code>
    /// </remarks>
    public class Map : CoroutineCommand
    {
        internal static IEnumerable<PValue> _ToEnumerable(PValue psource)
        {
            if (psource.Type is ListPType ||
                psource.Type is ObjectPType && psource.Value is IEnumerable<PValue>)
                return (IEnumerable<PValue>) psource.Value;
            else
                return null;
        }

        protected static IEnumerable<PValue> CoroutineRun(StackContext sctx, IIndirectCall f, IEnumerable<PValue> source)
        {
            foreach (PValue x in source)
                yield return f != null ? f.IndirectCall(sctx, new PValue[] { x }) : x;
        }

        /// <summary>
        /// Executes the map command.
        /// </summary>
        /// <param name="sctx">The stack context in which to call the supplied function.</param>
        /// <param name="args">The list of arguments to be passed to the command.</param>
        /// <returns>A coroutine that maps the.</returns>
        protected  override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            //Get f
            IIndirectCall f;
            if (args.Length < 1)
                f = null;
            else
                f = args[0];

            //Get the source
            IEnumerable<PValue> source;
            if (args.Length == 2)
            {
                PValue psource = args[1];
                source = _ToEnumerable(psource) ?? new PValue[] {psource};
            }
            else
            {
                List<PValue> lstsource = new List<PValue>();
                for (int i = 1; i < args.Length; i++)
                {
                    IEnumerable<PValue> multiple = _ToEnumerable(args[i]);
                    if (multiple != null)
                        lstsource.AddRange(multiple);
                    else
                        lstsource.Add(args[i]);
                }
                source = lstsource;
            }

            return CoroutineRun(sctx, f, source);
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; } //makes use of indirect call, 
            //which might lead to premature initialization of the parent engine.
        }
    }
}