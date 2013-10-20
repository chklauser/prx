using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class NamespaceSymbol : Symbol, IEquatable<NamespaceSymbol>
    {
        [NotNull]
        private readonly string _logicalName;

        private readonly Namespace _namespace;
        private readonly ISourcePosition _position;

        public string LogicalName
        {
            get { return _logicalName; }
        }

        public Namespace Namespace
        {
            get { return _namespace; }
        }

        public override string ToString()
        {
            return String.Format("namespace {0}", LogicalName);
        }

        private NamespaceSymbol([NotNull] ISourcePosition position, [NotNull] Namespace @namespace,
            [NotNull] string logicalName)
        {
            _position = position;
            _namespace = @namespace;
            _logicalName = logicalName;
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

        internal static NamespaceSymbol _Create([NotNull] Namespace @namespace, [NotNull] string logicalName, [NotNull] ISourcePosition position)
        {
            return new NamespaceSymbol(position, @namespace, logicalName);
        }

        public override bool TryGetNamespaceSymbol(out NamespaceSymbol namespaceSymbol)
        {
            namespaceSymbol = this;
            return true;
        }
    }
}