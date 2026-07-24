namespace Prexonite.Commands;

/// <summary>
///     Base class for commands that lazily produce a sequence of <see cref="PValue" /> values.
/// </summary>
public abstract class EnumerableCommand : PCommand
{
    /// <inheritdoc />
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        ArgumentNullException.ThrowIfNull(sctx);

        var carrier = new ContextCarrier(sctx);
        return CreatePValue(sctx, EnumerableRun(carrier, args.ToArray()));
    }

    protected abstract IEnumerable<PValue> EnumerableRun(ContextCarrier sctxCarrier, PValue[] args);

    protected static PValue CreatePValue(StackContext sctx, IEnumerable<PValue> enumerable)
    {
        return sctx.CreateNativePValue(new PValueEnumerable(enumerable));
    }

    /// <summary>
    ///     Gives lazy command implementations access to the call-site stack context.
    /// </summary>
    public sealed class ContextCarrier(StackContext stackContext)
    {
        public StackContext StackContext { get; } =
            stackContext ?? throw new ArgumentNullException(nameof(stackContext));
    }
}
