namespace Prexonite.Compiler.Ast;

public class AstHashLiteral : AstExpr, IAstHasExpressions
{
    public List<AstExpr> Elements = new();

    internal AstHashLiteral(Parser p)
        : base(p) { }

    public AstHashLiteral(string file, int line, int column)
        : base(file, line, column) { }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Elements.ToArray();

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        AstExpr oArg;
        foreach (var arg in Elements.ToArray())
        {
            if ((AstExpr?)arg == null)
                throw new PrexoniteException(
                    "Invalid (null) argument in HashLiteral node ("
                        + ToString()
                        + ") detected at position "
                        + Elements.IndexOf(null!)
                        + "."
                );
            oArg = _GetOptimizedNode(target, arg);
            if (!ReferenceEquals(oArg, arg))
            {
                var idx = Elements.IndexOf(arg);
                Elements.Insert(idx, oArg);
                Elements.RemoveAt(idx + 1);
            }
        }
        expr = null;
        return false;
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (Elements.Count == 0)
        {
            target.Emit(Position, OpCode.newobj, 0, "Hash");
        }
        else
        {
            foreach (var element in Elements)
            {
                if (element is AstConstant)
                    throw new PrexoniteException(
                        string.Concat(
                            "Hashes are built from key-value pairs, not constants like ",
                            element,
                            ". [File: ",
                            File,
                            ", Line: ",
                            Line,
                            "]"
                        )
                    );
                element.EmitCode(target, stackSemantics);
            }

            if (stackSemantics == StackSemantics.Effect)
                return;

            target.EmitStaticGetCall(Position, Elements.Count, "Hash", "Create", false);
        }
    }
}
