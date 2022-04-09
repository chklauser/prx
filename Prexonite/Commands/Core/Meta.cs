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
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Meta : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    private Meta()
    {
    }

    public static Meta Instance { get; } = new();

    #endregion

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.RequiresCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        if (ins.Arguments > 0)
            throw new PrexoniteException("The meta command no longer accepts arguments.");

        state.EmitLoadLocal(state.SctxLocal);
        state.EmitLoadArg(CompilerState.ParamSourceIndex);
        var getMeta = typeof (PFunction).GetProperty("Meta").GetGetMethod();
        state.Il.EmitCall(OpCodes.Callvirt, getMeta, null);
        state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreateNativePValue, null);
    }

    #endregion

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args != null && args.Length > 0)
            throw new PrexoniteException("The meta command no longer accepts arguments.");

        if (!(sctx is FunctionContext fctx))
            throw new PrexoniteException(
                "The meta command uses dynamic features and can therefor only be called from a Prexonite function.");

        return fctx.CreateNativePValue(fctx.Implementation.Meta);
    }
}