#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Prexonite.Compiler.Ast
{
    public class AstConstantTypeExpression : AstTypeExpr
    {
        #region AstExpr Members

        public string TypeExpression;

        public AstConstantTypeExpression(string file, int line, int column, string typeExpression)
            : base(file, line, column)
        {
            TypeExpression = typeExpression ?? throw new ArgumentNullException(nameof(typeExpression));
        }

        internal AstConstantTypeExpression(Parser p, string typeExpression)
            : this(p.scanner.File, p.t.line, p.t.col, typeExpression)
        {
        }

        /// <inheritdoc />
        public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
        {
            expr = null;
            return false;
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics == StackSemantics.Effect)
                return;

            target.Emit(Position,OpCode.ldr_type, TypeExpression);
        }

        #endregion

        public override string ToString()
        {
            return "~" + TypeExpression;
        }
    }
}