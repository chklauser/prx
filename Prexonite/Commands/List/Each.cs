

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class Each : PCommand, ICilCompilerAware
{
    #region Singleton

    Each()
    {
    }

    public static Each Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException("Each requires at least two arguments");
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
                f.IndirectCall(sctx, eargs);
            }
        }

        return PType.Null;
    }

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}