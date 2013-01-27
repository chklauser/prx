// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using Moq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;

namespace PrexoniteTests.Tests
{
    [TestFixture, Explicit]
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

function main(x)
{
    var f' = f;

    declare function f;
    return g + f' + f(x);
}
");

            Expect(3*2 + 5 + 7, 2);
            Expect(3*11 + 5 + 7, 11);

            {
                Assert.That(ldr.Symbols.Contains("f"), Is.True,
                    "Symbol table must contain an entry for 'f'.");
                var entry = LookupSymbolEntry(ldr.Symbols,"f");
                Assert.That(entry.Interpretation,
                    Is.EqualTo(SymbolInterpretations.GlobalObjectVariable));
                Assert.That(entry.InternalId, Is.EqualTo("f"));
            }

            {
                Assert.That(ldr.Symbols.Contains("g"), Is.True,
                    "Symbol table must contain an entry for 'g'.");
                var entry = LookupSymbolEntry(ldr.Symbols,"g");
                Assert.That(entry.Interpretation, Is.EqualTo(SymbolInterpretations.Function));
                Assert.That(entry.InternalId, Is.EqualTo("g"));
            }

            {
                Assert.That(ldr.Symbols.Contains("p"), Is.True,
                    "Symbol table must contain an entry for 'p'.");
                var entry = LookupSymbolEntry(ldr.Symbols,"p");
                Assert.That(entry.Interpretation, Is.EqualTo(SymbolInterpretations.Function));
                Assert.That(entry.InternalId, Is.EqualTo("f"));
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
    }
}