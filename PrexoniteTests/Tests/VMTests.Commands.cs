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
#if ((!(DEBUG || Verbose)) || forceIndex) && allowIndex
#define useIndex
#endif

#define UseCil
//need to change this in VMTestsBase.cs too!

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

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
    public void FlatMapImplementation()
    {
        Compile(@"
function main() {
    coroutine f(n) {
        for(var i = 1; i <= n; i++)
            yield i;
    }
    return var args >> flat_map(f(?), [1,0], []) >> foldl( (l,r) => l + "","" + r, """");
}
");
            
        Expect(",1,1,2,3,1,2,3,4", 3,4);
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
            PType.List.CreatePValue(new PValue[] {a, b, c}),
            PType.List.CreatePValue(new PValue[] {d, e}),
            PType.List.CreatePValue(new PValue[] {f}),
            PType.List.CreatePValue(new PValue[] {g}));
    }

    [Test]
    public void CallCommandImplementationForPa()
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
    return call(call(?), [sum(?)], var args);
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
            PType.List.CreatePValue(new PValue[] {a, b, c}),
            PType.List.CreatePValue(new PValue[] {d, e}),
            PType.List.CreatePValue(new PValue[] {f}),
            PType.List.CreatePValue(new PValue[] {g}));
    }

    [Test]
    public void PartialCallCommand()
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
    var f = call(call(?), [?], ?);
    println(var x = f.(sum(?), var args));
    return x;
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
            PType.List.CreatePValue(new PValue[] {a, b, c}),
            PType.List.CreatePValue(new PValue[] {d, e}),
            PType.List.CreatePValue(new PValue[] {f}),
            PType.List.CreatePValue(new PValue[] {g}));
    }

    [Test]
    public void PartialCallCommandPa()
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
    var f = call(call(sum(?),?), ?);
    println(var x = f.(var args));
    return x;
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
            PType.List.CreatePValue(new PValue[] {a, b, c}),
            PType.List.CreatePValue(new PValue[] {d, e}),
            PType.List.CreatePValue(new PValue[] {f}),
            PType.List.CreatePValue(new PValue[] {g}));
    }

    [Test]
    public void CallMemberCommandImplementation()
    {
        var obj = new MemberCallable {Name = "obj"};
        obj.Expect("m", new PValue[] {1, 2, 3}, PCall.Get, 6);
        obj.Expect("", new PValue[] {4, "a"}, PCall.Set);

        Compile(
            @"
function main(obj, x, xs, y1, y2)
{
    var f1 = call\member(?,""m"",[?],?);
    var f2 = call\member(?,?,?) = [?];

    f1.(obj,x,xs);
    f2.(obj,"""",y1, y2);
}
");
        ExpectNull(sctx.CreateNativePValue(obj), 1, (PValue) new List<PValue> {2, 3},
            (PValue) new List<PValue> {4}, "a");
        obj.AssertCalledAll();
    }

    [Test]
    public void CallTailImplementation()
    {
        Compile(
            @"
ref check_stack;

var result;

function factorial(n,r)
{
    r ??= 1;
    if(n <= 1)
    {
        check_stack();
        result = r;
    }
    else
    {
        call\tail(factorial(?,?),[n-1,r*n]);
    }
}

function tailed(ref f, n)[is volatile;]
{
    return f(n);
}

function main(check, n)
{
    ->check_stack = check;
    
    var f = call\tail(factorial(?,?),[?]);
    tailed(f, n);
    return result;
}
");

        var check = CompilerTarget.CreateFunctionValue((s, _) =>
        {
            if (s.ParentEngine.Stack.Count > 2)
            {
                foreach (var stackContext in s.ParentEngine.Stack)
                    TestContext.WriteLine(" - " + stackContext);

                throw new PrexoniteException("Stack size is not constant.");
            }
            return PType.Null;
        });

        var fac = target.Functions["factorial"];
        Assert.IsTrue(fac!.Meta[PFunction.VolatileKey].Switch,
            "The factorial function should be volatile.");
        Assert.IsTrue(fac.Meta[PFunction.DeficiencyKey].Text.Contains(Engine.Call_TailAlias),
            "Deficiency of factorial function must mention " + Engine.Call_TailAlias);

        Expect(1, check, 1);
        //Expect(40320,check,8);
    }

    [Test]
    public void CallAsyncCommandImplementation()
    {
        Compile(
            @"
function fib_rec(n, /*lazy*/ fibT, ref combine) = 
    if(n <= 0) 0 else if(n <= 2) 1 else combine(force(fibT).(n-1), force(fibT).(n-2));

function main(n,m, i)
{
    //Simple, background
    let simple_fib = fib_rec(?, simple_fib, ? + ?);
    var f1 = call\async(fib_rec(?),[?, simple_fib, ? + ?]);
    println(var c1 = f1.(n));
    
    //Recursive, background
    let rec_fib = call\async(fib_rec(?), [?, rec_fib, (ca,cb) => ca.receive + cb.receive]);
    var f2 = force(rec_fib);
    println(var c2 = f2.(m));

    //No lazyness involved
    function sfib(n) = if(n <= 0) 0 else if(n <= 2) 1 else sfib(n-1) + sfib(n-2);
    var c3 = call\async(sfib(?),[i]);

    println(var r = [c1.receive, c2.receive, c3.receive]);
    return r;
}
");
        //fib 8 = 21
        //fib 7 = 13
        //fib 6 = 8
        Expect(new List<PValue> {8, 13, 21}, 6, 7, 8);
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

        Expect("54321", Array.Empty<PValue>());
    }

    [Test]
    public void AsyncSeqSemantics()
    {
        Compile(
            @"
function main(xs)
{
    return foldl((a,b) => a + "">"" + b, """") << async_seq(xs);
}
");

        Expect(">1>2>3", (PValue) new List<PValue> {1, 2, 3});
    }

    #region Call\*

    [Test]
    public void CallStarSimple()
    {
        Compile(
            @"
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
        Compile(
            @"
function main(x, xs)
{
    var f = call\star(call([?],?),?,?);
    println(var x = f.(xs,x));
    return x;
}
");

        Expect(new List<PValue> {1, 2, 3}, (PValue) new List<PValue> {3},
            (PValue) new List<PValue> {1, 2});
    }

    [Test]
    public void CallStarAllArgumentsDirected()
    {
        Compile(
            @"
function main(x, y, z)
{
    var f = call\star(call([?],?),[?,?],[?]);
    println(var x = f.(x,y,z));
    return x;
}
");

        Expect(new List<PValue> {1, 2, 3}, 1, 2, 3);
    }

    [Test]
    public void CallStarCustom()
    {
        Compile(
            @"
function main(x, xs)
{
    var f = call\star(2,call(?),[?],?,[?]);
    println(var x = f.(xs,x));
    return x;
}
");

        Expect(new List<PValue> {1, 2, 3}, 3, (PValue) new List<PValue> {1, 2});
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

        Expect(new List<PValue> {1, 2, 3}, (PValue) new List<PValue> {3}, 1, 2);
    }

    [Test]
    public void CallStarMapped()
    {
        Compile(
            @"
function main(x, xs)
{
    var f = call\star(call([?],?),?1,[?0]);
    println(var x = f.(x, xs));
    return x;
}
");

        Expect(new List<PValue> {1, 2, 3}, 3, (PValue) new List<PValue> {1, 2});
    }


    [Test]
    public void CallStarCustomMapped()
    {
        Compile(
            @"
function main(x, xs)
{
    var f = call\star(2, call(?), [?], ?1, [?0]);
    println(var x = f.(x, xs));
    return x;
}
");

        Expect(new List<PValue> {1, 2, 3}, 3, (PValue) new List<PValue> {1, 2});
    }

    #endregion

    [Test]
    public void CreateEnumeratorCommand()
    {
        Compile(
            @"
function main(xs)
{
    var s = new Structure;
    var cM = 0;
    var cC = 0;
    var cD = 0;
    s.\\(""GetEnumerator"") = () =>
    {
        var e = xs.GetEnumerator;
        return new enumerator(
            () => {cM++; return e.MoveNext;},
            () => {cC++; return e.Current; },
            () => {cD++; dispose(e);       },
        );
    };

    return ""M$cM.C$cC.D$cD: "" + foldl(?+?,"""",s) + "" :M$cM.C$cC.D$cD"";
}
");

        Expect("M0.C0.D0: abc :M4.C3.D1", (PValue) new List<PValue> {"a", "b", "c"});
    }

    [Test]
    public void ListExceptCommand()
    {
        Compile(
            @"
function main(xs,ys)
{
    xs >> except(ys) >> all >> var r;
    println(""RESULT = "",r);
    return r;
}
");
        PValue a = 11, b = 13, c = 17, d = 19;
        var xs = (PValue) new List<PValue> {a, c, d};
        var ys = (PValue) new List<PValue> {a, b};

        Expect(new List<PValue> {c, d}, xs, ys);
    }

    [Test]
    public void AppendCommand()
    {
        Compile(
            @"
function main(xs,ys)
{
    append(xs,ys) >> all >> var r;
    println(""RESULT = "",r);
    return r;
}
");
        PValue a = 11, b = 13, c = 17, d = 19;
        var xs = (PValue) new List<PValue> {a, c, d};
        var ys = (PValue) new List<PValue> {a, b};

        Expect(new List<PValue> {a, c, d, a, b}, xs, ys);
    }
}