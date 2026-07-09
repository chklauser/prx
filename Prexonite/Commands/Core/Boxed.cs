

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Boxed : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Boxed()
    {
    }

    public static Boxed Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args.Length == 0)
            return PType.Null;

        var arg = args[0];
        if (arg == null)
            return PType.Null;

        return sctx.CreateNativePValue(arg);
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }

    #endregion
}