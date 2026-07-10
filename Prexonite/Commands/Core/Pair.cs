using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

/// <summary>
///     Turns to arguments into a key-value pair
/// </summary>
/// <remarks>
///     Equivalent to:
///     <code>function pair(key, value) = key: value;</code>
/// </remarks>
public sealed class Pair : PCommand, ICilCompilerAware
{
    Pair() { }

    public static Pair Instance { get; } = new();

    /// <summary>
    ///     Turns to arguments into a key-value pair
    /// </summary>
    /// <param name = "sctx">Unused.</param>
    /// <param name = "args">The arguments to pass to this command. Array must contain 2 elements.</param>
    /// <remarks>
    ///     Equivalent to:
    ///     <code>function pair(key, value) = key: value;</code>
    /// </remarks>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (args.Length < 2)
            return PType.Null.CreatePValue();
        else
            return PType.Object.CreatePValue(
                new PValueKeyValuePair(
                    args[0] ?? PType.Null.CreatePValue(),
                    args[1] ?? PType.Null.CreatePValue()
                )
            );
    }

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        var argc = ins.Arguments;

        if (argc < 2)
        {
            state.EmitLoadNullAsPValue();
        }
        else
        {
            //pop excessive arguments
            for (var i = 2; i < argc; i++)
                state.Il.Emit(OpCodes.Pop);

            //make pvkvp
            state.Il.Emit(OpCodes.Newobj, Compiler.Cil.Compiler.NewPValueKeyValuePair);

            //save pvkvp in temporary variable
            state.EmitStoreTemp(0);

            //PType.Object.CreatePValue(temp)
            state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.GetObjectPTypeSelector, null);
            state.EmitLoadTemp(0);
            state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreatePValueAsObject, null);
        }
    }

    #endregion
}
