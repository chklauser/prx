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
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication;

/// <summary>
///     <para>A more efficient implementation of the partial application pattern <code>obj.(?,c_1,c_2,c_3,...,c_n)</code>.</para>
///     <para>For a more general implementation of partial application of indirect calls, see <see cref = "PartialCall" />.</para>
/// </summary>
public class FlippedFunctionalPartialCall : IMaybeStackAware
{
    readonly PValue _subject;
    readonly PValue[] _closedArguments;

    /// <summary>
    ///     Creates a new flipped, functional partial call, implementing a partial call to <code><paramref name = "subject" />.(?,<paramref
    ///     name = "closedArguments" />)</code>.
    /// </summary>
    /// <param name = "subject">The subject of the indirect call.</param>
    /// <param name = "closedArguments">The closed arguments. Will be inserted starting at parameter index 1.</param>
    public FlippedFunctionalPartialCall(PValue subject, PValue[] closedArguments)
    {
        _subject = subject;
        _closedArguments = closedArguments;
    }

    public PValue IndirectCall(StackContext sctx, PValue[] args)
    {
        return _subject.IndirectCall(sctx, _getEffectiveArgs(args));
    }

    PValue[] _getEffectiveArgs(PValue[] args)
    {
        var effectiveArgs =
            new PValue[System.Math.Max(args.Length, 1) + _closedArguments.Length];
        if (args.Length > 0 && args[0] != null)
            effectiveArgs[0] = args[0];
        else
            effectiveArgs[0] = PType.Null;
        Array.Copy(_closedArguments, 0, effectiveArgs, 1, _closedArguments.Length);
        Array.Copy(args, System.Math.Min(1, args.Length), effectiveArgs,
            _closedArguments.Length + 1, System.Math.Max(args.Length - 1, 0));
        return effectiveArgs;
    }

    public bool TryDefer(StackContext sctx, PValue[] args,
        out StackContext partialApplicationContext, out PValue result)
    {
        var effectiveArgs = _getEffectiveArgs(args);

        partialApplicationContext = null;
        result = null;

        //The following code exists in a very similar form in PartialCall.cs, FunctionalPartialCall.cs
        if (_subject.Type is ObjectPType)
        {
            var raw = _subject.Value;
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

        result = _subject.IndirectCall(sctx, effectiveArgs);
        return false;
    }
}