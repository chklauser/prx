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
    /// Implementation of the 'sort' command.
    /// </summary>
    public class Sort : PCommand
    {
        /// <summary>
        /// Sorts an IEnumerable.
        /// <code>function sort(ref f1(a,b), ref f2(a,b), ... , xs)
        /// { ... }</code>
        /// </summary>
        /// <param name="sctx">The stack context in which the sort is performed.</param>
        /// <param name="args">A list of sort expressions followed by the list to sort.</param>
        /// <returns>The a sorted copy of the list.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            List<PValue> lst = new List<PValue>();
            if (args.Length == 0)
                return PType.Null.CreatePValue();
            else if (args.Length == 1)
            {
                foreach (PValue x in Map._ToEnumerable(args[0]))
                    lst.Add(x);
                return (PValue) lst;
            }
            else
            {
                List<PValue> clauses = new List<PValue>();
                for (int i = 0; i + 1 < args.Length; i++)
                    clauses.Add(args[i]);
                foreach (PValue x in Map._ToEnumerable(args[args.Length - 1]))
                    lst.Add(x);
                lst.Sort(
                    delegate(PValue a, PValue b)
                    {
                        foreach (PValue f in clauses)
                        {
                            PValue pdec = f.IndirectCall(sctx, new PValue[] {a, b});
                            if (!(pdec.Type is IntPType))
                                pdec = pdec.ConvertTo(sctx, PType.Int);
                            int dec = (int) pdec.Value;
                            if (dec != 0)
                                return dec;
                        }
                        return 0;
                    });
                return (PValue) lst;
            }
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; } //The function makes heavy use indirect call, 
            //which might lead to initialization of the application.
        }
    }
}