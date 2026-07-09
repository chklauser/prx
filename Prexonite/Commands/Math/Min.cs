

using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Math;

public class Min : PCommand, ICilCompilerAware
{
    #region Singleton

    Min()
    {
    }

    public static Min Instance { get; } = new();

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
            throw new PrexoniteException("Min requires at least one argument.");

        if (args.Length == 1)
            return args[0];

        var arg0 = args[0];
        var arg1 = args[1];
        return RunStatically(arg0, arg1, sctx);
    }

    public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
    {
        if (arg0.Type == PType.Int && arg1.Type == PType.Int)
        {
            var a = (int) arg0.Value!;
            var b = (int) arg1.Value!;

            return System.Math.Min(a, b);
        }
        else
        {
            var a = (double) arg0.ConvertTo(sctx, PType.Real, true).Value!;
            var b = (double) arg1.ConvertTo(sctx, PType.Real, true).Value!;

            return System.Math.Min(a, b);
        }
    }

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
            case 0:
            case 1:
            case 2:
                return CompilationFlags.PrefersCustomImplementation;
            default:
                return CompilationFlags.PrefersRunStatically;
        }
    }

    static readonly MethodInfo RunStaticallyMethod =
        typeof (Min).GetMethod(nameof(RunStatically),
            [typeof (PValue), typeof (PValue), typeof (StackContext)])!;

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        if (ins.JustEffect)
        {
            for (var i = 0; i < ins.Arguments; i++)
                state.Il.Emit(OpCodes.Pop);
        }
        else
        {
            switch (ins.Arguments)
            {
                case 0:
                    state.EmitLoadNullAsPValue();
                    state.EmitLoadNullAsPValue();
                    break;
                case 1:
                    state.EmitLoadNullAsPValue();
                    break;
                case 2:
                    break;
                default:
                    throw new NotSupportedException();
            }

            state.EmitLoadLocal(state.SctxLocal);
            state.EmitCall(RunStaticallyMethod);
        }
    }

    #endregion
}