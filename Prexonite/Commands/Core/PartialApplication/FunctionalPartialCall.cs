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
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class FunctionalPartialCall : IMaybeStackAware
    {
        private readonly PValue _subject;
        private readonly PValue[] _closedArguments;

        public FunctionalPartialCall(PValue subject, PValue[] closedArguments)
        {
            _subject = subject;
            _closedArguments = closedArguments;
        }

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _subject.IndirectCall(sctx, _getEffectiveArgs(args));
        }

        public bool TryDefer(StackContext sctx, PValue[] args,
            out StackContext partialApplicationContext, out PValue result)
        {
            var effectiveArgs = _getEffectiveArgs(args);

            partialApplicationContext = null;
            result = null;

            //The following code exists in a very similar form in PartialCall.cs, FlippedFunctionalPartialCall.cs
            if ((_subject.Type is ObjectPType))
            {
                var raw = _subject.Value;
                var stackAware = raw as IStackAware;
                if (stackAware != null)
                {
                    partialApplicationContext = stackAware.CreateStackContext(sctx, effectiveArgs);
                    return true;
                }

                var partialApplication = raw as IMaybeStackAware;
                if (partialApplication != null)
                    return partialApplication.TryDefer(sctx, effectiveArgs,
                        out partialApplicationContext,
                        out result);
            }

            result = _subject.IndirectCall(sctx, effectiveArgs);
            return false;
        }

        private PValue[] _getEffectiveArgs(PValue[] args)
        {
            var effectiveArgs = new PValue[args.Length + _closedArguments.Length];
            Array.Copy(_closedArguments, effectiveArgs, _closedArguments.Length);
            Array.Copy(args, 0, effectiveArgs, _closedArguments.Length, args.Length);
            return effectiveArgs;
        }
    }
}