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
using PrexoniteTests.Tests;

namespace Prx.Tests;

public abstract partial class VMTests : VMTestsBase
{
    [Test]
    public void Basic()
    {
        const string input1 = @"
function test1
{
    var x = 5 + 5;
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        Assert.AreEqual(0, ldr.ErrorCount);

        var test1 = target.Functions["test1"];
        var fctx = new FunctionContext(engine, test1);
        var x = fctx.LocalVariables["x"];
        Assert.IsTrue(
            x.Value?.Value == null, "variable x must be null in some way.");
        engine.Stack.AddLast(fctx);
        engine.Process();
        Assert.AreEqual(
            0, engine.Stack.Count, "Machine stack is expected to be empty after execution.");
        Assert.IsNotNull(x.Value, "Value of PVariable is null (violates invariant).");
        Assert.AreEqual(PType.BuiltIn.Int, x.Value.Type.ToBuiltIn());
        Assert.IsNotNull(x.Value.Value, "Result is null (while PType is Int)");
        Assert.AreEqual(10, (int) x.Value.Value);
    }

    [Test]
    public void IncDecrement()
    {
        const string input1 =
            @"
function test1(x)
{
    x++;    
    x = 2*x++;
    return x--;
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        foreach (var s in ldr.Errors)
            TestContext.WriteLine(s);

        Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation");

        var rnd = new Random();
        var x0 = rnd.Next(1, 200);
        var x = x0;
        x++;
        x = 2*x;
        var expected = x--;

        var fctx =
            target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
        engine.Stack.AddLast(fctx);
        var rv = engine.Process();

        Assert.AreEqual(PType.BuiltIn.Int, rv.Type.ToBuiltIn());
        Assert.AreEqual(
            expected,
            (int) rv.Value,
            "Return value is expected to be " + expected + ".");

        Assert.AreEqual(
            x,
            (int) fctx.LocalVariables["x"].Value.Value,
            "Value of x is supposed to be " + x + ".");
    }

    [Test]
    public void LateReturnIsIllegal()
    {
        const string input1 =
            @"
function test1(x)
{
    x*=2;
    return = x-2;
    x+=55;
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        foreach (var s in ldr.Errors)
            TestContext.WriteLine(s);
        Assert.AreEqual(1, ldr.ErrorCount, "One error expected.");
        Assert.IsTrue(
            ldr.Errors[0].Text.Contains("Return value assignment is no longer supported."),
            "The compiler did not reject a return value assignment.");
    }

    [Test]
    public void Return()
    {
        const string input1 =
            @"
function twice(v) = 2*v;
function complicated(x,y) does
{
    var z = x*y;
    x = z-x;
    y = x+z;
    //z     := x*y
    //x     := x*y-x
    //y     := 2*x*y-x
    //y+z   := 3*x*y-2*x
    //y+z   := x*(3*y-2)
    return y+x;
    //dummy     
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        foreach (var s in ldr.Errors)
            TestContext.WriteLine(s);
        Assert.AreEqual(0, ldr.ErrorCount);

        var rnd = new Random();

        //Test simple
        var v0 = rnd.Next(1, 100);
        var expected = 2*v0;

        var result = target.Functions["twice"].Run(engine, new PValue[] {v0});
        Assert.AreEqual(
            PType.BuiltIn.Int,
            result.Type.ToBuiltIn(),
            "Result is expected to be an integer. (twice)");
        Assert.AreEqual(expected, (int) result.Value);

        //Test complicated            
        var x0 = rnd.Next(1, 100);
        var y0 = rnd.Next(1, 100);
        var z = x0*y0;
        var x1 = z - x0;
        var y1 = x1 + z;
        expected = y1 + x1;

        result = target.Functions["complicated"].Run(engine, new PValue[] {x0, y0});
        Assert.AreEqual(
            PType.BuiltIn.Int,
            result.Type.ToBuiltIn(),
            "Result is expected to be an integer. (complicated)");
        Assert.AreEqual(expected, (int) result.Value);
    }

    [Test]
    public void FunctionAndGlobals()
    {
        const string input1 =
            @"
var J; //= random();
function h(x) = x+2+J;
function test1(x) does
{
    J = 0;
    x = h(J);
    J = h(7*x);
    return h(x)/J;
}
";
        var rnd = new Random();
        var j = rnd.Next(1, 1000);

        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        target.Variables["J"].Value = j;
        Assert.AreEqual(0, ldr.ErrorCount);

        TestContext.WriteLine(target.StoreInString());

        //Expectation
        var x0 = rnd.Next(1, 589);
        j = 0;
        j = 7*x0 + 2 + j;
        var expected = (x0 + 2 + j)/j;

        var fctx =
            target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
        engine.Stack.AddLast(fctx);
        var rv = engine.Process();
        Assert.AreEqual(PType.BuiltIn.Int, rv.Type.ToBuiltIn());
        Assert.AreEqual(expected, (int) rv.Value);
    }

    [Test]
    public void FunctionCallSimple()
    {
        const string input1 =
            @"
var J;
function h(x) = 2+x+J;
function test1(x) does
{
    J = 0;
    x = h(J);
    J = h(7*x);
    return h(x)/J;
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        Assert.AreEqual(0, ldr.ErrorCount);

        TestContext.WriteLine(target.StoreInString());

        var rnd = new Random();
        const int j0 = 0;
        var x0 = rnd.Next(1, 300);
        const int x1 = 2 + j0 + j0;
        const int j1 = 2 + 7*x1 + j0;
        const int expected = (2 + x1 + j1)/j1;

        var fctx =
            target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
        engine.Stack.AddLast(fctx);
        var rv = engine.Process();
        Assert.AreEqual(PType.BuiltIn.Int, rv.Type.ToBuiltIn());
        Assert.AreEqual(expected, (int) fctx.ReturnValue.Value);

        Assert.AreEqual(PType.BuiltIn.Int, target.Variables["J"].Value.Type.ToBuiltIn());
        Assert.AreEqual(j1, (int) target.Variables["J"].Value.Value);
    }

    [Test]
    public void FibRecursion()
    {
        const string input1 =
            @"
function fib(n) does
{
    if(n <= 2)
        return 1;
    else
        return fib(n-1) + fib(n-2);
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        Assert.AreEqual(0, ldr.ErrorCount);

        for (var n = 1; n <= 6; n++)
        {
            TestContext.WriteLine("\nFib(" + n + ") do ");
            var expected = _fibonacci(n);
            var fctx =
                target.Functions["fib"].CreateFunctionContext(engine, new PValue[] {n});
            engine.Stack.AddLast(fctx);
            var rv = engine.Process();
            Assert.AreEqual(
                PType.BuiltIn.Int, rv.Type.ToBuiltIn(), "Result must be a ~Int");
            Assert.AreEqual(
                expected,
                (int) rv.Value,
                "Fib(" + n + ") = " + expected + " and not " + (int) rv.Value);
        }
    }

    [Test]
    public void Recursion()
    {
        const string input1 =
            @"
function fib(n) does asm
{
    //if n <= 2
    ldloc   n
    ldc.int 2
    cle
    jump.f  else
    //return 1;
    ldc.int 1
    ret.value
    jump    endif
    //else do
    label   else
    //return = fib(n-1) + fib(n-2);
    ldloc   n
    ldc.int 1
    sub
    func.1  fib
    ldloc   n
    ldc.int 2
    sub
    func.1  fib
    add
    ret.value
    
    label   endif
}
";
        var ldr = new Loader(engine, target);
        ldr.LoadFromString(input1);
        Assert.AreEqual(0, ldr.ErrorCount);

        for (var n = 1; n <= 6; n++)
        {
            TestContext.WriteLine("\nFib(" + n + ") do ");
            var expected = _fibonacci(n);
            var fctx =
                target.Functions["fib"].CreateFunctionContext(engine, new PValue[] {n});
            engine.Stack.AddLast(fctx);
            var rv = engine.Process();
            Assert.AreEqual(
                PType.BuiltIn.Int, rv.Type.ToBuiltIn(), "Result must be a ~Int");
            Assert.AreEqual(
                expected,
                (int) rv.Value,
                "Fib(" + n + ") = " + expected + " and not " + (int) rv.Value);
        }
    }

    [DebuggerNonUserCode]
    static int _fibonacci(int n)
    {
        return
            n <= 2
                ? 1
                : _fibonacci(n - 1) + _fibonacci(n - 2);
    }

    [Test]
    public void WhileLoop()
    {
        Compile(
            @"
var M;
function modify(x) =  M*x+12;

function main(newM, iterations)
{
    M = newM;
    var i = 0;
    var sum = 0;
    while(i<iterations)       
        sum = sum + modify(i++);
    return sum;
}
");

        var rnd = new Random();
        var m = rnd.Next(1, 13);
        var iterations = rnd.Next(3, 10);
        var sum = 0;
        for (var i = 0; i < iterations; i++)
            sum += m*i + 12;
        var expected = sum;

        ExpectNamed("main", expected, m, iterations);
    }

    [Test]
    public void ForLoop()
    {
        Compile(
            @"
var theList;

function getNextElement =
    if(static index < theList.Count)
        theList[index++]
    else 
        null;

function print(text) does static buffer.Append(text);

function main(aList, max)
{
    theList = aList;
    declare var print\static\buffer, getNextElement\static\index;
    print\static\buffer = new Text::StringBuilder;
    getNextElement\static\index = 0;
    
    var cnt = 0;
    for(var     elem;
        do      elem = getNextElement;
        until   elem == null
       )
    {
        var len = elem.Length;
        if(cnt + len > max)
            continue;
        print(elem);
        cnt += len;
    }
    return print\static\buffer.ToString;
}
");
        var buffer = new StringBuilder();
        const int max = 20;
        var aList = new List<string>(
            new[]
            {
                GenerateRandomString(5),
                GenerateRandomString(10),
                GenerateRandomString(15),
                GenerateRandomString(3),
                GenerateRandomString(5)
            });

        foreach (var elem in aList)
            if (buffer.Length + elem.Length < max)
                buffer.Append(elem);

        Expect(buffer.ToString(), engine.CreateNativePValue(aList), max);
    }

    [Test]
    public void StaticClrCalls()
    {
        Compile(
            @"
entry main;
function main(rawInteger)
{
    return System::Int32.Parse(rawInteger);
}
");
        var rnd = new Random();
        var expected = rnd.Next(1, 45);
        Expect(expected, expected.ToString());
    }

    [Test]
    public void Conditions()
    {
        Compile(
            @"
entry conditions;
var G;
function action1 does G += ""1"";
function action2 does G += ""2"";
function conditions(x,y)
{
    G = """";
    //Simple:
    if(x)
        action1;

    //Simple #2
    unless(y)
        {action1;}
    else
        action2;

    //Constant
    if(true and true)
        action1;
    else
        action2;

    //Complex
    if(x)
        unless (y)
            action1;
        else
            action2;
    else
    {
        action1;
        action2;
    }

    //Redundant blocks/conditions
    if(y){}else action2;
    
    if(not x){}else{}

    return G;
}
");
        const string tt = "1212";
        const string tx = "11112";
        const string xT = "2112";
        const string xx = "11122";

        TestContext.WriteLine("// TRUE  - TRUE ");
        Expect(tt, true, true);
        TestContext.WriteLine("// TRUE  - FALSE");
        Expect(tx, true, false);
        TestContext.WriteLine("// FALSE - TRUE ");
        Expect(xT, false, true);
        TestContext.WriteLine("// FALSE - FALSE");
        Expect(xx, false, false);
    }

    [Test]
    public void IndexAccess()
    {
        Compile(
            @"
declare function print;
function main(str, idx)
{
    var i = 0;
    
_while:
    unless(i < str.Length)
        goto _endwhile;
    print(str[i++] + "" "");
    goto _while;
_endwhile:
    return print(""--"" + str[idx]);    
}

function print(text) does
{
    if (static buffer == null) buffer = """";
    unless (text == null) buffer += text;
    return buffer;
}
");

        var str = Guid.NewGuid().ToString("N").Substring(0, 3);
        var rnd = new Random();
        var idx = rnd.Next(0, str.Length);
        var buffer = new StringBuilder();
        foreach (var ch in str)
            buffer.Append(ch.ToString() + ' ');
        buffer.Append("--" + str[idx]);
        var expect = buffer.ToString();
        Expect(expect, str, idx);
    }

    [Test]
    public void NonRecursiveTailCall()
    {
        options.RegisterCommands = true;
        Compile(
            @"
var buffer;
function print(text) = buffer.Append = text;
function work
{
    var args;
    buffer = new System::Text::StringBuilder(args[0]);
    print(args[1]);
    print(args[2]);
    return buffer;
}

function main(a,b,c) = work(a,b,c).ToString;
");
        var a = Guid.NewGuid().ToString("N");
        var b = Guid.NewGuid().ToString("N");
        var c = Guid.NewGuid().ToString("N");
        var expect = a + b + c;
        Expect(expect, a, b, c);
    }

    [Test]
    public void Commands()
    {
        options.RegisterCommands = true;
        engine.Commands.AddUserCommand(
            "conRev",
            new DelegatePCommand(
                delegate(StackContext localSctx, PValue[] args)
                {
                    var sb = new StringBuilder();
                    for (var i = args.Length - 1; i > -1; i--)
                        sb.Append(args[i].CallToString(localSctx));
                    return (PValue) sb.ToString();
                }));

        var list =
            new[] {"the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"};

        engine.Commands.AddUserCommand(
            "theList",
            new DelegatePCommand(
                (localSctx, args) => localSctx.CreateNativePValue(list)));
        Compile(
            @"function main = conRev(theList[0], theList[1], theList[2], theList[3], theList[4], theList[5], theList[6], theList[7], theList[8]);");

        var buffer = new StringBuilder();
        for (var i = list.Length - 1; i > -1; i--)
            buffer.Append(list[i]);

        Expect(buffer.ToString());
    }

    public class SomeSortOfList : IEnumerable<string>
    {
        public SomeSortOfList(string input)
        {
            _input = input;
        }

        readonly string _input;

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            var words = _input.Split(new[] {' ', '\t', '\n', '\r'});

            foreach (var word in words)
                if (word.Length > 0)
                    yield return word[0].ToString().ToUpperInvariant();

            yield return ">>";

            foreach (var word in words)
                if (word.Length > 0)
                    if (word[0]%2 == 0)
                        yield return word.Insert(1, "\\").ToUpperInvariant();
                    else
                        yield return word.ToLowerInvariant();

            yield return "<<";
            yield return words.Length.ToString();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal string _PrintList()
        {
            var buffer = new StringBuilder();
            foreach (var s in this)
            {
                buffer.Append(' ');
                buffer.Append(s);
            }
            return buffer.ToString();
        }

        internal int _CountList()
        {
            using var e = GetEnumerator();
            while (e.MoveNext())
                if (e.Current == ">>")
                    break;
            var cnt = 0;
            while (e.MoveNext())
                if (e.Current == "<<")
                    break;
                else
                    cnt += e.Current.Length;

            return cnt;
        }
    }

    [Test]
    public void SimpleForeach()
    {
        Compile(
            @"
function main(lst)
{
    var i = 0;
    foreach(var e in lst)
        i++;
    return i;
}
");
        Expect(5, PType.List.CreatePValue(new PValue[] {1, 2, 3, 4, 5}));
    }

    [Test]
    public void Foreach()
    {
        var lst = new SomeSortOfList("The quick brown fox jumps over the lazy dog");
        Compile(
            @"
var buffer;
function print does
    foreach( var arg in var args)
        buffer.Append(arg.ToString); 

function init does
    buffer = new System::Text::StringBuilder;

function printList(lst) does
{
    init;
    foreach( print("" "") in lst);
    return buffer.ToString;
}

function countList(lst) does
{
    var cnt = 0;
    var state = 0;
    foreach(var e in lst)
        if (state == 0)
            if (e == "">>"")
                state = 1;
            else 
                continue;
        else if (state == 1)
            if (e == ""<<"")
                state = 2;
            else
                cnt += e.Length;
        else
            continue;
    return cnt;
}
");

        ExpectNamed("printList", lst._PrintList(), sctx.CreateNativePValue(lst));
        ExpectNamed("countList", lst._CountList(), sctx.CreateNativePValue(lst));
    }

    [Test]
    public void GlobalVarInit()
    {
        Compile(
            @"
var buffer = new ::Text::StringBuilder;
var HW = ""Hello"";

var HW = HW + "" World"";

function print does foreach (buffer.Append in var args);

function main(x)
{
    if (x >= HW.Length) x = HW.Length -1;
    for (var i = 0; until i == x; i++)
    {
        HW = HW.Insert(i, i.ToString);
        print("">"", HW);
    }
    return buffer.ToString;
}
");
        var buffer = new StringBuilder();
        var hw = new StringBuilder("Hello World");
        var rnd = new Random();
        var x = rnd.Next(0, hw.Length + 1);
        var xi = x >= hw.Length ? hw.Length - 1 : x;
        for (var i = 0; i < xi; i++)
        {
            hw.Insert(i, i.ToString());
            buffer.Append(">");
            buffer.Append(hw.ToString());
        }
        var expect = buffer.ToString();

        Expect(expect, x);
    }

    [Test]
    public void PartialInitialization()
    {
        var ldr =
            Compile(
                @"

Add System::Text to Import;

var buffer = new System::Text::StringBuilder;
function print does foreach( buffer.Append in var args);

var L1 = ""1o1"";
var L2;

function main(level)
{
    unless( level < 1)
       print(""#1="",L1,"";"");
    
    unless (level < 2)
       print(""#2="",L2,"";"");

    declare var L3;

    unless (level < 3)
        print(""#3="",L3,"";"");

    return buffer.ToString; 
}
");

        Expect("#1=1o1;", 1);

        //Continue compilation using the same loader
        Compile(
            ldr,
            @"
var L2 = ""2p2"";

{
    L1 = ""1o2"";
    buffer = new System::Text::StringBuilder;
}
");
        Expect("#1=1o2;#2=2p2;", 2);

        //Continue compilation using a different loader
        Compile(
            @"
var L3 = ""3z3"";
var L2 = ""2m3"";
var L1 = ""1k3"";

declare var buffer;
{ buffer = new System::Text::StringBuilder; }
");

        Expect("#1=1k3;#2=2m3;#3=3z3;", 3);
    }

    [Test]
    public void UselessBuildBlock()
    {
        var ldr = Compile(@"
    var myGlob; var initGlob;
");

        Compile(
            ldr,
            @"
build
{
    var loc = ""hello"";
    var kop = 55 * 77;
    var hup = loc.ToUpper + kop~String;
    
    declare var myGlob, initGlob;
    myGlob = hup.Substring(1);
    initGlob = 42;
}

var myGlob;
var initGlob = ""init"";

function main = myGlob + initGlob;
");
        Expect("ELLO" + 55*77 + "init");
    }

    [Test]
    public void References()
    {
        Compile(
            @"
function foldl(ref f, var left, var lst) does // (b -> a -> b) -> b -> a -> [b]
{
    foreach (var right in lst) left = f(left,right);
    return left;
}

function map(ref f, var lst) does // (a -> b) -> [a] -> [b]
{
    var nlst = new List;
    foreach(var e in lst) nlst.Add = f(e);
    return nlst;
}

var tuple\lst;
function tuple(x)
{
    static idx;
    declare tuple\lst as lst;

    if(idx == null)
        idx = 0;

    var ret = ~List.Create(x, lst[idx++]);
    unless(idx < lst.Count)
        idx = null;
    return ret;
}

ref reduce\f;
function reduce(x) = reduce\f(x[0], x[1]); // (a -> a -> b) -> (a,a) -> b

var chain;
function chained(x)
{
    foreach(ref f in chain) x = f(x);
    return x;
}

function add(left, right) = left + right; //a -> a -> a
function sub(left, right) = left - right;
function mul(left, right) = left * right;

function id(x) = x;                       //a -> a
function twice(x) = 2*x;
function binary(x) = 2^x;

function dummy(x) {}

function assert(actual, expected, msg)
{
    if(actual != expected)
    {
        throw msg ?? ""Expected $expected, actual $actual"";
    }
}

function main()                           // IO() -> IO()
{
    //Create [1..10]
    var lst = new List;
    for(var i = 1; until i == 11; i++)
        lst.Add = i;
    
    var bin = map(->binary, lst); // 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024
    var bin\sum = foldl(->add, 0, bin); // 2046
    assert(bin\sum, 2046);

    chain = ~List.Create( -> twice, -> twice); //*4
    var bin\quad = map(->chained, bin); // 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096

    var twi = map(->twice, lst); // 2, 4, 6, 8, 10, 12, 14, 16, 18, 20

    tuple\lst = twi;
    var tup\bin_twi = map(->tuple, bin) >> all; // (2,2), (4,4), (6,8), (8,16), (10,32), (12,64), (14,128), (16,256), (18,512), (20,1024)
    println(tup\bin_twi);

    ->reduce\f = ->sub;
    var tup\bin_twi\sub = map(->reduce, tup\bin_twi) >> all; // 0, 0, 2, 8, 22, 52, 114, 240, 494, 1004
    assert(tup\bin_twi\sub[0],0);
    assert(tup\bin_twi\sub[1],0);
    assert(tup\bin_twi\sub[2],2);
    assert(tup\bin_twi\sub[9],1004);
    
    var tup\bin_twi\sub\sum = foldl(->add, 0 , tup\bin_twi\sub); // 1936
    assert(tup\bin_twi\sub\sum, 1936);
    
    var bin\quad\sum = foldl(->add, 0, bin\quad); // 8184
    assert(bin\quad\sum,8184);

    return  (bin\quad\sum - tup\bin_twi\sub\sum)~Int; // 6248
}
");
        Expect(6248);
    }

    [Test]
    public void TypeIdentification()
    {
        Compile(
            @"
function main(arg)
{
    var r = """";
    if(arg is String)
        var r = arg + arg;
    else if(arg is List)
        foreach(var e in arg) var r += e;
    else if(arg is System::Text::StringBuilder)
        r = arg.ToString;

    return r;       
}
");
        var rs = GenerateRandomString(3);
        Expect(rs + rs, rs);

        var lst = new List<PValue>(
            new PValue[]
            {
                GenerateRandomString(2), GenerateRandomString(3),
                GenerateRandomString(4)
            });
        var ls = lst.Aggregate("", (current, e) => current + (e.Value as string));
        Expect(ls, (PValue) lst);

        var sb = new StringBuilder(GenerateRandomString(5));
        Expect(sb.ToString(), engine.CreateNativePValue(sb));
    }

    [Test]
    public void ClosureCreation()
    {
        Compile(
            @"
function clo1 = x => 2*x;

function clo2(a)
{
    return x => a*x;
}
");

        var rnd = new Random();

#if UseCil
        var pclo1 = GetReturnValueNamed("clo1");
        Assert.AreEqual(PType.Object[typeof (PFunction)], pclo1.Type);
        var clo1 = pclo1.Value as PFunction;
        Assert.IsNotNull(clo1);

        var pclo2 = GetReturnValueNamed("clo2", rnd.Next(1, 10));
        if (CompileToCil)
        {
            Assert.AreEqual(PType.Object[typeof (CilClosure)], pclo2.Type);
            var clo2 = pclo2.Value as CilClosure;
            Assert.IsNotNull(clo2);
            Assert.AreEqual(1, clo2.SharedVariables.Length);
        }
        else
        {
            Assert.AreEqual(PType.Object[typeof (Closure)], pclo2.Type);
            var clo2 = pclo2.Value as Closure;
            Assert.IsNotNull(clo2);
            Assert.AreEqual(1, clo2.SharedVariables.Length);
        }

#else
            PValue pclo1 = _getReturnValueNamed("clo1");
            Assert.AreEqual(PType.Object[typeof(Closure)], pclo1.Type);
            Closure clo1 = pclo1.Value as Closure;
            Assert.IsNotNull(clo1);
            Assert.AreEqual(0, clo1.SharedVariables.Length);

            PValue pclo2 = _getReturnValueNamed("clo2", rnd.Next(1, 10));
            Assert.AreEqual(PType.Object[typeof(Closure)], pclo2.Type);
            Closure clo2 = pclo2.Value as Closure;
            Assert.IsNotNull(clo2);
            Assert.AreEqual(1, clo2.SharedVariables.Length);
#endif
    }

    [Test]
    public void Lambda()
    {
        Compile(
            @"
function split(ref f, var lst, ref left, ref right)
{
    var l;
    var r;
    
    left = new List;
    right = new List;
    
    foreach(var x in lst)
    {
        f(x, ->l, ->r);
        left.Add = l;
        right.Add = r;
    }   
}

function splitter(ref f, ref g) = (x, ref left, ref right) => { left = f(x); right = g(x); };

function combine(ref f, left, right)
{
    var lst = new List;
    var max = left.Count;
    if(right.Count > max)
        max = right.Count;
    for(var i = 0; until i == max; i++)
        lst.Add = f(left[i], right[i]);
    return lst;
}

function main(lst)
{
    //Lambda expressions
    var twi = map( x => 2*x, lst);        
    var factors;
    var rests;
    //using splitter higher order function
    split( splitter( (x) => (x / 10)~Int, (x) => x mod 10 ), twi, ->factors, ->rests);
    var tuples = combine( (l,r) => ""("" + l + "","" + r + "")"", factors, rests);
    return foldl( (l,r) => l + "" "" + r, """", tuples);
}
");
        var lst = new int[10];
        var rnd = new Random();
        var sb = new StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            lst[i] = rnd.Next(4, 49);
            var twi = 2*lst[i];
            var factors = twi/10;
            var rests = twi%10;
            sb.Append(" (" + factors + "," + rests + ")");
        }
        var expected = sb.ToString();

        var plst = lst.Select(x => (PValue) x).ToList();

        Expect(expected, PType.List.CreatePValue(plst));
    }

    [Test]
    public void Currying()
    {
        Compile(
            @"
function curry(ref f) = a => b => f(a,b);

function uncurry(ref f) = (a, b) => f(a).(b);

function map(ref f, lst)
{
    var nlst = new List;
    foreach( var x in lst)
        nlst.Add = f(x);
    return nlst;
}

function elementFeeder(lst) 
{
    var i = 0;
    var len = lst.Count;
    return () =>
    {
        if(i < len)
            return lst[i++];
        else
            return null;
    };
}

function listDifference(lst) does
{
    ref feed = elementFeeder(lst);
    return (x) => x - feed;
}

function main(lst, s)
{
    var add = (x,y) => x+y;
    ref additions = map(curry( add ),lst); // [ y => _const+y ]
    ref headComparer = uncurry(->listDifference);    
    var compared = map( x => headComparer(lst,x), additions(s) );
    var sb = new ::Text::StringBuilder;
    foreach(var e in compared) sb.Append("" "" + e);
    return sb.ToString;
}
");
        var rnd = new Random();
        var s = rnd.Next(2, 9);
        var plst = new List<PValue>();
        var head = -1;
        var sb = new StringBuilder();
        for (var i = 0; i < 10; i++)
        {
            var c = rnd.Next(11, 99);
            if (head < 0)
                head = c;
            plst.Add(c);
            var d = c + s;
            var compared = d - head;
            sb.Append(" ");
            sb.Append(compared.ToString());
        }
        var expect = sb.ToString();

        Expect(expect, PType.List.CreatePValue(plst), s);
    }

    [Test]
    public void NestedFunctions()
    {
        Compile(
            @"
function apply(ref f, x) = f(x);

function main(p)
{
    function koo(x)
    {
        var q = x.ToString;
        if(q.Length > 1)
            return q + ""koo"";
        else
            return q;
    }
    
    ref goo;
    if(p mod 10 == 0)
        function goo(x) = 2*x;
    else
        function goo(x) = 2+x;

    if(p > 50)
        function koo(x)
        {
            var q = x.ToString;
            q = q+q;
            if(q.Length mod 2 != 0)
                return q + ""q"";
            else
                return q;
        }

    return apply( ->koo , goo( p ) );
}
");
        var rnd = new Random();
        var ps =
            new[]
            {
                1, 2, 10, 27, 26, 57, 60, 157, rnd.Next(1, 190), rnd.Next(1, 190),
                rnd.Next(1, 190)
            };
        foreach (var p in ps)
        {
            int goo;
            if (p%10 == 0)
                goo = 2*p;
            else
                goo = 2 + p;

            string koo;
            var q = goo.ToString();
            if (p <= 50)
                koo = q.Length > 1 ? q + "koo" : q;
            else
            {
                q = q + q;
                koo = q.Length%2 != 0 ? q + "q" : q;
            }

            Expect(koo, p);
        }
    }

    [Test]
    public void DeDereference()
    {
        Compile(
            @"
function applyInChain(var chain, var x)
{
    var y = x;
    foreach( ref f in chain)
        y = f(y);
    return y;
}

function createChain(ref t, var f, var g)
{
    t = new List;
    t[] = f;
    t[] = g;
}

function main(var m)
{
    ref flst; //Would be easier as { var flst; } but this is a test after all...
    createChain(->->flst, x => 2*x, x => 2+x);
    return applyInChain(->flst, m);
}
");
        var rnd = new Random();
        var m = rnd.Next(3, 500);
        var expected = 2 + 2*m;

        Expect(expected, m);
    }

    [Test]
    public void PowerSqrt()
    {
        Compile(@"
function main(x,y)
{
    return ::Math.Sqrt(x^2 + y^2);
}
");
        const double x = 113.0;
        const double y = 13.0;
        Expect(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)), x, y);
    }

    [Test]
    public void ExplicitIndirectCall()
    {
        Compile(
            @"
function map(f, lst)
{
    var nlst = new List;
    foreach( var e in lst)
        nlst[] = f.(e);
    return nlst;
}

function combine(flst, olst)
{
    var nlst = new List;
    for(var i = 0; until i == flst.Count; i++)
        nlst[] = flst[i].(olst[i]);
    return nlst;
}


function main(xlst, ylst)
{
    var dx = map( x => y => ::Math.Sqrt(x^2 + y^2), xlst);
    var d = combine( dx, ylst );
    var addition = x => y => x+y;
    var sum = 0;
    foreach(var e in d)
        sum = addition.(sum).(e);
    return sum;
}
");
        var rnd = new Random();
        var sum = 0.0;
        List<PValue> xlst = new(),
            ylst = new();
        for (var i = 0; i < 10; i++)
        {
            var x = rnd.Next(3, 50);
            var y = rnd.Next(6, 34);

            xlst.Add(x);
            ylst.Add(y);

            var d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            sum += d;
        }
        Expect(sum, PType.List.CreatePValue(xlst), PType.List.CreatePValue(ylst));
    }

    [Test]
    public void ConditionalExpression()
    {
        Compile(
            @"
function abs(x) = if(x > 0) x else -x;
function max(a,b) = if(a > b) a else b;
var rnd = new System::Random;
function randomImplementation(fa, fb) = (a,b) => if(rnd.Next(0,2) mod 2 == 0) fa.(a,b) else fb.(a,b);
function sum(lst)
{
    var s = 0;
    foreach(var e in lst)
        s += e;
    return s;
}

function main(lst, limit)
{
    ref min = randomImplementation((a,b) => if(max(a,b) == a) b else a, (a,b) => if(a < b) a else b);
    return -sum(all(map(a => min(a, limit), lst)));
}
");
        var rnd = new Random();
        var lst = new List<PValue>();
        var sum = 0;
        for (var i = 0; i < 10; i++)
        {
            var e = rnd.Next(-5, 6);
            lst.Add(e);
            if (e < 0)
                sum += e;
        }

        lst.Add(4);
        lst.Add(1);
        lst.Add(0);
        lst.Add(-2);
        sum -= 2;
        Expect(-sum, PType.List.CreatePValue(lst));
    }

    [Test]
    public void NestedConditionalExpressions()
    {
        Compile(
            @"
function main(xs)
{
    var ys = [];
    foreach(var x in xs)
        ys[] = 
            if(x mod 2 == 0) if(x > 5)   x
                             else        x*2
            else             if(x < 10)  (x+1) / 2
                             else        x+2
        ;
    
    var s = 0;
    foreach(var y in ys)   
        s+=y;
    return s;
}
");
        var xs = new List<PValue>(
            new PValue[]
            {
                12, //=> 12
                4, //=> 8
                5, //=> 3
                13 //=> 15
            });

        Expect(12 + 8 + 3 + 15, PType.List.CreatePValue(xs));
    }

    [Test]
    public void GlobalRefAssignment()
    {
        Compile(
            @"
var theList;

function accessor(index) = v => 
{
    if(v != null)
        theList[index] = v;
    return theList[index];
};

ref first = accessor(0);
ref second = accessor(1);

function average(lst)
{
    var av = 0;
    foreach(var e in lst)
        av += e;
    return (av / lst.Count)~Int;
}

function main(lst)
{
    theList = lst;
    first = 4;
    second = 7;
    return ""f$first::$(average(lst))::s$second"";
}
");
        var lst = new List<PValue>();
        var av = 0;
        for (var i = 0; i < 10; i++)
        {
            lst.Add(i);
            var k = i != 0 ? i != 1 ? i : 7 : 4;
            av += k;
        }
        av = av/10;
        Expect("f4::" + av + "::s7", PType.List.CreatePValue(lst));
    }

    [Test]
    public void NoReturnTransformationInInit()
    {
        Compile(
            @"
var x = 3;

function f
{
    x = 4;
}

{
    x = 5;
    f();
}

{
    x = 6;
}

function main()
{
    return x;
}
");

        Expect(6);
    }

    [Test]
    public void FakeSharedCtor()
    {
        Compile(
            @"
    function main(x)
    {
        function create_obj()
        {
            var s = new Structure;
            s.\(""x"") = x;
            return s;
        }
        function inner()
        {
            return (new obj).x + 1;
        }

        return inner() * 2;
    }
");

        Expect(12, 5);
        Expect(14, 6);
    }

    public void ConvertListToEnumerableOfT()
    {
        Compile(@"
function main()
{
    var xs = [1, ""2"", 3, 4.0];
    var ys = xs~Object<""System.Collections.Generic.IEnumerable`1[System.String0.3]"">;
    return ys;
}
");
        Expect(rs =>
        {
            Assert.That(rs,Is.InstanceOf<IEnumerable<string>>());
            Assert.That(((IEnumerable<string>)rs.Value).ToList(),Is.EquivalentTo(new[]{"1","2","3","4.0"}));
        });
    }
}