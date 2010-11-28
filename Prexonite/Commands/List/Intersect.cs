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

namespace Prexonite.Commands.List
{
    public class Intersect : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");

            var sctx = sctxCarrier.StackContext;

            var xss = new List<IEnumerable<PValue>>();
            foreach (var arg in args)
            {
                var xs = Map._ToEnumerable(sctx, arg);
                if (xs != null)
                    xss.Add(xs);
            }

            var n = xss.Count;
            if (n < 2)
                throw new PrexoniteException("Intersect requires at least two sources.");

            var t = new Dictionary<PValue, int>();
            //All elements of the first source are considered candidates
            foreach (var x in xss[0])
                if (!t.ContainsKey(x))
                    t.Add(x, 1);

            var d = new Dictionary<PValue, object>();
            for (var i = 1; i < n - 1; i++)
            {
                foreach (var x in xss[i])
                    if ((!d.ContainsKey(x)) && t.ContainsKey(x))
                    {
                        d.Add(x, null); //only current source
                        t[x]++;
                    }
                d.Clear();
            }

            foreach (var x in xss[n - 1])
                if ((!d.ContainsKey(x)) && t.ContainsKey(x))
                {
                    d.Add(x, null); //only current source
                    var k = t[x] + 1;
                    if (k == n)
                        yield return x;
                }
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