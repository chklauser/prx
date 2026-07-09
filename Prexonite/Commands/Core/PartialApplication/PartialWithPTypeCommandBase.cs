

using System.Reflection.Emit;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication;

/// <summary>
///     <para>Common base class for partial application commands (constructors) that deal with an additional PType parameter (such as type casts)</para>
///     <para>This class exists to share implementation. DO NOT use it for classification.</para>
/// </summary>
/// <typeparam name="TRuntimeParam"><see cref="RuntimePTypeInfo"/> can be used if no additional information is required.</typeparam>
/// <typeparam name="TCompileTimeParam"><see cref="CompileTimePTypeInfo"/> can be used if no additional information is required.</typeparam>
public abstract class PartialWithPTypeCommandBase<TRuntimeParam, TCompileTimeParam> : PartialApplicationCommandBase<TRuntimeParam, TCompileTimeParam>
    where TRuntimeParam : IRuntimePTypeInfo<TRuntimeParam>
    where TCompileTimeParam : ICompileTimePType<TCompileTimeParam>
{
    /// <summary>
    ///     The human readable name of this kind of partial application. Used in error messages.
    /// </summary>
    protected abstract string PartialApplicationKind { get; }

    protected override TRuntimeParam FilterRuntimeArguments(
        StackContext sctx,
        ref Span<PValue> arguments
    )
    {
        if (arguments.Length < 1)
        {
            throw new PrexoniteException(
                $"{PartialApplicationKind} requires a PType argument (or a PType expression).");
        }

        var raw = arguments[^1];
        PType? ptype;
        //Allow the type to be specified as a type expression (instead of a type instance)
        if (!(raw.Type is ObjectPType && (object?) (ptype = raw.Value as PType) != null))
        {
            var ptypeExpr = raw.CallToString(sctx);
            ptype = sctx.ConstructPType(ptypeExpr);
        }

        arguments = arguments[..^1];
        return TRuntimeParam.Create(ptype);
    }

    protected override bool FilterCompileTimeArguments(
        ref Span<CompileTimeValue> staticArgv,
        [NotNullWhen(true)] out TCompileTimeParam? parameter
    )
    {
        parameter = default;
        if (staticArgv.Length < 1)
            return false;

        var raw = staticArgv[^1];
        if (!raw.TryGetString(out var ptypeExpr))
            return false;

        parameter = TCompileTimeParam.Create(ptypeExpr);
        staticArgv = staticArgv[..^1];
        return true;
    }

    protected override void EmitConstructorCall(CompilerState state, TCompileTimeParam parameter)
    {
        state.EmitLoadLocal(state.SctxLocal);
        state.Il.Emit(OpCodes.Ldstr, parameter.Expr);
        state.EmitCall(Runtime.ConstructPTypeMethod);
        base.EmitConstructorCall(state, parameter);
    }
}