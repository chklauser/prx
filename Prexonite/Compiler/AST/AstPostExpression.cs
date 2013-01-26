using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// <para>A expression consisting of an inner expression and a statement that is to be performed after the expression
    /// has been evaluated. The 'value' of this expression is the value of the inner expression.</para>
    /// <para>This kind of AST node is used to implement post-increment/decrement operators.</para>
    /// </summary>
    public class AstPostExpression : AstExpr
    {
        [NotNull]
        private readonly AstExpr _expression;

        [NotNull]
        private readonly AstNode _action;

        public AstPostExpression([NotNull] ISourcePosition position, [NotNull] AstExpr expression, [NotNull] AstNode action) : base(position)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            if (action == null)
                throw new ArgumentNullException("action");
            
            _expression = expression;
            _action = action;
        }

        [NotNull]
        public AstExpr Expression
        {
            get { return _expression; }
        }

        [NotNull]
        public AstNode Action
        {
            get { return _action; }
        }

        #region Class

        protected bool Equals(AstPostExpression other)
        {
            return _expression.Equals(other._expression) && _action.Equals(other._action);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AstPostExpression) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_expression.GetHashCode()*397) ^ _action.GetHashCode();
            }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics semantics)
        {
            Expression.EmitCode(target, semantics);
            Action.EmitCode(target,StackSemantics.Effect);
            // At this point, the value of the expression remains on the stack.
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            var constant = Expression as AstConstant;
            if (constant != null)
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
}
