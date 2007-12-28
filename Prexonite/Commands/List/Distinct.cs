using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands.List
{
    public class Distinct : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx"); 
            
            Dictionary<PValue, object> t = new Dictionary<PValue, object>();

            foreach (PValue arg in args)
            {
                IEnumerable<PValue> xs = Map._ToEnumerable(sctx, arg);
                if(xs == null)
                    continue;
                foreach (PValue x in xs)
                    if (!t.ContainsKey(x))
                    {
                        t.Add(x, null);
                        yield return x;
                    }
            }
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get
            {
                return false;
            }
        }
    }
}
