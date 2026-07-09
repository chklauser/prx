

namespace Prexonite.Compiler.Ast;

/// <summary>
/// <para>A expression consisting of an inner expression and a statement that is to be performed after the expression
/// has been evaluated. The 'value' of this expression is the value of the inner expression.</para>
/// <para>This kind of AST node is used to implement post-increment/decrement operators.</para>
/// </summary>
public class AstPostExpression : AstExpr
{
    public AstPostExpression(ISourcePosition position, AstExpr expression, AstNode action) : base(position)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public AstExpr Expression { get; }

    public AstNode Action { get; }

    #region Class

    protected bool Equals(AstPostExpression other)
    {
        return Expression.Equals(other.Expression) && Action.Equals(other.Action);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AstPostExpression) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Expression.GetHashCode()*397) ^ Action.GetHashCode();
        }
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics semantics)
    {
        Expression.EmitCode(target, semantics);
        Action.EmitCode(target,StackSemantics.Effect);
        // At this point, the value of the expression remains on the stack.
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        if (Expression is AstConstant)
        {
            // Constants have no side-effects, convert this to a block with a return value
            var block = target.Factory.Block(Position);
            block.Add(Action);
            block.Expression = Expression;
            expr = block;
            return true;
        }
        else
        {
            expr = null;
            return false;
        }
    }
}