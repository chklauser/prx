
#region

using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types;

[PTypeLiteral(nameof(Bool))]
public class BoolPType : PType, ICilCompilerAware
{
    #region Singleton

    BoolPType()
    {
    }

    static BoolPType()
    {
        Instance = new();
    }

    public static BoolPType Instance { get; }

    #endregion

    #region Static

    public static PValue CreateValue(bool value)
    {
        return new(value, Bool);
    }

    public static PValue CreateValue(object? value)
    {
        if (value is bool)
            return CreateValue((bool) value);
        else
            return new(value != null, Bool);
    }

    #endregion

    #region Access interface implementation

    public override bool TryConstruct(StackContext sctx, ReadOnlySpan<PValue> args, [NotNullWhen(true)] out PValue? result)
    {
        if (args.Length <= 0)
        {
            result = false;
            return true;
        }
        else
        {
            return args[0].TryConvertTo(sctx, Bool, out result);
        }
    }

    public override bool TryDynamicCall(
        StackContext sctx,
        PValue subject,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)]
        out PValue? result
    )
    {
        //Try CLR dynamic call
        var clrint = Object[subject.ClrType!];
        return clrint.TryDynamicCall(sctx, subject, args, call, id, out result);
    }

    public override bool TryStaticCall(
        StackContext sctx, ReadOnlySpan<PValue> args, PCall call, string id, [NotNullWhen(true)] out PValue? result)
    {
        if (Object[typeof (bool)].TryStaticCall(sctx, args, call, id, out result))
            return true;

        return false;
    }

    protected override bool InternalConvertTo(
        StackContext sctx,
        PValue subject,
        PType target,
        bool useExplicit,
        [NotNullWhen(true)] out PValue? result)
    {
        if (target is ObjectPType)
            return
                Object[typeof (bool)].TryConvertTo(
                    sctx, subject, target, useExplicit, out result);

        result = null;

        switch (target)
        {
            case IntPType _ when useExplicit:
                result = (bool) subject.Value! ? 1 : 0;
                break;
            case RealPType _ when useExplicit:
                result = (bool) subject.Value! ? 1.0 : 0.0;
                break;
            case StringPType _ when useExplicit:
                result = (bool) subject.Value! ? bool.TrueString : bool.FalseString;
                break;
            default:
                return false;
        }

        return true;
    }

    protected override bool InternalConvertFrom(
        StackContext sctx,
        PValue subject,
        bool useExplicit,
        [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (subject.Type is ObjectPType objTy && objTy.ClrType == typeof (bool))
            result = (bool) subject.Value!;
        else if (useExplicit && subject.Type is StringPType)
        {
            if (bool.TryParse(subject.Value as string, out var parsed))
                result = parsed;
            else
                result = false;
            return true;
        }
        return false;
    }

    protected override bool InternalIsEqual(PType otherType)
    {
        return otherType is BoolPType;
    }

    #endregion

    #region Operators

    static bool _tryConvertToBool(StackContext sctx, PValue operand, out bool value)
    {
        value = false;

        switch (operand.Type.ToBuiltIn())
        {
            case BuiltIn.Real:
            case BuiltIn.Int:
            case BuiltIn.Bool:
                value = (bool) operand.Value!;
                return true;
            case BuiltIn.String:
                if (!bool.TryParse(operand.Value as string, out value))
                {
                    value = false;
                    return true;
                }
                break;
            case BuiltIn.Object:
                if (operand.TryConvertTo(sctx, Bool, out var asBool))
                {
                    value = (bool) asBool.Value!;
                    return true;
                }
                break;
            case BuiltIn.Null:
                value = false;
                return true;
        }

        return false;
    }

    public override bool BitwiseAnd(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (_tryConvertToBool(sctx, leftOperand, out var left) &&
            _tryConvertToBool(sctx, rightOperand, out var right))
            result = left & right;
        return result != null;
    }

    public override bool BitwiseOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (_tryConvertToBool(sctx, leftOperand, out var left) &&
            _tryConvertToBool(sctx, rightOperand, out var right))
            result = left | right;
        return result != null;
    }

    public override bool Equality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (_tryConvertToBool(sctx, leftOperand, out var left) &&
            _tryConvertToBool(sctx, rightOperand, out var right))
            result = left == right;
        return result != null;
    }

    public override bool Inequality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (_tryConvertToBool(sctx, leftOperand, out var left) &&
            _tryConvertToBool(sctx, rightOperand, out var right))
            result = left != right;
        return result != null;
    }

    public override bool ExclusiveOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, [NotNullWhen(true)] out PValue? result)
    {
        result = null;
        if (_tryConvertToBool(sctx, leftOperand, out var left) &&
            _tryConvertToBool(sctx, rightOperand, out var right))
            result = left ^ right;
        return result != null;
    }

    public override bool LogicalNot(StackContext sctx, PValue operand, [NotNullWhen(true)] out PValue? result)
    {
        result = !(bool) operand.Value!;
        return true;
    }

    public override bool OnesComplement(StackContext sctx, PValue operand, [NotNullWhen(true)] out PValue? result)
    {
        result = !(bool) operand.Value!; //Identical to LogicalNot
        return true;
    }

    public override bool UnaryNegation(StackContext sctx, PValue operand, [NotNullWhen(true)] out PValue? result)
    {
        result = !(bool) operand.Value!; //Identical to UnaryNegation
        return true;
    }

    #endregion

    public const string Literal = nameof(Bool);

    public override int GetHashCode()
    {
        return -1181897690;
    }

    public override string ToString()
    {
        return Literal;
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
        state.EmitCall(Compiler.Cil.Compiler.GetBoolPType);
    }

    #endregion
}