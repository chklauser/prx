using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler
{
    [DebuggerDisplay("declaration scope {ToString()}")]
    public class DeclarationScope
    {
        [NotNull]
        private readonly LocalNamespace _namespace;

        [NotNull]
        private readonly SymbolStore _store;
        private readonly QualifiedId _pathPrefix;

        [NotNull]
        public Namespace Namespace
        {
            get { return _namespace; }
        }

        [NotNull]
        internal LocalNamespace _LocalNamespace
        {
            get { return _namespace; }
        }

        public QualifiedId PathPrefix
        {
            get { return _pathPrefix; }
        }

        /// <summary>
        /// Symbol store for symbols local to the scope (private, not necessarily exported)
        /// </summary>
        public SymbolStore Store
        {
            get { return _store; }
        }

        internal DeclarationScope([NotNull] LocalNamespace ns, QualifiedId pathPrefix, [NotNull] SymbolStore store)
        {
            if (ns == null) throw new ArgumentNullException("ns");
            if (store == null)
                throw new ArgumentNullException("store");
                
            _namespace = ns;
            _pathPrefix = pathPrefix;
            _store = store;
        }

        public override string ToString()
        {
            return _pathPrefix.ToString();
        }
    }
}