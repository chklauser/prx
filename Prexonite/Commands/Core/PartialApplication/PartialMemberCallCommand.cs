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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication;

public class PartialMemberCallCommand :
    PartialApplicationCommandBase<PartialMemberCallCommand.MemberCallInfo>
{
    #region Singleton pattern

    private PartialMemberCallCommand()
    {
    }

    public static PartialMemberCallCommand Instance { get; } = new();

    #endregion

    public struct MemberCallInfo
    {
        public string MemberId;
        public PCall Call;
    }

    #region Overrides of PartialApplicationCommandBase<MemberCallInfo>

    protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings,
        PValue[] closedArguments, MemberCallInfo parameter)
    {
        return new PartialMemberCall(mappings, closedArguments, parameter.MemberId,
            parameter.Call);
    }

    protected override Type GetPartialCallRepresentationType(MemberCallInfo parameter)
    {
        return typeof (PartialMemberCall);
    }

    protected override MemberCallInfo FilterRuntimeArguments(StackContext sctx,
        ref ArraySegment<PValue> arguments)
    {
        if (arguments.Count < 2)
            throw new PrexoniteException(
                "Partial member call constructor needs call type and member id.");

        var lastIndex = arguments.Offset + arguments.Count - 1;
        var rawMemberId = arguments.Array[lastIndex];
        var rawCall = arguments.Array[lastIndex - 1];

        if (!rawMemberId.TryConvertTo(sctx, PType.String, out rawMemberId))
            throw new PrexoniteException(
                "Partial member call constructor expects the second but last argument to be the member id.");
        if (!rawCall.TryConvertTo(sctx, PType.Int, out rawCall))
            throw new PrexoniteException(
                $"Partial member call constructor expects the last argument to be the call type. (either {(int) PCall.Get} or {(int) PCall.Set})");

        MemberCallInfo info;
        info.MemberId = (string) rawMemberId.Value;
        info.Call = (PCall) (int) rawCall.Value;

        arguments = new ArraySegment<PValue>(arguments.Array, 0, arguments.Count - 2);
        return info;
    }

    protected override bool FilterCompileTimeArguments(
        ref ArraySegment<CompileTimeValue> staticArgv, out MemberCallInfo parameter)
    {
        parameter = default;
        if (staticArgv.Count < 2)
            return false;

        var lastIndex = staticArgv.Offset + staticArgv.Count - 1;
        var rawMemberId = staticArgv.Array[lastIndex];
        var rawCall = staticArgv.Array[lastIndex - 1];

        if (!rawMemberId.TryGetString(out parameter.MemberId) ||
            !rawCall.TryGetInt(out var rawCallInt32))
            return false;
        parameter.Call = (PCall) rawCallInt32;

        if (!Enum.IsDefined(typeof (PCall), parameter.Call))
            return false;

        staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array, staticArgv.Offset,
            staticArgv.Count - 2);
        return true;
    }

    private ConstructorInfo _partialMemberCallCtor;

    protected override void EmitConstructorCall(CompilerState state, MemberCallInfo parameter)
    {
        state.Il.Emit(OpCodes.Ldstr, parameter.MemberId);
        state.EmitLdcI4((int) parameter.Call);
        state.Il.Emit(
            OpCodes.Newobj,
            _partialMemberCallCtor ??= typeof (PartialMemberCall).GetConstructor(
                new[]
                {
                    typeof (int[]),
                    typeof (PValue[]),
                    typeof (string),
                    typeof (PCall)
                }));
    }

    #endregion
}

public class PartialMemberCall : PartialApplicationBase
{
    public string MemberId { [DebuggerStepThrough] get; }

    public PCall Call { [DebuggerStepThrough] get; }

    public PartialMemberCall(int[] mappings, PValue[] closedArguments, string memberId,
        PCall call) : base(mappings, closedArguments, 1)
    {
        MemberId = memberId;
        Call = call;
    }

    #region Overrides of PartialApplicationBase

    protected override PValue Invoke(StackContext sctx, PValue[] nonArguments,
        PValue[] arguments)
    {
        var result = nonArguments[0].DynamicCall(sctx, arguments, Call, MemberId);
        if (Call == PCall.Get)
            return result;
        else if (arguments.Length == 0)
            return PType.Null.CreatePValue();
        else
            return arguments[^1];
    }

    #endregion
}