using System;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialStaticCallCommand : PartialWithPTypeCommandBase<StaticCallInfo>
    {
        private static readonly PartialStaticCallCommand _instance = new PartialStaticCallCommand();
        private PartialStaticCallCommand()
        {
        }
        public static PartialStaticCallCommand Instance
        {
            get { return _instance; }
        }

        private ConstructorInfo _partialStaticCallCtor;

        protected override ConstructorInfo GetConstructorCtor(StaticCallInfo parameter)
        {
            return _partialStaticCallCtor ??
                   (_partialStaticCallCtor =
                    typeof(PartialStaticCall).GetConstructor(new[] { typeof(int[]), typeof(PValue[]), typeof(PCall), typeof(string), typeof(PType) }));
        }

        protected override void EmitConstructorCall(Compiler.Cil.CompilerState state, StaticCallInfo parameter)
        {
            state.EmitLdcI4((int) parameter.Call);
            state.Il.Emit(OpCodes.Ldstr, parameter.MemberId);
            base.EmitConstructorCall(state, parameter);
        }

        protected override bool FilterCompileTimeArguments(ref System.ArraySegment<CompileTimeValue> staticArgv, out StaticCallInfo parameter)
        {
            parameter = default(StaticCallInfo);
            if(staticArgv.Count < 3)
                return false;
            
            //Read call and memberId
            var lastIdx = staticArgv.Offset + staticArgv.Count;
            var rawMemberId = staticArgv.Array[lastIdx-1];
            var rawCall = staticArgv.Array[lastIdx - 2];
            string memberId;
            if(!rawMemberId.TryGetString(out memberId))
                return false;
            int callValue;
            if(!rawCall.TryGetInt(out callValue) || !Enum.IsDefined(typeof(PCall),callValue))
                return false;
            var call = (PCall) callValue;

            //Transfer control to base implementation for PType handling
            staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array, staticArgv.Offset, staticArgv.Count - 2);
            if(!base.FilterCompileTimeArguments(ref staticArgv, out parameter))
                return false;

            //Combine call information
            parameter.MemberId = memberId;
            parameter.Call = call;
            return true;
        }

        protected override StaticCallInfo FilterRuntimeArguments(StackContext sctx, ref ArraySegment<PValue> arguments)
        {
            if(arguments.Count < 3)
                throw new PrexoniteException(string.Format("{0} requires a PType, a call-kind and a member id.", PartialApplicationKind));

            //read call and memberId
            var lastIdx = arguments.Offset + arguments.Count;
            var memberId = arguments.Array[lastIdx - 1].CallToString(sctx);
            var rawCall = arguments.Array[lastIdx - 2];

            PCall call;
            if(rawCall.Type is ObjectPType && rawCall.Value is PCall)
            {
                call = (PCall) rawCall.Value;
            }
            else
            {
                var callValue = (int)rawCall.ConvertTo(sctx, PType.Int).Value;
                if(!Enum.IsDefined(typeof(PCall),callValue))
                    throw new PrexoniteException(string.Format("The value {0} is not a valid PCall value.", callValue));
                call = (PCall) callValue;
            }

            //Call base implementation to handle PType argument
            arguments = new ArraySegment<PValue>(arguments.Array, arguments.Offset, arguments.Count - 2);
            var p = base.FilterRuntimeArguments(sctx, ref arguments);

            //Combine static call information
            p.Call = call;
            p.MemberId = memberId;
            return p;
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial static call"; }
        }

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, StaticCallInfo parameter)
        {
            return new PartialStaticCall(mappings, closedArguments, parameter.Call, parameter.MemberId, parameter.Type);
        }

        protected override Type GetPartialCallRepresentationType(StaticCallInfo parameter)
        {
            return typeof (PartialStaticCall);
        }
    }

    /// <summary>
    /// Holds information about a static call at compile- and run-time.
    /// </summary>
    public class StaticCallInfo : PTypeInfo
    {
        /// <summary>
        /// The call type.
        /// </summary>
        public PCall Call;

        /// <summary>
        /// The id of the static member to call.
        /// </summary>
        public string MemberId;
    }
}