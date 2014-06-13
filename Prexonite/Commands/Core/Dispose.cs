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
using System.ComponentModel;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    ///     Command that calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
    /// </summary>
    /// <remarks>
    ///     Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.
    /// </remarks>
    public sealed class Dispose : PCommand, ICilCompilerAware
    {
        private Dispose()
        {
        }

        private static readonly Dispose _instance = new Dispose();

        public static Dispose Instance
        {
            get { return _instance; }
        }

        public const string DisposeMemberId = "Dispose";

        /// <summary>
        ///     Executes the dispose function.<br />
        ///     Calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
        /// </summary>
        /// <param name = "sctx">The stack context. Ignored by this command.</param>
        /// <param name = "args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks>
        ///     <para>
        ///         Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        /// <summary>
        ///     Executes the dispose function.<br />
        ///     Calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
        /// </summary>
        /// <param name = "sctx">The stack context. Ignored by this command.</param>
        /// <param name = "args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks>
        ///     <para>
        ///         Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            foreach (var arg in args)
                if (arg != null)
                {
                    RunStatically(arg, sctx);
                }
            return PType.Null.CreatePValue();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RunStatically(PValue arg, StackContext sctx)
        {
            PValue dummy;
            if (arg.Type is ObjectPType)
            {
                var toDispose = arg.Value as IDisposable;
                if (toDispose != null)
                    toDispose.Dispose();
                else
                {
                    var isObj = arg.Value as IObject;
                    if (isObj != null)
                    {
                        isObj.TryDynamicCall(
                            sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
                    }
                }
            }
            else
            {
                arg.TryDynamicCall(sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
            }
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            switch (ins.Arguments)
            {
                case 0:
                    if (!ins.JustEffect)
                        state.EmitLoadNullAsPValue();
                    break;
                case 1:
                    //Emit call to RunStatically(PValue, StackContext)
                    state.EmitLoadLocal(state.SctxLocal);
                    var run =
                        typeof (Dispose).GetMethod("RunStatically",
                            new[] {typeof (PValue), typeof (StackContext)});
                    state.Il.EmitCall(OpCodes.Call, run, null);
                    if (!ins.JustEffect)
                        state.EmitLoadNullAsPValue();
                    break;
                default:
                    //Emit call to RunStatically(StackContext, PValue[])
                    state.EmitEarlyBoundCommandCall(typeof (Dispose), ins);
                    break;
            }
        }

        #endregion
    }
}