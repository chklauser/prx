using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands
{
    public class Limit : CoroutineCommand
    {
        protected override IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Limit requires at least one argument.");

            int i = 0;
            int count = (int) args[0].ConvertTo(sctx, PType.Int, true).Value;

            for (int j = 1; j < args.Length; j++)
            {
                PValue arg = args[j];
                IEnumerable<PValue> set = Map._ToEnumerable(arg);
                if (set == null)
                    throw new PrexoniteException(arg + " is neither a list nor a coroutine.");
                foreach (PValue value in set)
                {
                    if(++i > count)
                        goto breakall;
                    else
                        yield return value;
                }
            }
            breakall:
            ;
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
