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
using System.Reflection;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Ceiling : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Ceiling()
        {
        }

        public static Ceiling Instance { get; } = new();

        #endregion

        /// <summary>
        ///     A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>
        ///     Pure commands can be applied at compile time.
        /// </remarks>
        [Obsolete]
        public override bool IsPure => true;

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
                throw new PrexoniteException("Ceiling requires at least one argument.");

            var arg0 = args[0];

            return RunStatically(arg0, sctx);
        }

        public static PValue RunStatically(PValue arg0, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;

            return System.Math.Ceiling(x);
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
            switch (ins.Arguments)
            {
                case 1:
                    return CompilationFlags.PrefersCustomImplementation;
                case 0:
                default:
                    return CompilationFlags.PrefersRunStatically;
            }
        }

        private static readonly MethodInfo RunStaticallyMethod =
            typeof (Ceiling).GetMethod("RunStatically",
                new[] {typeof (PValue), typeof (StackContext)});

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            Abs._CallStaticFunc1(state, ins, RunStaticallyMethod);
        }

        #endregion
    }
}