namespace Prexonite.Compiler.Ast;

[SuppressMessage(
    "Microsoft.Naming",
    "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = nameof(Coroutine)
)]
public class AstCreateCoroutine : AstExpr, IAstHasExpressions
{
    public ArgumentsProxy Arguments { get; }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Arguments.ToArray();

    public required AstExpr Expression { get; set; }

    #endregion

    List<AstExpr> _arguments = new();

    public AstCreateCoroutine(string file, int line, int col)
        : base(file, line, col)
    {
        Arguments = new(_arguments);
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (stackSemantics == StackSemantics.Effect)
            return;

        if (Expression == null)
            throw new PrexoniteException("CreateCoroutine node requires an Expression.");

        Expression.EmitValueCode(target);
        foreach (var argument in _arguments)
            argument.EmitValueCode(target);

        target.Emit(Position, OpCode.newcor, _arguments.Count);
    }

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        Expression = _GetOptimizedNode(target, Expression);

        //Optimize arguments
        foreach (var arg in _arguments.ToArray())
        {
            if (arg == null)
                throw new PrexoniteException(
                    "Invalid (null) argument in CreateCoroutine node ("
                        + ToString()
                        + ") detected at position "
                        + _arguments.IndexOf(null!)
                        + "."
                );
            var oArg = _GetOptimizedNode(target, arg);
            if (!ReferenceEquals(oArg, arg))
            {
                var idx = _arguments.IndexOf(arg);
                _arguments.Insert(idx, oArg);
                _arguments.RemoveAt(idx + 1);
            }
        }
        expr = null;
        return false;
    }

    #endregion
}
