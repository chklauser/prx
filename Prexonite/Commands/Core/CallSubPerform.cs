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
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class CallSubPerform : PCommand, ICilCompilerAware
    {
        #region singleton pattern

        private static readonly CallSubPerform _instance = new CallSubPerform();

        public static CallSubPerform Instance
        {
            get { return _instance; }
        }

        private CallSubPerform()
        {
        }

        #endregion

        #region Overrides of PCommand

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

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args.Length < 1)
                throw new PrexoniteException(
                    "call\\sub\\perform needs at least one argument, the function to call.");
            var fpv = args[0];

            var iargs = Call.FlattenArguments(sctx, args, 1).ToArray();

            return RunStatically(sctx, fpv, iargs);
        }

        public static PValue RunStatically(StackContext sctx, PValue fpv, PValue[] iargs)
        {
            return RunStatically(sctx, fpv, iargs, false);
        }

        public static PValue RunStatically(StackContext sctx, PValue fpv, PValue[] iargs,
            bool useIndirectCallAsFallback)
        {
            IStackAware f;
            IMaybeStackAware m;
            CilClosure cilClosure;
            PFunction func = null;
            PVariable[] sharedVars = null;

            PValue result;
            ReturnMode returnMode;

            if ((cilClosure = fpv.Value as CilClosure) != null)
            {
                func = cilClosure.Function;
                sharedVars = cilClosure.SharedVariables;
            }

            if ((func = func ?? fpv.Value as PFunction) != null && func.HasCilImplementation)
            {
                func.CilImplementation.Invoke(
                    func, CilFunctionContext.New(sctx, func), iargs, sharedVars ?? new PVariable[0],
                    out result, out returnMode);
            }
            else if ((f = fpv.Value as IStackAware) != null)
            {
                //Create stack context, let the engine execute it
                var subCtx = f.CreateStackContext(sctx, iargs);
                sctx.ParentEngine.Process(subCtx);
                result = subCtx.ReturnValue;
                returnMode = subCtx.ReturnMode;
            }
            else if ((m = fpv.Value as IMaybeStackAware) != null)
            {
                StackContext subCtx;
                if (m.TryDefer(sctx, iargs, out subCtx, out result))
                {
                    sctx.ParentEngine.Process(subCtx);
                    result = subCtx.ReturnValue;
                    returnMode = subCtx.ReturnMode;
                }
                else if (useIndirectCallAsFallback)
                {
                    returnMode = ReturnMode.Exit;
                }
                else
                {
                    throw new PrexoniteException(
                        string.Format(
                            "Invocation of {0} did not produce a valid return mode. " +
                                "Only Prexonite functions have a return mode.",
                            fpv.CallToString(sctx)));
                }
            }
            else if (useIndirectCallAsFallback)
            {
                result = fpv.IndirectCall(sctx, iargs);
                returnMode = ReturnMode.Exit;
            }
            else
            {
                throw new PrexoniteException(
                    "call\\sub\\perform requires its argument to be stack aware.");
            }

            return new PValueKeyValuePair(sctx.CreateNativePValue(returnMode), result);
        }

        #endregion

        #region Implementation of ICilCompilerAware

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + GetType().Name +
                " does not support CIL compilation via ICilCompilerAware.");
        }

        #endregion
    }
}