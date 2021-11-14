﻿// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
using System.Linq;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler.Macro.Commands;

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
    public EntityRef CallImplementation { get; }

    /// <summary>
    /// Creates a new instance of <see cref="PartialCallWrapper"/> around the specified call implementation.
    /// </summary>
    /// <param name="alias">The name of this macro command.</param>
    public PartialCallWrapper(string alias, EntityRef callImplementation)
        : base(alias)
    {
        CallImplementation = callImplementation ?? throw new ArgumentNullException(nameof(callImplementation));
    }

    #region Overrides of MacroCommand

    private static bool _hasPlaceholder(AstExpr expr)
    {
        return expr.IsPlaceholder() || expr is AstListLiteral lit && lit.CheckForPlaceholders();
    }

    protected override void DoExpand(MacroContext context)
    {
        if (context.Invocation.Arguments.Count == 0)
        {
            // Call with no arguments returns null.
            // The macro system will supply that null.
            return;
        }

        if (context.Invocation.Arguments.Count == 1
            && context.Invocation.Arguments[0] is AstPlaceholder p
            && p.Index.GetValueOrDefault(0) == 0)
        {
            // call(?0) ⇒ call\perform(?0)

            context.Block.Expression = GetTrivialPartialApplication(context);
            return;
        }

        if (!context.Invocation.Arguments.Any(_hasPlaceholder))
        {
            // no placeholders, invoke call\perform directly

            var call = context.Factory.IndirectCall(context.Invocation.Position,
                context.Factory.Reference(context.Invocation.Position,
                    CallImplementation),
                context.Call);
            call.Arguments.AddRange(GetCallArguments(context));
            context.Block.Expression = call;
            return;
        }

        // Assemble the invocation of call\*(passThrough,call\perform(?),callArguments...)
        //  Note: this is a get-call in all cases, because we are computing a partial application
        //  whether the programmer wrote a get or a set call needs to be captured by concrete
        //  implementations of partial call wrapers (see Call_Member)
        var inv = context.Factory.Expand(context.Invocation.Position, EntityRef.MacroCommand.Create(CallStar.Instance.Id));
            
        // Protect the first two arguments
        inv.Arguments.Add(context.CreateConstant(GetPassThroughArguments(context)));

        // Indicate the kind of call by passing `call\perform(?)`, a partial application of call
        var paCall = context.Factory.Call(context.Invocation.Position, CallImplementation, context.Call,
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
    protected virtual AstGetSet GetTrivialPartialApplication(MacroContext context)
    {
        var cp = context.Factory.Call(context.Invocation.Position, CallImplementation, context.Call,
            new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, 0));
        return cp;
    }

    /// <summary>
    /// Provides access to the call arguments, including the call target and any other 
    /// parameters (like the member id for call\member).
    /// </summary>
    /// <param name="context">The context from which to derive the arguments.</param>
    /// <returns>The arguments to the call invocation.</returns>
    protected virtual IEnumerable<AstExpr> GetCallArguments(MacroContext context)
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