

using System.Reflection;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Math;

public class Ceiling : PCommand, ICilCompilerAware
{
    #region Singleton

    Ceiling()
    {
    }

    public static Ceiling Instance { get; } = new();

    #endregion

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException("Ceiling requires at least one argument.");

        var arg0 = args[0];

        return RunStatically(arg0, sctx);
    }

    public static PValue RunStatically(PValue arg0, StackContext sctx)
    {
        var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value!;

        return System.Math.Ceiling(x);
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        switch (ins.Arguments)
        {
            case 1:
                return CompilationFlags.PrefersCustomImplementation;
            case 0:
            default:
                return CompilationFlags.PrefersRunStatically;
        }
    }

    static readonly MethodInfo RunStaticallyMethod =
        typeof (Ceiling).GetMethod(nameof(RunStatically),
            [typeof (PValue), typeof (StackContext)])!;

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        Abs._CallStaticFunc1(state, ins, RunStaticallyMethod);
    }

    #endregion
}