

using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of the caller command. Returns the stack context of the caller.
/// </summary>
public sealed class Caller : PCommand, ICilCompilerAware
{
    Caller()
    {
    }

    public static Caller Instance { get; } = new();

    /// <summary>
    ///     Returns the caller of the supplied stack context.
    /// </summary>
    /// <param name = "sctx">The stack contetx that wishes to find out, who called him.</param>
    /// <param name = "args">Ignored</param>
    /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return sctx.CreateNativePValue(GetCaller(sctx));
    }

    /// <summary>
    ///     Returns the caller of the supplied stack context.
    /// </summary>
    /// <param name = "sctx">The stack context that wishes tp find out, who called him.</param>
    /// <returns>Either the stack context of the caller or null.</returns>
    public static StackContext? GetCaller(StackContext sctx)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        var stack = sctx.ParentEngine.Stack;
        if (!stack.Contains(sctx))
            return null;
        else
        {
            var callee = stack.FindLast(sctx);
            if (callee?.Previous == null)
                return null;
            else
                return callee.Previous.Value;
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public static PValue GetCallerFromCilFunction(StackContext sctx)
    {
        var stack = sctx.ParentEngine.Stack;
        if (stack.Count == 0)
            return PType.Null;
        else
            return sctx.CreateNativePValue(stack.Last!.Value);
    }

    static readonly MethodInfo GetCallerFromCilFunctionMethod =
        typeof (Caller).GetMethod(nameof(GetCallerFromCilFunction), [typeof (StackContext)])!;

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.OperatesOnCaller | CompilationFlags.RequiresCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        for (var i = 0; i < ins.Arguments; i++)
            state.Il.Emit(OpCodes.Pop);
        if (!ins.JustEffect)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.EmitCall(OpCodes.Call, GetCallerFromCilFunctionMethod, null);
        }
    }

    #endregion
}