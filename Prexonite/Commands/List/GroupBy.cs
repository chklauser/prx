using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class GroupBy : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            if (args.Length < 1)
                throw new PrexoniteException("GroupBy requires at least one argument.");

            PValue f = args[0];

            Dictionary<PValue, List<PValue>> groups =
                new Dictionary<PValue, List<PValue>>();

            for (int i = 1; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> xs = Map._ToEnumerable(sctx, arg);
                if(xs == null)
                    continue;
                foreach (PValue x in xs)
                {
                    PValue fx = f.IndirectCall(sctx, new PValue[] {x});
                    if (!groups.ContainsKey(fx))
                    {
                        List<PValue> lst = new List<PValue>();
                        lst.Add(x);
                        groups.Add(fx, lst);
                    }
                    else
                    {
                        groups[fx].Add(x);
                    }
                }
            }

            foreach (KeyValuePair<PValue, List<PValue>> pair in groups)
                yield return new PValueKeyValuePair(pair.Key, (PValue)pair.Value);
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
