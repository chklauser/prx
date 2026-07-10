using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication;

public sealed class PartialStaticCallCommand
    : PartialWithPTypeCommandBase<RuntimeStaticCallInfo, CompileTimeStaticCallInfo>
{
    PartialStaticCallCommand() { }

    public static PartialStaticCallCommand Instance { get; } = new();

    ConstructorInfo? _partialStaticCallCtor;

    protected override ConstructorInfo GetConstructorCtor(CompileTimeStaticCallInfo parameter)
    {
        return _partialStaticCallCtor ??=
            typeof(PartialStaticCall).GetConstructor([
                typeof(int[]),
                typeof(PValue[]),
                typeof(PCall),
                typeof(string),
                typeof(PType),
            ])
            ?? throw new InvalidOperationException(
                $"{nameof(PartialStaticCall)} does not have an (int[], PValue[], PCall, string, PType) constructor."
            );
    }

    protected override void EmitConstructorCall(
        CompilerState state,
        CompileTimeStaticCallInfo parameter
    )
    {
        state.EmitLdcI4((int)parameter.Call);
        state.Il.Emit(OpCodes.Ldstr, parameter.MemberId);
        base.EmitConstructorCall(state, parameter);
    }

    protected override bool FilterCompileTimeArguments(
        ref Span<CompileTimeValue> staticArgv,
        [NotNullWhen(true)] out CompileTimeStaticCallInfo? parameter
    )
    {
        parameter = default;
        if (staticArgv.Length < 3)
            return false;

        //Read call and memberId
        var rawMemberId = staticArgv[^1];
        var rawCall = staticArgv[^2];
        if (!rawMemberId.TryGetString(out var memberId))
            return false;
        if (!rawCall.TryGetInt(out var callValue) || !Enum.IsDefined(typeof(PCall), callValue))
            return false;
        var call = (PCall)callValue;

        //Transfer control to base implementation for PType handling
        staticArgv = staticArgv[..^2];
        if (!base.FilterCompileTimeArguments(ref staticArgv, out parameter) || parameter == null)
            return false;

        //Combine call information
        parameter.MemberId = memberId;
        parameter.Call = call;
        return true;
    }

    protected override RuntimeStaticCallInfo FilterRuntimeArguments(
        StackContext sctx,
        ref Span<PValue> arguments
    )
    {
        if (arguments.Length < 3)
            throw new PrexoniteException(
                $"{PartialApplicationKind} requires a PType, a call-kind and a member id."
            );

        //read call and memberId
        var memberId = arguments[^1].CallToString(sctx);
        var rawCall = arguments[^2];

        PCall call;
        if (rawCall is { Type: ObjectPType, Value: PCall })
        {
            call = (PCall)rawCall.Value;
        }
        else
        {
            var callValue = (int)rawCall.ConvertTo(sctx, PType.Int).Value!;
            if (!Enum.IsDefined(typeof(PCall), callValue))
                throw new PrexoniteException($"The value {callValue} is not a valid PCall value.");
            call = (PCall)callValue;
        }

        //Call base implementation to handle PType argument
        arguments = arguments[..^2];
        var p = base.FilterRuntimeArguments(sctx, ref arguments);

        //Combine static call information
        p.Call = call;
        p.MemberId = memberId;
        return p;
    }

    protected override string PartialApplicationKind => "Partial static call";

    protected override IIndirectCall CreatePartialApplication(
        StackContext sctx,
        int[] mappings,
        PValue[] closedArguments,
        RuntimeStaticCallInfo parameter
    )
    {
        return new PartialStaticCall(
            mappings,
            closedArguments,
            parameter.Call,
            parameter.MemberId,
            parameter.Type
        );
    }

    protected override Type GetPartialCallRepresentationType(CompileTimeStaticCallInfo parameter)
    {
        return typeof(PartialStaticCall);
    }
}

public record RuntimeStaticCallInfo : RuntimePTypeInfo<RuntimeStaticCallInfo>, IStaticCallInfo
{
    public PCall Call { get; set; } = PCall.Get;
    public string MemberId { get; set; } = null!;
}

public record CompileTimeStaticCallInfo
    : CompileTimePTypeInfo<CompileTimeStaticCallInfo>,
        IStaticCallInfo
{
    public PCall Call { get; set; } = PCall.Get;
    public string MemberId { get; set; } = null!;
}

public interface IStaticCallInfo
{
    PCall Call { get; }
    string MemberId { get; }
}
