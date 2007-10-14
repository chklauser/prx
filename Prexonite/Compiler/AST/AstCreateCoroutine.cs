using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstCreateCoroutine : AstNode,
                                      IAstExpression,
                                      IAstHasExpressions
    {
        public IAstExpression Expression;

        private AstGetSet.ArgumentsProxy _proxy;

        public AstGetSet.ArgumentsProxy Arguments
        {
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        private List<IAstExpression> _arguments = new List<IAstExpression>();

        public AstCreateCoroutine(string file, int line, int col)
            : base(file, line, col)
        {
            _proxy = new AstGetSet.ArgumentsProxy(_arguments);
        }

        internal AstCreateCoroutine(Parser p)
            : base(p)
        {
            _proxy = new AstGetSet.ArgumentsProxy(_arguments);
        }

        public override void EmitCode(CompilerTarget target)
        {
            if (Expression == null)
                throw new PrexoniteException("CreateCoroutine node requires an Expression.");

            Expression.EmitCode(target);
            foreach (IAstExpression argument in _arguments)
                argument.EmitCode(target);

            target.Emit(OpCode.newcor, _arguments.Count);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            OptimizeNode(target, ref Expression);

            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in CreateCoroutine node (" + ToString() +
                        ") detected at position " + _arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }
            expr = null;
            return false;
        }

        #endregion
    }
}