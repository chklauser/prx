using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands;

public abstract class StaticPatternCommand : PCommand, ICilCompilerAware
{
    public CompilationFlags CheckQualification(Instruction ins) =>
        CompilationFlags.PrefersRunStatically;

    public void ImplementInCil(CompilerState state, Instruction ins) =>
        throw new NotSupportedException("This pattern helper uses RunStatically.");
}

public sealed class PatternMemberProbe : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\member\probe";
    public static PatternMemberProbe Instance { get; } = new();

    PatternMemberProbe() { }

    sealed class MissingMemberMarker
    {
        internal static MissingMemberMarker Instance { get; } = new();
    }

    static readonly PValue _missingMember = new(
        MissingMemberMarker.Instance,
        PType.Object[typeof(MissingMemberMarker)]
    );

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var subject = args.Length > 0 ? args[0] : PType.Null;
        var memberName =
            args.Length > 1
                ? (string?)args[1].ConvertTo(sctx, PType.String, false).Value ?? ""
                : "";
        return ProbeMember(sctx, subject, memberName);
    }

    public static PValue ProbeMember(StackContext sctx, PValue subject, string memberName)
    {
        var handled = subject.TryDynamicCall(
            sctx,
            ReadOnlySpan<PValue>.Empty,
            PCall.Get,
            memberName,
            out var value
        );
        return handled ? value ?? PType.Null : _missingMember;
    }

    public static bool IsPresent(PValue value) => !ReferenceEquals(value, _missingMember);

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 1 && staticArgv[0].TryGetString(out _);

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        staticArgv[0].TryGetString(out var memberName);
        state.EmitStoreTemp(0);
        state.EmitLoadLocal(state.SctxLocal);
        state.EmitLoadTemp(0);
        state.Il.Emit(OpCodes.Ldstr, memberName!);
        state.EmitCall(
            typeof(PatternMemberProbe).GetMethod(
                nameof(ProbeMember),
                [typeof(StackContext), typeof(PValue), typeof(string)]
            )!
        );
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

public sealed class PatternMemberPresent : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\member\present";
    public static PatternMemberPresent Instance { get; } = new();

    PatternMemberPresent() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args) =>
        args.Length > 0 && PatternMemberProbe.IsPresent(args[0]);

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 0;

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        state.EmitCall(typeof(PatternMemberProbe).GetMethod(nameof(PatternMemberProbe.IsPresent))!);
        state.EmitWrapBool();
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

public sealed class PatternOutputAt : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\output\at";
    public static PatternOutputAt Instance { get; } = new();

    PatternOutputAt() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0)
            return PType.Null;
        var index =
            args.Length > 1 ? (int?)args[1].ConvertTo(sctx, PType.Int, false).Value ?? 0 : 0;
        return OutputAt(args[0], index);
    }

    public static PValue OutputAt(PValue subject, int index)
    {
        if (subject.Value is List<PValue> list)
            return OutputAt(list, index);
        return index == 0 ? subject : PType.Null;
    }

    public static PValue OutputAt(List<PValue> list, int index) =>
        index >= 0 && index < list.Count ? list[index] : PType.Null;

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 1 && staticArgv[0].TryGetInt(out _);

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        staticArgv[0].TryGetInt(out var index);
        Type subjectType;
        if (state.TryGetListPatternLocation(out var location) && !location.IsShape)
        {
            state.Il.Emit(OpCodes.Pop);
            state.EmitLoadLocal(location.Local);
            subjectType = typeof(List<PValue>);
        }
        else
        {
            subjectType = typeof(PValue);
        }
        state.EmitLdcI4(index);
        state.EmitCall(
            typeof(PatternOutputAt).GetMethod(nameof(OutputAt), [subjectType, typeof(int)])!
        );
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

public sealed class PatternListShape : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\list\shape";
    public static PatternListShape Instance { get; } = new();

    PatternListShape() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0)
            return false;
        var minimum =
            args.Length > 1 ? (int?)args[1].ConvertTo(sctx, PType.Int, false).Value ?? 0 : 0;
        var hasRest =
            args.Length > 2 && ((bool?)args[2].ConvertTo(sctx, PType.Bool, false).Value ?? false);
        return Matches(args[0], minimum, hasRest);
    }

    public static bool Matches(PValue subject, int minimum, bool hasRest) =>
        MatchingList(subject, minimum, hasRest) != null;

    public static List<PValue>? MatchingList(PValue subject, int minimum, bool hasRest)
    {
        if (subject.Value is not List<PValue> list)
            return null;
        return hasRest
            ? list.Count >= minimum
                ? list
                : null
            : list.Count == minimum
                ? list
                : null;
    }

    public static PValue MatchAndCapture(
        PValue subject,
        int minimum,
        bool hasRest,
        out List<PValue>? list
    )
    {
        list = MatchingList(subject, minimum, hasRest);
        return list != null;
    }

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1
        && staticArgv.Length == 2
        && staticArgv[0].TryGetInt(out _)
        && staticArgv[1].TryGetBool(out _);

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        staticArgv[0].TryGetInt(out var minimum);
        staticArgv[1].TryGetBool(out var hasRest);
        state.EmitLdcI4(minimum);
        state.EmitLdcI4(hasRest ? 1 : 0);
        var capturesList = state.TryGetListPatternLocation(out var location) && location.IsShape;
        if (capturesList)
        {
            state.Il.Emit(OpCodes.Ldloca_S, location.Local);
            state.EmitCall(
                typeof(PatternListShape).GetMethod(
                    nameof(MatchAndCapture),
                    [
                        typeof(PValue),
                        typeof(int),
                        typeof(bool),
                        typeof(List<PValue>).MakeByRefType(),
                    ]
                )!
            );
        }
        else
        {
            state.EmitCall(
                typeof(PatternListShape).GetMethod(
                    nameof(Matches),
                    [typeof(PValue), typeof(int), typeof(bool)]
                )!
            );
        }
        if (!capturesList)
            state.EmitWrapBool();
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

public sealed class PatternListSlice : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\list\slice";
    public static PatternListSlice Instance { get; } = new();

    PatternListSlice() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0)
            return PType.Null;
        var start =
            args.Length > 1 ? (int?)args[1].ConvertTo(sctx, PType.Int, false).Value ?? 0 : 0;
        return Slice(args[0], start);
    }

    public static PValue Slice(PValue subject, int start)
    {
        if (subject.Value is not List<PValue> list)
            return PType.Null;
        return Slice(list, start);
    }

    public static PValue Slice(List<PValue> list, int start)
    {
        start = System.Math.Clamp(start, 0, list.Count);
        return PType.List.CreatePValue(list.GetRange(start, list.Count - start));
    }

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 1 && staticArgv[0].TryGetInt(out _);

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        staticArgv[0].TryGetInt(out var start);
        Type subjectType;
        if (state.TryGetListPatternLocation(out var location) && !location.IsShape)
        {
            state.Il.Emit(OpCodes.Pop);
            state.EmitLoadLocal(location.Local);
            subjectType = typeof(List<PValue>);
        }
        else
        {
            subjectType = typeof(PValue);
        }
        state.EmitLdcI4(start);
        state.EmitCall(
            typeof(PatternListSlice).GetMethod(nameof(Slice), [subjectType, typeof(int)])!
        );
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

public sealed class PatternNonNull : StaticPatternCommand, ICilExtension
{
    public const string Alias = @"pattern\non_null";
    public static PatternNonNull Instance { get; } = new();

    PatternNonNull() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args) =>
        args.Length > 0 && IsNonNull(args[0]);

    public static bool IsNonNull(PValue subject) => !subject.IsNull;

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 0;

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        state.EmitCall(typeof(PatternNonNull).GetMethod(nameof(IsNonNull))!);
        state.EmitWrapBool();
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}

/// <summary>Compatibility view for the former psr.pattern <c>kvp</c> deconstructor.</summary>
public sealed class PatternKvpView : StaticPatternCommand, ICilExtension
{
    public const string Alias = "view_kvp";
    public static PatternKvpView Instance { get; } = new();

    PatternKvpView() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        return args.Length == 0 ? PType.Null : Apply(sctx, args[0]);
    }

    public static PValue Apply(StackContext sctx, PValue subject)
    {
        if (
            !subject.TryDynamicCall(sctx, ReadOnlySpan<PValue>.Empty, PCall.Get, "Key", out var key)
            || !subject.TryDynamicCall(
                sctx,
                ReadOnlySpan<PValue>.Empty,
                PCall.Get,
                "Value",
                out var value
            )
        )
            return PType.Null;
        return PType.List.CreatePValue(new List<PValue> { key, value });
    }

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc) =>
        dynamicArgc == 1 && staticArgv.Length == 0;

    void ICilExtension.Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    )
    {
        state.EmitStoreTemp(0);
        state.EmitLoadLocal(state.SctxLocal);
        state.EmitLoadTemp(0);
        state.EmitCall(
            typeof(PatternKvpView).GetMethod(nameof(Apply), [typeof(StackContext), typeof(PValue)])!
        );
        if (ins.JustEffect)
            state.Il.Emit(OpCodes.Pop);
    }
}
