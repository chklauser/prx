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
using System.Diagnostics;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Lazy
{
    public class ForceCommand : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private ForceCommand()
        {
        }

        private static readonly ForceCommand _instance = new ForceCommand();

        public static ForceCommand Instance
        {
            get { return _instance; }
        }

        #endregion

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
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length < 1)
                throw new PrexoniteException("force requires an argument.");

            var arg = args[0] ?? PType.Null;
            if (arg.IsNull)
                return PType.Null;

            return Force(sctx, arg);
        }

        public static PValue Force(StackContext sctx, PValue arg)
        {
            var t = arg.Value as Thunk;

            var result = t != null ? t.Force(sctx) : arg;

            Debug.Assert(!(result.Value is Thunk), "Force wanted to return an unevaluated thunk.");

            return result;
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
    }
}