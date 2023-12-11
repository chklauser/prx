using System.Collections.Concurrent;
using System.Diagnostics;

namespace Prexonite.Compiler.Symbolic.Internal;

class ModuleLevelView : SymbolStore
{
    /// <summary>
    /// The scope that this filter wraps.
    /// </summary>
    internal SymbolStore BackingStore { get; }

    /// <summary>
    /// Maps namespaces to already constructed proxies.
    /// </summary>
    /// <remarks>
    /// This dictionary is shared between all child-<see cref="ModuleLevelView"/>s. 
    /// </remarks>
    readonly ConcurrentDictionary<Namespace, LocalNamespaceImpl> _localProxies;

    ModuleLevelView(SymbolStore backingStore, ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
    {
        BackingStore = backingStore ?? throw new ArgumentNullException(nameof(backingStore));
        _localProxies = localProxies ?? throw new ArgumentNullException(nameof(localProxies));
    }

    public static ModuleLevelView Create(SymbolStore externalScope)
    {
        return new(externalScope, new());
    }

    internal class LocalNamespaceImpl : LocalNamespace
    {
        /// <summary>
        /// Wrapper around the symbols of this namespace coming from external sources.
        /// </summary>
        readonly ModuleLevelView _localView;

        /// <summary>
        /// Holds module-local exports. Uses <see cref="_localView"/> as its external scope.
        /// </summary>
        readonly SymbolStore _exportScope;

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
        string? _prefix;

        internal LocalNamespaceImpl(ISymbolView<Symbol> externalScope, ConcurrentDictionary<Namespace, LocalNamespaceImpl> localProxies)
        {
            _exportScope = Create(externalScope);
            _localView = new(_exportScope, localProxies);
        }

        public override string? Prefix
        {
            get => _prefix;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (_prefix != null)
                    throw new InvalidOperationException( 
                        "The prefix for this namespace is already assigned. " 
                        + $"(Existing prefix: '{_prefix}', new prefix: '{value}')");

                _prefix = value;
            }
        }

        public bool HasSameRootAs(ModuleLevelView view)
        {
            return ReferenceEquals(_localView._localProxies, view._localProxies);
        }

        public override bool TryGetExported(string id, [NotNullWhen(true)] out Symbol? exported)
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
                throw new ArgumentNullException(nameof(exportScope));
                
            foreach (var newExport in exportScope)
                _exportScope.Declare(newExport.Key, newExport.Value);
        }

        public override IEnumerable<KeyValuePair<string, Symbol>> Exports => _exportScope.LocalDeclarations;

        public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
        {
            return _localView.GetEnumerator();
        }


        public override bool TryGet(string id, [NotNullWhen(true)] out Symbol? value)
        {
            return _localView.TryGet(id, out value);
        }

        public override bool IsEmpty => _localView.IsEmpty;
    }

    Symbol _filterSymbol(Symbol symbol)
    {
        if (symbol.TryGetNamespaceSymbol(out var nsSymbol))
        {
            var ns = nsSymbol.Namespace;
            // Check if we already have a wrap.
            // This happens when an alias to a wrapped namespace is declared 
            // but hasn't been accessed from this namespace until now
            if (ns is LocalNamespaceImpl localNamespace)
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
                
            localNamespace = _localProxies.GetOrAdd(ns, (externalNs, proxies) => 
                new(externalNs, proxies), _localProxies);

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

    public override bool IsEmpty => BackingStore.IsEmpty;

    public override void Declare(string id, Symbol symbol)
    {
        BackingStore.Declare(id, symbol);
    }

    public override bool IsDeclaredLocally(string id)
    {
        return BackingStore.IsDeclaredLocally(id);
    }

    public override void ClearLocalDeclarations()
    {
        BackingStore.ClearLocalDeclarations();
    }

    public override ISymbolView<Symbol>? ExternalScope
    {
        get => BackingStore.ExternalScope;
        set => BackingStore.ExternalScope = value;
    }

    public override IEnumerable<KeyValuePair<string, Symbol>> LocalDeclarations => BackingStore.LocalDeclarations;

    public override IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator()
    {
        foreach (var entry in BackingStore)
        {
            var localSym = _filterSymbol(entry.Value);
            if (!ReferenceEquals(localSym, entry.Value))
                yield return new(entry.Key, localSym);
            else
                yield return entry;
        }
    }

    public override bool TryGet(string id, [NotNullWhen(true)] out Symbol? value)
    {
        if (BackingStore.TryGet(id, out value))
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