using JetBrains.Annotations;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class FlatMap : EnumerableCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static FlatMap Instance { get; } = new();

    FlatMap() { }

    public const string Alias = "flat_map";

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

        var sctx = sctxCarrier.StackContext;

        //Get f
        IIndirectCall? f = args.Length < 1 ? null : args[0];

        foreach (var arg in args.Skip(1))
        {
            var xs = Map._ToEnumerable(sctx, arg);
            foreach (var x in xs)
            {
                var rawYs = f != null ? f.IndirectCall(sctx, x) : x;
                var ys = Map._ToEnumerable(sctx, rawYs);
                foreach (var y in ys)
                {
                    yield return y;
                }
            }
        }
    }

    [PublicAPI]
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
