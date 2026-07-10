using Prexonite.Compiler;

namespace Prexonite.Internal;

partial class Parser
{
    internal Parser(IScanner scanner, StackContext sctx)
        : this(scanner)
    {
        Sctx = sctx;
    }

    public StackContext Sctx { get; } = null!; // initialized in the only externally accessible constructor

    PType? _lastType;

    public PType? LastType => _lastType;

    // ReSharper disable once InconsistentNaming
    void SemErr(string message, string messageClass, ISourcePosition? position = null)
    {
        errors.Add(Message.Error(message, position ?? GetPosition(), messageClass));
    }
}

partial class Errors
{
    internal void Add(Message message)
    {
        AddLast(message);
        OnMessageReceived(message);
    }
}
