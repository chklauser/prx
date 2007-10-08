using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstCoalescence : AstNode,
                                  IAstExpression
    {
        public AstCoalescence(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstCoalescence(Parser p)
            : base(p)
        {
        }

        public readonly List<IAstExpression> Expressions = new List<IAstExpression>(2);

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in Expressions.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + Expressions.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = Expressions.IndexOf(arg);
                    Expressions.Insert(idx, oArg);
                    Expressions.RemoveAt(idx + 1);
                }
            }

            foreach (IAstExpression iexpr in Expressions.ToArray())
            {
                if (iexpr is AstNull ||
                    (iexpr is AstConstant && ((AstConstant) iexpr).Constant == null))
                    Expressions.Remove(iexpr);
            }

            if (Expressions.Count == 1)
            {
                expr = Expressions[0];
                return true;
            }
            else if (Expressions.Count == 0)
            {
                expr = new AstNull(File, Line, Column);
                return true;
            }
            else
                return false;
        }

        #endregion

        private static int _count = -1;

        public override void EmitCode(CompilerTarget target)
        {
            //Expressions contains at least two expressions

            _count++;
            string endOfExpression_label = "coal\\n" + _count + "\\end";
            for (int i = 0; i < Expressions.Count; i++)
            {
                IAstExpression expr = Expressions[i];

                if (i > 0)
                {
                    target.EmitPop();
                }

                expr.EmitCode(target);

                if (i + 1 < Expressions.Count)
                {
                    target.EmitDuplicate();
                    target.Emit(OpCode.check_const, "Null");
                    target.EmitJumpIfFalse(endOfExpression_label);
                }
            }

            target.EmitLabel(endOfExpression_label);
        }
    }
}