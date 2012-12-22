using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public class MessageSymbol : WrappingSymbol, IEquatable<MessageSymbol>
    {
        public override string ToString()
        {
            return string.Format(
                "{0}({1}) {2}", Enum.GetName(typeof (MessageSeverity), Message.Severity),
                Message.MessageClass, base.InnerSymbol);
        }

        [NotNull]
        internal static MessageSymbol _Create([NotNull] Message message, [NotNull] Symbol inner, [CanBeNull] ISourcePosition position)
        {
            return new MessageSymbol(position ?? inner.Position, message, inner);
        }

        [NotNull]
        private readonly Message _message;

        private MessageSymbol([NotNull] ISourcePosition position, [NotNull] Message message, [NotNull] Symbol symbol)
            : base(position, symbol)
        {
            _message = message;
        }

        [NotNull]
        public Message Message
        {
            get { return _message; }
        }

        #region Equality members

        public bool Equals(MessageSymbol other)
        {
            return base.Equals(other) && _message.Equals(other._message);
        }

        public override bool Equals(Symbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is MessageSymbol && Equals((MessageSymbol)other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ _message.GetHashCode();
            }
        }

        protected override int HashCodeXorFactor
        {
            get { return 599; }
        }

        public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition newPosition = null)
        {
            return new MessageSymbol(newPosition ?? Position, Message,newInnerSymbol);
        }

        #endregion

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