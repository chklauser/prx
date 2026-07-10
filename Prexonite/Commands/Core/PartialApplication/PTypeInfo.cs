namespace Prexonite.Commands.Core.PartialApplication;

/// <summary>
///     Holds information about a PType at compile- and at run-time. Used in <see cref = "PartialWithPTypeCommandBase{T}" />.
/// </summary>
public class PTypeInfo
{
    /// <summary>
    ///     The runtime instance of the PType.
    /// </summary>
    public PType? Type;

    /// <summary>
    ///     The compile time constant PType expression.
    /// </summary>
    public required string? Expr;
}

public abstract record RuntimePTypeInfo<TSelf> : IRuntimePTypeInfo<TSelf>
    where TSelf : IRuntimePTypeInfo<TSelf>, new()
{
    public PType Type { get; init; } = null!;

    public static TSelf Create(PType type) => new() { Type = type };
}

public record RuntimePTypeInfo : RuntimePTypeInfo<RuntimePTypeInfo>;

public abstract record CompileTimePTypeInfo<TSelf> : ICompileTimePType<TSelf>
    where TSelf : ICompileTimePType<TSelf>, new()
{
    public string Expr { get; init; } = null!;

    public static TSelf Create(string expr) => new() { Expr = expr };
}

public record CompileTimePTypeInfo : CompileTimePTypeInfo<CompileTimePTypeInfo>;

[SuppressMessage("ReSharper", "TypeParameterCanBeVariant", Justification = "Self")]
public interface IRuntimePTypeInfo<TSelf>
    where TSelf : IRuntimePTypeInfo<TSelf>
{
    PType Type { get; init; }
    static abstract TSelf Create(PType type);
}

[SuppressMessage("ReSharper", "TypeParameterCanBeVariant", Justification = "Self")]
public interface ICompileTimePType<TSelf>
    where TSelf : ICompileTimePType<TSelf>
{
    string Expr { get; init; }
    static abstract TSelf Create(string expr);
}
