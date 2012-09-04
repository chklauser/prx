using System;
using JetBrains.Annotations;

namespace Prexonite.Compiler
{
    [Serializable]
    public class Message : ISourcePosition
    {
        [NotNull]
        public static Message Create(MessageSeverity severity, [NotNull, LocalizationRequired] string text, [NotNull] ISourcePosition position, string messageClass)
        {
            return new Message(severity, text, position, messageClass);
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

        private Message(MessageSeverity severity, [NotNull] string text, string file, int line, int column,
                        [CanBeNull]string messageClass = null)
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

        [NotNull]
        public static Message Error([NotNull, LocalizationRequired] string message, [NotNull] ISourcePosition position, [CanBeNull] string messageClass)
        {
            return Create(MessageSeverity.Error, message, position, messageClass);
        }

        [NotNull]
        public static Message Warning([NotNull, LocalizationRequired] string message, [NotNull] ISourcePosition position, [CanBeNull] string messageClass)
        {
            return Create(MessageSeverity.Warning, message, position, messageClass);
        }

        [NotNull]
        public static Message Info([NotNull, LocalizationRequired] string message, ISourcePosition position, [CanBeNull] string messageClass)
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

        [NotNull]
        public Message Repositioned([NotNull] ISourcePosition position)
        {
            if (position == null)
                throw new ArgumentNullException("position");
            return new Message(Severity,Text,position,MessageClass);
        }
    }
}
