

using System.Text;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public class AstListLiteral : AstExpr,
    IAstHasExpressions,
    IAstPartiallyApplicable
{
    public List<AstExpr> Elements = new();

    internal AstListLiteral(Parser p)
        : base(p)
    {
    }

    public AstListLiteral(string file, int line, int column)
        : base(file, line, column)
    {
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Elements.ToArray();

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        foreach (var arg in Elements.ToArray())
        {
            if (arg == null)
                throw new PrexoniteException(
                    "Invalid (null) argument in ListLiteral node (" + ToString() +
                    ") detected at position " + Elements.IndexOf(null!) + ".");
            var oArg = _GetOptimizedNode(target, arg);
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
        var call = target.Factory.Call(Position, EntityRef.Command.Create(Engine.ListAlias));
        call.Arguments.AddRange(Elements);
        call.EmitCode(target,stackSemantics);
    }

    #region Implementation of IAstPartiallyApplicable

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        DoEmitCode(target,StackSemantics.Value);
        //Code is the same. Partial application is handled by AstGetSetSymbol
    }

    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(
            Elements.Any(x => x.IsPlaceholder()), 
            Elements.Any(x => x.IsArgumentSplice()));
    }

    #endregion

    public override string ToString()
    {
        const int limit = 20;
        var end = Elements.Count == limit + 1 ? limit + 1 : Math.Min(limit, Elements.Count);
        var sb = new StringBuilder("[ ", end*15);
        var i = 0;
        for (; i < end; i++)
        {
            sb.Append(Elements[i]);
            if (i + 1 < end)
                sb.Append(", ");
        }

        if (i < Elements.Count)
        {
            sb.AppendFormat(", ... «{0}» ..., {1} ]", Elements.Count - limit,
                Elements[^1]);
        }
        else
        {
            sb.Append(" ]");
        }

        return sb.ToString();
    }
}