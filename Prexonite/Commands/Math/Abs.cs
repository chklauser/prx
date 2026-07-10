using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Math;

public sealed class Abs : PCommand, ICilCompilerAware
{
    #region Singleton

    Abs() { }

    public static Abs Instance { get; } = new();

    #endregion


    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

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
            throw new PrexoniteException("Abs requires at least one argument.");

        return RunStatically(args[0], sctx);
    }

    public static PValue RunStatically(PValue arg, StackContext sctx)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (arg == null)
            throw new ArgumentNullException(nameof(arg));

        if (arg.Type == PType.Int)
        {
            var x = (int)arg.Value!;

            return System.Math.Abs(x);
        }
        else
        {
            var x = (double)arg.ConvertTo(sctx, PType.Real, true).Value!;

            return System.Math.Abs(x);
        }
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        if (ins == null)
            throw new ArgumentNullException(nameof(ins));
        switch (ins.Arguments)
        {
            case 1:
                return CompilationFlags.PrefersCustomImplementation;
            case 0:
            default:
                return CompilationFlags.PrefersRunStatically;
        }
    }

    static readonly MethodInfo RunStaticallyMethod = typeof(Abs).GetMethod(
        nameof(RunStatically),
        [typeof(PValue), typeof(StackContext)]
    )!;

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        _CallStaticFunc1(state, ins, RunStaticallyMethod);
    }

    internal static void _CallStaticFunc1(
        CompilerState state,
        Instruction ins,
        MethodInfo runStaticallyMethod
    )
    {
        if (ins == null)
            throw new ArgumentNullException(nameof(ins));

        switch (ins.Arguments)
        {
            case 1:
                break;
            default:
                throw new NotSupportedException();
        }

        if (ins.JustEffect)
        {
            state.Il.Emit(OpCodes.Pop);
        }
        else
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.EmitCall(runStaticallyMethod);
        }
    }

    #endregion
}
