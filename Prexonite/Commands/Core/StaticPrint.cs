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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class StaticPrint : PCommand, ICilCompilerAware, ICilExtension
{
    #region Singleton

    private StaticPrint()
    {
    }

    public static StaticPrint Instance { get; } = new();

    private static TextWriter _writer = Console.Out;

    public static TextWriter Writer
    {
        get => _writer;
        set => _writer = value ?? throw new ArgumentNullException(nameof(value));
    }

    #endregion

    /// <summary>
    ///     A flag indicating whether the command acts like a pure function.
    /// </summary>
    /// <remarks>
    ///     Pure commands can be applied at compile time.
    /// </remarks>
    [Obsolete]
    public override bool IsPure => false;

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var text = Concat.ConcatenateString(sctx, args);

        Writer.Write(text);

        return text;
    }

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion

    #region Implementation of ICilExtension

    /// <summary>
    ///     Checks whether the static arguments and number of dynamic arguments are valid for the CIL extension. 
    /// 
    ///     <para>Returning false means that the CIL extension cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see
    ///       cref = "ICilCompilerAware" /> and finally the built-in mechanisms.</para>
    ///     <para>Returning true means that the CIL extension can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see
    ///      cref = "ICilExtension.Implement" /> with the same set of arguments.</para>
    /// </summary>
    /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
    /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
    /// <returns>true if the extension can provide a CIL implementation for the set of arguments; false otherwise</returns>
    public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        return dynamicArgc <= 0 && staticArgv.All(ctv => !ctv.IsReference);
    }

    /// <summary>
    ///     Implements the CIL extension in CIL for the supplied arguments. The CIL compiler guarantees to always first call <see
    ///      cref = "ICilExtension.ValidateArguments" /> in order to establish whether the extension can actually implement a particular call.
    ///     Thus, this method does not have to verify <paramref name = "staticArgv" /> and <paramref name = "dynamicArgc" />.
    /// </summary>
    /// <param name = "state">The CIL compiler state. This object is used to emit instructions.</param>
    /// <param name = "ins">The instruction that "calls" the CIL extension. Usually a command call.</param>
    /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
    /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
    public void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv,
        int dynamicArgc)
    {
        var text = string.Concat(staticArgv.Select(_ToString));
        if (text.Length == 0)
        {
            if (!ins.JustEffect)
            {
                state.Il.Emit(OpCodes.Ldstr, "");
                state.EmitWrapString();
            }
            return;
        }

        state.EmitCall(_StaticPrintTextWriterGetMethod);
        state.Il.Emit(OpCodes.Ldstr, text);
        if (!ins.JustEffect)
        {
            state.Il.Emit(OpCodes.Dup);
            state.EmitStoreTemp(0);
        }
        state.EmitVirtualCall(_textWriterWriteMethod);
        if (!ins.JustEffect)
        {
            state.EmitLoadTemp(0);
            state.EmitWrapString();
        }
    }

    internal static readonly MethodInfo _StaticPrintTextWriterGetMethod =
        typeof (StaticPrint).GetProperty("Writer").GetGetMethod();

    private static readonly MethodInfo _textWriterWriteMethod = typeof (TextWriter).GetMethod(
        "Write", new[] {typeof (string)});

    internal static string _ToString(CompileTimeValue value)
    {
        switch (value.Interpretation)
        {
            case CompileTimeInterpretation.Null:
                return "";
            case CompileTimeInterpretation.String:
                if (!value.TryGetString(out var str))
                    goto default;
                return str;
            case CompileTimeInterpretation.Int:
                if (!value.TryGetInt(out var integer))
                    goto default;
                return integer.ToString();
            case CompileTimeInterpretation.Bool:
                if (!value.TryGetBool(out var boolean))
                    goto default;
                return boolean.ToString();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
}