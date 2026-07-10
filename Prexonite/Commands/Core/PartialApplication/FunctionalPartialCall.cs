namespace Prexonite.Commands.Core.PartialApplication;

public class FunctionalPartialCall(PValue subject, PValue[] arguments) : IMaybeStackAware
{
    public PValue IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
    {
        return subject.IndirectCall(sctx, _getEffectiveArgs(args));
    }

    public bool TryDefer(
        StackContext sctx,
        PValue[] args,
        [NotNullWhen(true)] out StackContext? partialApplicationContext,
        [NotNullWhen(false)] out PValue? result
    )
    {
        var effectiveArgs = _getEffectiveArgs(args);

        partialApplicationContext = null;
        result = null;

        //The following code exists in a very similar form in PartialCall.cs, FlippedFunctionalPartialCall.cs
        if (subject.Type is ObjectPType)
        {
            var raw = subject.Value;
            if (raw is IStackAware stackAware)
            {
                partialApplicationContext = stackAware.CreateStackContext(sctx, effectiveArgs);
                return true;
            }

            if (raw is IMaybeStackAware partialApplication)
                return partialApplication.TryDefer(
                    sctx,
                    effectiveArgs,
                    out partialApplicationContext,
                    out result
                );
        }

        result = subject.IndirectCall(sctx, effectiveArgs);
        return false;
    }

    PValue[] _getEffectiveArgs(ReadOnlySpan<PValue> args)
    {
        var effectiveArgs = new PValue[args.Length + arguments.Length];
        Array.Copy(arguments, effectiveArgs, arguments.Length);
        args.CopyTo(effectiveArgs.AsSpan(arguments.Length, args.Length));
        return effectiveArgs;
    }
}
