using System;
using System.Linq;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// <para>Wraps a get-set node in the new-declaration of a local variable.</para>
    /// <para>Syntax:
    /// <code>var new x</code> (iff <code>x</code> isn't the first reference to the variable in the current scope)</para>
    /// <para>In addition to the supplied expression, the variables identity is changed (similar to the unbind command)</para>
    /// </summary>
    public class AstGetSetNewDecl : AstGetSet
    {
        private PCall _fallbackCall;

        /// <summary>
        /// Creates a new New-Declaration node.
        /// </summary>
        /// <param name="file">The file in which the code for this node is located</param>
        /// <param name="line">The line in the file where the code for this node is located</param>
        /// <param name="column">The column in the line where the code for this node is located</param>
        /// <param name="id">The id pf the local variable to be new-declared</param>
        /// <param name="expression">The expression to be wrapped by this new-declaration</param>
        public AstGetSetNewDecl(string file, int line, int column, string id, AstGetSet expression)
            : base(file, line, column, expression == null ? PCall.Get : expression.Call)
        {
            Expression = expression;
            Id = id;
        }

        /// <summary>
        /// Creates a new New-Declaration node.
        /// </summary>
        /// <param name="file">The file in which the code for this node is located</param>
        /// <param name="line">The line in the file where the code for this node is located</param>
        /// <param name="column">The column in the line where the code for this node is located</param>
        public AstGetSetNewDecl(string file, int line, int column)
            : base(file, line, column, PCall.Get)
        {
        }

        /// <summary>
        /// Creates a new New-Declaration node.
        /// </summary>
        /// <param name="p">The parser that created this node.</param>
        internal AstGetSetNewDecl(Parser p)
            : base(p, PCall.Get)
        {
        }

        #region Overrides of AstGetSet

        public override IAstExpression[] Expressions
        {
            get
            {
                var expr = Expression;
                if (expr == null)
                    return base.Expressions;
                else
                    return base.Expressions.Append(expr).ToArray();
            }
        }

        /// <summary>
        /// Emits code responsible for changing the variables identity.
        /// </summary>
        /// <param name="target">The target to compile to</param>
        protected virtual void EmitNewDeclareCode(CompilerTarget target)
        {
            _ensureValid();
            //create command call
            //  unbind(->variable)
            var unlinkCall = new AstGetSetSymbol(File, Line, Column, 
                PCall.Get, Engine.UnbindAlias, SymbolInterpretations.Command);
            var targetRef = new AstGetSetReference(File, Line, Column, 
                Id, SymbolInterpretations.LocalObjectVariable);
            unlinkCall.Arguments.Add(targetRef);

            //Optimize call and emit code
            var call = (IAstExpression) unlinkCall;
            _OptimizeNode(target, ref call);
            var optCall = call as IAstEffect;
            if (optCall != null)
                optCall.EmitEffectCode(target);
            else
                call.EmitCode(target);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            _emitCode(target, justEffect);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            _emitCode(target, false);
        }

        private void _emitCode(CompilerTarget target, bool justEffect)
        {
            EmitNewDeclareCode(target);
            if (Expression != null)
                if (justEffect)
                    Expression.EmitEffectCode(target);
                else
                    Expression.EmitCode(target);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            var wrappedExpr = (IAstExpression) Expression;
            if (wrappedExpr != null)
            {
                _OptimizeNode(target, ref wrappedExpr);
                var optExpr = wrappedExpr as AstGetSet;
                if (optExpr != null)
                    Expression = optExpr;
            }

            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            var expr2 = Expression == null ? null : Expression.GetCopy();
            var newDecl2 = new AstGetSetNewDecl(File, Line, Column, Id, expr2);
            CopyBaseMembers(newDecl2);
            return newDecl2;
        }

        #endregion

        /// <summary>
        /// <para>The expression wrapped by the new decl. Most of the time either <see cref="AstGetSetSymbol"/> or <see cref="AstGetSetReference"/>.</para>
        /// <para>Other expressions are possible as well, though they make little sense wrapped by a new-declaration.</para>
        /// </summary>
        public AstGetSet Expression { get; set; }

        public override PCall Call
        {
            get
            {
                var expr = Expression;
                if (expr == null)
                    return _fallbackCall;
                else
                    return expr.Call;
            }
            set
            {
                var expr = Expression;
                if (expr == null)
                    _fallbackCall = value;
                else
                    expr.Call = value;
            }
        }

        /// <summary>
        /// The physical id of the local variable to be new-declared.
        /// </summary>
        public string Id { get; set; }

        private void _ensureValid()
        {
            if (Id == null)
                throw new InvalidOperationException("NewDecl node must have a non-null id");
        }
    }
}