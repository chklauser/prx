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
using System.Collections.Generic;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List;

public class HeadTail : PCommand, ICilCompilerAware
{
    #region Singleton

    HeadTail()
    {
    }

    public static HeadTail Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        PValue head;
        var nextArg = ((IEnumerable<PValue>) args).GetEnumerator();
        IEnumerator<PValue> nextX;
        try
        {
            if (!nextArg.MoveNext())
                throw new PrexoniteException("headtail requires at least one argument.");
            var arg = nextArg.Current;
            var xs = Map._ToEnumerable(sctx, arg);
            nextX = xs.GetEnumerator();
            try
            {
                if (!nextX.MoveNext())
                    return PType.Null;
                head = nextX.Current;
            }
            catch (Exception)
            {
                nextX.Dispose();
                throw;
            }
        }
        catch (Exception)
        {
            nextArg.Dispose();
            throw;
        }

        return
            (PValue)
            new List<PValue>
            {
                head,
                sctx.CreateNativePValue(
                    new Coroutine(new CoroutineContext(sctx, _tail(sctx, nextX, nextArg))))
            };
    }

    static IEnumerable<PValue> _tail(StackContext sctx, IEnumerator<PValue> current,
        IEnumerator<PValue> remaining)
    {
        using (current)
            while (current.MoveNext())
                yield return current.Current;
        using (remaining)
        {
            while (remaining.MoveNext())
            {
                var xs = Map._ToEnumerable(sctx, remaining.Current);
                foreach (var x in xs)
                    yield return x;
            }
        }
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}