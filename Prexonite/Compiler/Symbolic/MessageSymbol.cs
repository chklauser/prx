using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public class MessageSymbol : WrappingSymbol, IEquatable<MessageSymbol>
{
    public override string ToString()
    {
        return $"{Enum.GetName(typeof(MessageSeverity), Message.Severity)}({Message.MessageClass}) {InnerSymbol}";
    }

    internal static MessageSymbol _Create(Message message, Symbol inner, ISourcePosition? position)
    {
        return new(position ?? inner.Position, message, inner);
    }

    MessageSymbol(ISourcePosition position, Message message, Symbol symbol)
        : base(position, symbol)
    {
        Message = message;
    }

    public Message Message { get; }

    #region Equality members

    public bool Equals(MessageSymbol? other)
    {
        return base.Equals(other) && Message.Equals(other?.Message);
    }

    public override bool Equals(Symbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return other is MessageSymbol otherMessage && Equals(otherMessage);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (base.GetHashCode() * 397) ^ Message.GetHashCode();
        }
    }

    protected override int HashCodeXorFactor => 599;

    public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition? newPosition = null)
    {
        return new MessageSymbol(newPosition ?? Position, Message, newInnerSymbol);
    }

    #endregion

    public override TResult HandleWith<TArg, TResult>(
        ISymbolHandler<TArg, TResult> handler,
        TArg argument
    )
    {
        return handler.HandleMessage(this, argument);
    }

    public override bool TryGetMessageSymbol([NotNullWhen(true)] out MessageSymbol? messageSymbol)
    {
        messageSymbol = this;
        return true;
    }
}
