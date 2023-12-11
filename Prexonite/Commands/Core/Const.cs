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

using System.Reflection;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Const : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static Const Instance { get; } = new();

    Const()
    {
    }

    #endregion

    public const string Alias = "const";

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        PValue constant;
        if (args.Length < 1)
            constant = PType.Null;
        else
            constant = args[0];

        return CreateConstFunction(constant, sctx);
    }

    class Impl : IIndirectCall
    {
        readonly PValue _value;

        public Impl(PValue value)
        {
            _value = value;
        }

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _value;
        }
    }

    MethodInfo? _createConstFunctionInfoCache;

    MethodInfo createConstFunction
    {
        get
        {
            return _createConstFunctionInfoCache ??= typeof (Const).GetMethod(nameof(CreateConstFunction),
                new[] {typeof (PValue), typeof (StackContext)})!;
        }
    }

    public static PValue CreateConstFunction(PValue constant, StackContext sctx)
    {
        return sctx.CreateNativePValue(new Impl(constant));
    }

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        var argc = ins.Arguments;
        if (argc > 1)
            state.EmitIgnoreArguments(argc - 1);

        state.EmitLoadLocal(state.SctxLocal);
        if (argc == 0)
            state.EmitLoadNullAsPValue();

        state.EmitCall(createConstFunction);
    }
}