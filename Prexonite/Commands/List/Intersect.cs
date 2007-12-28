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
                throw new PrexoniteException("Union requires at least two sources.");
            
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
            get { throw new NotImplementedException(); }
        }
    }
}
