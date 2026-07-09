

namespace Prexonite.Commands.List;

public class ForAll : PCommand
{
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args.IsEmpty)
            throw new PrexoniteException("Exists requires at least two arguments");
        var f = args[0];

        var eargs = new PValue[1];
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            var set = Map._ToEnumerable(sctx, arg);
            foreach (var value in set)
            {
                eargs[0] = value;
                var result = f.IndirectCall(sctx, eargs);
                if (!result.TryConvertTo(sctx, PType.Bool, true, out var existence) ||
                    !(bool) existence.Value!)
                    return false;
            }
        }

        return true;
    }
}