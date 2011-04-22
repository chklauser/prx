#if ((!(DEBUG || Verbose)) || forceIndex) && allowIndex
#define useIndex
#endif

#define UseCil //need to change this in VMTestsBase.cs too!

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Types;

namespace Prx.Tests
{
    public abstract partial class VMTests : VMTestsBase
    {
        [Test]
        public void CallSubMacroCommandNested()
        {
            Compile(@"
function main(xs)
{
    var zs = [];
    function f(x) 
    {
        if(x mod 2 == 0)
            continue;
        if(x > 6)
            break;
        return x*3+1;
    }
    foreach(var x in xs)
    {
        zs[] = call\sub(f(?),[x]);
    }

    return zs.ToString();
}
");

            var xs = new List<PValue> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            Expect("[ 4, 10, 16 ]", (PValue)xs);
        }

        [Test]
        public void CallSubMacroCommandTopLevel()
        {
            Compile(@"
var zs = [];
function main(x1,x2,x3)
{
    function f(x) 
    {
        if(x mod 2 == 0)
            continue;
        if(x > 6)
            break;
        return x*3+1;
    }
    
    zs[] = call\sub(f(?),[x1]);
    zs[] = call\sub(f(?),[x2]);
    call\sub(f(?),[x3]);

    return zs.ToString();
}
");
            Func<List<PValue>> getZs = () =>
            {
                var pv = target.Variables["zs"].Value.Value as List<PValue>;
                return pv ?? new List<PValue>(0);
            };
            Action resetZs = () => getZs().Clear();

            Expect("[ 4, 10 ]", 1, 3);
            resetZs();

            ExpectNull(2, 4, 8);
            Assert.AreEqual(0, getZs().Count);
            resetZs();

            ExpectNull(1, 8, 8);
            var zs = getZs();
            Assert.AreEqual(1, zs.Count);
            AssertPValuesAreEqual(4, zs[0]);
            resetZs();

            ExpectNull(1, 3, 8);
            zs = getZs();
            Assert.AreEqual(2, zs.Count);
            AssertPValuesAreEqual(4, zs[0]);
            AssertPValuesAreEqual(10, zs[1]);
            resetZs();

        }

        [Test]
        public void CallSubOfPartial()
        {
            Compile(@"
function main(xs,y)
{
    var zs = [];
    function f(x,y) 
    {
        if(x mod 2 == 0)
            continue;
        if(x > y)
            break;
        return x*3+1;
    }
    foreach(var x in xs)
    {
        zs[] = call\sub(f(?,y),[x]);
    }

    return zs.ToString();
}
");

            var xs = new List<PValue> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            Expect("[ 4, 10, 16 ]", (PValue)xs, 6);
            Expect("[ 4, 10 ]", (PValue)xs, 4);
        }

        [Test]
        public void CaptureUnmentionedMacroVariable()
        {
            Compile(@"
    macro echo() 
    {
        var f = (x) => 
            if(context is null)
                ""context is null""
            else
                context.LocalSymbols[x];
        println(f.(""x""));
    }

    function main()
    {
        var x = 15;
        echo;
        return x;
    }
");
            var clo = target.Functions["echo\\0"];
            Assert.IsNotNull(clo, "Closure must exist.");
            Assert.IsTrue(clo.Meta.ContainsKey(PFunction.SharedNamesKey));
            Assert.AreEqual(clo.Meta[PFunction.SharedNamesKey].List.Length, 1);
            Assert.AreEqual(clo.Meta[PFunction.SharedNamesKey].List[0].Text, MacroAliases.ContextAlias);

            Expect(15, new PValue[0]);
        }

        [Test]
        public void CallMacroOnFunction()
        {
            Compile(@"
macro __append(con)
{
    con.Constant += ""__"";
    return con;
}

macro __surround(con)
{
    con.Constant = ""__"" + con.Constant;
    return call\macro(__append,[con]);
}

function main(x,y)
{
    return x + __surround(""xXx"") + y;
}
");

            Expect("a__xXx__b","a","b");
        }
    }
}
