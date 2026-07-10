using System.Diagnostics;

namespace Prexonite.Compiler.Ast;

public class AstLoopBlock : AstScopedBlock, ILoopBlock
{
    public const string ContinueWord = "continue";
    public const string BreakWord = "break";
    public const string BeginWord = "begin";

    [DebuggerStepThrough]
    public AstLoopBlock(
        string file,
        int line,
        int column,
        AstBlock parentBlock,
        string? uid = null,
        string? prefix = null
    )
        : this(new SourcePosition(file, line, column), parentBlock, uid, prefix) { }

    [DebuggerStepThrough]
    internal AstLoopBlock(
        ISourcePosition p,
        AstBlock parentNode,
        string? uid = null,
        string? prefix = null
    )
        : base(p, parentNode, uid: uid, prefix: prefix)
    {
        //See other ctor!
        ContinueLabel = CreateLabel(ContinueWord);
        BreakLabel = CreateLabel(BreakWord);
        BeginLabel = CreateLabel(BeginWord);
    }

    public string ContinueLabel { get; }

    public string BreakLabel { get; }

    public string BeginLabel { get; }
}
