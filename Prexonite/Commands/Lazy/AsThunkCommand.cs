using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Lazy;

/// <summary>
///     Turns values in WHNF into thunks and leaves existing thunks alone. This helps
///     building functions that can be callled with both strict and lazy arguments.
/// </summary>
public class AsThunkCommand : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    AsThunkCommand() { }

    public static AsThunkCommand Instance { get; } = new();

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
        if (args == null || args.Length == 0 || args[0] == null)
            throw new PrexoniteException("The asThunk command requires a value.");

        return ThunkCommand._EnforceThunk(args[0]);
    }

    #endregion

    #region Implementation of ICilCompilerAware

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(
            "The command "
                + GetType().Name
                + " does not support CIL compilation via ICilCompilerAware."
        );
    }

    #endregion
}
