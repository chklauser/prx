namespace Prexonite.Commands.Core;

public class Not : PCommand
{
    public static Not Instance { get; } = new();

    Not()
    {
    }

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        foreach (var arg in args)
        {
            var b = arg.Type.ToBuiltIn() != PType.BuiltIn.Bool
                ? (bool) arg.ConvertTo(sctx, PType.Bool, true).Value!
                : (bool) arg.Value!;
            if(b)
                return false;
        }

        return true;
    }
}