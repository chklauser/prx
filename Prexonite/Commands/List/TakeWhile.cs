using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of takewhile
/// </summary>
public class TakeWhile : EnumerableCommand, ICilCompilerAware
{
    #region Singleton

    TakeWhile() { }

    public static TakeWhile Instance { get; } = new();

    #endregion

    protected override IEnumerable<PValue> EnumerableRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return EnumerableRunStatically(sctxCarrier, args);
    }

    protected static IEnumerable<PValue> EnumerableRunStatically(
        ContextCarrier sctxCarrier,
        PValue[] args
    )
    {
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (args.Length < 2)
            throw new PrexoniteException("TakeWhile requires at least two arguments.");

        var sctx = sctxCarrier.StackContext;

        var f = args[0];

        var i = 0;
        for (var k = 1; k < args.Length; k++)
        {
            var arg = args[k];
            var set = Map._ToEnumerable(sctx, arg);
            foreach (var value in set)
                if ((bool)f.IndirectCall(sctx, value, i++).ConvertTo(sctx, PType.Bool, true).Value!)
                    yield return value;
        }
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var carrier = new ContextCarrier(sctx);
        return CreatePValue(sctx, EnumerableRunStatically(carrier, args));
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
