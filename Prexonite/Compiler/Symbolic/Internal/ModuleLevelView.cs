using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic.Internal
{
    internal class ModuleLevelView : ISymbolView<Symbol>
    {
        /// <summary>
        /// The scope that this filter wraps.
        /// </summary>
        [NotNull]
        private readonly ISymbolView<Symbol> _externalScope;

        /// <summary>
        /// Maps namespaces to already constructed proxies.
        /// </summary>
        /// <remarks>
        /// This dictionary is shared between all child-<see cref="ModuleLevelView"/>s. 
        /// </remarks>
        [NotNull]
        private readonly ConcurrentDictionary<Namespace, LocalNamespaceImpl> _localProxies;

        private ModuleLevelView([NotNull] ISymbolView<Symbol> externalScope, [NotNull] ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
        {
            if (externalScope == null)
                throw new ArgumentNullException("externalScope");
            if (localProxies == null)
                throw new ArgumentNullException("localProxies");

            _externalScope = externalScope;
            _localProxies = localProxies;
        }

        public class LocalNamespaceImpl : LocalNamespace
        {
            /// <summary>
            /// Wrapper around the symbols of this namespace coming from external sources.
            /// </summary>
            [NotNull]
            private readonly ModuleLevelView _externalView;

            /// <summary>
            /// Holds module-local exports. Uses <see cref="_externalView"/> as its external scope.
            /// </summary>
            [CanBeNull]
            private SymbolStore _exportScope;

            /// <summary>
            /// This lock is used to guard access to <see cref="_exportScope"/>. 
            /// </summary>
            [NotNull]
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

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

            public LocalNamespaceImpl([NotNull] ISymbolView<Symbol> externalScope, [NotNull] ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
            {
                _externalView = new ModuleLevelView(externalScope, localProxies);
            }

            public override string Prefix
            {
                get { return _prefix; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException("value");

                    if (_prefix != null)
                        throw new InvalidOperationException("The prefix for this namespace is already assigned.");

                    _prefix = value;
                }
            }

            public bool HasSameRootAs(ModuleLevelView view)
            {
                return ReferenceEquals(_externalView._localProxies, view._localProxies);
            }

            /// <summary>
            /// Adds a set of declarations to the exports of this namespace. Will replace conflicting symbols instead of merging with them.
            /// </summary>
            /// <param name="exportScope">The set of symbols to export.</param>
            public override void DeclareExports(IEnumerable<KeyValuePair<string, Symbol>> exportScope)
            {
                if (exportScope == null)
                    throw new ArgumentNullException("exportScope");
                _lock.EnterWriteLock();
                try
                {
                    if (_exportScope == null)
                        _exportScope = SymbolStore.Create(_externalView);
                    foreach (var newExport in exportScope)
                        _exportScope.Declare(newExport.Key, newExport.Value);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public override IEnumerable<KeyValuePair<string, Symbol>> Exports
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        if (_exportScope != null)
                        {
                            foreach (var localDecl in _exportScope.LocalDeclarations)
                                yield return localDecl;
                        }
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }

            public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
            {
                _lock.EnterReadLock();
                try
                {
                    foreach (var entry in (ISymbolView<Symbol>)_exportScope ?? _externalView)
                    {
                        yield return entry;
                    }
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }


            public override bool TryGet(string id, out Symbol value)
            {
                _lock.EnterReadLock();
                try
                {
                    return ((ISymbolView<Symbol>)_exportScope ?? _externalView).TryGet(id, out value);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            public override bool IsEmpty
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return ((ISymbolView<Symbol>)_exportScope ?? _externalView).IsEmpty;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
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
                    externalNs => new LocalNamespaceImpl(externalNs, _localProxies));

                return Symbol.CreateNamespace(localNamespace, nsSymbol.LogicalName, nsSymbol.Position);
            }
            else
            {
                return symbol;
            }
        }

        public bool IsEmpty
        {
            get { return _externalScope.IsEmpty; }
        }

        public IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            foreach (var entry in _externalScope)
            {
                var localSym = _filterSymbol(entry.Value);
                if (!ReferenceEquals(localSym, entry.Value))
                    yield return new KeyValuePair<string, Symbol>(entry.Key, localSym);
                else
                    yield return entry;
            }
        }

        public bool TryGet(string id, out Symbol value)
        {
            if (_externalScope.TryGet(id, out value))
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
