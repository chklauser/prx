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
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;

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
            r += 1;
        else
            r += 0;
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
    }
}