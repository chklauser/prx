using JetBrains.Annotations;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Limit : CoroutineCommand, ICilCompilerAware
{
    #region Singleton

    Limit() { }

    public static Limit Instance { get; } = new();

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
        ContextCarrier ctxCarrier,
        PValue[] args
    )
    {
        if (ctxCarrier == null)
            throw new ArgumentNullException(nameof(ctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException("Limit requires at least one argument.");

        var i = 0;
        var sctx = ctxCarrier.StackContext;
        var count = (int)args[0].ConvertTo(sctx, PType.Int, true).Value!;

        for (var j = 1; j < args.Length; j++)
        {
            var arg = args[j];
            var set = Map._ToEnumerable(sctx, arg);
            if (set == null)
                throw new PrexoniteException(arg + " is neither a list nor a coroutine.");
            using var setEnumerator = set.GetEnumerator();
            while (i++ < count && setEnumerator.MoveNext())
            {
                yield return setEnumerator.Current;
            }
            if (i >= count)
                yield break;
        }
    }

    [UsedImplicitly]
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var carrier = new ContextCarrier();
        var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
        carrier.StackContext = corctx;
        return sctx.CreateNativePValue(new Coroutine(corctx));
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}
