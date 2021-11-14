// Prexonite
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of the caller command. Returns the stack context of the caller.
/// </summary>
public sealed class Caller : PCommand, ICilCompilerAware
{
    private Caller()
    {
    }

    public static Caller Instance { get; } = new();

    /// <summary>
    ///     Returns the caller of the supplied stack context.
    /// </summary>
    /// <param name = "sctx">The stack contetx that wishes to find out, who called him.</param>
    /// <param name = "args">Ignored</param>
    /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return sctx.CreateNativePValue(GetCaller(sctx));
    }

    /// <summary>
    ///     Returns the caller of the supplied stack context.
    /// </summary>
    /// <param name = "sctx">The stack context that wishes tp find out, who called him.</param>
    /// <returns>Either the stack context of the caller or null.</returns>
    public static StackContext GetCaller(StackContext sctx)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        var stack = sctx.ParentEngine.Stack;
        if (!stack.Contains(sctx))
            return null;
        else
        {
            var callee = stack.FindLast(sctx);
            if (callee?.Previous == null)
                return null;
            else
                return callee.Previous.Value;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public static PValue GetCallerFromCilFunction(StackContext sctx)
    {
        var stack = sctx.ParentEngine.Stack;
        if (stack.Count == 0)
            return PType.Null;
        else
            return sctx.CreateNativePValue(stack.Last.Value);
    }

    private static readonly MethodInfo GetCallerFromCilFunctionMethod =
        typeof (Caller).GetMethod("GetCallerFromCilFunction", new[] {typeof (StackContext)});

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.OperatesOnCaller | CompilationFlags.RequiresCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        for (var i = 0; i < ins.Arguments; i++)
            state.Il.Emit(OpCodes.Pop);
        if (!ins.JustEffect)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.EmitCall(OpCodes.Call, GetCallerFromCilFunctionMethod, null);
        }
    }

    #endregion
}