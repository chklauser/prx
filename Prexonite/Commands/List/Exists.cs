namespace Prexonite.Commands.List;

public class Exists : PCommand
{
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        if (args.Length < 1)
            throw new PrexoniteException("Exists requires at least two arguments");
        var f = args[0];

        var eargs = new PValue[1];
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            var set = Map._ToEnumerable(sctx, arg);
            if (set == null)
                continue;
            foreach (var value in set)
            {
                eargs[0] = value;
                var result = f.IndirectCall(sctx, eargs);
                if (
                    result.TryConvertTo(sctx, PType.Bool, true, out var existance)
                    && (bool)existance.Value!
                )
                    return true;
            }
        }

        return false;
    }
}
