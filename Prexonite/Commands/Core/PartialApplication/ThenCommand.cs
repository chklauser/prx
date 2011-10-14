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
using System.Diagnostics;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class ThenCommand : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private ThenCommand()
        {
        }

        private static readonly ThenCommand _instance = new ThenCommand();

        public static ThenCommand Instance
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
            if (args.Length < 2)
                throw new PrexoniteException("then command requires two arguments.");

            return sctx.CreateNativePValue(new CallComposition(args[0], args[1]));
        }

        #endregion

        #region Implementation of ICilCompilerAware

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class CallComposition : IIndirectCall
    {
        private readonly PValue _innerExpression;
        private readonly PValue _outerExpression;

        public PValue InnerExpression
        {
            [DebuggerStepThrough]
            get { return _innerExpression; }
        }

        public PValue OuterExpression
        {
            [DebuggerStepThrough]
            get { return _outerExpression; }
        }

        public CallComposition(PValue innerExpression, PValue outerExpression)
        {
            if (innerExpression == null)
                throw new ArgumentNullException("innerExpression");
            if (outerExpression == null)
                throw new ArgumentNullException("outerExpression");
            _innerExpression = innerExpression;
            _outerExpression = outerExpression;
        }

        #region Implementation of IIndirectCall

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _outerExpression.IndirectCall(sctx,
                new[] {_innerExpression.IndirectCall(sctx, args)});
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} then ({1})", InnerExpression, OuterExpression);
        }
    }
}