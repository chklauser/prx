namespace Prexonite.Compiler;

class MessageEventArgs : EventArgs
{
    public Message Message { get; }

    public MessageEventArgs(Message message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}
