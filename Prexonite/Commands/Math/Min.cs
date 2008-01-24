using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Min : PCommand
    {
        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 2)
                throw new PrexoniteException("Min requires at least two arguments.");

            PValue arg0 = args[0];
            PValue arg1 = args[1];
            if (arg0.Type == PType.Int && arg1.Type == PType.Int)
            {
                int a = (int)arg0.Value;
                int b = (int)arg1.Value;

                return System.Math.Min(a, b);
            }
            else
            {
                double a = (double)arg0.ConvertTo(sctx, PType.Real, true).Value;
                double b = (double)arg1.ConvertTo(sctx, PType.Real, true).Value;

                return System.Math.Min(a, b);
            }
        }
    }
}
