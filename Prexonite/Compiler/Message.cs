using System;

namespace Prexonite.Compiler
{
    [Serializable]
    public partial class Message : ISourcePosition
    {
        public static Message Create(MessageSeverity severity, string text, ISourcePosition position, string messageClass)
        {
            return new Message(severity, text, position, messageClass);
        }

        public static Message Create(MessageSeverity severity, string text, string file, int line, int column, string messageClass)
        {
            return new Message(severity, text, file, line, column, messageClass);
        }

        private const string MessageFormat = "-- ({3}) line {0} col {1}: {2}"; // 0=line, 1=column, 2=text, 3=file
        private readonly string _text;
        public string Text { get { return _text; } }
        private readonly string _file;
        private readonly int _line;
        private readonly int _column;
        public string File { get { return _file; } }
        public int Line { get { return _line; } }
        public int Column { get { return _column; } }
        private readonly MessageSeverity _severity;
        private readonly string _messageClass;
        public string MessageClass { get { return _messageClass; } }
        public MessageSeverity Severity { get { return _severity; } }

        private Message(MessageSeverity severity, string text, string file, int line, int column, string messageClass = null)
        {
            if (text == null)
                throw new ArgumentNullException();
            _text = text;
            _file = file;
            _line = line;
            _column = column;
            _severity = severity;
            _messageClass = messageClass;
        }

        public static Message Error(string message, string file, int line, int column, string messageClass = null)
        {
            return Create(MessageSeverity.Error, message, file, line, column, messageClass);
        }

        public static Message Error(string message, ISourcePosition position, string messageClass = null)
        {
            return Create(MessageSeverity.Error, message, position, messageClass);
        }

        public static Message Warning(string message, string file, int line, int column, string messageClass = null)
        {
            return Create(MessageSeverity.Warning, message, file, line, column,messageClass);
        }

        public static Message Warning(string message, ISourcePosition position, string messageClass = null)
        {
            return Create(MessageSeverity.Warning, message, position, messageClass);
        }

        public static Message Info(string message, string file, int line, int column, string messageClass = null)
        {
            return Create(MessageSeverity.Info, message, file, line, column,messageClass);
        }

        public static Message Info(string message, ISourcePosition position, string messageClass = null)
        {
            return Create(MessageSeverity.Info, message, position, messageClass);
        }

        private Message(MessageSeverity severity, string text, ISourcePosition position, string messageClass = null)
            : this(severity, text, position.File, position.Line, position.Column,messageClass)
        {
        }

        public override string ToString()
        {
            return String.Format(MessageFormat, Line, Column, Text, File);
        }
    }
}
