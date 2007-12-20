using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the all command.
    /// </summary>
    public class All : PCommand
    {
        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
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
            List<PValue> lst = new List<PValue>();
            foreach (PValue arg in args)
            {
                IEnumerable<PValue> set = Map._ToEnumerable(arg);
                if(set == null)
                    continue;
                else
                    lst.AddRange(set);
            }

            return (PValue) lst;
        }
    }
}
