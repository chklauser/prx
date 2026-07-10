namespace Prexonite.Commands.Core.PartialApplication;

/// <summary>
///     Represents a partial application of an indirect call (the default call interface in Prexonite)
/// </summary>
public class PartialCall : PartialApplicationBase
{
    public PartialCall(int[] mappings, PValue[] closedArguments)
        : base(mappings, closedArguments, 1) { }

    #region Overrides of PartialApplicationBase

    protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
    {
        return nonArguments[0].IndirectCall(sctx, arguments);
    }

    protected override bool DoTryDefer(
        StackContext sctx,
        PValue[] nonArguments,
        PValue[] arguments,
        [NotNullWhen(true)] out StackContext? partialApplicationContext,
        [NotNullWhen(false)] out PValue? result
    )
    {
        partialApplicationContext = null;
        result = null;

        //The following code exists in a very similar form in FunctionalPartialCall.cs, FlippedFunctionalPartialCall.cs
        if (nonArguments[0].Type is ObjectPType)
        {
            var raw = nonArguments[0].Value;
            if (raw is IStackAware stackAware)
            {
                partialApplicationContext = stackAware.CreateStackContext(sctx, arguments);
                return true;
            }

            if (raw is IMaybeStackAware partialApplication)
                return partialApplication.TryDefer(
                    sctx,
                    arguments,
                    out partialApplicationContext,
                    out result
                );
        }

        result = Invoke(sctx, nonArguments, arguments);
        return false;
    }

    #endregion
}
