namespace Prexonite.Compiler.Ast;

public class AstExplicitLabel : AstNode
{
    public string Label;

    public AstExplicitLabel(string file, int line, int column, string label)
        : base(file, line, column)
    {
        Label = label;
    }

    internal AstExplicitLabel(Parser p, string label)
        : this(p.scanner.File, p.t.line, p.t.col, label) { }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        target.EmitLabel(Position, Label);
    }

    public override string ToString()
    {
        return "label " + Label;
    }
}
