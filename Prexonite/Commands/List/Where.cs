using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    /// <summary>
    /// Implementation of the where coroutine.
    /// </summary>
    /// <remarks>
    /// <code>
    /// coroutine where f xs does
    ///     foreach(var x in xs)
    ///         if(f.(x))
    ///             yield x;</code>
    /// </remarks>
    public class Where : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 2)
                throw new PrexoniteException("Where(f, xs) requires at least two arguments.");

            PValue f = args[0];

            for (int i = 1; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> set = Map._ToEnumerable(sctx, arg);
                if(set == null)
                    continue;
                foreach (PValue value in set)
                {
                    PValue include = f.IndirectCall(sctx, new PValue[] { value }).ConvertTo(sctx, PType.Bool, true);
                    if ((bool)include.Value)
                        yield return value;
                }
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
