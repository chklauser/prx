using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    public class AstGetException : AstNode, IAstExpression
    {
        internal AstGetException(Parser p)
            : base(p)
        {
        }

        public AstGetException(string file, int line, int column)
            : base(file, line, column)
        {
        }

        public override void EmitCode(CompilerTarget target)
        {
            target.Emit(OpCode.exc);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            return false;
        }

        #endregion
    }
}
