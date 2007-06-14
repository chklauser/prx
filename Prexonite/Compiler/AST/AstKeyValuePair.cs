using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    public class AstKeyValuePair : AstNode, IAstExpression
    {
        public AstKeyValuePair(string file, int line, int column)
            : this(file, line, column, null, null)
        {
        }

        public AstKeyValuePair(string file, int line, int column, IAstExpression key, IAstExpression value)
            : base(file, line, column)
        {
            Key = key;
            Value = value;
        }

        internal AstKeyValuePair(Parser p)
            : this(p, null, null)
        {
        }

        internal AstKeyValuePair(Parser p, IAstExpression key, IAstExpression value)
            : base(p)
        {
            Key = key;
            Value = value;
        }

        public IAstExpression Key;
        public IAstExpression Value;

        public override void EmitCode(CompilerTarget target)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException("AstKeyValuePair.Value must be initialized.");

            Key.EmitCode(target);
            Value.EmitCode(target);
            target.EmitCommandCall(2, Engine.PairCommand);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException("AstKeyValuePair.Value must be initialized.");

            OptimizeNode(target, ref Key);
            OptimizeNode(target, ref Value);

            expr = null;

            return false;
        }

        #endregion
    }
}
