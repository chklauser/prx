using System;
using System.Collections.Generic;

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
        /// <param name="sctx">The stack contetx that wishes to find out, who called him.</param>
        /// <param name="args">Ignored</param>
        /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return sctx.CreateNativePValue(GetCaller(sctx));
        }

        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack context that wishes tp find out, who called him.</param>
        /// <returns>Either the stack context of the caller or null.</returns>
        public static StackContext GetCaller(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            LinkedList<StackContext> stack = sctx.ParentEngine.Stack;
            if (!stack.Contains(sctx))
                return null;
            else
            {
                LinkedListNode<StackContext> callee = stack.FindLast(sctx);
                if (callee.Previous == null)
                    return null;
                else
                    return callee.Previous.Value;
            }
        }
    }
}