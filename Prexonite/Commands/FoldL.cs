using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the foldl function.
    /// </summary>
    /// <remarks>
    /// <code>function foldl(ref f, left, source)
    /// {
    ///     foreach(var right in source)
    ///         left = f(left,right);
    ///     return left;
    /// }</code>
    /// </remarks>
    internal class FoldL : PCommand
    {
        public PValue Run(
            StackContext sctx, IIndirectCall f, PValue left, IEnumerable<PValue> source)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (f == null)
                throw new ArgumentNullException("f");
            if (left == null)
                left = PType.Null.CreatePValue();
            if (source == null)
                source = new PValue[] {};

            foreach (PValue right in source)
            {
                left = f.IndirectCall(sctx, new PValue[] {left, right});
            }
            return left;
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            //Get f
            IIndirectCall f;
            if (args.Length < 1)
                throw new PrexoniteException("The foldl command requires a function argument.");
            else
                f = args[0];

            //Get left
            PValue left;
            if (args.Length < 2)
                left = null;
            else
                left = args[1];

            //Get the source
            IEnumerable<PValue> source;
            if (args.Length == 3)
            {
                PValue psource = args[2];
                source = MapAll._ToEnumerable(psource) ?? new PValue[] {psource};
            }
            else
            {
                List<PValue> lstsource = new List<PValue>();
                for (int i = 1; i < args.Length; i++)
                {
                    IEnumerable<PValue> multiple = MapAll._ToEnumerable(args[i]);
                    if (multiple != null)
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