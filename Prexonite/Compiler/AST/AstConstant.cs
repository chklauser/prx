using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public class AstConstant : AstExpr
{
    public readonly object? Constant;

    internal AstConstant(Parser p, object? constant)
        : this(p.scanner.File, p.t.line, p.t.col, constant) { }

    public AstConstant(string file, int line, int column, object? constant)
        : base(file, line, column)
    {
        Constant = constant;
    }

    public static bool TryCreateConstant(
        CompilerTarget target,
        ISourcePosition position,
        PValue value,
        [NotNullWhen(true)] out AstExpr? expr
    )
    {
        expr = null;
        if (value.Type is ObjectPType)
            target.Loader.ParentEngine.CreateNativePValue(value.Value);
        if (
            value.Type is IntPType or RealPType or BoolPType or StringPType or NullPType
            || _isModuleName(value)
        )
            expr = new AstConstant(position.File, position.Line, position.Column, value.Value);
        else //Cannot represent value in a constant instruction
            return false;
        return true;
    }

    static bool _isModuleName(PValue value)
    {
        ObjectPType? objectType;
        return (object?)(objectType = value.Type as ObjectPType) != null
            && typeof(ModuleName).IsAssignableFrom(objectType.ClrType);
    }

    public PValue ToPValue(CompilerTarget target)
    {
        return target.Loader.ParentEngine.CreateNativePValue(Constant);
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (stackSemantics == StackSemantics.Effect)
            return;

        if (Constant == null)
            target.EmitNull(Position);
        else
            switch (Type.GetTypeCode(Constant.GetType()))
            {
                case TypeCode.Boolean:
                    target.EmitConstant(Position, (bool)Constant);
                    break;
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    target.EmitConstant(Position, (int)Constant);
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    target.EmitConstant(Position, (double)Constant);
                    break;
                case TypeCode.String:
                    target.EmitConstant(Position, (string)Constant);
                    break;
                default:
                    if (Constant is ModuleName moduleName)
                    {
                        target.EmitConstant(Position, moduleName);
                    }
                    else
                    {
                        throw new PrexoniteException(
                            "Prexonite does not support constants of type "
                                + Constant.GetType().Name
                                + "."
                        );
                    }
                    break;
            }
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    #endregion

    public override string? ToString()
    {
        string? str;
        if (Constant != null)
            if ((str = Constant as string) != null)
                return string.Concat("\"", StringPType.Escape(str), "\"");
            else
                return Constant.ToString();
        else
            return "-null-";
    }
}
