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
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    ///     Represents a partial application of an indirect call (the default call interface in Prexonite)
    /// </summary>
    public class PartialCall : PartialApplicationBase
    {
        public PartialCall(int[] mappings, PValue[] closedArguments)
            : base(mappings, closedArguments, 1)
        {
        }

        #region Overrides of PartialApplicationBase

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments,
            PValue[] arguments)
        {
            return nonArguments[0].IndirectCall(sctx, arguments);
        }

        protected override bool DoTryDefer(StackContext sctx, PValue[] nonArguments,
            PValue[] arguments, out StackContext partialApplicationContext, out PValue result)
        {
            partialApplicationContext = null;
            result = null;

            //The following code exists in a very similar form in FunctionalPartialCall.cs, FlippedFunctionalPartialCall.cs
            if (nonArguments[0].Type is ObjectPType)
            {
                var raw = nonArguments[0].Value;
                if (raw is IStackAware stackAware)
                {
                    partialApplicationContext = stackAware.CreateStackContext(sctx, arguments);
                    return true;
                }

                if (raw is IMaybeStackAware partialApplication)
                    return partialApplication.TryDefer(sctx, arguments,
                        out partialApplicationContext,
                        out result);
            }

            result = Invoke(sctx, nonArguments, arguments);
            return false;
        }

        #endregion
    }
}