using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Lazy;

public class ToSeqCommand : CoroutineCommand, ICilCompilerAware
{
    #region Singleton pattern

    ToSeqCommand() { }

    public static ToSeqCommand Instance { get; } = new();

    #endregion

    #region Overrides of CoroutineCommand

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    [SuppressMessage(
        "Microsoft.Naming",
        "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Coroutine)
    )]
    public static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier getSctx, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (getSctx == null)
            throw new ArgumentNullException(nameof(getSctx));

        if (args.Length < 1)
            throw new PrexoniteException("toseq requires one argument.");

        var xsT = args[0];
        PValue xs;

        var sctx = getSctx.StackContext;

        while (!(xs = ForceCommand.Force(sctx, xsT)).IsNull)
        {
            //Accept key value pairs directly
            if (xs.Value is PValueKeyValuePair kvp)
            {
                yield return kvp.Key;
                xsT = kvp.Value;
            }
            //Late bound
            else
            {
                var k = xs.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get, "Key");
                yield return k;
                xsT = xs.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get, "Value");
            }
        }
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
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

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(
            "The command "
                + GetType().Name
                + " does not support CIL compilation via ICilCompilerAware."
        );
    }

    #endregion
}
