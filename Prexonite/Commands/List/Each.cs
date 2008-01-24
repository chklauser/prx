using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    public class Each : PCommand
    {
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Each requires at least two arguments");
            PValue f = args[0];

            PValue[] eargs = new PValue[1];
            for (int i = 1; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> set = Map._ToEnumerable(sctx, arg);
                if(set == null)
                    continue;
                foreach (PValue value in set)
                {
                    eargs[0] = value;
                    f.IndirectCall(sctx, eargs);
                }
            }

            return PType.Null;
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
