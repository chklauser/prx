using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Reverse : CoroutineCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static Reverse Instance { get; } = new();

    Reverse() { }

    #endregion

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    static IEnumerable<PValue> CoroutineRunStatically(
        ContextCarrier sctxCarrier,
        IEnumerable<PValue> args
    )
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        var sctx = sctxCarrier.StackContext;

        var lst = new List<PValue>();

        foreach (var arg in args)
            lst.AddRange(Map._ToEnumerable(sctx, arg));

        for (var i = lst.Count - 1; i >= 0; i--)
            yield return lst[i];
    }

    // Bound statically by CIL compiler
    // ReSharper disable UnusedMember.Global
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    // ReSharper restore UnusedMember.Global
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
