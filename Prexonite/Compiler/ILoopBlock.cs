namespace Prexonite.Compiler
{
    public interface ILoopBlock
    {
        string ContinueLabel { get; }
        string BreakLabel { get; }
        string BeginLabel { get; }
    }
}