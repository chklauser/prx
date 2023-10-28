#nullable enable
using System;
using JetBrains.Annotations;

namespace Prexonite.Compiler;

public class ErrorMessageException : PrexoniteException
{
    public ErrorMessageException(Message compilerMessage)
        : base(compilerMessage.Text)
    {
        CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
    }

    public ErrorMessageException(string message, Message compilerMessage) : base(message)
    {
        CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
    }

    public ErrorMessageException(string message, Message compilerMessage, Exception? innerException) : base(message, innerException)
    {
        CompilerMessage = compilerMessage ?? throw new ArgumentNullException(nameof(compilerMessage));
    }

    public Message CompilerMessage { get; }
}