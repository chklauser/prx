// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;
using Prexonite.Modular;

namespace PrexoniteTests.Tests;

[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self)]
[TestFixture]
public class NamespaceInfrastructureTests
{
    [Test]
    public void WrapImported()
    {
        // referenced module
        var refdMod = new ModuleName("refd", new Version());
        var refd = SymbolStore.Create();
        var a = SymbolStore.Create();
        var b = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        a.Declare("b", b);
        var nsa = new MergedNamespace(a);
        refd.Declare("a", Symbol.CreateNamespace(nsa, NoSourcePosition.Instance));

        // referencing module
        var external = SymbolStore.Create(conflictUnionSource: refd.Select(_exportFromModule(refdMod)));
        var mlv = ModuleLevelView.Create(external);
        Assert.IsTrue(mlv.TryGet("a", out var syma), "external symbol is accessible");

        // retrive namespace
        Assert.That(syma, Is.InstanceOf<NamespaceSymbol>(), "symbol a");
        Assert.IsTrue(syma.TryGetNamespaceSymbol(out var nssyma), "looking up a results in a namespace symbol");

        // retrieve referenced symbol
        Assert.That(nssyma.Namespace.TryGet("b", out var symb), Is.True, "external symbol a.b is accessible");
        Assert.That(symb, Is.InstanceOf<ReferenceSymbol>(), "external symbol a.b");
        Assert.That(symb, Is.SameAs(b));

        // check that namespace is wrapped
        Assert.That(nssyma.Namespace,Is.InstanceOf<LocalNamespace>(),"namespace a when looked up locally");
        var localns = (LocalNamespace) nssyma.Namespace;
        var symd = Symbol.CreateReference(EntityRef.Command.Create("e"),NoSourcePosition.Instance);
        localns.DeclareExports(new KeyValuePair<string, Symbol>("d",symd).Singleton());

        Assert.That(nssyma.Namespace.TryGet("d",out var symd2),Is.True,"Symbol a.d looked up locally");
        Assert.That(symd2,Is.EqualTo(symd),"Symbol retrieved locally compared to the symbol declared");
            
        Assert.That(nsa.TryGet("d",out symd2),Is.False,"Existence of symbol a.d looked up from referenced module");
    }

    [Test]
    public void WrapMergedNoConflict()
    {
        // Reference module #1
        var refd1Mod = new ModuleName("refd1", new Version());
        var refd1 = SymbolStore.Create();
        var a1 = SymbolStore.Create();
        var b = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        a1.Declare("b", b);
        var nsa1 = new MergedNamespace(a1);
        refd1.Declare("a", Symbol.CreateNamespace(nsa1, NoSourcePosition.Instance));

        // Referenced module #2
        var refd2Mod = new ModuleName("refd2", new Version());
        var refd2 = SymbolStore.Create();
        var a2 = SymbolStore.Create();
        var f = Symbol.CreateReference(EntityRef.Function.Create("f",refd2Mod), NoSourcePosition.Instance);
        a2.Declare("f", f);
        var nsa2 = new MergedNamespace(a2);
        refd2.Declare("a", Symbol.CreateNamespace(nsa2, NoSourcePosition.Instance));

        // Referencing module
        var external = SymbolStore.Create(conflictUnionSource: refd1.Select(_exportFromModule(refd1Mod)).Append(refd2.Select(_exportFromModule(refd2Mod))));
        var mlv = ModuleLevelView.Create(external);
        Assert.IsTrue(mlv.TryGet("a", out var syma), "external symbol is accessible");

        // retrive namespace
        Assert.That(syma, Is.InstanceOf<NamespaceSymbol>(), "symbol a");
        Assert.IsTrue(syma.TryGetNamespaceSymbol(out var nssyma), "looking up `a` results in a namespace symbol");

        // retrieve referenced symbol b
        Assert.That(nssyma.Namespace.TryGet("b", out var symb), Is.True, "external symbol a.b is accessible");
        Assert.That(symb, Is.InstanceOf<ReferenceSymbol>(), "external symbol a.b");
        Assert.That(symb, Is.SameAs(b));

        // retrieve reference symbol f
        Assert.That(nssyma.Namespace.TryGet("f", out symb), Is.True, "external symbol a.f is accessible");
        Assert.That(symb, Is.InstanceOf<ReferenceSymbol>(), "external symbol a.f");
        Assert.That(symb, Is.SameAs(f));

        // check that namespace is wrapped
        Assert.That(nssyma.Namespace, Is.InstanceOf<LocalNamespace>(), "namespace a when looked up locally");
        var localns = (LocalNamespace)nssyma.Namespace;
        var symd = Symbol.CreateReference(EntityRef.Command.Create("e"), NoSourcePosition.Instance);
        // shadows f, but doesn't modify the external namespace that defines f
        localns.DeclareExports(new KeyValuePair<string, Symbol>("f", symd).Singleton());

        Assert.That(nssyma.Namespace.TryGet("f", out var symd2), Is.True, "Symbol a.f looked up locally");
        Assert.That(symd2, Is.EqualTo(symd), "Symbol retrieved locally compared to the symbol declared");

        // Check original namespaces (should be unmodified)
        Assert.That(nsa1.TryGet("f", out symd2), Is.False, "Existence of symbol a.f looked up from referenced module #1");
        Assert.That(nsa2.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #2");
        Assert.That(symd2,Is.EqualTo(f),"a.f looked up from module #2 (should be unmodified)");
    }

    [Test]
    public void Wrap2MergedConflicted()
    {
        // Reference module #1
        var refd1Mod = new ModuleName("refd1", new Version());
        var refd1 = SymbolStore.Create();
        var a1 = SymbolStore.Create();
        var f1 = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        a1.Declare("f", f1);
        var nsa1 = new MergedNamespace(a1);
        refd1.Declare("a", Symbol.CreateNamespace(nsa1, NoSourcePosition.Instance));

        // Referenced module #2
        var refd2Mod = new ModuleName("refd2", new Version());
        var refd2 = SymbolStore.Create();
        var a2 = SymbolStore.Create();
        var f2 = Symbol.CreateReference(EntityRef.Function.Create("f", refd2Mod), NoSourcePosition.Instance);
        a2.Declare("f", f2);
        var nsa2 = new MergedNamespace(a2);
        refd2.Declare("a", Symbol.CreateNamespace(nsa2, NoSourcePosition.Instance));

        // Referencing module
        var external = SymbolStore.Create(conflictUnionSource: refd1.Select(_exportFromModule(refd1Mod)).Append(refd2.Select(_exportFromModule(refd2Mod))));
        var mlv = ModuleLevelView.Create(external);
        Assert.IsTrue(mlv.TryGet("a", out var syma), "external symbol is accessible");

        // retrive namespace
        Assert.That(syma, Is.InstanceOf<NamespaceSymbol>(), "symbol a");
        Assert.IsTrue(syma.TryGetNamespaceSymbol(out var nssyma), "looking up `a` results in a namespace symbol");

        // retrieve reference symbol f
        Assert.That(nssyma.Namespace.TryGet("f", out var symb), Is.True, "external symbol a.f is accessible");
        Assert.That(symb, Is.InstanceOf<MessageSymbol>(), "external symbol a.f"); // a conflict symbol

        // check that namespace is wrapped
        Assert.That(nssyma.Namespace, Is.InstanceOf<LocalNamespace>(), "namespace a when looked up locally");
        var localns = (LocalNamespace)nssyma.Namespace;
        var symd = Symbol.CreateReference(EntityRef.Command.Create("e"), NoSourcePosition.Instance);
        // shadows f, but doesn't modify the external namespace that defines f
        localns.DeclareExports(new KeyValuePair<string, Symbol>("f", symd).Singleton());

        Assert.That(nssyma.Namespace.TryGet("f", out var symd2), Is.True, "Symbol a.f looked up locally");
        Assert.That(symd2, Is.EqualTo(symd), "Symbol retrieved locally compared to the symbol declared");

        // Check original namespaces (should be unmodified)
        Assert.That(nsa1.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #1");
        Assert.That(symd2, Is.EqualTo(f1), "a.f looked up from module #1 (should be unmodified)");
        Assert.That(nsa2.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #2");
        Assert.That(symd2, Is.EqualTo(f2), "a.f looked up from module #2 (should be unmodified)");
    }

    [Test]
    public void Wrap3MergedConflicted()
    {
        // Reference module #1
        var refd1Mod = new ModuleName("refd1", new Version());
        var refd1 = SymbolStore.Create();
        var a1 = SymbolStore.Create();
        var f1 = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        a1.Declare("f", f1);
        var nsa1 = new MergedNamespace(a1);
        refd1.Declare("a", Symbol.CreateNamespace(nsa1, NoSourcePosition.Instance));

        // Referenced module #2
        var refd2Mod = new ModuleName("refd2", new Version());
        var refd2 = SymbolStore.Create();
        var a2 = SymbolStore.Create();
        var f2 = Symbol.CreateReference(EntityRef.Function.Create("f", refd2Mod), NoSourcePosition.Instance);
        a2.Declare("f", f2);
        var nsa2 = new MergedNamespace(a2);
        refd2.Declare("a", Symbol.CreateNamespace(nsa2, NoSourcePosition.Instance));

        // Referenced module #3
        var refd3Mod = new ModuleName("refd3", new Version());
        var refd3 = SymbolStore.Create();
        var a3 = SymbolStore.Create();
        var f3 = Symbol.CreateReference(EntityRef.MacroCommand.Create("g"), NoSourcePosition.Instance);
        a3.Declare("f",f3);
        var nsa3 = new MergedNamespace(a3);
        refd3.Declare("a",Symbol.CreateNamespace(nsa3,NoSourcePosition.Instance));

        // Referencing module
        var external = SymbolStore.Create(conflictUnionSource: 
            refd1.Select(_exportFromModule(refd1Mod)).Append(
                refd2.Select(_exportFromModule(refd2Mod))).Append(
                refd3.Select(_exportFromModule(refd3Mod))));
        var mlv = ModuleLevelView.Create(external);
        Assert.IsTrue(mlv.TryGet("a", out var syma), "external symbol is accessible");

        // retrive namespace
        Assert.That(syma, Is.InstanceOf<NamespaceSymbol>(), "symbol a");
        Assert.IsTrue(syma.TryGetNamespaceSymbol(out var nssyma), "looking up `a` results in a namespace symbol");

        // retrieve reference symbol f
        Assert.That(nssyma.Namespace.TryGet("f", out var symb), Is.True, "external symbol a.f is accessible");
        Assert.That(symb, Is.InstanceOf<MessageSymbol>(), "external symbol a.f"); // a conflict symbol

        // check that namespace is wrapped
        Assert.That(nssyma.Namespace, Is.InstanceOf<LocalNamespace>(), "namespace a when looked up locally");
        var localns = (LocalNamespace)nssyma.Namespace;
        var symd = Symbol.CreateReference(EntityRef.Command.Create("e"), NoSourcePosition.Instance);
        // shadows f, but doesn't modify the external namespace that defines f
        localns.DeclareExports(new KeyValuePair<string, Symbol>("f", symd).Singleton());

        Assert.That(nssyma.Namespace.TryGet("f", out var symd2), Is.True, "Symbol a.f looked up locally");
        Assert.That(symd2, Is.EqualTo(symd), "Symbol retrieved locally compared to the symbol declared");

        // Check original namespaces (should be unmodified)
        Assert.That(nsa1.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #1");
        Assert.That(symd2, Is.EqualTo(f1), "a.f looked up from module #1 (should be unmodified)");
        Assert.That(nsa2.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #2");
        Assert.That(symd2, Is.EqualTo(f2), "a.f looked up from module #2 (should be unmodified)");
        Assert.That(nsa3.TryGet("f", out symd2), Is.True, "Existence of symbol a.f looked up from referenced module #3");
        Assert.That(symd2, Is.EqualTo(f3), "a.f looked up from module #3 (should be unmodified)");
    }

    /// <summary>
    /// Creates an alias to a namespace, adds a symbol via one alias an verifies that the symbol also appears via the other alias.
    /// </summary>
    [Test]
    public void LocalAlias()
    {
        var globalScope = SymbolStore.Create();
        var mlv = ModuleLevelView.Create(globalScope);

        var nsa = mlv.CreateLocalNamespace(new EmptySymbolView<Symbol>());
        var a = Symbol.CreateNamespace(nsa,NoSourcePosition.Instance);

        globalScope.Declare("a",a);

        Assert.That(mlv.TryGet("a", out var syma),Is.True,"Existence of symbol a viewed through MLV");
        Assert.That(syma,Is.Not.Null,"symbol a viewed through MLV");
        if(syma == null)
            throw new AssertionException("symbol a viewed through MLV");

        // note how we declare on the module scope and then perform the lookup through the module-level-view
        globalScope.Declare("b",syma);
        Assert.That(mlv.TryGet("b",out syma),Is.True,"Existence of symbol v viewed through MLV");
        Assert.That(syma,Is.Not.Null,"Symbol b viewed through MLV");
        if (syma == null)
            throw new AssertionException("Symbol b viewed through MLV");

        // unwrap the namespace, add an export to it
        Assert.That(syma,Is.InstanceOf<NamespaceSymbol>(),"Symbol b viewed through MLV");
        var b = (NamespaceSymbol) syma;
        Assert.That(b.Namespace,Is.InstanceOf<LocalNamespace>(),"Namespace in symbol b viewed through MLV");
        var nsb = (LocalNamespace) b.Namespace;
        var c = Symbol.CreateReference(EntityRef.Command.Create("d"),NoSourcePosition.Instance);
        nsb.DeclareExports(new KeyValuePair<string, Symbol>("c",c).Singleton());

        // perform lookup of c through original alias
        if(!mlv.TryGet("a",out syma))
            Assert.Fail("Cannot find symbol a");
        if(!syma.TryGetNamespaceSymbol(out var nsSymA))
            Assert.Fail("symbol a is not a namespace");
        if(!nsSymA.Namespace.TryGet("c",out var symc))
            Assert.Fail("Cannot find symbol c in namespace a");
        Assert.That(symc,Is.EqualTo(c),"symbol c when retrieved through MLV and other alias");
    }

    [Test]
    public void ImportSingleAliased()
    {
        var globalScope = SymbolStore.Create();
        var mlv = ModuleLevelView.Create(globalScope);

        var nsa = mlv.CreateLocalNamespace(new EmptySymbolView<Symbol>());
        var b = Symbol.CreateReference(EntityRef.Command.Create("c"),NoSourcePosition.Instance);
        var d = Symbol.CreateReference(EntityRef.Command.Create("e"),NoSourcePosition.Instance);
        nsa.DeclareExports(new[]
        {
            new KeyValuePair<string, Symbol>("b",b), 
            new KeyValuePair<string, Symbol>("d",d)
        });
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);
        globalScope.Declare("a", a);

        Assert.That(mlv.TryGet("a", out var syma), Is.True, "Existence of symbol a viewed through MLV");
        Assert.That(syma, Is.Not.Null, "symbol a viewed through MLV");
        if (syma == null)
            throw new AssertionException("symbol a viewed through MLV");
        if(!syma.TryGetNamespaceSymbol(out var nssyma))
            Assert.Fail("symbol a must be a namespace");
            
        var ssb = SymbolStoreBuilder.Create(mlv);
        ssb.Forward(new SymbolOrigin.NamespaceImport(new QualifiedId("a"),NoSourcePosition.Instance),nssyma.Namespace,
            new []
            {
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","f"),
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","g")
            });
        var scope = ssb.ToSymbolStore();

        _assertNotExists(scope, "d");
        _assertNotExists(scope, "b");
        var ib = _assertGetSymbol(scope, "f");
        Assert.That(ib,Is.EqualTo(b),"symbol f retrieved from import scope");
        Assert.That(_assertGetSymbol(scope,"g"),Is.EqualTo(ib),"symbol g retrieved from import scope");
    }

    [Test]
    public void ImportWildcardWithRename()
    {
        var globalScope = SymbolStore.Create();
        var mlv = ModuleLevelView.Create(globalScope);

        var nsa = mlv.CreateLocalNamespace(new EmptySymbolView<Symbol>());
        var b = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        var d = Symbol.CreateReference(EntityRef.Command.Create("e"), NoSourcePosition.Instance);
        nsa.DeclareExports(new[]
        {
            new KeyValuePair<string, Symbol>("b",b), 
            new KeyValuePair<string, Symbol>("d",d)
        });
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);
        globalScope.Declare("a", a);

        Assert.That(mlv.TryGet("a", out var syma), Is.True, "Existence of symbol a viewed through MLV");
        Assert.That(syma, Is.Not.Null, "symbol a viewed through MLV");
        if (syma == null)
            throw new AssertionException("symbol a viewed through MLV");
        if (!syma.TryGetNamespaceSymbol(out var nssyma))
            Assert.Fail("symbol a must be a namespace");

        var ssb = SymbolStoreBuilder.Create(mlv);
        ssb.Forward(new SymbolOrigin.NamespaceImport(new QualifiedId("a"), NoSourcePosition.Instance), nssyma.Namespace,
            new SymbolTransferDirective[]
            {
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","f"),
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","g"),
                SymbolTransferDirective.CreateWildcard(NoSourcePosition.Instance),
            });
        var scope = ssb.ToSymbolStore();

        _assertNotExists(scope, "b","import scope");
        var ib = _assertGetSymbol(scope, "f");
        Assert.That(ib, Is.EqualTo(b), "symbol f retrieved from import scope");
        Assert.That(_assertGetSymbol(scope, "g"), Is.EqualTo(ib), "symbol g retrieved from import scope");
        var id = _assertGetSymbol(scope, "d");
        Assert.That(id, Is.EqualTo(d), "Symbol d retrieved from import scope");
    }

    [Test]
    public void ImportWildcardWithDrop()
    {
        var globalScope = SymbolStore.Create();
        var mlv = ModuleLevelView.Create(globalScope);

        var nsa = mlv.CreateLocalNamespace(new EmptySymbolView<Symbol>());
        var b = Symbol.CreateReference(EntityRef.Command.Create("c"), NoSourcePosition.Instance);
        var d = Symbol.CreateReference(EntityRef.Command.Create("e"), NoSourcePosition.Instance);
        nsa.DeclareExports(new[]
        {
            new KeyValuePair<string, Symbol>("b",b), 
            new KeyValuePair<string, Symbol>("d",d)
        });
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);
        globalScope.Declare("a", a);

        Assert.That(mlv.TryGet("a", out var syma), Is.True, "Existence of symbol a viewed through MLV");
        Assert.That(syma, Is.Not.Null, "symbol a viewed through MLV");
        if (syma == null)
            throw new AssertionException("symbol a viewed through MLV");
        if (!syma.TryGetNamespaceSymbol(out var nssyma))
            Assert.Fail("symbol a must be a namespace");

        var ssb = SymbolStoreBuilder.Create(mlv);
        ssb.Forward(new SymbolOrigin.NamespaceImport(new QualifiedId("a"), NoSourcePosition.Instance), nssyma.Namespace,
            new SymbolTransferDirective[]
            {
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","f"),
                SymbolTransferDirective.CreateRename(NoSourcePosition.Instance, "b","g"),
                SymbolTransferDirective.CreateWildcard(NoSourcePosition.Instance),
                SymbolTransferDirective.CreateDrop(NoSourcePosition.Instance, "d")
            });
        var scope = ssb.ToSymbolStore();

        _assertNotExists(scope,"d","import scope");
        _assertNotExists(scope, "b", "import scope");
        var ib = _assertGetSymbol(scope, "f");
        Assert.That(ib, Is.EqualTo(b), "symbol f retrieved from import scope");
        Assert.That(_assertGetSymbol(scope, "g"), Is.EqualTo(ib), "symbol g retrieved from import scope");
    }

    // ReSharper disable once UnusedParameter.Local
    void _assertNotExists([NotNull] ISymbolView<Symbol> view, [NotNull] string id, string viewDesc = null)
    {
        if(view.TryGet(id, out var dummy))
            Assert.Fail("Unexpected presence of symbol {0} in {1}", id, viewDesc ?? "scope");
    }

    [NotNull]
    // ReSharper disable once UnusedParameter.Local
    Symbol _assertGetSymbol([NotNull] ISymbolView<Symbol> view, [NotNull] string id, string viewDesc = null)
    {
        if(!view.TryGet(id, out var symbol))
            Assert.Fail("Expected {0} in {1}", id, viewDesc ?? "scope");
        return symbol;
    }

    // ReSharper disable once UnusedParameter.Local
    NamespaceSymbol _assertGetNamespaceSymbol([NotNull] ISymbolView<Symbol> view, [NotNull] string id, string viewDesc = null)
    {
        if (!view.TryGet(id, out var symbol))
            Assert.Fail("Expected {0} in {1}", id, viewDesc ?? "scope");
        if(!symbol.TryGetNamespaceSymbol(out var namespaceSymbol))
            Assert.Fail("Expected {0} in {1} to be a namespace symbol. Was {2} instead.", id, viewDesc ?? "scope", symbol);
        return namespaceSymbol;
    }

    static Func<KeyValuePair<string, Symbol>, SymbolInfo> _exportFromModule(ModuleName refdMod)
    {
        return entry =>
            new SymbolInfo(entry.Value,
                new SymbolOrigin.ModuleTopLevel(refdMod, NoSourcePosition.Instance), entry.Key);
    }
}