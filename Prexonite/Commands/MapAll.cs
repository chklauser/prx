using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the map function. Applies a supplied function (#1) to every 
    /// value in the supplied list (#2) and returns a list with the result values.
    /// </summary>
    /// <remarks>
    /// <code>function map(ref f, var lst)
    /// {
    ///     var nlst = [];
    ///     foreach(var x in lst)
    ///         nlst[] = f(x);
    ///     return nlst;
    /// }</code>
    /// </remarks>
    public class MapAll : PCommand
    {
        internal static IEnumerable<PValue> _ToEnumerable(PValue psource)
        {
            if (psource.Type is ListPType ||
                psource.Type is ObjectPType && psource.Value is IEnumerable<PValue>)
                return (IEnumerable<PValue>) psource.Value;
            else
                return null;
        }

        /// <summary>
        /// Executes the map command.
        /// </summary>
        /// <param name="sctx">The stack context in which to run <paramref name="f"/>.</param>
        /// <param name="f">The function to be applied to all elements.</param>
        /// <param name="source">The source of the elements to map.</param>
        /// <returns>A list with all calculated return values.</returns>
        /// <remarks>This function will fetch <strong>all</strong> from the supplied <paramref name="source"/>.</remarks>
        public PValue Run(StackContext sctx, IIndirectCall f, IEnumerable<PValue> source)
        {
            List<PValue> nlst = new List<PValue>();
            foreach (PValue x in source)
                nlst.Add(f != null ? f.IndirectCall(sctx, new PValue[] {x}) : x);

            return PType.List.CreatePValue(nlst);
        }

        /// <summary>
        /// Executes the map command.
        /// </summary>
        /// <param name="sctx">The stack context in which to call the supplied function.</param>
        /// <param name="args">The list of arguments to be passed to the command.</param>
        /// <returns>A list with all calculated return values.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            //Get f
            IIndirectCall f;
            if (args.Length < 1)
                f = null;
            else
                f = args[0];

            //Get the source
            IEnumerable<PValue> source;
            if (args.Length == 2)
            {
                PValue psource = args[1];
                source = _ToEnumerable(psource) ?? new PValue[] {psource};
            }
            else
            {
                List<PValue> lstsource = new List<PValue>();
                for (int i = 1; i < args.Length; i++)
                {
                    IEnumerable<PValue> multiple = _ToEnumerable(args[i]);
                    if (multiple != null)
                        lstsource.AddRange(multiple);
                    else
                        lstsource.Add(args[i]);
                }
                source = lstsource;
            }

            return Run(sctx, f, source);
        }
    }
}