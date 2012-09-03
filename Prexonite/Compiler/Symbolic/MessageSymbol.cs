using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{Message.Severity}({Message.MessageClass}) {Symbol}")]
    public sealed class MessageSymbol : Symbol
    {
        [NotNull]
        public static MessageSymbol Create([NotNull] Message message, [CanBeNull] Symbol symbol)
        {
            return new MessageSymbol(message, symbol);
        }

        [NotNull]
        private readonly Message _message;

        [CanBeNull]
        private readonly Symbol _symbol;

        private MessageSymbol([NotNull] Message message, [CanBeNull] Symbol symbol)
        {
            _message = message;
            _symbol = symbol;
        }

        [NotNull]
        public Message Message
        {
            get { return _message; }
        }

        [CanBeNull]
        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public bool Equals([CanBeNull] MessageSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._message, _message) && Equals(other._symbol, _symbol);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MessageSymbol)) return false;
            return Equals((MessageSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_message.GetHashCode()*397) ^ (_symbol != null ? _symbol.GetHashCode() : 0);
            }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleMessage(this, argument);
        }

        public override bool TryGetMessageSymbol(out MessageSymbol messageSymbol)
        {
            messageSymbol = this;
            return true;
        }
    }
}