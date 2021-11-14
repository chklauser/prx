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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

public class Translation : VMTestsBase
{
    [Test]
    public void SimpleSwitchMetaEntry()
    {
        Compile(
            @"
globalSwitch;
is gloS;
is not gloS2;
not glos3;

function main()[loc]
{
    
}

function main2 [loc2; 
loc3; 
not loc4]
{
}

");

        var main = target.Functions["main"];
        var main2 = target.Functions["main2"];

        //Global entries
        Assert.That(target, Meta.ContainsExact("globalSwitch", true));
        Assert.That(target, Meta.ContainsExact("gloS", true));
        Assert.That(target, Meta.ContainsExact("gloS2", false));
        Assert.That(target, Meta.ContainsExact("glos3", false));

        //First function
        Assert.That(main, Is.Not.Null);
        Assert.That(main, Meta.ContainsExact("loc", true));

        //Second function
        Assert.That(main2, Is.Not.Null);
        Assert.That(main2, Meta.ContainsExact("loc2", true));
        Assert.That(main2, Meta.ContainsExact("loc3", true));
        Assert.That(main2, Meta.ContainsExact("loc4", false));
    }

    [Test]
    public void TrailingCommaMetaList()
    {
        Compile(@"
glob {1,2,3,};

function main [loc {1,2,3,}]
{}

");

        var entry = new MetaEntry(new MetaEntry[] {"1", "2", "3"});

        Assert.That(target, Meta.Contains("glob", entry));
        var main = target.Functions["main"];

        Assert.That(main, Is.Not.Null);
        Assert.That(main, Meta.Contains("loc", entry));
    }

    [Test]
    public void TrailingCommaListLiteral()
    {
        Compile(@"
function main = [1,2,3,];
");

        Expect(new List<PValue> {1, 2, 3});
    }

    [Test]
    public void TrailingCommaHashLiteral()
    {
        Compile(
            @"
function main(ks,vs)
{
    var h = {1: ""a"", 2: ""b"", 3: ""c"",};
    var r = """";
    for(var i = 0; i < ks.Count; i++)
        if(h.ContainsKey(ks[i]) and h[ks[i]] == vs[i])
            r += ""1"";
        else
            r += ""0"";
    return r;
}
");

        Expect("110101", (PValue) new List<PValue> {1, 2, 4, 3, 2, 1},
            (PValue) new List<PValue> {"a", "b", "d", "c", "a", "a"});
    }

    [Test]
    public void TrailingArgumentList()
    {
        Compile(
            @"
function f(a,b,) = a + 2*b;
function main(x,y)
{
    return f(x,y,);
}
");

        Expect(2 + 6, 2, 3);
    }

    [Test]
    public void SuppressSymbols()
    {
        var ldr =
            Compile(
                @"
function g = 5;
var f = 7;

var g[\sps] = 3;

function f as p(x) [\sps]
{
    declare var g;
    return g*x;
}

// At this point, we should have 
// g    -> function g
// f    -> variable f
// p    -> function f

function main(x)
{
    var f' = f;

    declare function f;
    return g + f' + f(x);
}
");

        Expect(3*2 + 5 + 7, 2);
        Expect(3*11 + 5 + 7, 11);

        var mn = ldr.ParentApplication.Module.Name;

        {
            Assert.That(ldr.Symbols.Contains("f"), Is.True,
                "Symbol table must contain an entry for 'f'.");
            var entry = LookupSymbolEntry(ldr.Symbols,"f");
            Assert.That(entry,Is.InstanceOf<DereferenceSymbol>());
            var deref = (DereferenceSymbol) entry;
            Assert.That(deref.InnerSymbol,Is.InstanceOf<ReferenceSymbol>());
            var refSym = (ReferenceSymbol) deref.InnerSymbol;
            Assert.That(refSym.Entity,Is.InstanceOf<EntityRef.Variable.Global>());
            refSym.Entity.TryGetGlobalVariable(out var globVar);
            Assert.That(globVar,Is.EqualTo(EntityRef.Variable.Global.Create("f",mn)));
        }

        {
            Assert.That(ldr.Symbols.Contains("g"), Is.True,
                "Symbol table must contain an entry for 'g'.");
            var entry = LookupSymbolEntry(ldr.Symbols, "g");
            Assert.That(entry, Is.InstanceOf<DereferenceSymbol>());
            var deref = (DereferenceSymbol)entry;
            Assert.That(deref.InnerSymbol, Is.InstanceOf<ReferenceSymbol>());
            var refSym = (ReferenceSymbol)deref.InnerSymbol;
            Assert.That(refSym.Entity, Is.InstanceOf<EntityRef.Function>());
            refSym.Entity.TryGetFunction(out var func);
            Assert.That(func, Is.EqualTo(EntityRef.Function.Create("g",mn)));
        }

        {
            Assert.That(ldr.Symbols.Contains("p"), Is.True,
                "Symbol table must contain an entry for 'p'.");
            var entry = LookupSymbolEntry(ldr.Symbols, "p");
            Assert.That(entry, Is.InstanceOf<DereferenceSymbol>());
            var deref = (DereferenceSymbol)entry;
            Assert.That(deref.InnerSymbol, Is.InstanceOf<ReferenceSymbol>());
            var refSym = (ReferenceSymbol)deref.InnerSymbol;
            Assert.That(refSym.Entity, Is.InstanceOf<EntityRef.Function>());
            refSym.Entity.TryGetFunction(out var func);
            Assert.That(func, Is.EqualTo(EntityRef.Function.Create("f",mn)));
        }
    }

    [Test]
    public void AppendRightLocalFunc()
    {
        Compile(@"
function main()
{
    var ys = [];
    coroutine trace(t,xs)
    {
        foreach(var x in xs)
        {
            ys[] = t:x;
            yield x;
        }
    } 
    ([1,2]) >> trace(33) >> all >> println;
    return (var args >> trace(77) >> map(?~String) >> foldl((l,r) => l + "" "" + r, """")) + ys;
}
");
        Expect(" 1 2 3 4 5 6 7[ 33: 1, 33: 2, 77: 1, 77: 2, 77: 3, 77: 4, 77: 5, 77: 6, 77: 7 ]",1,2,3,4,5,6,7);
    }

    [Test]
    public void TestPsrTestRunSingleTest()
    {
        Compile(@"function test\run_single_test as run_single_test(testFunc)
{
    var t = new Structure;
    t.\(""test"") = testFunc;
    try
    {
        testFunc.();
        return true: t;
    }
    catch(var e)
    {
        t.\(""e"") = e;
        return false: t;
    }
}

function main()
{
    var tp = run_single_test(() => 15);
    return ""$(tp.Key):$(tp.Value.test.Id)"";
}");

        Expect("True:main\\0");
    }

    [Test]
    public void TestPsrAst3WithPos()
    {
        Compile(@"
function ast3\withPos(factory,type) [compiler]
{
	var args;
	var targs = args >> skip(2);
	
	if(factory is null)
		throw ""AST factory cannot be null."";
		
	return call\member(factory,type, targs);
}");
        var factory = new Mock<IAstFactory>(MockBehavior.Strict);
        var astPlaceholder = new AstPlaceholder(NoSourcePosition.MissingFileName, NoSourcePosition.Instance.Line, NoSourcePosition.Instance.Column);
        factory.Setup(f => f.Placeholder(It.IsAny<ISourcePosition>(), 5))
            .Returns(astPlaceholder);
        ExpectNamed("ast3\\withPos", astPlaceholder, sctx.CreateNativePValue(factory.Object), "Placeholder", sctx.CreateNativePValue(NoSourcePosition.Instance), 5);
    }

    [Test]
    public void TestSysDeclaresMacroCommand()
    {
        Compile(@"//PRX

Name sys;

declare(
  print = ref command ""print"",
  println = ref command ""println"",
  meta = ref command ""meta"",
  boxed = ref command ""boxed"",
  concat = ref command ""concat"",
  map = ref command ""map"",
  select = ref command ""select"",
  foldl = ref command ""foldl"",
  foldr = ref command ""foldr"",
  dispose = ref command ""dispose"",
  call = expand macro command ""call"",
  call\perform = ref command ""call\\perform"",
  thunk = ref command ""thunk"",
  asthunk = ref command ""asthunk"",
  force = ref command ""force"",
  toseq = ref command ""toseq"",
  call\member = expand macro command ""call\\member"",
  call\member\perform = ref command ""call\\member\\perform"",
  caller = ref command ""caller"",
  pair = ref command ""pair"",
  unbind = ref command ""unbind"",
  sort = ref command ""sort"",
  orderby = ref command ""orderby"",
  LoadAssembly = ref command ""LoadAssembly"",
  debug = ref command ""debug"",
  setcenter = ref command ""setcenter"",
  setleft = ref command ""setleft"",
  setright = ref command ""setright"",
  all = ref command ""all"",
  where = ref command ""where"",
  skip = ref command ""skip"",
  limit = ref command ""limit"",
  take = ref command ""take"",
  abs = ref command ""abs"",
  ceiling = ref command ""ceiling"",
  exp = ref command ""exp"",
  floor = ref command ""floor"",
  log = ref command ""log"",
  max = ref command ""max"",
  min = ref command ""min"",
  pi = ref command ""pi"",
  round = ref command ""round"",
  sin = ref command ""sin"",
  cos = ref command ""cos"",
  sqrt = ref command ""sqrt"",
  tan = ref command ""tan"",
  char = ref command ""char"",
  count = ref command ""count"",
  distinct = ref command ""distinct"",
  union = ref command ""union"",
  unique = ref command ""unique"",
  frequency = ref command ""frequency"",
  groupby = ref command ""groupby"",
  intersect = ref command ""intersect"",
  call\tail = expand macro command ""call\\tail"",
  call\tail\perform = ref command ""call\\tail\\perform"",
  list = ref command ""list"",
  each = ref command ""each"",
  exists = ref command ""exists"",
  forall = ref command ""forall"",
  CompileToCil = ref command ""CompileToCil"",
  takewhile = ref command ""takewhile"",
  except = ref command ""except"",
  range = ref command ""range"",
  reverse = ref command ""reverse"",
  headtail = ref command ""headtail"",
  append = ref command ""append"",
  sum = ref command ""sum"",
  contains = ref command ""contains"",
  chan = ref command ""chan"",
  call\async = expand macro command ""call\\async"",
  call\async\perform = ref command ""call\\async\\perform"",
  async_seq = ref command ""async_seq"",
  call\sub\perform = ref command ""call\\sub\\perform"",
  pa\ind = ref command ""pa\\ind"",
  pa\mem = ref command ""pa\\mem"",
  pa\ctor = ref command ""pa\\ctor"",
  pa\check = ref command ""pa\\check"",
  pa\cast = ref command ""pa\\cast"",
  pa\smem = ref command ""pa\\smem"",
  pa\fun\call = ref command ""pa\\fun\\call"",
  pa\flip\call = ref command ""pa\\flip\\call"",
  pa\call\star = ref command ""pa\\call\\star"",
  then = ref command ""then"",
  id = ref command ""id"",
  const = ref command ""const"",
  (+) = ref command ""plus"",
  (-) = ref command ""minus"",
  (*) = ref command ""times"",
  (/) = ref command ""dividedBy"",
  $mod = ref command ""mod"",
  (^) = ref command ""raisedTo"",
  (&) = ref command ""bitwiseAnd"",
  (|) = ref command ""bitwiseOr"",
  $xor = ref command ""xor"",
  (==) = ref command ""isEqualTo"",
  (!=) = ref command ""isInequalTo"",
  (>) = ref command ""isGreaterThan"",
  (>=) = ref command ""isGreaterThanOrEqual"",
  (<) = ref command ""isLessThan"",
  (<=) = ref command ""isLessThanOrEqual"",
  (-.) = ref command ""negation"",
  $complement = ref command ""complement"",
  $not = ref command ""not"",
  create_enumerator = ref command ""create_enumerator"",
  create_module_name = ref command ""create_module_name"",
  seqconcat = ref command ""seqconcat"",
  call\sub = expand macro command ""call\\sub"",
  call\sub\interpret = expand macro command ""call\\sub\\interpret"",
  macro\pack = expand macro command ""macro\\pack"",
  macro\unpack = expand macro command ""macro\\unpack"",
  macro\reference = expand macro command ""macro\\reference"",
  call\star = expand macro command ""call\\star"",
  call\macro = expand macro command ""call\\macro"",
  call\macro\impl = expand macro command ""call\\macro\\impl"",
  main = ref function(""main"",""testApplication"",0.0),
);

function main(x,y)
{
    return call\member(x,y);
}
");

        var x = new Mock<ISourcePosition>(MockBehavior.Strict);
        x.SetupGet(s => s.Line).Returns(15);
        Expect(15,sctx.CreateNativePValue(x.Object),"Line");
    }

    [Test]
    public void BlockDeclarationOfMacroCommand()
    {
        Compile(@"
declare macro command call\member;

function main(x,y)
{
    return call\member(x,y);
}
");
        var x = new Mock<ISourcePosition>(MockBehavior.Strict);
        x.SetupGet(s => s.Line).Returns(15);
        Expect(15, sctx.CreateNativePValue(x.Object), "Line");
    }

    [Test]
    public void ReferenceToSymbolWithMessage()
    {
        var ldr = Compile(@"
function t1 = 7;
ref t2 = ->t1;
declare(
    t3 = warn(pos(""Translation.cs.pxs"",434,5),""T.tt"",""Hooder"", sym ""t2"")
);

function main()
{
    return ->t1 == ->t3;
}
");

        Expect(true);
        Assert.That(ldr.Warnings.Count,Is.EqualTo(1));
        Assert.That(ldr.Warnings[0].MessageClass,Is.EqualTo("T.tt"));
    }

    [Test]
    public void EntityRefToCommand()
    {
        var ldr = Compile(@"
function f{}
var v;
ref r;
macro m{}

function main()
{   
    var loc;
    ref rloc;
    var sep = ""|"";
    return """" + entityref_to(f) + sep
        + entityref_to(v) + sep
        + entityref_to(->r) + sep
        + entityref_to(m) + sep
        + entityref_to(loc) + sep
        + entityref_to(->rloc) + sep
        + entityref_to(entityref_to) + sep
        + entityref_to(print);
}
");
        var nm = ldr.ParentApplication.Module.Name;

        Expect(rv =>
        {
            var r = rv.CallToString(sctx).Split('|');
            TestContext.WriteLine(rv);
            Assert.That((object) r.Length,Is.EqualTo(8),"Expected return value to consist of 8 elements. Returned {0}",rv);

            Assert.That(r[0], Is.EqualTo(EntityRef.Function.Create("f",nm).ToString()));
            Assert.That(r[1], Is.EqualTo(EntityRef.Variable.Global.Create("v", nm).ToString()));
            Assert.That(r[2], Is.EqualTo(EntityRef.Variable.Global.Create("r", nm).ToString()));
            Assert.That(r[3], Is.EqualTo(EntityRef.Function.Create("m", nm).ToString()));
            Assert.That(r[4], Is.EqualTo(EntityRef.Variable.Local.Create("loc").ToString()));
            Assert.That(r[5], Is.EqualTo(EntityRef.Variable.Local.Create("rloc").ToString()));
            Assert.That(r[6], Is.EqualTo(EntityRef.MacroCommand.Create("entityref_to").ToString()));
            Assert.That(r[7], Is.EqualTo(EntityRef.Command.Create("print").ToString()));
        });
    }

    [Test]
    public void NamespaceLookup()
    {
        var ldr = new Loader(options);
        Compile(ldr, @"
function f = 17;
");
        if(!ldr.Symbols.TryGet("f",out var f))
            Assert.Fail("Expected module level symbol f to exist.");

        var scopea = SymbolStore.Create();
        scopea.Declare("g",f);
        var nsa = new MergedNamespace(scopea);
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);
        ldr.Symbols.Declare("a",a);

        Compile(ldr, @"
function main()
{
    return a.g;
}
");

        Expect(17);
    }

    [Test]
    public void NestedNamespaceLookup()
    {

        var ldr = new Loader(options);
        Compile(ldr, @"
function f = 17;
");
        if (!ldr.Symbols.TryGet("f", out var f))
            Assert.Fail("Expected module level symbol f to exist.");

        var scopea = SymbolStore.Create();
        scopea.Declare("g", f);
        var nsa = new MergedNamespace(scopea);
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);

        var scopeb = SymbolStore.Create();
        scopeb.Declare("a",a);
        var nsb = new MergedNamespace(scopeb);
        var b = Symbol.CreateNamespace(nsb, NoSourcePosition.Instance);

        ldr.Symbols.Declare("b",b);

        Compile(ldr, @"
function main()
{
    return b.a.g;
}
");

        Expect(17);
    }

    [Test]
    public void AliasedNamespaceLookup()
    {

        var ldr = new Loader(options);
        Compile(ldr, @"
function f = 17;
");
        if (!ldr.Symbols.TryGet("f", out var f))
            Assert.Fail("Expected module level symbol f to exist.");

        var scopea = SymbolStore.Create();
        scopea.Declare("g", f);
        var nsa = new MergedNamespace(scopea);
        var a = Symbol.CreateNamespace(nsa, NoSourcePosition.Instance);

        var scopeb = SymbolStore.Create();
        scopeb.Declare("a", a);
        var nsb = new MergedNamespace(scopeb);
        var b = Symbol.CreateNamespace(nsb, NoSourcePosition.Instance);

        ldr.Symbols.Declare("b", b);

        Compile(ldr, @"
declare(z = sym(""b"",""a""));
function main()
{
    return z.g;
}
");

        Expect(17);
    }

    [Test]
    public void NamespaceDeclaration()
    {
        var ldr = Compile(@"
namespace a 
{
    function f = 17;
}

function main = a.f;
");
        Expect(17);
        Assert.That(ldr.TopLevelSymbols.TryGet("f",out var dummy),Is.False,"Existence of symbol f in the global scope");
    }

    [Test]
    public void SugaredNestedNamespaceDeclaration()
    {

        Compile(@"
namespace a.b 
{
    function f = 17;
}

function main = a.b.f;
");
        Expect(17);
    }

    [Test]
    public void NestedNamespaceDeclaration()
    {

        Compile(@"
namespace a 
{
    namespace b 
    {
        function f = 17;
    }
}

function main = a.b.f;
");
        Expect(17);
    }

    [Test]
    public void TopLevelAccessFromNamespace()
    {

        Compile(@"
function f = 17;

namespace a 
{
    function g = f;
}

function main = a.g;
");
        Expect(17);
    }

    [Test]
    public void SurroundingAccessFromNamespace()
    {

        Compile(@"
namespace a 
{
    function f = 17;
    namespace b
    {
        function g = f;
    }
}

function main = a.b.g;
");
        Expect(17);
    }

    [Test]
    public void Surrounding2AccessFromNamespace()
    {

        Compile(@"
namespace a 
{
    function f = 17;
    namespace b
    {
        namespace c
        {
            function g = f;
        }
    }
}

function main = a.b.c.g;
");
        Expect(17);
    }

    [Test]
    public void SugarComposeNamespaces()
    {
        Compile(@"
namespace a.b
{
    function f = 17;
}

namespace a.c
{
    function g = 3;
}

function main = a.b.f + a.c.g;
");
        Expect(20);
    }

    [Test]
    public void SugarNsOverride()
    {
        CompileInvalid(@"
namespace a
{
    function b = 13;
}

namespace a.b
{
    function f = 17;
}

function main = a.b.f;
","Expected","namespace","func");
    }

    [Test]
    public void DontReExportSurrounding()
    {
        CompileInvalid(@"
function f = 13;
namespace a 
{
    function g = 15;
}

function main = a.f;
","symbol","resolve","f");

    }

    [Test]
    public void RestoreExportedOnExtend()
    {
        Compile(@"
namespace a {
    function f = 13;
}

namespace a {
    function g = f+2;
}

function main = a.g;
");

        Expect(13+2);
    }

    [Test]
    public void SuppressRestoreExportedOnExtend()
    {
        CompileInvalid(@"
namespace a {
    function zz_f = 13;
}

namespace a 
    import()
{
    function g = zz_f+2;
}

function main = a.g;
","symbol","resolve","zz_f");
    }

    [Test]
    public void SimpleNsSugarExtend()
    {
        Compile(@"
namespace a.c
{
    function f = 17;
}

namespace a
{
    namespace c
    {
        function g = 2;
    }
}

function main = a.c.f + a.c.g;
");
        Expect(17+2);
    }

    [Test]
    public void SugarNsExtend()
    {
        Compile(@"
namespace a
{
    function b = 13;
}

namespace a.c
{
    function f = 17;
}

namespace a
{
    function d = 10;
}

namespace a
{
    namespace c
    {
        function g = 2;
    }
}

function main = a.b + a.c.f + a.d + a.c.g;
");

        Expect(13 + 17 + 10 + 2);
    }

    [Test]
    public void NsPhysical()
    {
        SkipStore = true;

        Compile(@"
namespace a 
{
    function f = 3;
}

namespace b
{
    function f = 14;
}

function main = a.f + b.f;
");

        Expect(17);
    }

    [Test]
    public void ImportBackgroundConflict()
    {
        Compile(@"
namespace a {
    function f = 13;
    function g = 12;
    function x = 3;
}

namespace b {
    var g = 19;
    declare(f = absolute sym(""a"",""f""));
    function y = 4;
}

namespace c
    import a.*, b.*
{
    function s = f + x + y;
}

function main = c.s;
");

        Expect(4+3+13);
    }

    [Test]
    public void ImportConflict()
    {
        CompileInvalid(@"
namespace a 
{
    function f = 13;
}

namespace b
{
    var f = 14;
}

namespace c
    import a.*, b.*
{
    function s = f;
}

function main = c.s;
","incompatible","namespace a","namespace b","symbol f");
    }

    [Test]
    public void RenameOneAvoidsConflict()
    {
        Compile(@"
namespace a 
{
    function f = 13;
}

namespace b
{
    var f = 14;
}

namespace c
    import a.*, b(f => z)
{
    function s = f + z;
}

function main = c.s;
");
        Expect(13+14);
    }

    [Test]
    public void UseMultiplicationAsNamespaceName()
    {
        Compile(@"
namespace a.(*).c {
    function f = 3;
}

namespace b 
    import a.(*).c.f
{
    function g = f;
}

function main = b.g;
");
        Expect(3);
    }

    [Test]
    public void UseMultiplicationInExplicitTransfer()
    {
        Compile(@"
namespace a {
    function (*) = 3;
    var gobb = 4;
}

namespace z {
    function gobb = 5;
}

namespace b 
    import a((*)), z(*) // should import only (*) from a, everything from z
{
    function g = (*) + gobb;
}

function main = b.g;
");
    }

    [Test]
    public void ExplicitWildcardTransfer()
    {
        Compile(@"
namespace a { function f = 13; }
namespace b import a(*)
{ function g = f; }
function main = b.g;
");

        Expect(13);
    }

    [Test]
    public void DropAvoidsConflict()
    {
        Compile(@"
namespace a 
{
    function f = 13;
}

namespace b
{
    var f = 14;
    function g = 12;
}

namespace c
    import 
        a.*, 
        b(*, not f)
{
    function s = f + g;
}

function main = c.s;
");
        Expect(13 + 12);
    }

    [Test]
    public void FunctionScopeImport()
    {
        Compile(@"
namespace a
{
    function f = 13;
}

function main namespace import a.f = f;
");
        Expect(13);
    }

    [Test]
    public void ForwardDeclarationAncientSyntax()
    {
        Compile(@"
namespace a
{
    declare function f;
    function g(x) = f(x);
    function f(x) = 2*x;
}

function main(x) = a.g(x);
");

        Expect(16,8);
    }

    [Test]
    public void ForwardDeclarationOldSyntax()
    {
        Compile(@"
namespace a
{
    declare { function: f };
    function g(x) = f(x);
    function f(x) = 2*x;
}

function main(x) = a.g(x);
");

        Expect(16,8);
    }

    [Test]
    public void ForwardDeclarationMachineSyntax()
    {
        Compile(@"
namespace a
{
    // This forward declaration is not taken as relative to the namesapce.
    // It is intended for machine consumption.
    declare( f = ref function(""f"",""testApplication"",0.0));
    function g(x) = f(x);
}

function f(x) = 2*x;

function main(x) = a.g(x);
");

        Expect(16, 8);
    }

    [Test]
    public void ExportInterferenceWithTimesLiteral()
    {
        Compile(@"
namespace a{var g;}
namespace b{}export(*),a(g);
");
    }

    [Test]
    public void AlternateExportAllSyntax()
    {
        Compile(@"
namespace a{var g;}
namespace b{}export.*,a(g);
");
    }

    [Test]
    public void SelfReferenceInVarInit()
    {
        Compile(@"
namespace a 
{
    var h = 4;
}
namespace a{
    var g = g ?? 15;
    var h = h ?? 3;
}

function main = a.g+a.h;
");
        Expect(15+4);
    }

    [Test]
    public void TopLevelNamespaceImport()
    {
        Compile(@"

namespace a 
{
  function f(x) = 2*x;
}

namespace import a.*;

function main(y) = f(y + 2);

");
            
        Expect(20, 8);
    }

    [Test]
    public void TopLevelNamespaceImportMultiple()
    {
        Compile(@"

namespace a 
{
  function f(x) = 2*x;
}
namespace b 
{
  namespace c
  {
    function g(x) = 7 + x;
  }
}

namespace import a.*, b.c.g;

function main(y) = f(g(y));

");
            
        Expect(20, 3);
    }

    [Test]
    public void TopLevelImportReplacement()
    {
        Compile(@"

namespace a 
{
  function f(x) = 2*x;
}

namespace b
{
  function f(x) = x/2; 
}

namespace import a.*;
// This set of imports should completely replace the other set of imports
namespace import b.*;

function main(y) = f(y + 2);

");
            
        Expect(5, 8);
    }

    [Test]
    public void TopLevelImportsDoNotShadow()
    {
        Compile(@"

function f(x) = x*2;

namespace a
{
  function f(x) = x/2; 
}

namespace import a.*;

function main(y) = f(y + 2);

");
            
        Expect(20, 8);
    }

    [Test]
    public void TopLevelImportIllegalInNamespace()
    {
        CompileInvalid(@"
namespace a 
{
    function f(x) = 2*x;
}

namespace b 
{
    namespace import a.f;
    function g(x) = f(x + 2);
}

function main(x) = b.g(3 * x);
", "namespace", "import", "inside");
    }

    [Test]
    public void TopLevelImportMustResolveSymbol()
    {
        CompileInvalid(@"
namespace import this_symbol_does_not_exist.*;
", "this_symbol_does_not_exist");
    }

    [Test]
    public void NamespaceExtensionCrossModule()
    {
        var plan = Plan.CreateSelfAssembling(StandardLibraryPreference.None);

        plan.Assemble(Source.FromString(@"
name one;
namespace a {
    function b = 15;
}
"));

        var moduleTwoDesc = plan.Assemble(Source.FromString(@"
name two;
references {one};
namespace a {
    function c =7;
}
"));

        var moduleTwo = plan.Build(moduleTwoDesc.Name);
        Assert.That(moduleTwo.Messages.Where(m => m.Severity == MessageSeverity.Error),Is.Empty,"Modules should compile without any error messages.");
        foreach (var message in moduleTwo.Messages)
            TestContext.WriteLine(message);
        Assert.That(moduleTwo.Exception,Is.Null);

        var symbols = moduleTwo.Symbols;
        Assert.That(symbols.TryGet("a",out var symbol),Is.True,"Expect module two to have a symbol called 'a'");
        Assert.That(symbol,Is.InstanceOf<NamespaceSymbol>());
        var nsSym = (NamespaceSymbol) symbol;
        _assumeNotNull(nsSym);
        Assert.That(nsSym.Namespace.TryGet("b",out symbol),"Expect namespace a to contain a symbol b.");
        Assert.That(nsSym.Namespace.TryGet("c",out symbol),"Expect namespace a to contain a symbol c.");

    }

    [Test]
    public void CommentAtEnd()
    {
        var ldr = new Loader(options);
        var add = new InternalLoadCommand(ldr);
        ldr.ParentEngine.Commands.AddHostCommand("add_internal", add);
        add.VirtualFiles.Add("f1",@"
function f1() {
    println(""f1 called"");
    // something
    return 15;
}

//s");
        Compile(ldr, @"
declare command add_internal;

function f0() {
    println(""f0 called"");
    /* something else */
    return 16;
}

build does add_internal(""f1"");

function main() {
    return f0 + f1;
}
");

        Expect(15+16);
    }

    [Test]
    public void AssignMetaInBuildBlock()
    {
        var ldr = Compile(@"
namespace ns {
  function fox(){}
}
build does asm(ldr.app).Meta[""psr.test.run_test""] = new Prexonite::MetaEntry(entityref_to(ns.fox).Id);");

        var actualFuncId = ldr.ParentApplication.Meta["psr.test.run_test"].Text;
        Assert.That(actualFuncId, Is.Not.Empty);
        var pointedToFunction = ldr.ParentApplication.Functions[actualFuncId];
        Assert.That(pointedToFunction, Is.Not.Null);
        Assert.That(pointedToFunction.LogicalId, Does.EndWith("fox"));
    }
        
    [Test]
    public void DotSeparatedMetaValues()
    {
        var ldr = Compile(@"
name some.module.test;
references {
  some.module
};

function main[key1 dot.separated.value; key2 value2;key3{a.b,c}] = true;
");

        var meta = target.Meta;
        Assert.That(meta, Does.ContainKey("name"));
        Assert.That(meta, Does.ContainKey("references"));
        Assert.That(meta["name"], Is.EqualTo(new MetaEntry("some.module.test")));
        Assert.That(meta["references"], Is.EqualTo(new MetaEntry(new[]{new MetaEntry("some.module")})));

        var f = target.Functions["main"];
        Assert.That(f.Meta, Does.ContainKey("key1"));
        Assert.That(f.Meta, Does.ContainKey("key2"));
        Assert.That(f.Meta, Does.ContainKey("key3"));
        Assert.That(f.Meta["key1"], Is.EqualTo(new MetaEntry("dot.separated.value")));
        Assert.That(f.Meta["key2"], Is.EqualTo(new MetaEntry("value2")));
        Assert.That(f.Meta["key3"], Is.EqualTo(new MetaEntry(new[]{new MetaEntry("a.b"), new MetaEntry("c")})));
    }

    [Test]
    public void DotNetStaticPropertyAccess()
    {
        Compile(@"
add Prexonite::Compiler to Imports;
function main {
    var x = ::SymbolInterpretations.Function;
    return x;
}
");
        Expect(SymbolInterpretations.Function);
    }

    [Test]
    public void ErrorSingleColonInMetaValue()
    {
        CompileInvalid(@"
// There is a typo in this declaration: single `:` instead of `::`.
name psr::pattern:test/2.0;
");
    }

    [Test]
    public void ErrorSingleColonInMetaValue2()
    {
        CompileInvalid(@"
// There is a typo in this declaration: single `:` instead of `.` (this can happen on German keyboards)
name psr.pattern:test/2.0;
");
    }

    [Test]
    public void ErrorDoubleColonInNamespaceDecl()
    {
        var ldr = CompileInvalid(@"
namespace a::b {
  function should_be_illegal = null;
}
");
            
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.UnexpectedDoubleColonInNamespaceName), Is.Not.Empty);
        Assert.That(ldr.ErrorCount, Is.EqualTo(1), "Error count");
    }

    [Test]
    public void ErrorDoubleColonInNamespaceImport()
    {
        var ldr = CompileInvalid(@"
namespace a.b 
{
    function f = null;
}
namespace a.c 
    import a::b::f 
{
    function should_be_illegal = null;
}
");
            
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.UnexpectedDoubleColonInNamespaceName), Is.Not.Empty);
        Assert.That(ldr.ErrorCount, Is.EqualTo(1), "Error count");
    }

    [Test]
    public void WarningUnqualifiedNamespace()
    {
        var ldr = new Loader(options);
        Compile(ldr, @"
namespace import unknown;
");
        Assert.That(ldr.Warnings.Where(m => m.MessageClass == MessageClasses.UnqualifiedImport), Is.Not.Empty);
        Assert.That(ldr.Warnings.Count, Is.EqualTo(1), "Warning count");
    }

    [Test]
    public void ErrorUnresolvedNamespace()
    {
        var ldr = CompileInvalid(@"
namespace a {}
namespace import a.b.c;
");
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.NamespaceExcepted), Is.Not.Empty);
        Assert.That(ldr.ErrorCount, Is.EqualTo(1));
    }

    [Test]
    public void ErrorDoubleColonInGlobalNamespaceImport()
    {
        var ldr = CompileInvalid(@"
namespace a.b 
{
    function f = null;
}
namespace import a::b::f;
function should_be_illegal = null;
");
            
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.UnexpectedDoubleColonInNamespaceName), Is.Not.Empty);
        Assert.That(ldr.ErrorCount, Is.EqualTo(1), "Error count");
    }

    [Test]
    public void ErrorDoubleColonInNamespaceExport()
    {
        var ldr = CompileInvalid(@"
namespace b.c.d {
    function f = null;
}
namespace a
{
    
} export b::c;
");
            
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.UnexpectedDoubleColonInNamespaceName), Is.Not.Empty);
        Assert.That(ldr.ErrorCount, Is.EqualTo(1), "Error count");
    }

    [Test]
    public void CustomTypeFunctionsInNamespaces()
    {
        Compile(@"
namespace a.b.c {
    function create_pot(x) = ""pot($x)"";
    function static_call_pot(m, arg) = ""pot.$m($arg)"";
    function to_pot(x) = ""$x~pot"";
    function is_pot(x) = x is String and x.Contains(""pot"");
}

function main(x,y) {
    var p1 = new a.b.c.pot(x);
    var p2 = y~a.b.c.pot;
    var z = ~a.b.c.pot.heat(x); 
    return [p1, p2, z, p1 is a.b.c.pot, 5 is not a.b.c.pot, ""fire"" is not a.b.c.pot];
}
");
        Expect(new List<PValue>
        {
            "pot(X)",
            "Y~pot",
            "pot.heat(X)",
            true,
            true,
            true
        }, "X", "Y");
    }

    [Test]
    public void CustomTypeFunctionsWithDotSuffix()
    {
        Compile(@"
namespace a.b.c {
    function create_pot() = ""pot()"";
    function static_call_pot(m) = ""pot.$m"";
    function to_pot(x) = ""$x~pot"";
    function is_pot(x) = x is String and x.Contains(""pot"");
}
function main(y) {
    var p1 = new a.b.c.pot.ToString;
    var p2 = y~a.b.c.pot.ToString;
    var z = ~a.b.c.pot.heat.ToString; 
    return [p1, p2, z, p1 is a.b.c.pot.ToString, 5 is not a.b.c.pot.ToString, ""fire"" is not a.b.c.pot.ToString];
}
");
        Expect(new List<PValue>
        {
            "pot()",
            "Y~pot",
            "pot.heat",
            "True",
            "True",
            "True"
                
        },"Y");
    }

    [Test]
    public void CustomTypeFunctionsInNamespacesWithTypeArgs()
    {
        Compile(@"
namespace a.b.c {
    function create_pot(nT, T1, T2, x)[pxs\supportsTypeArguments] = ""pot`$nT<$T1,$T2>($x)"";
    function static_call_pot(nT, T1, T2, m, arg)[pxs\supportsTypeArguments] = ""pot`$nT<$T1,$T2>.$m($arg)"";
    function to_pot(nT, T1, T2, x)[pxs\supportsTypeArguments] = ""$x~pot`$nT<$T1,$T2>"";
    function is_pot(nT, T1, T2, x)[pxs\supportsTypeArguments] = x is String 
        and x.Contains(""pot"") 
        and x.Contains(T1) 
        and x.Contains(T2);
}

function main(x,y) {
    var p1 = new a.b.c.pot<""a"", ""b"">(x);
    var p2 = y~a.b.c.pot<""c"", ""d"">;
    var z = ~a.b.c.pot<""e"", ""f"">.heat(x); 
    return [p1, p2, z,
        // should match 
        p1 is a.b.c.pot<""a"", ""b"">,
        // should not match => eval to true 
        p2 is not a.b.c.pot<""c"", ""zz"">,
        // should not match => eval to true 
        ""fire"" is not a.b.c.pot
    ];
}
");
        Expect(new List<PValue>
        {
            "pot`2<a,b>(X)",
            "Y~pot`2<c,d>",
            "pot`2<e,f>.heat(X)",
            true,
            true,
            true
        }, "X", "Y");
    }

    [Test]
    public void QuestionMarkSpliceIsInvalid()
    {
        var ldr = CompileInvalid(@"
function main = println(?*);
");
        Assert.That(ldr.Errors.Where(m => m.MessageClass == MessageClasses.IncompleteBinaryOperation), Is.Not.Empty);
    }

    [ContractAnnotation("value:null=>halt")]
    private static void _assumeNotNull(object value)
    {
        Assert.That(value,Is.Not.Null);
    }

    /// <summary>
    /// A command that works in a fashion very similar to the add and requires commands
    /// that a loader exposes in build blocks.
    /// Needs to be initialized first.
    /// </summary>
    private class InternalLoadCommand : PCommand
    {
        [NotNull]
        private readonly Loader _loaderReference;

        public InternalLoadCommand([NotNull] Loader loaderReference)
        {
            _loaderReference = loaderReference;
        }

        [NotNull]
        public Dictionary<string, string> VirtualFiles { get; } = new();

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            var n = args[0].CallToString(sctx);
            var virtualFile = VirtualFiles[n];
            using var cr = new StringReader(virtualFile);
            _loaderReference.LoadFromReader(cr,n);
            return PType.Null;
        }
    }
}