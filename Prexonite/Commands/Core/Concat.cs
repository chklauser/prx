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
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    ///     Implementation of the <c>concat</c> command.
    /// </summary>
    public sealed class Concat : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Concat()
        {
        }

        private static readonly Concat _instance = new Concat();

        public static Concat Instance
        {
            get { return _instance; }
        }

        #endregion

        /// <summary>
        ///     Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
        /// <param name = "args">The list of fragments to concatenate.</param>
        /// <returns>The concatenated string.</returns>
        /// <remarks>
        ///     Please note that this method uses a string builder. The addition operator is faster for only two fragments.
        /// </remarks>
        public static string ConcatenateString(StackContext sctx, PValue[] args)
        {
            var elements = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var element = arg.Type is StringPType ? (string) arg.Value : arg.CallToString(sctx);
                elements[i] = element;
            }

            return String.Concat(elements);
        }

        /// <summary>
        ///     Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
        /// <param name = "args">The list of fragments to concatenate.</param>
        /// <returns>The concatenated string.</returns>
        /// <remarks>
        ///     Please note that this method uses a string builder. The addition operator is faster for only two fragments.
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            return ConcatenateString(sctx, args);
        }

        /// <summary>
        ///     Concatenates all arguments and return one big string.
        /// </summary>
        /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
        /// <param name = "args">The list of fragments to concatenate.</param>
        /// <returns>A PValue containing the concatenated string.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}