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

using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    ///     Turns to arguments into a key-value pair
    /// </summary>
    /// <remarks>
    ///     Equivalent to:
    ///     <code>function pair(key, value) = key: value;</code>
    /// </remarks>
    public sealed class Pair : PCommand, ICilCompilerAware
    {
        private Pair()
        {
        }

        private static readonly Pair _instance = new Pair();

        public static Pair Instance
        {
            get { return _instance; }
        }

        /// <summary>
        ///     Turns to arguments into a key-value pair
        /// </summary>
        /// <param name = "args">The arguments to pass to this command. Array must contain 2 elements.</param>
        /// <param name = "sctx">Unused.</param>
        /// <remarks>
        ///     Equivalent to:
        ///     <code>function pair(key, value) = key: value;</code>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                args = new PValue[] {};

            if (args.Length < 2)
                return PType.Null.CreatePValue();
            else
                return PType.Object.CreatePValue(
                    new PValueKeyValuePair(
                        args[0] ?? PType.Null.CreatePValue(),
                        args[1] ?? PType.Null.CreatePValue()
                        ));
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            var argc = ins.Arguments;

            if (argc < 2)
            {
                state.EmitLoadNullAsPValue();
            }
            else
            {
                //pop excessive arguments
                for (var i = 2; i < argc; i++)
                    state.Il.Emit(OpCodes.Pop);

                //make pvkvp
                state.Il.Emit(OpCodes.Newobj, Compiler.Cil.Compiler.NewPValueKeyValuePair);

                //save pvkvp in temporary variable
                state.EmitStoreTemp(0);

                //PType.Object.CreatePValue(temp)
                state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.GetObjectPTypeSelector, null);
                state.EmitLoadTemp(0);
                state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreatePValueAsObject, null);
            }
        }

        #endregion
    }
}