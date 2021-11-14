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

/// <summary>
///     Implementation of the foldr function.
/// </summary>
/// <remarks>
///     <code>function foldr(ref f, right, source)
///         {
///         var lst = [];
///         foreach(var e in source)
///             lst[] = e;
///         for(var i = lst.Count-1; i>=0; i--)
///             right = f(lst[i],right);
///         return right;
///         }</code>
/// </remarks>
public class FoldR : PCommand, ICilCompilerAware
{
    #region Singleton

    private FoldR()
    {
    }

    public static FoldR Instance { get; } = new();

    #endregion

    public static PValue Run(
        StackContext sctx, IIndirectCall f, PValue right, IEnumerable<PValue> source)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (f == null)
            throw new ArgumentNullException(nameof(f));
        right ??= PType.Null.CreatePValue();
        source ??= Array.Empty<PValue>();

        var lst = new List<PValue>(source);

        for (var i = lst.Count - 1; i >= 0; i--)
        {
            right = f.IndirectCall(sctx, new[] {lst[i], right});
        }
        return right;
    }

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

        //Get f
        IIndirectCall f;
        if (args.Length < 1)
            throw new PrexoniteException("The foldr command requires a function argument.");
        else
            f = args[0];

        //Get left
        var left = args.Length < 2 ? null : args[1];

        //Get the source
        IEnumerable<PValue> source;
        if (args.Length == 3)
        {
            var psource = args[2];
            source = Map._ToEnumerable(sctx, psource) ?? new[] {psource};
        }
        else
        {
            var lstsource = new List<PValue>();
            for (var i = 1; i < args.Length; i++)
            {
                var multiple = Map._ToEnumerable(sctx, args[i]);
                if (multiple != null)
                    lstsource.AddRange(multiple);
                else
                    lstsource.Add(args[i]);
            }
            source = lstsource;
        }

        return Run(sctx, f, left, source);
    }

    /// <summary>
    ///     A flag indicating whether the command acts like a pure function.
    /// </summary>
    /// <remarks>
    ///     Pure commands can be applied at compile time.
    /// </remarks>
    [Obsolete]
    public override bool IsPure => false; //use of indirect call

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}