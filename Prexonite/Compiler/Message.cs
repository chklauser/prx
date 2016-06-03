// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using JetBrains.Annotations;

namespace Prexonite.Compiler
{
    [Serializable]
    public class Message : IEquatable<Message>, IComparable<Message>
    {
        [NotNull]
        public static Message Create(MessageSeverity severity, [NotNull, LocalizationRequired] string text, [NotNull] ISourcePosition position, string messageClass)
        {
            return new Message(severity, text, position, messageClass);
        }

        private const string MessageFormat = "-- ({3}) line {0} col {1}: {2}"; // 0=line, 1=column, 2=text, 3=file
        private readonly string _text;

        private readonly ISourcePosition _position;
        private readonly MessageSeverity _severity;
        private readonly string _messageClass;

        public string Text { get { return _text; } }
        public string MessageClass { get { return _messageClass; } }
        public MessageSeverity Severity { get { return _severity; } }
        public string File { get { return _position.File; } }
        public int Line { get { return _position.Line; } }
        public int Column { get { return _position.Column; } }
        public ISourcePosition Position { get { return _position; } }

        private Message(MessageSeverity severity, [NotNull] String text, [NotNull] ISourcePosition position,
                        [CanBeNull]string messageClass = null)
        {
            if (text == null)
                throw new ArgumentNullException();
            _text = text;
            _position = position;
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
        public static Message Info([NotNull, LocalizationRequired] string message, [NotNull] ISourcePosition position, [CanBeNull] string messageClass)
        {
            return Create(MessageSeverity.Info, message, position, messageClass);
        }

        public int CompareTo(Message other)
        {
            if (other == null)
            {
                return -1;
            }

            var r = String.Compare(File, other.File, StringComparison.Ordinal);
            if (r != 0)
                return r;
            
            r = Line.CompareTo(other.Line);
            if (r != 0)
                return r;

            r = Column.CompareTo(other.Column);
            if (r != 0)
                return r;

            r = Severity.CompareTo(other.Severity);
            if (r != 0)
                return r;

            r = String.Compare(MessageClass, other.MessageClass, StringComparison.Ordinal);
            if (r != 0)
                return r;

            return r;
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

        public bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_text, other._text) && Equals(_position, other._position) && _severity == other._severity && string.Equals(_messageClass, other._messageClass);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Message) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_text != null ? _text.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_position != null ? _position.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) _severity;
                hashCode = (hashCode*397) ^ (_messageClass != null ? _messageClass.GetHashCode() : 0);
                return hashCode;
            }
        }


    }
}
