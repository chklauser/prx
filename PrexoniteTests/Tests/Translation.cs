using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prx.Tests;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class Translation : VMTestsBase
    {

        [Test]
        public void SimpleSwitchMetaEntry()
        {
            Compile(@"
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
            Assert.That(target, Meta.ContainsExact("glos3",false));

            //First function
            Assert.That(main,Is.Not.Null);
            Assert.That(main, Meta.ContainsExact("loc",true));

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
            Assert.That(main, Meta.Contains("loc",entry));
        }

        [Test]
        public void TrailingCommaListLiteral()
        {
            Compile(@"
function main = [1,2,3,];
");

            Expect(new List<PValue>{1,2,3});
        }

        [Test]
        public void TrailingCommaHashLiteral()
        {
            Compile(@"
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

            Expect("110101", (PValue) new List<PValue>{1,2,4,3,2,1}, (PValue) new List<PValue>{"a", "b", "d", "c", "a", "a"});
        }

        [Test]
        public void TrailingArgumentList()
        {
            Compile(@"
function f(a,b,) = a + 2*b;
function main(x,y)
{
    return f(x,y,);
}
");

            Expect(2 + 6, 2,3);
        }

        [Test]
        public void SuppressSymbols()
        {
            var ldr = Compile(@"
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
            Expect(3 * 11 + 5 + 7, 11);

            {
                Assert.That(ldr.Symbols.ContainsKey("f"),Is.True,"Symbol table must contain an entry for 'f'.");
                var entry = ldr.Symbols["f"];
                Assert.That(entry.Interpretation,Is.EqualTo(SymbolInterpretations.GlobalObjectVariable));
                Assert.That(entry.Id, Is.EqualTo("f"));
            }

            {
                Assert.That(ldr.Symbols.ContainsKey("g"), Is.True, "Symbol table must contain an entry for 'g'.");
                var entry = ldr.Symbols["g"];
                Assert.That(entry.Interpretation, Is.EqualTo(SymbolInterpretations.Function));
                Assert.That(entry.Id, Is.EqualTo("g"));
            }

            {
                Assert.That(ldr.Symbols.ContainsKey("p"), Is.True, "Symbol table must contain an entry for 'p'.");
                var entry = ldr.Symbols["p"];
                Assert.That(entry.Interpretation, Is.EqualTo(SymbolInterpretations.Function));
                Assert.That(entry.Id, Is.EqualTo("f"));
            }
        }

    }
}
