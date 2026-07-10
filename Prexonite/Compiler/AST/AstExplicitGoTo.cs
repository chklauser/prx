namespace Prexonite.Compiler.Ast;

public sealed class AstExplicitGoTo : AstNode
{
    public AstExplicitGoTo(string file, int line, int column, string destination)
        : base(file, line, column)
    {
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
    }

    internal AstExplicitGoTo(Parser p, string destination)
        : this(p.scanner.File, p.t.line, p.t.col, destination) { }

    public string Destination { get; }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        target.EmitJump(Position, Destination);
    }

    public override string ToString()
    {
        return "goto " + Destination;
    }
}
