using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    public class AstBlockExpression : AstBlock, IAstExpression
    {

        public AstBlockExpression(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstBlockExpression(Parser p)
            : base(p)
        {
        }

        #region IAstExpression Members

        public virtual bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Will be optimized after code generation, hopefully
            expr = null;
            return false;
        }

        #endregion
    }
}
