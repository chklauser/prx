

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Sum : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Sum()
    {
    }

    public static Sum Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        //let sum xs acc = Seq.foldl (fun a b -> a + b) acc xs

        PValue acc;
        IEnumerable<PValue> xsArgs;

        if (args.Length == 0)
            return PType.Null;

        if (args.Length == 1)
        {
            acc = PType.Null;
            xsArgs = args;
        }
        else
        {
            acc = args[^1];
            xsArgs = args.Take(args.Length - 1);
        }

        var xss = xsArgs.Select(e => Map._ToEnumerable(sctx, e)).Where(e => e != null);

        foreach (var xs in xss)
        foreach (var x in xs)
            acc = acc.Addition(sctx, x);

        return acc;
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }
}