using System.Text;

namespace Prexonite.Compiler.Ast;

public class AstDynamicTypeExpression : AstTypeExpr, IAstHasExpressions
{
    public List<AstExpr> Arguments { get; } = new();
    public string TypeId { get; }

    public AstDynamicTypeExpression(string file, int line, int column, string typeId)
        : this(new SourcePosition(file, line, column), typeId) { }

    public AstDynamicTypeExpression(ISourcePosition position, string typeId)
        : base(position)
    {
        TypeId = typeId ?? throw new ArgumentNullException(nameof(typeId));
    }

    internal AstDynamicTypeExpression(Parser p, string typeId)
        : this(p.GetPosition(), typeId) { }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Arguments.ToArray();

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;

        var isConstant = true;
        var buffer = new StringBuilder(TypeId);
        buffer.Append("(");

        //Optimize arguments
        AstExpr oArg;
        foreach (var arg in Arguments.ToArray())
        {
            oArg = _GetOptimizedNode(target, arg);
            if (!ReferenceEquals(oArg, arg))
            {
                Arguments.Remove(arg);
                Arguments.Add(oArg);
            }

            var constValue = oArg as AstConstant;
            var constType = oArg as AstConstantTypeExpression;

            if (constValue == null && constType == null)
            {
                isConstant = false;
            }
            else if (isConstant)
            {
                if (constValue != null)
                {
                    buffer.Append('"');
                    buffer.Append(
                        StringPType.Escape(constValue.ToPValue(target).CallToString(target.Loader))
                    );
                    buffer.Append('"');
                }
                else if (constType != null)
                {
                    buffer.Append(constType.TypeExpression);
                }

                buffer.Append(",");
            }
        }
        if (!isConstant)
            return false;

        buffer.Remove(buffer.Length - 1, 1); //remove , or (
        if (Arguments.Count != 0)
            buffer.Append(")"); //Add ) if necessary

        expr = new AstConstantTypeExpression(File, Line, Column, buffer.ToString());
        return true;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        foreach (var expr in Arguments)
            expr.EmitCode(target, stackSemantics);

        if (stackSemantics == StackSemantics.Value)
            target.Emit(Position, OpCode.newtype, Arguments.Count, TypeId);
    }

    #endregion
}
