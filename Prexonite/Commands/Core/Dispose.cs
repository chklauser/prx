

using System.ComponentModel;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

/// <summary>
///     Command that calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
/// </summary>
/// <remarks>
///     Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.
/// </remarks>
public sealed class Dispose : PCommand, ICilCompilerAware
{
    Dispose()
    {
    }

    public static Dispose Instance { get; } = new();

    public const string DisposeMemberId = nameof(Dispose);

    /// <summary>
    ///     Executes the dispose function.<br />
    ///     Calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
    /// </summary>
    /// <param name = "sctx">The stack context. Ignored by this command.</param>
    /// <param name = "args">The list of values to dispose.</param>
    /// <returns>Always null.</returns>
    /// <remarks>
    ///     <para>
    ///         Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
    /// </remarks>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    /// <summary>
    ///     Executes the dispose function.<br />
    ///     Calls <see cref = "IDisposable.Dispose" /> on object values that support the interface.
    /// </summary>
    /// <param name = "sctx">The stack context. Ignored by this command.</param>
    /// <param name = "args">The list of values to dispose.</param>
    /// <returns>Always null.</returns>
    /// <remarks>
    ///     <para>
    ///         Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
    /// </remarks>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        foreach (var arg in args)
        {
            RunStatically(arg, sctx);
        }

        return PType.Null.CreatePValue();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void RunStatically(PValue arg, StackContext sctx)
    {
        if (arg.Type is ObjectPType)
        {
            if (arg.Value is IDisposable toDispose)
                toDispose.Dispose();
            else
            {
                if (arg.Value is IObject isObj)
                {
                    isObj.TryDynamicCall(
                        sctx, [], PCall.Get, DisposeMemberId, out _);
                }
            }
        }
        else
        {
            arg.TryDynamicCall(sctx, [], PCall.Get, DisposeMemberId, out _);
        }
    }

    #region ICilCompilerAware Members

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersCustomImplementation;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        switch (ins.Arguments)
        {
            case 0:
                if (!ins.JustEffect)
                    state.EmitLoadNullAsPValue();
                break;
            case 1:
                //Emit call to RunStatically(PValue, StackContext)
                state.EmitLoadLocal(state.SctxLocal);
                var run =
                    typeof (Dispose).GetMethod(nameof(RunStatically),
                        [typeof (PValue), typeof (StackContext)])!;
                state.Il.EmitCall(OpCodes.Call, run, null);
                if (!ins.JustEffect)
                    state.EmitLoadNullAsPValue();
                break;
            default:
                //Emit call to RunStatically(StackContext, PValue[])
                state.EmitEarlyBoundCommandCall(typeof (Dispose), ins);
                break;
        }
    }

    #endregion
}