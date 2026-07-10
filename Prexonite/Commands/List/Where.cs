using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of the where coroutine.
/// </summary>
/// <remarks>
///     <code>
///         coroutine where f xs does
///         foreach(var x in xs)
///         if(f.(x))
///         yield x;</code>
/// </remarks>
public class Where : CoroutineCommand, ICilCompilerAware
{
    #region Singleton

    Where() { }

    public static Where Instance { get; } = new();

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
        PValue[] args
    )
    {
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 2)
            throw new PrexoniteException("Where(f, xs) requires at least two arguments.");

        var f = args[0];

        var sctx = sctxCarrier.StackContext;

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            var set = Map._ToEnumerable(sctx, arg);
            if (set == null)
                continue;
            foreach (var value in set)
            {
                var include = f.IndirectCall(sctx, value).ConvertTo(sctx, PType.Bool, true);
                if ((bool)include.Value!)
                    yield return value;
            }
        }
    }

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
