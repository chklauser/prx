using System;
using System.Collections.Generic;
using System.Text;

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

            var carrier = new ContextCarrier();
            var corctx = new CoroutineContext(sctx, CoroutineRun(carrier, args));
            carrier.StackContext = corctx;
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        protected abstract IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args);

        public sealed class ContextCarrier
        {
            public ContextCarrier()
            {
            }

            public ContextCarrier(StackContext sctx)
            {
                _stackContext = sctx;
            }

            private StackContext _stackContext;

            public StackContext StackContext
            {
                get
                {
                    if (_stackContext == null)
                        throw new InvalidOperationException("StackContext has not been assigned yet.");
                    return _stackContext;
                }
                set
                {
                    if (_stackContext != null)
                        throw new InvalidOperationException("StackContext can only be set once.");
                    _stackContext = value;
                }
            }
        }
    }
}