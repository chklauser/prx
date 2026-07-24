using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

/// <summary>
///     Creates an enumerable whose enumerators are supplied by an indirect-callable factory.
/// </summary>
public sealed class CreateEnumerable : PCommand, ICilCompilerAware
{
    public const string Alias = "create_enumerable";

    public static CreateEnumerable Instance { get; } = new();

    CreateEnumerable() { }

    /// <inheritdoc />
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        ArgumentNullException.ThrowIfNull(sctx);
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length != 1 || args[0].Value is not IIndirectCall getEnumerator)
            throw new PrexoniteException(
                Alias + " requires one indirect-callable argument: " + Alias + "(getEnumerator);"
            );

        return sctx.CreateNativePValue(
            new PValueEnumerable(() =>
                Runtime.ExtractEnumerator(getEnumerator.IndirectCall(sctx), sctx)
            )
        );
    }

    /// <inheritdoc />
    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <inheritdoc />
    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}
