namespace Prexonite.Commands;

public abstract class StackAwareCommand : PCommand, IStackAware
{
    public abstract StackContext CreateStackContext(StackContext sctx, PValue[] args);

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        var rctx = CreateStackContext(sctx, args.ToArray());
        return sctx.ParentEngine.Process(rctx);
    }
}
