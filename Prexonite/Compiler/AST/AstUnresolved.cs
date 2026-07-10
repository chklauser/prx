using Prexonite.Properties;

namespace Prexonite.Compiler.Ast;

public class AstUnresolved : AstGetSetImplBase
{
    public AstUnresolved(string file, int line, int column, string id)
        : base(file, line, column, PCall.Get)
    {
        Id = id;
    }

    internal AstUnresolved(Parser p, string id)
        : base(p, PCall.Get)
    {
        Id = id;
    }

    public AstUnresolved(ISourcePosition position, string id)
        : base(position, PCall.Get)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    #region Overrides of AstGetSet

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        _reportUnresolved(target);
        target.EmitNull(Position);
    }

    void _reportUnresolved(CompilerTarget target)
    {
        target.Loader.ReportMessage(
            Message.Error(
                string.Format(Resources.AstUnresolved_The_symbol__0__has_not_been_resolved_, Id),
                Position,
                MessageClasses.SymbolNotResolved
            )
        );
    }

    public string Id { get; }

    protected override void EmitSetCode(CompilerTarget target)
    {
        _reportUnresolved(target);
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstUnresolved(File, Line, Column, Id);
        CopyBaseMembers(copy);
        return copy;
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        if (base.TryOptimize(target, out expr))
            return true;
        else
        {
            AstExpr? sol = this;
            do
            {
                foreach (var resolver in target.Loader.CustomResolvers)
                {
                    sol = resolver.Resolve(target, (sol as AstUnresolved)!);
                    if (sol != null)
                        break;
                }
                expr = sol;
            } while (sol != this && expr is AstUnresolved);
            if (sol == this)
                return false;
            else
                return expr != null;
        }
    }

    #endregion
}
