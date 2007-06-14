using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    public class AstCreateCoroutine : AstNode, IAstExpression
    {
        public IAstExpression Expression;
        public List<IAstExpression> Arguments = new List<IAstExpression>();

        public AstCreateCoroutine(string file, int line, int col)
            : base(file, line,col)
        {
        }

        internal AstCreateCoroutine(Parser p)
            : base(p)
        {
        }

        public override void EmitCode(CompilerTarget target)
        {
            if(Expression == null)
                throw new PrexoniteException("CreateCoroutine node requires an Expression.");

            Expression.EmitCode(target);
            foreach (IAstExpression argument in Arguments)
                argument.EmitCode(target);

            target.Emit(OpCode.newcor,Arguments.Count);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            OptimizeNode(target, ref Expression);
            
            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in Arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException("Invalid (null) argument in CreateCoroutine node (" + ToString() +
                                                 ") detected at position " + Arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = Arguments.IndexOf(arg);
                    Arguments.Insert(idx, oArg);
                    Arguments.RemoveAt(idx + 1);
                }
            }
            expr = null;
            return false;
        }

        #endregion
    }
}
