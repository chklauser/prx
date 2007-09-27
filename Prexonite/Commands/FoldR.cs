using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the foldr function.
    /// </summary>
    /// <remarks>
    /// <code>function foldr(ref f, right, source)
    /// {
    ///     var lst = [];
    ///     foreach(var e in source)
    ///         lst[] = e;
    ///     for(var i = lst.Count-1; i>=0; i--)
    ///         right = f(lst[i],right);
    ///     return right;
    /// }</code>
    /// </remarks>
    class FoldR : PCommand
    {

        public PValue Run(StackContext sctx, IIndirectCall f, PValue right, IEnumerable<PValue> source)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (f == null)
                throw new ArgumentNullException("f");
            if (right == null)
                right = PType.Null.CreatePValue();
            if (source == null)
                source = new PValue[] {};

            List<PValue> lst = new List<PValue>(source);

            for (int i = lst.Count - 1; i >= 0; i--)
            {
                right = f.IndirectCall(sctx, new PValue[] {lst[i], right});
            }
            return right;
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args"); 

            //Get f
            IIndirectCall f;
            if(args.Length < 1)
                throw new PrexoniteException("The foldr command requires a function argument.");
            else
                f = args[0];

            //Get left
            PValue left;
            if(args.Length < 2)
                left = null;
            else
                left = args[1];

            //Get the source
            IEnumerable<PValue> source;
            if(args.Length == 3)
            {
                PValue psource = args[2];
                source = MapAll._ToEnumerable(psource) ?? new PValue[] {psource};
            }
            else
            {
                List<PValue> lstsource = new List<PValue>();
                for(int i = 1; i < args.Length ; i++)
                {
                    IEnumerable<PValue> multiple = MapAll._ToEnumerable(args[i]);
                    if(multiple != null)
                        lstsource.AddRange(multiple);
                    else 
                        lstsource.Add(args[i]);
                }
                source = lstsource; 
            }

            return Run(sctx, f, left, source);
        }
    }
}
