using System;

namespace Prexonite.Compiler.Ast
{
    public delegate void AstAction(CompilerTarget target);

    public class AstActionBlock : AstBlock
    {
        public AstAction Action = null;

        public AstActionBlock(string file, int line, int column, AstAction action)
            : base(file, line, column)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            Action = action;
        }

        public AstActionBlock(AstNode parent, AstAction action)
            : this(parent.File, parent.Line, parent.Column, action)
        {
        }

        public override void EmitCode(CompilerTarget target)
        {
            base.EmitCode(target);
            Action(target);
        }

        public override bool IsEmpty
        {
            get { return false; }
        }

        public override bool IsSingleStatement
        {
            get { return false; }
        }
    }
}