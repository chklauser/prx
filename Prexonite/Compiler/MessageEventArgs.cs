using System;

namespace Prexonite.Compiler
{
    internal class MessageEventArgs : EventArgs
    {
        private readonly Message _message;
        public Message Message { get { return _message; } }
        public MessageEventArgs(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            _message = message;
        }
    }
}