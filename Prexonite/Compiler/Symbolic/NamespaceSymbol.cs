#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class NamespaceSymbol : Symbol, IEquatable<NamespaceSymbol>
{
    public Namespace Namespace { get; }

    public override string ToString()
    {
        return "namespace";
    }

    NamespaceSymbol([NotNull] ISourcePosition position, [NotNull] Namespace @namespace)
    {
        Position = position;
        Namespace = @namespace;
    }

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleNamespace(this, argument);
    }

    public override ISourcePosition Position { get; }

    public override bool Equals(Symbol? other)
    {
        if (ReferenceEquals(other, this)) return true;
        if (ReferenceEquals(other, null)) return false;
        return other is NamespaceSymbol nsSymbol && _equalsNonNull(nsSymbol);
    }

    public bool Equals(NamespaceSymbol? other)
    {
        if (ReferenceEquals(other, this)) return true;
        return !ReferenceEquals(other, null) && _equalsNonNull(other);
    }

    bool _equalsNonNull(NamespaceSymbol other)
    {
        return Namespace.Equals(other.Namespace);
    }

    public override int GetHashCode()
    {
        return Namespace.GetHashCode();
    }

    internal static NamespaceSymbol _Create([NotNull] Namespace @namespace, [NotNull] ISourcePosition position)
    {
        return new(position, @namespace);
    }

    public override bool TryGetNamespaceSymbol(out NamespaceSymbol namespaceSymbol)
    {
        namespaceSymbol = this;
        return true;
    }

    static readonly UnwrapHandler _unwrapHandler = new();

    class UnwrapHandler : SymbolHandler<(IMessageSink? sink,ISourcePosition position,IList<Message>? errors), NamespaceSymbol?>
    {
        public override NamespaceSymbol? HandleMessage(MessageSymbol self, (IMessageSink? sink,ISourcePosition position,IList<Message>? errors) argument)
        {
            if (self.Message.Severity == MessageSeverity.Error && argument.errors != null)
            {
                argument.errors.Add(self.Message);
                return self.InnerSymbol.HandleWith(this, argument);
            }
            else
            {
                argument.sink?.ReportMessage(self.Message);
                // Collect further messages but return null to indicate that there was an error
                self.InnerSymbol.HandleWith(this, argument);
                return null;
            }
        }

        protected override NamespaceSymbol? HandleSymbolDefault(Symbol self, (IMessageSink?,ISourcePosition,IList<Message>?) argument)
        {
            var (sink, position, errors) = argument;
            var msg = Message.Error(string.Format(Resources.Parser_NamespaceExpected, "symbol", self), position,
                MessageClasses.NamespaceExcepted);
            errors?.Add(msg);
            if(errors != null || msg.Severity != MessageSeverity.Error)
                sink?.ReportMessage(msg);
            return null;
        }

        public override NamespaceSymbol HandleNamespace(NamespaceSymbol self, (IMessageSink?,ISourcePosition,IList<Message>?) argument)
        {
            return self;
        }
    }

    /// <summary>
    /// Unwraps a symbol to retrieve a namespace symbol. Reports any messages found along the way.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="symbolPosition"></param>
    /// <param name="messageSink"></param>
    /// <param name="errors">If non-null, collects instead of reports errors (other messages are reported directly); Otherwise errors are reported too.</param>
    /// <returns>The namespace symbol or null if the symbol is not actually a namespace symbol (has already been reported as an error). If an error collection list (<paramref name="errors"/>) has been supplied, can be non-null when errors are present)</returns>
    public static NamespaceSymbol? UnwrapNamespaceSymbol([NotNull] Symbol symbol, [NotNull] ISourcePosition symbolPosition,
        IMessageSink? messageSink, IList<Message>? errors = null)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));
        if (messageSink == null)
            throw new ArgumentNullException(nameof(messageSink));
        if (symbolPosition == null)
            throw new ArgumentNullException(nameof(symbolPosition));

        return symbol.HandleWith(_unwrapHandler, (messageSink, symbolPosition, errors));
    }
}