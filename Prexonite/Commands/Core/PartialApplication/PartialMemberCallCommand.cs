using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialMemberCallCommand : PartialApplicationCommandBase<PartialMemberCallCommand.MemberCallInfo>
    {

        #region Singleton pattern

        private PartialMemberCallCommand()
        {
        }

        private static readonly PartialMemberCallCommand _instance = new PartialMemberCallCommand();

        public static PartialMemberCallCommand Instance
        {
            get { return _instance; }
        }

        #endregion 

        public struct MemberCallInfo
        {
            public string MemberId;
            public PCall Call;
        }

        #region Overrides of PartialApplicationCommandBase<MemberCallInfo>

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, MemberCallInfo parameter)
        {
            return new PartialMemberCall(mappings, closedArguments, parameter.MemberId, parameter.Call);
        }

        protected override Type GetPartialCallRepresentationType(MemberCallInfo parameter)
        {
            return typeof(PartialMemberCall);
        }

        protected override MemberCallInfo FilterRuntimeArguments(StackContext sctx, ref ArraySegment<PValue> arguments)
        {
            if (arguments.Count < 2)
                throw new PrexoniteException("Partial member call constructor needs call type and member id.");

            var lastIndex = arguments.Offset + arguments.Count - 1;
            var rawMemberId = arguments.Array[lastIndex];
            var rawCall = arguments.Array[lastIndex-1];

            if(!rawMemberId.TryConvertTo(sctx, PType.String, out rawMemberId))
                throw new PrexoniteException("Partial member call constructor expects the second but last argument to be the member id.");
            if(!rawCall.TryConvertTo(sctx, PType.Int, out rawCall))
                throw new PrexoniteException(string.Format("Partial member call constructor expects the last argument to be the call type. (either {0} or {1})", (int)PCall.Get, (int)PCall.Set));

            MemberCallInfo info;
            info.MemberId = (string) rawMemberId.Value;
            info.Call = (PCall) ((int)rawCall.Value);

            arguments = new ArraySegment<PValue>(arguments.Array, 0, arguments.Count - 2);
            return info;
        }

        protected override bool FilterCompileTimeArguments(ref ArraySegment<CompileTimeValue> staticArgv, out MemberCallInfo parameter)
        {
            parameter = default(MemberCallInfo);
            if(staticArgv.Count < 2)
                return false;

            var lastIndex = staticArgv.Offset + staticArgv.Count - 1;
            var rawMemberId = staticArgv.Array[lastIndex];
            var rawCall = staticArgv.Array[lastIndex - 1];

            int rawCallInt32;
            if(!rawMemberId.TryGetString(out parameter.MemberId) || !rawCall.TryGetInt(out rawCallInt32))
                return false;
            parameter.Call = (PCall) rawCallInt32;

            if(!Enum.IsDefined(typeof (PCall), parameter.Call))
                return false;

            staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array,staticArgv.Offset, staticArgv.Count-2);
            return true;
        }

        private ConstructorInfo _partialMemberCallCtor;

        protected override void EmitConstructorCall(Compiler.Cil.CompilerState state, MemberCallInfo parameter)
        {
            state.Il.Emit(OpCodes.Ldstr,parameter.MemberId);
            state.EmitLdcI4((int) parameter.Call);
            state.Il.Emit(
                OpCodes.Newobj,
                _partialMemberCallCtor
                ??
                (_partialMemberCallCtor =
                 typeof (PartialMemberCall).GetConstructor(
                     new[]
                     {
                         typeof (int[]), 
                         typeof (PValue[]), 
                         typeof (string), 
                         typeof (PCall)
                     })));
        }

        #endregion
    }

    public class PartialMemberCall : PartialApplicationBase
    {
        private readonly string _memberId;
        private readonly PCall _call;

        public string MemberId
        {
            [DebuggerStepThrough]
            get { return _memberId; }
        }

        public PCall Call
        {
            [DebuggerStepThrough]
            get { return _call; }
        }

        public PartialMemberCall(int[] mappings, PValue[] closedArguments, string memberId, PCall call) : base(mappings, closedArguments, 1)
        {
            _memberId = memberId;
            _call = call;
        }

        #region Overrides of PartialApplicationBase

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
        {
            var result = nonArguments[0].DynamicCall(sctx, arguments, _call, _memberId);
            if (_call == PCall.Get)
                return result;
            else if (arguments.Length == 0)
                return PType.Null.CreatePValue();
            else
                return arguments[arguments.Length - 1];
        }

        #endregion
    }
}
