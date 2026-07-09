

using JetBrains.Annotations;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class SeqConcat : CoroutineCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static SeqConcat Instance { get; } = new();

    SeqConcat()
    {
    }

    public const string Alias = "seqconcat";

    #endregion

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier, PValue[] args)
    {
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        var sctx = sctxCarrier.StackContext;

        foreach (var arg in args)
        {
            if (arg == null)
                throw new ArgumentException("No element in seqconcat(args...) must be null.", nameof(args));
            var xss = Map._ToEnumerable(sctx, arg);
            if (xss == null)
                continue;
            foreach (var xsRaw in xss)
            {
                var xs = Map._ToEnumerable(sctx, xsRaw);
                if(xs == null)
                    throw new ArgumentException("The elements in the sequences passed to seqconcat(..) must be sequences themselves.");
                foreach (var x in xs)
                    yield return x;
            }
        }
    }

    [PublicAPI]
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