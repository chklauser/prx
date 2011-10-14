// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Reflection;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Log : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Log()
        {
        }

        private static readonly Log _instance = new Log();

        public static Log Instance
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
            get { return true; }
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execut the command.</param>
        /// <param name = "args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Log requires at least one argument.");

            if (args.Length > 1)
            {
                var arg0 = args[0];
                var arg1 = args[1];

                return RunStatically(arg0, arg1, sctx);
            }
            else
            {
                var arg0 = args[0];

                return RunStatically(arg0, sctx);
            }
        }

        public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;
            var b = (double) arg1.ConvertTo(sctx, PType.Real, true).Value;
            return System.Math.Log(x, b);
        }

        public static PValue RunStatically(PValue arg0, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;
            return System.Math.Log(x);
        }

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
            return CompilationFlags.PrefersCustomImplementation;
        }

        private static readonly MethodInfo RunStaticallyNaturalMethod =
            typeof (Log).GetMethod("RunStatically", new[] {typeof (PValue), typeof (StackContext)});

        private static readonly MethodInfo RunStaticallyAnyMethod =
            typeof (Log).GetMethod("RunStatically",
                new[] {typeof (PValue), typeof (PValue), typeof (StackContext)});

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            var argc = ins.Arguments;
            if (ins.JustEffect)
            {
                state.EmitIgnoreArguments(argc);
            }
            else
            {
                if (argc > 2)
                {
                    state.EmitIgnoreArguments(argc - 2);
                    argc = 2;
                }
                switch (argc)
                {
                    case 0:
                        state.EmitLdcI4(0);
                        state.EmitWrapInt();
                        goto case 1;
                    case 1:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(RunStaticallyNaturalMethod);
                        break;
                    case 2:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(RunStaticallyAnyMethod);
                        break;
                }
            }
        }

        #endregion
    }
}