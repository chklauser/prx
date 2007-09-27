using System;
using System.Collections.Generic;
using System.Text;
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
                args = new PValue[] { } ;
            List<PValue> lst = new List<PValue>();
            if (args.Length == 0)
                return PType.Null.CreatePValue();
            else if(args.Length == 1)
            {
                foreach(PValue x in MapAll._ToEnumerable(args[0]))
                    lst.Add(x);
                return (PValue) lst;
            }
            else
            {
                List<PValue> clauses = new List<PValue>();
                for (int i = 0; i +1 < args.Length; i++)
                    clauses.Add(args[i]);
                foreach (PValue x in MapAll._ToEnumerable(args[args.Length-1]))
                    lst.Add(x);
                lst.Sort(delegate(PValue a, PValue b)
                {
                    foreach (PValue f in clauses)
                    {
                        PValue pdec = f.IndirectCall(sctx, new PValue[] { a, b });
                        if (!(pdec.Type is IntPType))
                            pdec = pdec.ConvertTo(sctx, PType.Int);
                        int dec = (int)pdec.Value;
                        if (dec != 0)
                            return dec;
                    }
                    return 0;
                });
                return (PValue) lst;
            }
        }
    }
}
