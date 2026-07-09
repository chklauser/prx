

using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public class Meta : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    Meta()
    {
    }

    public static Meta Instance { get; } = new();

    #endregion

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.RequiresCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        if (ins.Arguments > 0)
            throw new PrexoniteException("The meta command no longer accepts arguments.");

        state.EmitLoadLocal(state.SctxLocal);
        state.EmitLoadArg(CompilerState.ParamSourceIndex);
        var getMeta = typeof (PFunction).GetProperty(nameof(Meta))?.GetGetMethod() ?? throw new InvalidOperationException("PFunction.Meta getter is missing.");
        state.Il.EmitCall(OpCodes.Callvirt, getMeta, null);
        state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreateNativePValue, null);
    }

    #endregion

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args.Length > 0)
            throw new PrexoniteException("The meta command no longer accepts arguments.");

        if (sctx is not FunctionContext fctx)
            throw new PrexoniteException(
                "The meta command uses dynamic features and can therefor only be called from a Prexonite function.");

        return fctx.CreateNativePValue(fctx.Implementation.Meta);
    }
}