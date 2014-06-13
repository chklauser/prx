using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic.Internal
{
    internal class ModuleLevelView : SymbolStore
    {
        /// <summary>
        /// The scope that this filter wraps.
        /// </summary>
        [NotNull]
        private readonly SymbolStore _backingStore;

        /// <summary>
        /// Maps namespaces to already constructed proxies.
        /// </summary>
        /// <remarks>
        /// This dictionary is shared between all child-<see cref="ModuleLevelView"/>s. 
        /// </remarks>
        [NotNull]
        private readonly ConcurrentDictionary<Namespace, LocalNamespaceImpl> _localProxies;

        private ModuleLevelView([NotNull] SymbolStore backingStore, [NotNull] ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
        {
            if (backingStore == null)
                throw new ArgumentNullException("backingStore");
            if (localProxies == null)
                throw new ArgumentNullException("localProxies");

            _backingStore = backingStore;
            _localProxies = localProxies;
        }

        public static ModuleLevelView Create([NotNull] SymbolStore externalScope)
        {
            return new ModuleLevelView(externalScope, new ConcurrentDictionary<Namespace, LocalNamespaceImpl>());
        }

        internal class LocalNamespaceImpl : LocalNamespace
        {
            /// <summary>
            /// Wrapper around the symbols of this namespace coming from external sources.
            /// </summary>
            [NotNull]
            private readonly ModuleLevelView _localView;

            /// <summary>
            /// Holds module-local exports. Uses <see cref="_localView"/> as its external scope.
            /// </summary>
            [NotNull]
            private readonly SymbolStore _exportScope;

            /// <summary>
            /// Physical name prefix used for functions, variables etc. defined inside the namespace.
            /// </summary>
            /// <remarks>
            /// <para>Technically, this has nothing to do with logical namespace juggling, but it is one
            /// of the reasons why we have namespaces in the first place.</para>
            /// <para>
            /// This information is transient. It will not be serialized to disk.
            /// </para>
            /// </remarks>
            [CanBeNull]
            private string _prefix;

            internal LocalNamespaceImpl([NotNull] ISymbolView<Symbol> externalScope, [NotNull] ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
            {
                _exportScope = Create(externalScope);
                _localView = new ModuleLevelView(_exportScope, localProxies);
            }

            public override string Prefix
            {
                get { return _prefix; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException("value");

                    if (_prefix != null)
                        throw new InvalidOperationException(String.Format("The prefix for this namespace is already assigned. (Existing prefix: '{0}', new prefix: '{1}')",_prefix,value));

                    _prefix = value;
                }
            }

            public bool HasSameRootAs(ModuleLevelView view)
            {
                return ReferenceEquals(_localView._localProxies, view._localProxies);
            }

            public override bool TryGetExported(string id, out Symbol exported)
            {
                exported = null;
                return _exportScope.IsDeclaredLocally(id) && _exportScope.TryGet(id, out exported);
            }

            /// <summary>
            /// Adds a set of declarations to the exports of this namespace. Will replace conflicting symbols instead of merging with them.
            /// </summary>
            /// <param name="exportScope">The set of symbols to export.</param>
            public override void DeclareExports(IEnumerable<KeyValuePair<string, Symbol>> exportScope)
            {
                if (exportScope == null)
                    throw new ArgumentNullException("exportScope");
                
                foreach (var newExport in exportScope)
                    _exportScope.Declare(newExport.Key, newExport.Value);
            }

            public override IEnumerable<KeyValuePair<string, Symbol>> Exports
            {
                get
                {
                    return _exportScope.LocalDeclarations;
                }
            }

            public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
            {
                return _localView.GetEnumerator();
            }


            public override bool TryGet(string id, out Symbol value)
            {
                return _localView.TryGet(id, out value);
            }

            public override bool IsEmpty
            {
                get
                {
                    return _localView.IsEmpty;
                }
            }
        }

        private Symbol _filterSymbol(Symbol symbol)
        {
            NamespaceSymbol nsSymbol;
            if (symbol.TryGetNamespaceSymbol(out nsSymbol))
            {
                var ns = nsSymbol.Namespace;
                var localNamespace = ns as LocalNamespaceImpl;
                // Check if we already have a wrap 
                // this happens when an alias to a wrapped namespace is declared 
                // but hasn't been accessed from this namespace until now
                if (localNamespace != null)
                {
                    if (localNamespace.HasSameRootAs(this))
                    {
                        // no need to record this namespace, the check is faster than
                        // the storage in a dictionary
                        return symbol;
                    }
                    else
                    {
                        // this namespace comes from a different scope, we need to wrap it
                        Symbol.Trace.TraceEvent(TraceEventType.Warning, 0,
                            "Found LocalNamespace coming from external scope of {0}. Prefix of that local namespace {1}.",
                            GetType().Name, localNamespace.Prefix);
                    }
                }

                localNamespace = _localProxies.GetOrAdd(ns,
                    externalNs => new LocalNamespaceImpl(externalNs,_localProxies));

                return Symbol.CreateNamespace(localNamespace, nsSymbol.Position);
            }
            else
            {
                return symbol;
            }
        }

        public LocalNamespace CreateLocalNamespace(ISymbolView<Symbol> externalScope)
        {
            return new LocalNamespaceImpl(externalScope, _localProxies);
        }

        public override bool IsEmpty
        {
            get { return _backingStore.IsEmpty; }
        }

        public override void Declare(string id, Symbol symbol)
        {
            _backingStore.Declare(id, symbol);
        }

        public override bool IsDeclaredLocally(string id)
        {
            return _backingStore.IsDeclaredLocally(id);
        }

        public override void ClearLocalDeclarations()
        {
            _backingStore.ClearLocalDeclarations();
        }

        public override IEnumerable<KeyValuePair<string, Symbol>> LocalDeclarations
        {
            get { return _backingStore.LocalDeclarations; }
        }

        public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            foreach (var entry in _backingStore)
            {
                var localSym = _filterSymbol(entry.Value);
                if (!ReferenceEquals(localSym, entry.Value))
                    yield return new KeyValuePair<string, Symbol>(entry.Key, localSym);
                else
                    yield return entry;
            }
        }

        public override bool TryGet(string id, out Symbol value)
        {
            if (_backingStore.TryGet(id, out value))
            {
                value = _filterSymbol(value);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
