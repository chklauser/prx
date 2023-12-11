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

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public sealed class Char : PCommand, ICilCompilerAware, ICilExtension
{
    Char()
    {
    }

    public static Char Instance { get; } = new();

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException("Char requires at least one argument.");

        var arg = args[0];
        if (arg.Type == PType.String)
        {
            var s = (string) arg.Value!;
            if (s.Length == 0)
                throw new PrexoniteException("Cannot create char from empty string.");
            else
                return s[0];
        }
        else if (arg.TryConvertTo(sctx, PType.Char, true, out var v))
        {
            return v;
        }
        else if (arg.TryConvertTo(sctx, PType.Int, true, out v))
        {
            return (char) (int) v.Value!;
        }
        else
        {
            throw new PrexoniteException("Cannot create char from " + arg);
        }
    }

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    #region Implementation of ICilCompilerAware

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }

    #endregion

    #region Implementation of ICilExtension

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        return dynamicArgc == 0 && staticArgv.Length == 1 &&
            (staticArgv[0].TryGetString(out var literal) && literal.Length > 0 ||
                staticArgv[0].TryGetInt(out var code) && code >= 0);
    }

    void ICilExtension.Implement(CompilerState state, Instruction ins,
        CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        if (ins.JustEffect)
            return; // Usually for commands without side-effects you have to at least
        //  pop dynamic arguments from the stack.
        // ValidateArguments proved that there are no arguments on the stack.
        int code;
        if (staticArgv[0].TryGetString(out var literal))
            code = literal[0];
        else if (!staticArgv[0].TryGetInt(out code))
            throw new ArgumentException(
                "char command requires one argument that is either a string or a 32-bit integer with the most significant bit cleared.");

        state.EmitLdcI4(code);
        state.EmitWrapChar();
    }

    #endregion
}