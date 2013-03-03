// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core
{
    public class ConsolePrintLine : PCommand, ICilCompilerAware, ICilExtension
    {
        #region Singleton

        private ConsolePrintLine()
        {
        }

        private static readonly ConsolePrintLine _instance = new ConsolePrintLine();

        public static ConsolePrintLine Instance
        {
            get { return _instance; }
        }

        #endregion

        /// <summary>
        ///     A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>
        ///     Pure commands can be applied at compile time.
        /// </remarks>
        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execut the command.</param>
        /// <param name = "args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var buffer = Concat.ConcatenateString(sctx, args);

            Console.WriteLine(buffer);

            return buffer;
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
            switch (ins.Arguments)
            {
                case 0:
                case 1:
                    return CompilationFlags.PrefersCustomImplementation;
                default:
                    return CompilationFlags.PrefersRunStatically;
            }
        }

        //Fix #10
        internal static readonly MethodInfo consoleWriteLineMethod_String =
            typeof (Console).GetMethod("WriteLine", new[] {typeof (String)});

        internal static readonly MethodInfo consoleWriteLineMethod_ =
            typeof (Console).GetMethod("WriteLine", Type.EmptyTypes);

        internal static readonly MethodInfo ConsoleWriteMethod =
            typeof (Console).GetMethod("Write", new[] {typeof (String)});

        internal static readonly MethodInfo PValueCallToString =
            typeof (PValue).GetMethod("CallToString", new[] {typeof (StackContext)});

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            switch (ins.Arguments)
            {
                case 0:
                    state.Il.EmitCall(OpCodes.Call, consoleWriteLineMethod_, null);
                    if (!ins.JustEffect)
                    {
                        state.Il.Emit(OpCodes.Ldstr, "");
                        state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.GetStringPType, null);
                        state.Il.Emit(OpCodes.Newobj, Compiler.Cil.Compiler.NewPValue);
                    }
                    break;
                case 1:
                    state.EmitLoadLocal(state.SctxLocal);
                    state.Il.EmitCall(OpCodes.Call, PValueCallToString, null);
                    if (!ins.JustEffect)
                    {
                        state.Il.Emit(OpCodes.Dup);
                        state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.GetStringPType, null);
                        state.Il.Emit(OpCodes.Newobj, Compiler.Cil.Compiler.NewPValue);
                        state.EmitStoreTemp(0);
                    }
                    state.Il.EmitCall(OpCodes.Call, consoleWriteLineMethod_String, null);
                    if (!ins.JustEffect)
                    {
                        state.EmitLoadTemp(0);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
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
            var text = String.Concat(staticArgv.Select(StaticPrint._ToString));

            state.Il.Emit(OpCodes.Ldstr, text);
            if (!ins.JustEffect)
            {
                state.Il.Emit(OpCodes.Dup);
            }
            state.EmitCall(consoleWriteLineMethod_String);
            if (!ins.JustEffect)
            {
                state.EmitWrapString();
            }
        }

        #endregion
    }
}