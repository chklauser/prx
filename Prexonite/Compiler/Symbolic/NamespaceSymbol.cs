using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class NamespaceSymbol : Symbol, IEquatable<NamespaceSymbol>
    {
        private readonly Namespace _namespace;
        private readonly ISourcePosition _position;

        public Namespace Namespace
        {
            get { return _namespace; }
        }

        public override string ToString()
        {
            return String.Format("namespace");
        }

        private NamespaceSymbol([NotNull] ISourcePosition position, [NotNull] Namespace @namespace)
        {
            _position = position;
            _namespace = @namespace;
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleNamespace(this, argument);
        }

        public override ISourcePosition Position
        {
            get { return _position; }
        }

        public override bool Equals(Symbol other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (ReferenceEquals(other, null)) return false;
            return (other is NamespaceSymbol) && _equalsNonNull((NamespaceSymbol)other);
        }

        public bool Equals(NamespaceSymbol other)
        {
            if (ReferenceEquals(other, this)) return true;
            return !ReferenceEquals(other, null) && _equalsNonNull(other);
        }

        private bool _equalsNonNull(NamespaceSymbol other)
        {
            return Namespace.Equals(other.Namespace);
        }

        public override int GetHashCode()
        {
            return _namespace.GetHashCode();
        }

        internal static NamespaceSymbol _Create([NotNull] Namespace @namespace, [NotNull] ISourcePosition position)
        {
            return new NamespaceSymbol(position, @namespace);
        }

        public override bool TryGetNamespaceSymbol(out NamespaceSymbol namespaceSymbol)
        {
            namespaceSymbol = this;
            return true;
        }

        private static readonly UnwrapHandler _unwrapHandler = new UnwrapHandler();
        private class UnwrapHandler : SymbolHandler<Tuple<IMessageSink,ISourcePosition,IList<Message>>, NamespaceSymbol>
        {
            public override NamespaceSymbol HandleMessage(MessageSymbol self, Tuple<IMessageSink,ISourcePosition,IList<Message>> argument)
            {
                if (self.Message.Severity == MessageSeverity.Error && argument.Item3 != null)
                {
                    argument.Item3.Add(self.Message);
                    return self.InnerSymbol.HandleWith(this, argument);
                }
                else
                {
                    argument.Item1.ReportMessage(self.Message);
                    // Collect further messages but return null to indicate that there was an error
                    self.InnerSymbol.HandleWith(this, argument);
                    return null;
                }
               }

            protected override NamespaceSymbol HandleSymbolDefault(Symbol self, Tuple<IMessageSink,ISourcePosition,IList<Message>> argument)
            {
                var msg = Message.Error(String.Format(Resources.Parser_NamespaceExpected, "symbol", self), argument.Item2,
                    MessageClasses.NamespaceExcepted);
                if (argument.Item3 != null)
                    argument.Item3.Add(msg);
                else
                    argument.Item1.ReportMessage(msg);
                return null;
            }

            public override NamespaceSymbol HandleNamespace(NamespaceSymbol self, Tuple<IMessageSink,ISourcePosition,IList<Message>> argument)
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
        [CanBeNull]
        public static NamespaceSymbol UnwrapNamespaceSymbol([NotNull] Symbol symbol, [NotNull] ISourcePosition symbolPosition,
            [NotNull]IMessageSink messageSink, [CanBeNull] IList<Message> errors = null)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));
            if (messageSink == null)
                throw new ArgumentNullException(nameof(messageSink));
            if (symbolPosition == null)
                throw new ArgumentNullException(nameof(symbolPosition));

            return symbol.HandleWith(_unwrapHandler, Tuple.Create(messageSink, symbolPosition, errors));
        }
    }
}