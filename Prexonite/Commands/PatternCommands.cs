using Prexonite.Compiler.Cil;

namespace Prexonite.Commands;

public abstract class StaticPatternCommand : PCommand, ICilCompilerAware
{
    public CompilationFlags CheckQualification(Instruction ins) =>
        CompilationFlags.PrefersRunStatically;

    public void ImplementInCil(CompilerState state, Instruction ins) =>
        throw new NotSupportedException("This pattern helper uses RunStatically.");
}

public sealed class PatternMemberProbe : StaticPatternCommand
{
    public const string Alias = @"pattern\member\probe";
    public static PatternMemberProbe Instance { get; } = new();

    PatternMemberProbe() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var subject = args.Length > 0 ? args[0] : PType.Null;
        var memberName =
            args.Length > 1
                ? (string?)args[1].ConvertTo(sctx, PType.String, false).Value ?? ""
                : "";
        var handled = subject.TryDynamicCall(
            sctx,
            ReadOnlySpan<PValue>.Empty,
            PCall.Get,
            memberName,
            out var value
        );
        return PType.List.CreatePValue(
            new List<PValue>
            {
                PType.Bool.CreatePValue(handled),
                handled ? value ?? PType.Null : PType.Null,
            }
        );
    }
}

public sealed class PatternOutputAt : StaticPatternCommand
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
        if (args[0].Value is List<PValue> list)
            return index >= 0 && index < list.Count ? list[index] : PType.Null;
        return index == 0 ? args[0] : PType.Null;
    }
}

public sealed class PatternListShape : StaticPatternCommand
{
    public const string Alias = @"pattern\list\shape";
    public static PatternListShape Instance { get; } = new();

    PatternListShape() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0 || args[0].Value is not List<PValue> list)
            return false;
        var minimum =
            args.Length > 1 ? (int?)args[1].ConvertTo(sctx, PType.Int, false).Value ?? 0 : 0;
        var hasRest =
            args.Length > 2 && ((bool?)args[2].ConvertTo(sctx, PType.Bool, false).Value ?? false);
        return hasRest ? list.Count >= minimum : list.Count == minimum;
    }
}

public sealed class PatternListSlice : StaticPatternCommand
{
    public const string Alias = @"pattern\list\slice";
    public static PatternListSlice Instance { get; } = new();

    PatternListSlice() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0 || args[0].Value is not List<PValue> list)
            return PType.Null;
        var start =
            args.Length > 1 ? (int?)args[1].ConvertTo(sctx, PType.Int, false).Value ?? 0 : 0;
        start = System.Math.Clamp(start, 0, list.Count);
        return PType.List.CreatePValue(list.Skip(start).ToList());
    }
}

public sealed class PatternNonNull : StaticPatternCommand
{
    public const string Alias = @"pattern\non_null";
    public static PatternNonNull Instance { get; } = new();

    PatternNonNull() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args) =>
        args.Length > 0 && !args[0].IsNull;
}

/// <summary>Compatibility view for the former psr.pattern <c>kvp</c> deconstructor.</summary>
public sealed class PatternKvpView : StaticPatternCommand
{
    public const string Alias = "view_kvp";
    public static PatternKvpView Instance { get; } = new();

    PatternKvpView() { }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args) =>
        RunStatically(sctx, args.ToArray());

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (
            args.Length == 0
            || !args[0]
                .TryDynamicCall(sctx, ReadOnlySpan<PValue>.Empty, PCall.Get, "Key", out var key)
            || !args[0]
                .TryDynamicCall(sctx, ReadOnlySpan<PValue>.Empty, PCall.Get, "Value", out var value)
        )
            return PType.Null;
        return PType.List.CreatePValue(new List<PValue> { key, value });
    }
}
