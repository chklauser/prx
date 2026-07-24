using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Append : EnumerableCommand, ICilCompilerAware
{
    #region Singleton pattern

    Append() { }

    public static Append Instance { get; } = new();

    #endregion

    protected override IEnumerable<PValue> EnumerableRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return EnumerableRunStatically(sctxCarrier, args);
    }

    protected static IEnumerable<PValue> EnumerableRunStatically(
        ContextCarrier sctxCarrier,
        PValue[]? args
    )
    {
        if (args == null)
            yield break;
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        var sctx = sctxCarrier.StackContext;

        foreach (var arg in args)
        {
            if (arg == null)
                throw new ArgumentException("No element in args must be null.", nameof(args));
            var xs = Map._ToEnumerable(sctx, arg);
            if (xs == null)
                continue;
            foreach (var x in xs)
                yield return x;
        }
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var carrier = new ContextCarrier(sctx);
        return CreatePValue(sctx, EnumerableRunStatically(carrier, args));
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}
