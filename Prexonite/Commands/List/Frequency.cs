using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class Frequency : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            Dictionary<PValue, int> t = new Dictionary<PValue, int>();

            foreach (PValue arg in args)
            {
                IEnumerable<PValue> xs = Map._ToEnumerable(sctx, arg);
                if(xs == null)
                    continue;
                foreach (PValue x in xs)
                    if (t.ContainsKey(x))
                        t[x]++;
                    else
                        t.Add(x, 1);
            }

            foreach (KeyValuePair<PValue, int> pair in t)
                yield return new PValueKeyValuePair(pair.Key, pair.Value);
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
