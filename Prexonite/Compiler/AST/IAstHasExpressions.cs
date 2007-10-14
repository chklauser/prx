namespace Prexonite.Compiler.Ast
{
    public interface IAstHasExpressions
    {
        IAstExpression[] Expressions
        {
            get;
        }
    }
}