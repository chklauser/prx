namespace Prexonite.Compiler.Ast;

public class AstArgumentSplice : AstExpr, IAstHasExpressions
{
    public AstExpr ArgumentList { get; private set; }

    public bool IsSplicedPlaceholder => ArgumentList is AstPlaceholder;

    public AstArgumentSplice(ISourcePosition position, AstExpr argumentList)
        : base(position)
    {
        ArgumentList = argumentList ?? throw new ArgumentNullException(nameof(argumentList));
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics semantics)
    {
        _throwSyntaxNotSupported();
    }

    void _throwSyntaxNotSupported()
    {
        throw new PartialApplicationSyntaxNotSupportedException(
            $"This syntax does not support argument slices (*some_expr). (Position {File}:{Line} col {Column})"
        );
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        if (ArgumentList.TryOptimize(target, out var next))
        {
            ArgumentList = next;
        }
        expr = null;
        return false;
    }

    public AstExpr[] Expressions => [ArgumentList];

    public bool IsPlaceholderSplice => ArgumentList is AstPlaceholder;

    public static void ReportNotSupported(
        AstNode splice,
        CompilerTarget target,
        StackSemantics semantics
    )
    {
        target.Loader.ReportMessage(
            Message.Error(
                // Resources.AstNode__argumentSpliceNotSupportedInThisPosition
                "Argument splice not supported in this position.",
                splice.Position,
                MessageClasses.ArgumentSpliceNotSupported
            )
        );
        if (semantics == StackSemantics.Value)
        {
            target.Factory.Null(splice.Position).EmitValueCode(target);
        }
    }

    public override string ToString()
    {
        return $"*({ArgumentList})";
    }
}
