

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Skip : CoroutineCommand, ICilCompilerAware
{
    #region Singleton

    Skip()
    {
    }

    public static Skip Instance { get; } = new();

    #endregion

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Coroutine))]
    protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        var sctx = sctxCarrier.StackContext;

        var i = 0;
        if (args.Length < 1)
            throw new PrexoniteException("Skip requires at least one argument.");

        var index = (int) args[0].ConvertTo(sctx, PType.Int, true).Value!;

        for (var j = 1; j < args.Length; j++)
        {
            var arg = args[j];
            var set = Map._ToEnumerable(sctx, arg);
            if (set == null)
                throw new PrexoniteException(arg + " is neither a list nor a coroutine.");
            foreach (var value in set)
            {
                if (i++ >= index)
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