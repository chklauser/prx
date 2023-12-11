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

namespace Prexonite.Commands.Core.PartialApplication;

public class FunctionalPartialCall(PValue subject, PValue[] arguments) : IMaybeStackAware
{
    public PValue IndirectCall(StackContext sctx, PValue[] args)
    {
        return subject.IndirectCall(sctx, _getEffectiveArgs(args));
    }

    public bool TryDefer(StackContext sctx, PValue[] args,
        [NotNullWhen(true)] out StackContext? partialApplicationContext,
        [NotNullWhen(false)] out PValue? result)
    {
        var effectiveArgs = _getEffectiveArgs(args);

        partialApplicationContext = null;
        result = null;

        //The following code exists in a very similar form in PartialCall.cs, FlippedFunctionalPartialCall.cs
        if (subject.Type is ObjectPType)
        {
            var raw = subject.Value;
            if (raw is IStackAware stackAware)
            {
                partialApplicationContext = stackAware.CreateStackContext(sctx, effectiveArgs);
                return true;
            }

            if (raw is IMaybeStackAware partialApplication)
                return partialApplication.TryDefer(sctx, effectiveArgs,
                    out partialApplicationContext,
                    out result);
        }

        result = subject.IndirectCall(sctx, effectiveArgs);
        return false;
    }

    PValue[] _getEffectiveArgs(PValue[] args)
    {
        var effectiveArgs = new PValue[args.Length + arguments.Length];
        Array.Copy(arguments, effectiveArgs, arguments.Length);
        Array.Copy(args, 0, effectiveArgs, arguments.Length, args.Length);
        return effectiveArgs;
    }
}