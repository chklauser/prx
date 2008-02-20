using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands
{
    public abstract class CoroutineCommand : PCommand
    {

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

            CoroutineContext corctx = new CoroutineContext(sctx, CoroutineRun(sctx, args));
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        protected abstract IEnumerable<PValue> CoroutineRun(StackContext sctx, PValue[] args);

    }
}
