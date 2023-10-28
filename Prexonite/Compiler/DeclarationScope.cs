#nullable enable

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;

namespace Prexonite.Compiler;

[DebuggerDisplay("declaration scope {ToString()}")]
public class DeclarationScope
{
    public Namespace Namespace => _LocalNamespace;

    internal LocalNamespace _LocalNamespace { get; }

    public QualifiedId PathPrefix { get; }

    /// <summary>
    /// Symbol store for symbols local to the scope (private, not necessarily exported)
    /// </summary>
    public SymbolStore Store { get; }

    internal DeclarationScope(LocalNamespace ns, QualifiedId pathPrefix, SymbolStore store)
    {
        _LocalNamespace = ns ?? throw new ArgumentNullException(nameof(ns));
        PathPrefix = pathPrefix;
        Store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public override string ToString()
    {
        return PathPrefix.ToString();
    }
}