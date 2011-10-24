// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro.Commands
{
    /// <summary>
    /// Wraps invocations of call\* commands (or functions), handling partial applications where
    /// placeholders occur in list literals.
    /// </summary>
    /// <remarks>
    /// <para>In the following example, <c>pcw</c> is a <see cref="PartialCallWrapper"/> 
    /// for the <see cref="Call"/> command.</para>
    /// <code>pcw(f(?),?,[1,?,2])</code>
    /// <para>becomes</para>
    /// <code>call\star(2,call(?),f(?),?,[1,?,2])</code>
    /// <para>But if there are no placeholders, the macro removes itself:</para>
    /// <code>pcw(f(?),x,[1,2,3])</code>
    /// <para>becomes</para>
    /// <code>call(f(?),x,[1,2,3])</code>
    /// <para>In most cases, <see cref="PartialCallWrapper"/> can be used directly, just by supplying the 
    /// underlying call implementation (doesn't need to be a command).</para>
    /// <para> In case your call\* has special
    /// requirement (e.g., like call\member, it has the member id as an additional parameter), you can
    /// inherit from <see cref="PartialCallWrapper"/> and override <see cref="GetTrivialPartialApplication"/>, 
    /// <see cref="GetCallArguments"/> and <see cref="GetPassThroughArguments"/>.</para>
    /// </remarks>
    public class PartialCallWrapper : PartialMacroCommand
    {
        private readonly SymbolEntry _callImplementation;

        public SymbolEntry CallImplementation
        {
            get { return _callImplementation; }
        }

        /// <summary>
        /// Creates a new instance of <see cref="PartialCallWrapper"/> around the specified call implementation.
        /// </summary>
        /// <param name="alias">The name of this macro command.</param>
        /// <param name="callImplementationId">The physical id of the call implementation.</param>
        /// <param name="callImplementetaionInterpretation">The interpretation of the call implementation.</param>
        public PartialCallWrapper(string alias, SymbolEntry callImplementation)
            : base(alias)
        {
            if (callImplementation == null)
                throw new ArgumentNullException("callImplementation");

            _callImplementation = callImplementation;
        }

        #region Overrides of MacroCommand

        private static bool _hasPlaceholder(IAstExpression expr)
        {
            var lit = expr as AstListLiteral;
            return expr.IsPlaceholder() || (lit != null && lit.CheckForPlaceholders());
        }

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                // Call with no arguments returns null.
                // The macro system will supply that null.
                return;
            }

            var p = context.Invocation.Arguments[0] as AstPlaceholder;
            if (context.Invocation.Arguments.Count == 1
                && p != null
                    && (p.Index.GetValueOrDefault(0) == 0))
            {
                // call(?0) ⇒ call\perform(?0)

                context.Block.Expression = GetTrivialPartialApplication(context);
                return;
            }

            if (!context.Invocation.Arguments.Any(_hasPlaceholder))
            {
                // no placeholders, invoke call\perform directly

                var call = context.CreateGetSetSymbol(_callImplementation,
                    context.Call, GetCallArguments(context).ToArray());
                context.Block.Expression = call;
                return;
            }

            var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column,
                CallStar.Instance.Id,
                SymbolInterpretations.MacroCommand);

            // Protect the first two arguments
            inv.Arguments.Add(context.CreateConstant(GetPassThroughArguments(context)));

            // Indicate the kind of call by passing `call\perform(?)`, a partial application of call
            var paCall = context.CreateGetSetSymbol(_callImplementation,
                context.Invocation.Call,
                new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                    context.Invocation.Column, 0));
            inv.Arguments.Add(paCall);

            // Pass all the other arguments through
            inv.Arguments.AddRange(GetCallArguments(context));

            context.Block.Expression = inv;
        }

        /// <summary>
        /// Returns a trivial partial application of the call implementation (call\perform(?))
        /// </summary>
        /// <param name="context">The macro context in which to create the AST node.</param>
        /// <returns>A trivial partial application of the call implementation.</returns>
        protected virtual AstGetSetSymbol GetTrivialPartialApplication(MacroContext context)
        {
            var cp = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, context.Invocation.Call, _callImplementation);
            cp.Arguments.Add(new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, 0));
            return cp;
        }

        /// <summary>
        /// Provides access to the call arguments, including the call target and any other 
        /// parameters (like the member id for call\member).
        /// </summary>
        /// <param name="context">The context from which to derive the arguments.</param>
        /// <returns>The arguments to the call invocation.</returns>
        protected virtual IEnumerable<IAstExpression> GetCallArguments(MacroContext context)
        {
            return context.Invocation.Arguments;
        }

        /// <summary>
        /// Determines the number of arguments that need to be protected when passed to call\star.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>For call and call\async, for instance, two arguments need to be passed to call\star 
        /// unprocessed: the reference to the call implementation (<c>call(?)</c> or <c>call\async(?)</c>) 
        /// and the call target.</para>
        /// <para>In the case of <c>call\member</c>, however, there is an additional argument to be 
        /// protected: the member id.</para></remarks>
        protected virtual int GetPassThroughArguments(MacroContext context)
        {
            return 2;
        }

        #endregion

        #region Overrides of PartialMacroCommand

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            DoExpand(context);
            return true;
        }

        #endregion
    }
}