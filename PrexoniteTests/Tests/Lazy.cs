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
using NUnit.Framework;
using Prexonite;

namespace PrexoniteTests.Tests;

public abstract class Lazy : VMTestsBase
{
    public Lazy()
    {
        CompileToCil = false;
    }


    [Test]
    public void SingularThunk()
    {
        Compile(
            @"
function _idT xT = xT.force;

function main(n)
{
    var t = thunk(->_idT,n);
    return t.force;
}
");
        const int n = 77;
        Expect(n, n);
    }

    [Test]
    public void BasicThunk()
    {
        Compile(
            @"
function _addT xT yT = xT.force + yT.force;
function _mulT xT yT = xT.force * yT.force;

function main(x1,y1,x2,y2)
{
    //x1*x2 + y1*y2
    var t = thunk(->_addT,thunk(->_mulT,x1,x2),thunk(->_mulT,y1,y2));
    return t.force;
}
");
        const int x1 = 15;
        const int x2 = 17;
        const int y1 = 5;
        const int y2 = -8;
        const int dot = x1*x2 + y1*y2;
        Expect(dot, x1, y1, x2, y2);
    }

    [Test]
    public void NotExecuted()
    {
        Compile(
            @"
function _divT xT yT = xT.force / yT.force;
function _throwT = throw ""Invalid computation"";
function _consT hT tT = [hT,tT];
function _headT xsT = xsT.force[0];

function main(x1)
{
    var t1 = thunk(->_throwT);
    var t2 = thunk(->_divT,4,0);
    var t3 = thunk(->_divT,2*x1,2);
    var t4 = thunk(->_consT,t3,thunk(->_consT,t2,null));
    var t5 = thunk(->_headT,t4); //x1
    return t5.force;
}
");

        Expect(15, 15);
    }

    [Test]
    public void Repeat()
    {
        Compile(
            @"
function _consT hT tT = [hT,tT];
function _headT xsT = xsT.force[0];
function _tailT xsT = xsT.force[1];
function _refT xT = xT.force.();
function _addT x1 x2 = x1.force + x2.force;

function main(x1)
{
    function repeatT(x)
    {
        var xsT;
        var xsT = thunk(->_consT,x,thunk(->_refT,->xsT));
        return xsT;
    }

    var x1s = repeatT(x1);
    var y1 = thunk(->_headT,x1s);
    var y1s = thunk(->_tailT,x1s);
    var z1 = thunk(->_headT,y1s);
    var z1s = thunk(->_tailT,y1s);
    var a1 = thunk(->_headT,z1s);

    var result = thunk(->_addT,y1,thunk(->_addT,z1,a1));

    return result.force;
}
");

        Expect(3*4, 4);
    }

    [Test]
    public void ByValueCapture()
    {
        Compile(
            @"
function main(xs)
{
    var ys = [];
    foreach(var x in xs)
    {
        ys[] = lazy x;
    }
    return foldl((acc,z) => acc + z.force,"""",ys);
}
");

        var xs = new List<PValue> {1, 2, 3};
        Expect("123", (PValue) xs);
    }

    [Test]
    public void AppendLazyFunction()
    {
        Compile(
            @"

lazy function cons hdT tlT = hdT : tlT;
lazy function head xsT = xsT.force.Key;
lazy function tail xsT = xsT.force.Value;

lazy function append xsT ysT
{
    if(xsT.force is not null)
    {
        var xT  = head << xsT;
        var xsT = tail << xsT;
        return cons(xT, lazy append(xsT, ysT));
    }
    else
    {
        return ysT;
    }
}

lazy function to_lazy_list seq
{
    var e = seq.force.GetEnumerator();
    lazy function nextElement =
        if(e.MoveNext())
            cons(e.Current, thunk(->nextElement))
        else
            null;
    return nextElement;
}

coroutine to_seq xsT
{
    while((var xs = asthunk(xsT).force) is not null)
    {
        yield asthunk(xs.Key).force;
        xsT = xs.Value;
    }
}

lazy function countlz(xsT)
{
    var count = 0;
    while((var xs = xsT.force) is not null)
    {
        count++;
        xsT = tail << xsT;
    }
    return count;
}

function main(xs, ys, seed)
{
    var xsT = to_lazy_list(xs);
    var ysT = to_lazy_list(ys);
    var zsT = append(xsT, ysT);
    println(countlz << xsT);
    return 
        zsT
        >> to_seq
        >> foldl((a,b) => a + b,seed);
}
");

        var xs = new List<PValue> {"a", "b", "c"};
        var ys = new List<PValue> {1, 2, 3};
        const string seed = ">>";

        Expect(">>abc123", (PValue) xs, (PValue) ys, seed);
    }


    [Test]
    public void SimpleLetBindingStmt()
    {
        Compile(
            @"
lazy function cons x xs = x : xs;

lazy function repeat x
{
    let xs = cons(x,xs);
    return xs;
}

coroutine to_seq xsT
{
    while((var xs = asthunk(xsT).force) is not null)
    {
        yield asthunk(xs.Key).force;
        xsT = xs.Value;
    }
}

function main(x,n)
{
    var one = 1;
    var zero = 0;
    let undefined = one/zero;
    return foldl((a,b) => a + b,""<<"") << take(n) << to_seq << repeat << x;
}

");

        Expect("<<xxx", "x", 3);
    }

    [Test]
    public void ArgumentLetBindingStmt()
    {
        Compile(
            @"
lazy function cons x xs = x : xs;

coroutine to_seq xsT
{
    while((var xs = asthunk(xsT).force) is not null)
    {
        yield asthunk(xs.Key).force;
        xsT = xs.Value;
    }
}

function main(n)
{
    let fib =
    {
        lazy function nextFib x1 x2
        {
            let x3 = x1.force + x2.force;
            return x3 : lazy nextFib(x2,x3);
        };
        return cons(1) << lazy nextFib(0,1);
    };

     return foldl((a,b) => a + b,""<<"") << take(n) << to_seq << fib;
}
");

        Expect("<<11235813", 7);
    }

    [Test]
    public void OutOfOrder()
    {
        Compile(
            @"
function main(a)
{
    let b;
    let c;
    let d = b.().(c);
    let c = 5;
    let b = (my_c) => force(a) + force(my_c);

    return d.();
}
");

        Expect(8, 3);
    }

    [Test]
    public void MutuallyRecursive()
    {
        Compile(
            @"
function main(n)
{
    let flip, flop,
        flip = 1 : flop,
        flop = 0 : flip;
    return foldl((a,b) => a + b, ""<<"") << take(n) << toseq(flip);
}
");

        Expect("<<101010", 6);
    }
}