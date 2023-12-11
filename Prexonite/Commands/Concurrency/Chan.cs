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
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;

namespace Prexonite.Commands.Concurrency;

public class Chan : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Chan()
    {
    }

    public static Chan Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return PType.Object.CreatePValue(new Channel());
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    static readonly ConstructorInfo _channelCtor =
        typeof (Channel).GetConstructor(Array.Empty<Type>())!;

    static readonly ConstructorInfo _newPValue =
        typeof (PValue).GetConstructor(new[] {typeof (object), typeof (PType)})!;

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        state.EmitIgnoreArguments(ins.Arguments);
        state.Il.Emit(OpCodes.Newobj, _channelCtor);
        PType.PrexoniteObjectTypeProxy._ImplementInCil(state, typeof (Channel));
        state.Il.Emit(OpCodes.Newobj, _newPValue);
    }

    #endregion
}