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
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            List<IEnumerable<PValue>> xss = new List<IEnumerable<PValue>>();
            foreach (PValue arg in args)
            {
                IEnumerable<PValue> xs = Map._ToEnumerable(sctx,arg);
                if(xs != null)
                    xss.Add(xs);
            }

            int n = xss.Count;
            if (n < 2)
                throw new PrexoniteException("Intersect requires at least two sources.");
            
            Dictionary<PValue, int> t = new Dictionary<PValue, int>();
            //All elements of the first source are considered candidates
            foreach (PValue x in xss[0])
                if (!t.ContainsKey(x))
                    t.Add(x, 1);

            Dictionary<PValue, object> d = new Dictionary<PValue, object>();
            for (int i = 1; i < n-1; i++)
            {
                foreach (PValue x in xss[i])
                    if((!d.ContainsKey(x)) && t.ContainsKey(x))
                    {
                        d.Add(x, null); //only current source
                        t[x]++;
                    }
                d.Clear();
            }

            foreach (PValue x in xss[n-1])
                if ((!d.ContainsKey(x)) && t.ContainsKey(x))
                {
                    d.Add(x, null); //only current source
                    int k = t[x]+1;
                    if(k == n)
                        yield return x;
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
