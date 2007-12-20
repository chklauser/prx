using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Round : PCommand
    {
        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return true; }
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

            if (args.Length < 1)
                throw new PrexoniteException("Round requires at least one argument.");

            double x = (double) args[0].ConvertTo(sctx, PType.Real, true).Value;

            int d;
            PValue pd;

            if (args.Length > 1 && args[1].TryConvertTo(sctx, PType.Int, true, out pd))
                d = System.Math.Abs((int)pd.Value);
            else
                d = 0;

            return System.Math.Round(x, d, MidpointRounding.AwayFromZero);
        }
    }
}
