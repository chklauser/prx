using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the caller command. Returns the stack context of the caller.
    /// </summary>
    public class Caller : PCommand
    {
        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack contetx that wished to find out, who called him.</param>
        /// <param name="args">Ignored</param>
        /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            LinkedList<StackContext> stack = sctx.ParentEngine.Stack;

            if (!stack.Contains(sctx))
                return PType.Null.CreatePValue();
            else
            {
                LinkedListNode<StackContext> callee = stack.FindLast(sctx);
                if (callee.Previous == null)
                    return PType.Null.CreatePValue();
                else
                    return sctx.CreateNativePValue(callee.Previous.Value);
            }
        }
    }
}