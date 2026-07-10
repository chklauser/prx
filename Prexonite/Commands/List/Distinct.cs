namespace Prexonite.Commands.List;

public class Distinct : CoroutineCommand
{
    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        var t = new Dictionary<PValue, object?>();

        var sctx = sctxCarrier.StackContext;

        foreach (var arg in args)
        {
            var xs = Map._ToEnumerable(sctx, arg);
            foreach (var x in xs)
                if (!t.ContainsKey(x))
                {
                    t.Add(x, null);
                    yield return x;
                }
        }
    }
}
