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
using System.Diagnostics;
using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
/// </summary>
/// <remarks>
///     <para>
///         Returns null if no callable object is passed.
///     </para>
///     <para>
///         Uses the <see cref = "IIndirectCall" /> interface.
///     </para>
/// </remarks>
/// <seealso cref = "IIndirectCall" />
public sealed class Call : StackAwareCommand, ICilCompilerAware
{
    private Call()
    {
    }

    public const string Alias = @"call\perform";

    public static Call Instance { get; } = new();

    /// <summary>
    ///     Implementation of (ref f, [arg1, arg2, arg3, ..., argn]) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns null if no callable object is passed.
    ///     </para>
    ///     <para>
    ///         Uses the <see cref = "IIndirectCall" /> interface.
    ///     </para>
    ///     <para>
    ///         Wrap Lists in other lists, if you want to pass them without being unfolded: 
    ///         <code>
    ///             function main()
    ///             {   var myList = [1, 2, 3];
    ///             var f = xs => xs.Count;
    ///             print( call(f, [ myList ]) );
    ///             }
    /// 
    ///             //Prints "3"
    ///         </code>
    ///     </para>
    /// </remarks>
    /// <seealso cref = "IIndirectCall" />
    /// <param name = "sctx">The stack context in which to call the callable argument.</param>
    /// <param name = "args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by <see cref = "IIndirectCall.IndirectCall" /> or PValue Null if no callable object has been passed.</returns>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null)
            return PType.Null.CreatePValue();

        var iargs = FlattenArguments(sctx, args, 1);

        return args[0].IndirectCall(sctx, iargs.ToArray());
    }

    /// <summary>
    ///     Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns null if no callable object is passed.
    ///     </para>
    ///     <para>
    ///         Uses the <see cref = "IIndirectCall" /> interface.
    ///     </para>
    ///     <para>
    ///         Wrap Lists in other lists, if you want to pass them without being unfolded: 
    ///         <code>
    ///             function main()
    ///             {   var myList = [1, 2, 3];
    ///             var f = xs => xs.Count;
    ///             print( call(f, [ myList ]) );
    ///             }
    /// 
    ///             //Prints "3"
    ///         </code>
    ///     </para>
    /// </remarks>
    /// <seealso cref = "IIndirectCall" />
    /// <param name = "sctx">The stack context in which to call the callable argument.</param>
    /// <param name = "args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by <see cref = "IIndirectCall.IndirectCall" /> or PValue Null if no callable object has been passed.</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    [DebuggerStepThrough]
    public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args)
    {
        return FlattenArguments(sctx, args, 0);
    }

    /// <summary>
    ///     Takes an argument list and injects elements of top-level lists into that argument list.
    /// </summary>
    /// <param name = "sctx">The stack context in which to convert enumerables.</param>
    /// <param name = "args">The raw list of arguments to process.</param>
    /// <param name = "offset">The offset at which to start processing.</param>
    /// <returns>A copy of the argument list with top-level lists expanded.</returns>
    public static List<PValue> FlattenArguments(StackContext sctx, PValue[] args, int offset)
    {
        args ??= Array.Empty<PValue>();
        var iargs = new List<PValue>();
        for (var i = offset; i < args.Length; i++)
        {
            var arg = args[i];
            var folded = Map._ToEnumerable(sctx, arg);
            if (folded == null)
                iargs.Add(arg);
            else
                iargs.AddRange(folded);
        }
        return iargs;
    }

    public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
            return new NullContext(sctx);

        var iargs = FlattenArguments(sctx, args, 1);

        var callable = args[0];
        return CreateStackContext(sctx, callable, iargs.ToArray());
    }

    public static StackContext CreateStackContext(StackContext sctx, PValue callable,
        PValue[] args)
    {
        if (callable.Type is ObjectPType && callable.Value is IStackAware sa)
            return sa.CreateStackContext(sctx, args);
        else
            return new IndirectCallContext(sctx, callable, args);
    }

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

    #region Macro for partial application

    private readonly PartialCallWrapper _partialCall = new(Engine.CallAlias,
        EntityRef.Command.Create(Alias));

    public PartialMacroCommand Partial => _partialCall;

    #endregion
}