

using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Math;

public class Pi : PCommand, ICilCompilerAware
{
    #region Singleton

    Pi()
    {
    }

    public static Pi Instance { get; } = new();

    #endregion

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return System.Math.PI;
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        for (var i = 0; i < ins.Arguments; i++)
            state.Il.Emit(OpCodes.Pop);

        if (!ins.JustEffect)
        {
            state.Il.Emit(OpCodes.Ldc_R8, System.Math.PI);
            state.EmitWrapReal();
        }
    }

    #endregion
}