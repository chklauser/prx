

namespace Prexonite.Commands.List;

public class Intersect : CoroutineCommand
{
    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));

        var sctx = sctxCarrier.StackContext;

        var xss = new List<IEnumerable<PValue>>();
        foreach (var arg in args)
        {
            var xs = Map._ToEnumerable(sctx, arg);
            xss.Add(xs);
        }

        var n = xss.Count;
        if (n < 2)
            throw new PrexoniteException("Intersect requires at least two sources.");

        var t = new Dictionary<PValue, int>();
        //All elements of the first source are considered candidates
        foreach (var x in xss[0])
            t.TryAdd(x, 1);

        var d = new Dictionary<PValue, object?>();
        for (var i = 1; i < n - 1; i++)
        {
            foreach (var x in xss[i])
                if (!d.ContainsKey(x) && t.ContainsKey(x))
                {
                    d.Add(x, null); //only current source
                    t[x]++;
                }
            d.Clear();
        }

        foreach (var x in xss[n - 1])
            if (!d.ContainsKey(x) && t.TryGetValue(x, out var value))
            {
                d.Add(x, null); //only current source
                var k = value + 1;
                if (k == n)
                    yield return x;
            }
    }
}