

namespace Prexonite.Commands.List;

public class GroupBy : CoroutineCommand
{
    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        if (args.Length < 1)
            throw new PrexoniteException("GroupBy requires at least one argument.");

        var f = args[0];

        var sctx = sctxCarrier.StackContext;

        var groups =
            new Dictionary<PValue, List<PValue>>();

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            var xs = Map._ToEnumerable(sctx, arg);
            if (xs == null)
                continue;
            foreach (var x in xs)
            {
                var fx = f.IndirectCall(sctx, x);
                if (!groups.ContainsKey(fx))
                {
                    var lst = new List<PValue>();
                    lst.Add(x);
                    groups.Add(fx, lst);
                }
                else
                {
                    groups[fx].Add(x);
                }
            }
        }
        // DO NO CONVERT TO LINQ, dereferencing of sctx MUST be delayed!
        // ReSharper disable LoopCanBeConvertedToQuery 
        foreach (var pair in groups)
        {
            yield return new PValueKeyValuePair(pair.Key, (PValue) pair.Value);
        }
        // ReSharper restore LoopCanBeConvertedToQuery
    }
}