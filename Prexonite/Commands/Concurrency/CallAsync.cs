// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Diagnostics;
using System.Threading;
using Prexonite.Commands.Core;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency
{
    public class CallAsync : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private CallAsync()
        {
        }

        private static readonly CallAsync _instance = new CallAsync();

        public static CallAsync Instance
        {
            get { return _instance; }
        }

        #endregion

        public const string Alias = @"call\async\perform";

        #region Overrides of PCommand

        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                return PType.Null.CreatePValue();

            var iargs = Call.FlattenArguments(sctx, args, 1);

            var retChan = new Channel();
            var T = new Thread(() =>
                {
                    PValue result;
                    try
                    {
                        result = args[0].IndirectCall(sctx, iargs.ToArray());
                    }
                    catch (Exception ex)
                    {
                        result = sctx.CreateNativePValue(ex);
                    }
                    retChan.Send(result);
                })
                {
                    IsBackground = true
                };
            T.Start();
            return PType.Object.CreatePValue(retChan);
        }

        public static Channel RunAsync(StackContext sctx, Func<PValue> comp)
        {
            var retChan = new Channel();
            var T = new Thread(() => retChan.Send(comp()))
                {
                    IsBackground = true
                };
            T.Start();
            return retChan;
        }

        #endregion

        #region Implementation of ICilCompilerAware

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

        #region Partial application via call\star

        private readonly PartialCallWrapper _partial = new PartialCallWrapper(
            Engine.Call_AsyncAlias, TODO);

        public PartialCallWrapper Partial
        {
            [DebuggerStepThrough]
            get { return _partial; }
        }

        #endregion
    }
}