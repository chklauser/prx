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
        public void MapCommandImplementation()
        {
            Compile(
                @"
function my_map(ref f, var lst)
{
    var nlst = [];
    foreach(var x in lst)
        nlst[] = f(x);
    return nlst;
}

function foldl(ref f, var left, lst)
{
    foreach(var right in lst)
        left = f(left,right);
    return left;
}

function main()
{
    var args;
    var k = 1;
    var ver1 =    map( x => x + 1,   args);
    var ver2 = my_map( x => x + k++, args);

    ref y = coroutine() =>
    {
        foreach(var yy in ver1)
            yield yy;
    };

    var diff = map( x => x - y, ver2);
    return foldl( (l,r) => l + r, """",diff);
}
");

            Expect("01234", 1, 2, 3, 4, 5);
        }

        [Test]
        public void FoldLCommandImplementation()
        {
            Compile(
                @"
function my_foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left, right);
    return left;
}

function main()
{
    var sum = my_foldl( (l,r) => l + r, 0, var args) + 13;
    return foldl( (l,r) => l - r, sum, var args);
}
");

            Expect(13, 4, 5, 6, 7);
        }

        [Test]
        public void CallCommandImplementation()
        {
            Compile(
                @"
function sum()
{
    var s = 0;
    foreach(var x in var args)
        s += x~Int;
    return s;
}

function main()
{
    return call(->call, [->sum], var args);
}
");
            const int a = 3;
            const int b = 7;
            const int c = 9;
            const int d = 13;
            const int e = 14;
            const int f = 99;
            const int g = 101;

            Expect(
                a + b + c + d + e + f + g,
                PType.List.CreatePValue(new PValue[] { a, b, c }),
                PType.List.CreatePValue(new PValue[] { d, e }),
                PType.List.CreatePValue(new PValue[] { f }),
                PType.List.CreatePValue(new PValue[] { g }));
        }

        [Test]
        public void UnbindCommandImplementation()
        {
            Compile(
                @"
function main()
{
    var buffer = new System::Text::StringBuilder;
    function print(s) does buffer.Append(s);
    var xs = [ 5,7,9,11,13,15 ];
    var fs = [];
    foreach(var x in xs)
    {
        fs[] = y => ""($(x)->$(y))"";
        unbind(->x);
        print(""$(x)."");
    }

    var i = 19;
    foreach(var f in fs)
        print(f.(i--));
    return buffer.ToString;
}
");

            const string expected = "5.7.9.11.13.15.(5->19)(7->18)(9->17)(11->16)(13->15)(15->14)";
            Expect(expected);
        }

        [Test]
        public void ListSort()
        {
            Compile(
                @"
function main() = 
    [ ""f"", ""A"", ""x"", ""a"", ""h"", ""g"", ""H"", ""A"", ""f"", ""X"", ""F"", ""G"" ] >>
    sort
    ( 
        (a,b) =>
            a.ToLower.CompareTo(b.ToLower),
        (a,b) =>
            -(a.CompareTo(b))
    ) >>
    foldl( (l,r) => l + "","" + r, """");
");

            Expect(",A,A,a,F,f,f,G,g,H,h,X,x");
        }

        [Test]
        public void MathPiWorksInCil()
        {
            Compile(@"
function main = pi;
");

            Expect(Math.PI);
        }

        [Test]
        public void ReverseCmd()
        {
            Compile(@"function main = [1,2,3,4,5] >> reverse >> foldl((a,b) => a + b,"""");");

            Expect("54321", new PValue[0]);
        }

        [Test]
        public void AsyncSeqSemantics()
        {
            Compile(@"
function main(xs)
{
    return foldl((a,b) => a + "">"" + b, """") << async_seq(xs);
}
");

            Expect(">1>2>3", (PValue)new List<PValue> { 1, 2, 3 });
        }

        #region Call\*

        [Test]
        public void CallStarSimple()
        {
            Compile(@"
function main(x, xs)
{
    var f = call\star(call([?],?),?,[?]);
    println(var x = f.(xs,x));
    return x;
}
");

            Expect(new List<PValue> {1, 2, 3}, 3, (PValue) new List<PValue> {1, 2});
        }

        [Test]
        public void CallStarAllArgumentsUndirected()
        {
            Compile(@"
function main(x, xs)
{
    var f = call\star(call([?],?),?,?);
    println(var x = f.(xs,x));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, (PValue)new List<PValue>{3}, (PValue)new List<PValue> { 1, 2 });
        }

        [Test]
        public void CallStarAllArgumentsDirected()
        {
            Compile(@"
function main(x, y, z)
{
    var f = call\star(call([?],?),[?,?],[?]);
    println(var x = f.(x,y,z));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, 1, 2, 3 );
        }

        [Test]
        public void CallStarCustom()
        {
            Compile(@"
function main(x, xs)
{
    var f = call\star(2,call(?),[?],?,[?]);
    println(var x = f.(xs,x));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, 3, (PValue)new List<PValue> { 1, 2 });
        }

        [Test]
        public void CallStarAllArgumentsCall()
        {
            Compile(
                @"
function main(x, xs1,xs2)
{
    println(var x = call\star(call([?],?),[xs1,xs2], x));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, (PValue)new List<PValue> { 3 }, 1, 2);
        }

        [Test]
        public void CallStarMapped()
        {
            Compile(@"
function main(x, xs)
{
    var f = call\star(call([?],?),?1,[?0]);
    println(var x = f.(x, xs));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, 3, (PValue)new List<PValue> { 1, 2 });
        }



        [Test]
        public void CallStarCustomMapped()
        {
            Compile(@"
function main(x, xs)
{
    var f = call\star(2, call(?), [?], ?1, [?0]);
    println(var x = f.(x, xs));
    return x;
}
");

            Expect(new List<PValue> { 1, 2, 3 }, 3, (PValue)new List<PValue> { 1, 2 });
        }

        #endregion
    }
}
