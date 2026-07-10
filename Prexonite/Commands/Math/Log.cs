using System.Reflection;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Math;

public class Log : PCommand, ICilCompilerAware
{
    #region Singleton

    Log() { }

    public static Log Instance { get; } = new();

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
            throw new PrexoniteException("Log requires at least one argument.");

        if (args.Length > 1)
        {
            var arg0 = args[0];
            var arg1 = args[1];

            return RunStatically(arg0, arg1, sctx);
        }
        else
        {
            var arg0 = args[0];

            return RunStatically(arg0, sctx);
        }
    }

    public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
    {
        var x = (double)arg0.ConvertTo(sctx, PType.Real, true).Value!;
        var b = (double)arg1.ConvertTo(sctx, PType.Real, true).Value!;
        return System.Math.Log(x, b);
    }

    public static PValue RunStatically(PValue arg0, StackContext sctx)
    {
        var x = (double)arg0.ConvertTo(sctx, PType.Real, true).Value!;
        return System.Math.Log(x);
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
        return CompilationFlags.PrefersCustomImplementation;
    }

    static readonly MethodInfo RunStaticallyNaturalMethod = typeof(Log).GetMethod(
        nameof(RunStatically),
        [typeof(PValue), typeof(StackContext)]
    )!;

    static readonly MethodInfo RunStaticallyAnyMethod = typeof(Log).GetMethod(
        nameof(RunStatically),
        [typeof(PValue), typeof(PValue), typeof(StackContext)]
    )!;

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        var argc = ins.Arguments;
        if (ins.JustEffect)
        {
            state.EmitIgnoreArguments(argc);
        }
        else
        {
            if (argc > 2)
            {
                state.EmitIgnoreArguments(argc - 2);
                argc = 2;
            }
            switch (argc)
            {
                case 0:
                    state.EmitLdcI4(0);
                    state.EmitWrapInt();
                    goto case 1;
                case 1:
                    state.EmitLoadLocal(state.SctxLocal);
                    state.EmitCall(RunStaticallyNaturalMethod);
                    break;
                case 2:
                    state.EmitLoadLocal(state.SctxLocal);
                    state.EmitCall(RunStaticallyAnyMethod);
                    break;
            }
        }
    }

    #endregion
}
