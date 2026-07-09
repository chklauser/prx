

namespace Prexonite.Commands;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = nameof(Coroutine))]
public abstract class CoroutineCommand : PCommand
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        var carrier = new ContextCarrier();
        var corctx = new CoroutineContext(sctx, CoroutineRun(carrier, args.ToArray()));
        carrier.StackContext = corctx;
        return sctx.CreateNativePValue(new Coroutine(corctx));
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Coroutine))]
    protected abstract IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
        PValue[] args);

    public sealed class ContextCarrier
    {
        public ContextCarrier()
        {
        }

        public ContextCarrier(StackContext sctx)
        {
            stackContext = sctx;
        }

        StackContext? stackContext;

        public StackContext StackContext
        {
            get
            {
                if (stackContext == null)
                    throw new InvalidOperationException(
                        "StackContext has not been assigned yet.");
                return stackContext;
            }
            set
            {
                if (stackContext != null)
                    throw new InvalidOperationException("StackContext can only be set once.");
                stackContext = value;
            }
        }
    }
}