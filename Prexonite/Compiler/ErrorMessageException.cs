#nullable enable
using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Prexonite.Compiler
{
    public class ErrorMessageException : PrexoniteException
    {
        public ErrorMessageException([NotNull] Message compilerMessage)
            : base(compilerMessage.Text)
        {
            CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
        }

        public ErrorMessageException(string message, [NotNull] Message compilerMessage) : base(message)
        {
            CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
        }

        public ErrorMessageException(string message, [NotNull] Message compilerMessage, Exception? innerException) : base(message, innerException)
        {
            CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
        }

        [NotNull]
        public Message CompilerMessage { get; }
    }
}