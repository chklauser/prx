

using System.Diagnostics;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Lazy;

public class ForceCommand : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    ForceCommand()
    {
    }

    public static ForceCommand Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

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
            throw new PrexoniteException("force requires an argument.");

        var arg = args[0] ?? PType.Null;
        if (arg.IsNull)
            return PType.Null;

        return Force(sctx, arg);
    }

    public static PValue Force(StackContext sctx, PValue arg)
    {
        var result = arg.Value is Thunk t ? t.Force(sctx) : arg;

        Debug.Assert(result.Value is not Thunk, "Force wanted to return an unevaluated thunk.");

        return result;
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