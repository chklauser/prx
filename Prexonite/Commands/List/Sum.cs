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

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Sum : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Sum()
    {
    }

    public static Sum Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        //let sum xs acc = Seq.foldl (fun a b -> a + b) acc xs

        PValue acc;
        IEnumerable<PValue> xsArgs;

        if (args.Length == 0)
            return PType.Null;

        if (args.Length == 1)
        {
            acc = PType.Null;
            xsArgs = args;
        }
        else
        {
            acc = args[^1];
            xsArgs = args.Take(args.Length - 1);
        }

        var xss = xsArgs.Select(e => Map._ToEnumerable(sctx, e)).Where(e => e != null);

        foreach (var xs in xss)
        foreach (var x in xs)
            acc = acc.Addition(sctx, x);

        return acc;
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