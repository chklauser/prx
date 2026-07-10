namespace Prexonite.Commands.List;

public class Frequency : CoroutineCommand
{
    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        var t = new Dictionary<PValue, int>();

        var sctx = sctxCarrier.StackContext;

        foreach (var arg in args)
        {
            var xs = Map._ToEnumerable(sctx, arg);
            if (xs == null)
                continue;
            foreach (var x in xs)
                if (t.ContainsKey(x))
                    t[x]++;
                else
                    t.Add(x, 1);
        }

        foreach (var pair in t)
            yield return new PValueKeyValuePair(pair.Key, pair.Value);
    }
}
