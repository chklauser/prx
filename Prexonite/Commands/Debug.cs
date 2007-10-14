using System;
using Prexonite.Compiler;

namespace Prexonite.Commands
{
    /// <summary>
    /// A command that aids in generating debug output. Best used in conjunction with the <see cref="DebugHook"/>.
    /// </summary>
    public class Debug : PCommand
    {
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            bool debugging = DebugHook.IsDebuggingEnabled(sctx.Implementation);
            PCommand println = sctx.ParentEngine.Commands[Engine.PrintLineCommand];
            if (debugging)
                foreach (PValue arg in args)
                {
                    println.Run(
                        sctx, new PValue[] {String.Concat("DEBUG ??? = ", arg.CallToString(sctx))});
                }
            return debugging;
        }
    }
}