namespace Prexonite.Compiler.Ast;

public delegate void AstAction(CompilerTarget target);

public class AstActionBlock : AstScopedBlock
{
    public AstAction Action;

    public AstActionBlock(ISourcePosition position, AstBlock parent, AstAction action)
        : base(position, parent)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        base.DoEmitCode(target, stackSemantics);
        Action(target);
    }

    public override bool IsEmpty => false;

    public override bool IsSingleStatement => false;
}
