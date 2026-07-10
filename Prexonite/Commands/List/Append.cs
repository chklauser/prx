using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Append : CoroutineCommand, ICilCompilerAware
{
    #region Singleton pattern

    Append() { }

    public static Append Instance { get; } = new();

    #endregion

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    [SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Coroutine)
    )]
    protected static IEnumerable<PValue> CoroutineRunStatically(
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
        var carrier = new ContextCarrier();
        var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
        carrier.StackContext = corctx;
        return sctx.CreateNativePValue(new Coroutine(corctx));
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}
