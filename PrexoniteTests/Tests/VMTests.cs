// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          self list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          self list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from self software without specific prior written permission.
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
//need to change self in VMTestsBase.cs too!

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prx.Tests
{
    // ReSharper disable InconsistentNaming
    public abstract partial class VMTests
        // ReSharper restore InconsistentNaming
    {
        #region Setup

        #endregion

        [Test]
        public virtual void CilVersusInterpreted()
        {
            Compile(
                @"
function main()
{
    var interpreter_stack = asm(ldr.eng).Stack.Count;
    println(interpreter_stack);
    return interpreter_stack == 0;
}
");
            Expect(CompileToCil);
        }


        [Test]
        public void DataStructure()
        {
            Compile(
                @"
function chain(lst, serial)
{
    var c = new Structure;
    c.\(""IsSerial"") = if(serial != null) serial else true;
    c.\(""Functions"") = if(lst != null) lst else new List();
    function Invoke(self, prev)
    {
        var res = prev;
        if(self.IsSerial)
        {
            foreach(var f in self.Functions)
                prev = f.(prev);
            return prev;
        }
        else
        {
            var nlst = new List();
            foreach(var f in self.Functions)
                nlst[] = f.(prev);
            return nlst;
        }
    }

    c.\\(""Invoke"") = ->Invoke;
    c.\\(""IndirectCall"") = ->Invoke;
    return c;
}

function main(seed)
{
    function sqrt(x) = System::Math.Sqrt(x);
    ref ch = chain(~List.Create(
        x => x+2,
        x => x*2,
        x => x mod 3,
        x => (sqrt(x)*10)~Int,
        x => ""The answer is: "" + x
    ));

    return ch(seed);
}
");
            var seed = (new Random()).Next(400, 500);
            var expected = seed;
            expected = expected + 2;
            expected = expected*2;
            expected = expected%3;
            expected = (int) (Math.Sqrt(expected)*10);

            Expect("The answer is: " + expected, seed);
        }

        [Test]
        public void ListConcat()
        {
            Compile(
                @"

function foldl(ref f, var left, var lst)
{
    foreach(var x in lst)
        left = f(left, x);
    return left;   
}

function map(ref f, lst)
{
    var nlst = [];
    foreach(var x in lst) 
        nlst += f(x);
    return nlst;
}

function main(lst)
{
    var L2 = [];
    var last = null;
    foreach(var e in lst)
    {
        if(last == null)
        {
            last = e;
        }
        else
        {
            L2 += [[last, e]];
            last = null;
        }
    }
    
    function select(index) = map( pair => pair[index], L2 );
    function toString(obj) = foldl( (l,r) => l + r, """", obj);

    return toString( select(0) + select(1) );
}
");
            var lst = new List<PValue>();
            var sbo = new StringBuilder();
            var sbe = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                lst.Add(i);
                if (i%2 == 0)
                    sbe.Append(i);
                else
                    sbo.Append(i);
            }
            Expect(
                string.Concat(sbe.ToString(), sbo.ToString()),
                new[] {PType.List.CreatePValue(lst)});
        }

        [Test]
        public void CoroutineSimple()
        {
            Compile(
                @"
function main(a,b,c)
{
    ref f = coroutine (x,y,z) => 
    { 
        yield x; 
        yield y; 
        yield z;
    } for (a,b,c);

    return f + f + f;
}
");
            Expect("abc", "a", "b", "c");
        }

        [Test]
        public void CoroutineFunction()
        {
            Compile(
                @"
function subrange(lst, index, count) does
    for(var i = index; i < index+count; i++)
        yield lst[i];

function main
{
    var f = coroutine -> subrange for ( var args, 2, 3 );
    var buffer = new System::Text::StringBuilder;
    foreach(var e in f)
        buffer.Append(e);
    return buffer.ToString;
}
");

            Expect("cde", "a", "b", "c", "d", "e", "f", "g");
        }

        [Test]
        public void CoroutineComplex()
        {
            Compile(
                @"
function map(ref f, var lst) = coroutine () =>
{
    foreach(var x in lst)
        yield f(x);
};

function where(ref predicate, var lst) = coroutine()=>
{
    foreach(var x in lst)
        if(predicate(x))
            yield x;
};

function limit(n, var lst) = coroutine() =>
{
    foreach(var x in lst)
        if(n-- > 0)
            yield x;
};

function skip(n, var lst) = coroutine() =>
{
    foreach(var x in lst)
        if(n-- <= 0)
            yield x;
};

function foldl(ref f, var left, var lst)
{
    foreach(var right in lst)
        left = f(left, right);
    return left;
}

function curry(ref f) = a => b => f(a,b);

function chain() does
var args; and return lst =>
{
    foreach(ref filter in var args)
        lst = filter(lst);
    return lst;
};

function main()
{
    function toString(lst) = foldl( (l,r) => l + r, """", lst);
    ref filterChain = chain(
        curry(->skip)   .(2),
        curry(->map)    .(x => 3*x),
        curry(->where)  .(x => x mod 2 == 0),
        curry(->limit)  .(3)
    );

    return toString(filterChain(var args));
}
");
            var lst = new List<PValue>();
            var buffer = new StringBuilder();
            var nums = 0;
            for (var i = 0; i < 20; i++)
            {
                lst.Add(i);
                if (i < 2)
                    continue;
                if (i*3%2 != 0)
                    continue;
                if (nums > 2)
                    continue;
                buffer.Append(i*3);
                nums++;
            }
            Expect(buffer.ToString(), lst.ToArray());
        }


        /// <summary>
        ///     Makes sure that coroutines do not return an unnecessary null at the end.
        /// </summary>
        [Test]
        public void CoroutineNoNull()
        {
            Compile(
                @"
function main()
{
    var c = coroutine() => 
    {
        yield 0;
        yield 1;
        yield 2;
        yield 3;
    };

    var buffer = new System::Text::StringBuilder;
    foreach(var ce in c)
    {
        buffer.Append(""=>"");
        buffer.Append(ce);
        buffer.Append(""\n"");
    }

    return buffer.ToString;
}
");
            Expect("=>0\n=>1\n=>2\n=>3\n");
        }

        [Test]
        public void CoroutineRecursive()
        {
            Compile(
                @"
coroutine unfolded(lst)
{
    foreach(var x in lst)
        if(x is List || x is ::Prexonite::$Coroutine)
            foreach(var y in unfolded(x))
                yield y;
        else
            yield x;
}

function main()
{
    var args;
    var buffer = new System::Text::StringBuilder();
    foreach(var a in unfolded(args))
    {
        buffer.Append(a);
        buffer.Append(""."");
    }
    if(buffer.Length > 0)
        buffer.Length -= 1;
    return buffer.ToString;
}
");

            var args = new List<PValue>();
            var sub1 = new List<PValue>();
            var sub2 = new List<PValue>();
            var sub21 = new List<PValue>();

            args.Add(1);
            args.Add(2);
            args.Add(PType.List.CreatePValue(sub1));
            sub1.Add(3);
            sub1.Add(4);
            sub1.Add(5);
            args.Add(6);
            args.Add(PType.List.CreatePValue(sub2));
            sub2.Add(7);
            sub2.Add(PType.List.CreatePValue(sub21));
            sub21.Add(8);
            sub21.Add(9);
            sub21.Add(10);
            sub2.Add(11);
            sub2.Add(12);
            args.Add(13);
            args.Add(14);

            Expect("1.2.3.4.5.6.7.8.9.10.11.12.13.14", PType.List.CreatePValue(args));
        }

        [Test]
        public void CoroutineFib()
        {
            Compile(
                @"
var numbers = [];

declare function fib;

ref nextfib = coroutine() =>
{
    yield 1;
    yield 1;
    for(var i = 3; true; i++)
        yield fib(i-1) + fib(i-2);
};

function fib(n)
{
    while(numbers.Count < n)
        numbers[] = nextfib;

    return numbers[n-1];
}
");

            ExpectNamed("fib", _fibonacci(6), 6);
        }

        [Test]
        public void Hashes()
        {
            Compile(
                @"
function mapToHash(ref f, xs)
{
    var h = {};
    foreach(var x in xs)
        h[] = x: f(x);
    return h;
}

coroutine reader(xs) does
    foreach(var x in xs)
        yield x;

function main()
{
    var h = mapToHash( x => (x+1)*2, var args);
    ref keys = reader(h.Keys);
    ref values = reader(h.Values);

    var diff = 0;
    for(var i = 0; i < h.Count; i++)
        diff += values - keys;
    return diff;
}
");
            var xs =
                new PValue[]
                    {
                        2, //4
                        3, //5
                        8, //10
                    };
            Expect(4 + 5 + 10, xs);
        }

        [Test]
        public void NestedFunctionCrossReference()
        {
            Compile(
                @"
function main()
{
    function A(xa)
    {
        return ""x"" + xa + ""x"";
    }

    function B(xb)
    {
        return ""b$(xb).$(->A)b"";
    }

    return B(var args[0]);
}
");

            //#if UseCil
            //            Expect("bs.CilClosure(function main\\A0( xa))b", "s");
            //#else
            //            Expect("bs.Closure(function main\\A0( xa))b", "s");
            //#endif
            Expect("bs.function main\\A0(xa)b", "s");
        }


        [Test]
        public void StructureToString()
        {
            Compile(
                @"
function main(x)
{
    var s = new Structure;
    s.\(""value"") = x;
    s.\\(""ToString"") = self => self.value;
    return s~String;
}
");
            Expect("xzzxy", "xzzxy");
        }

        [Test]
        public void GlobalCode()
        {
            Compile(
                @"
var price = {};

{
    price[""apple""] = 3;
    price[""juice""] = 4;
    price[""pencil""] = 1;
}

//In a different file for example
{
    price[""apple""] *= 2;
}

function main(var lst)
{
    var sum = 0;
    foreach(var item in lst)
        sum += price[item.Key] * item.Value;
    return sum;
}
");
            var lst = new List<PValue>(4)
                {
                    new PValueKeyValuePair("apple", 1),
                    new PValueKeyValuePair("pencil", 5),
                    new PValueKeyValuePair("juice", 2),
                    new PValueKeyValuePair("apple", 2)
                };

            Expect(3*3*2 + 5*1 + 2*4, PType.List.CreatePValue(lst));
        }

        [Test]
        public void CoalescenceOperator()
        {
            Compile(
                @"
coroutine fetch(xs) does 
    foreach(var x in xs)
        yield x;

coroutine blit(xs, ys) does
    ref nextY = fetch(ys); and
    foreach(var x in xs)
        var y = nextY; and
        yield x ?? y ?? ""?"";

function main()
{
    var xs = [6,null,4,null,null,1];
    var ys = [1,2   ,3,4   ,null,6];
    return foldl((l,r) => l + ""."" + r, """", blit(xs,ys));
}        
");

            Expect(".6.2.4.4.?.1");
        }

        [Test]
        public void LeftAppendArgument()
        {
            Compile(
                @"
coroutine where(ref f, xs) does foreach(var x in xs)
    if(f(x))
        yield x;

coroutine limit(max, xs) does
    var i = 0; and
    foreach(var x in xs)
        if(i++ >= max)
            break;
        else
            yield x;

coroutine skip(cnt, xs) does
    var i = 0; and
    foreach(var x in xs)
        if(i++ >= cnt)
            yield x;

coroutine map(ref f, xs) does
    foreach(var x in xs)
        yield f(x);

function main(sep) = foldl( (l,r) => $l + "" "" + $r, ""BEGIN"")
    << limit(3) << map( x => x.Length + sep + x ) << where( x => x.Length >= 3 ) << skip(1) << var args;
");

            Expect("BEGIN 3:abc 5:hello 3:123", ":", "ab", "abc", "hello", "12", "123", "8965");
        }

        [Test]
        public void RightAppendArgument()
        {
            Compile(
                @"
coroutine where(ref f, xs) does foreach(var x in xs)
    if(f(x))
        yield x;

coroutine limit(max, xs) does
    var i = 0; and
    foreach(var x in xs)
        if(i++ >= max)
            break;
        else
            yield x;

coroutine skip(cnt, xs) does
    var i = 0; and
    foreach(var x in xs)
        if(i++ >= cnt)
            yield x;

coroutine map(ref f, xs) does
    foreach(var x in xs)
        yield f(x);

function main(sep) = 
    var args >> 
    skip(1) >> 
    where( x => x.Length >= 3 ) >> 
    map( x => x.Length + sep + x ) >> 
    limit(3) >>
    foldl( (l,r) => $l + "" "" + $r, ""BEGIN"");
");

            Expect("BEGIN 3:abc 5:hello 3:123", ":", "ab", "abc", "hello", "12", "123", "8965");
        }

        [Test]
        public void InitializationCodeHook()
        {
            Compile(
                @"
Imports { System, Prexonite, Prexonite::Types, Prexonite::Compiler, Prexonite::Compiler::Ast };

function ast(type) [is compiler;]
{
    var args;
    var targs = [];
    for(var i = 1; i < args.Count; i++)
        targs[] = args[i];

    return 
        asm(ldr.eng)
        .CreatePType(""Object(\""Prexonite.Compiler.Ast.Ast$(type)\"")"")
        .Construct((["""",-1,-1]+targs)~Object<""Prexonite.PValue[]"">)
        .self;
}

var SI [is compiler;] = null;
build {
    SI = new Structure;
    SI.\(""var"") = ::SymbolInterpretations.LocalObjectVariable;
    SI.\(""ref"") = ::SymbolInterpretations.LocalReferenceVariable;
    SI.\(""gvar"") = ::SymbolInterpretations.GlobalObjectVariable;
    SI.\(""gref"") = ::SymbolInterpretations.GlobalReferenceVariable;
    SI.\(""func"") = ::SymbolInterpretations.$Function;
    SI.\(""cmd"") = ::SymbolInterpretations.$Command;
    SI.\\(""eq"") = (self, l, r) => l~Int == r~Int;
    SI.\\(""is_lvar"") = (self, s) => s~Int == self.$var~Int;
    SI.\\(""is_lref"") = (self, s) => s~Int == self.$ref~Int;
    SI.\\(""is_gvar"") = (self, s) => s~Int == self.$gvar~Int;
    SI.\\(""is_gref"") = (self, s) => s~Int == self.$gref~Int;
    SI.\\(""is_func"") = (self, s) => s~Int == self.$func~Int;
    SI.\\(""is_cmd"") = (self, s) => s~Int == self.$cmd~Int;
    SI.\\(""is_obj"") = (self, s) => self.is_lvar(s) || self.is_gvar(s);
    SI.\\(""is_ref"") = (self, s) => self.is_lref(s) || self.is_gref(s);
    SI.\\(""is_global"") = (self, s) => self.is_gvar(s) || self.is_gref(s);
    SI.\\(""is_local"") = (self, s) => self.is_lvar(s) || self.is_lref(s);
    SI.\\(""make_global"") = (self, s) => 
        if(self.is_obj(s))
            self.gvar
        else if(self.is_ref(s))
            self.gref
        else
            throw ""$s cannot be made global."";            
    SI.\\(""make_local"") = (self, s) => 
        if(self.is_obj(s))
            self.lvar
        else if(self.is_ref(s))
            self.lref
        else
            throw ""$s cannot be made local."";
    SI.\\(""make_obj"") = (self, s) =>
        if(self.is_local(s))
            self.lvar
        else if(self.is_global(s))
            self.gvar
        else
            throw ""$s cannot be made object."";
    SI.\\(""make_ref"") = (self, s) =>
        if(self.is_local(s))
            self.lref
        else if(self.is_global(s))
            self.gref
        else
            throw ""$s cannot be made reference."";
}

build does hook(t => 
{
    //Promote local to global variables
    var init = t.$Function;

    if(init.Id != Prexonite::Application.InitializationId)
        return;

    var alreadyPromoted = [];
    foreach(var entry in init.Meta[""alreadyPromoted""].List)
        alreadyPromoted[] = entry.Text;
    
    
    var toPromote = [];
    foreach(var loc in init.Variables)
        unless(alreadyPromoted.Contains(loc))
            toPromote[] = loc;
    
    foreach(var loc in toPromote)
    {
        if(not t.Symbols.TryGet(loc~String,->loc))
            throw ""Cannot find $loc in hook."";
        loc =  Prexonite::Compiler::Symbolic::Compatibility::LegacyExtensions.ToSymbolEntry(loc);
        var glob = new ::SymbolEntry(SI.make_global(loc.Interpretation), loc.InternalId, asm(ldr.app).Module.Name);
        var ss = Prexonite::Compiler::Symbolic::Compatibility::LegacyExtensions.ToSymbol(glob);
        t.Loader.Symbols.Declare(loc.InternalId, ss);
        t.Loader.Options.TargetApplication.Variables[loc.InternalId] = new ::PVariable(loc.InternalId);
        var assignment = ast(""GetSetSymbol"", ::PCall.Set, glob);
        assignment.Arguments.Add(ast(""GetSetSymbol"", ::PCall.Get, loc));
        t.Ast.Add(assignment);        
        println(""Declared $glob"");
    }

    init.Meta[""alreadyPromoted""].AddToList(::MetaEntry.CreateArray(toPromote));
});

{
    var goo = 600;
}

{
    var goo2 = 780;
}

function main()
{
    return ""goo = $goo; goo2 = $goo2;"";
}
");
            Expect("goo = 600; goo2 = 780;");
        }

        [Test]
        public void CastAssign()
        {
            Compile(
                @"
function main(a,b,c)
{
    a~=Int;
    b ~ = Bool;
    c ~= String;

    return ((a+10)/5) + (if(b) c*a else c);
}
");

            const double a = 2.9;
            const int b = 27;
            const bool c = true;

            Expect("2TrueTrueTrue", a, b, c);
        }

        [Test]
        public void StoreBasic()
        {
            CompileStore(
                @"
function main(a,b,c)
{
    //string int bool
    var x = a.Substring(2);
    var y = b*5;
    var z = if(c) 
                ""x""   
            else 
                -1;    
    return ""$(x)$(y)$(z)"";
}
");

            Expect("cd50x", "abcd", 10, true);
        }

#if useIndex && false

        [Test]
        public void Index_Nested()
        {
            _compile(@"
function main(a,b)
{
    var s = ""+$a+"";
    
    function text(nt)
    {
        if(Not nt is Null)
            s = nt;
        return s;
    }

    function ToString
}
");
        }

#endif

        [Test]
        public void RotateIns()
        {
            Compile(
                @"
function main(a)
{   
    var s = new Structure;
    return s.\(""text"") = a;
}
");
            Expect("ham", "ham");
        }

        private static int _fac(int n)
        {
            var r = 1;
            while (n > 1)
                r *= n--;
            return r;
        }

        [Test]
        public void DirectTailRecursion()
        {
            Compile(
                @"
function fac n r =
    if(n == 1)
        r
    else
        fac(n-1, n*r);
");

            ExpectNamed("fac", _fac(6), 6, 1);
        }

        [Test]
        public void IsNotSyntax()
        {
            Compile(
                @"
function main(a,b)
{
    if(a is not String)
        return a;
    else
        return b;
}
");

            Expect(125, 125, "s-b-s");
            Expect(125.0, 125.0, "s-b-s");
            Expect(true, true, "s-b-s");
        }


        [Test]
        public void SuperFastPrintLn()
        {
            //Covers #10
            Compile(@"
function main = println;
");

            Expect("");
        }

        [Test]
        public void UseFunctionMacro()
        {
            Compile(@"
macro nothing = null;

function main does return nothing;
");

            ExpectNull();
        }

        [Test]
        public void MacroTemporaryAllocateFree()
        {
            Compile(
                @"
macro acquire_free()
{
    var v = context.AllocateTemporaryVariable;
    var node = new Prexonite::Compiler::Ast::AstConstant(""none"",-1,-1,v);
    context.FreeTemporaryVariable(v);
    return node;
}

function main = acquire_free;
");

            var mainFunc = target.Functions["main"];

            Assert.AreEqual(1, mainFunc.Variables.Count);
            var v = mainFunc.Variables.First();
            Expect(v);
        }

        [Test]
        public void ConstantFoldingReferenceEquality()
        {
            Compile(
                @"
function interpreted [is volatile;] = System::Object.ReferenceEquals(""ab"", ""a"" + ""b"");
function compiled [is volatile;] = System::Object.ReferenceEquals(""ab"", ""a"" + ""b"");
");

            ExpectNamed("interpreted", true, new PValue[0]);
            ExpectNamed("compiled", true, new PValue[0]);
        }

        [Test]
        public void UnlessConditionalExpression()
        {
            Compile(@"
function main()
{
    return unless(true) 1 else 2;
}
");

            Expect(2, new PValue[0]);
        }

        [Test]
        public void BuildBlockDoesNotTriggerInitialization()
        {
            Compile(
                @"
var flag = true;

function write does print(var args);

build does write(""nothing"");
");

            Assert.IsNull(target.Variables["flag"].Value.Value);
        }

        [Test]
        public void NestedVariableShadowing()
        {
            Compile(
                @"
function main(x,y)
{
    var a = x;
    function innerShadow
    {
        new var a = y; //variable is new-declared, it should not capture the outer variable
        return a;
    }
    function innerCapture
    {
        a = y;
        return a;
    }

    var t1 = a;
    var k1 = innerShadow;
    var t2 = a;
    var k2 = innerCapture;
    var t3 = a;

    return ""$t1,$k1; $t2,$k2; $t3"";
}
");

            Expect("x,y; x,y; y", "x", "y");
        }

        [Test]
        public void DeclareNewVarTopLevel()
        {
            Compile(
                @"
function main()
{
    var buffer = new System::Text::StringBuilder;
    function print(s) does buffer.Append(s);
    new var xs = [ 5,7,9,11,13,15 ];
    var fs = [];
    foreach(var x in xs)
    {
        fs[] = y => ""($(x)->$(y))"";
        print(""$(new var x)."");
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
        public void ObjectCreationFallback()
        {
            Compile(
                @"
declare function make_foo as create_foo;

function main(x,y)
{
    var a = new foo(x);
    function create_bar(z) = ""bar($z)"";
    var b = new bar(y);
    return a + b;
}

function make_foo(z) = ""foo($z)"";
");

            Expect("foo(x)bar(y)", "x", "y");
        }

        [Test] //#19
        public void ObjectIdentity()
        {
            Compile(@"
function eq(x) = x == x;
function neq(x) = x != x;
");

            ExpectNamed("eq", true, new PValue(new object(), PType.Object[typeof (object)]));
            ExpectNamed("neq", false, new PValue(new object(), PType.Object[typeof (object)]));
        }

        [Test] //#18
        public void HexEscapeSequences()
        {
            Compile(
                @"
function main = ""\x20\x21\x9\x0020\x020\xAAAA\uABCD\U0000ABCD"".ToCharArray() >> map(x => x~Int) >> all;
");

            Expect(new List<PValue> {0x20, 0x21, 0x9, 0x0020, 0x020, 0xAAAA, 0xABCD, 0x0000ABCD});
        }

        [Test]
        public void InnerFunctionNamespaceImport()
        {
            Compile(
                @"
Import {
    System,
    Prexonite
};

function main(x)[ Add Prexonite::Types to Import; ]
{
    function inner  k => k is ::PValueKeyValuePair;
    ref lambda =    k => k is ::PValueKeyValuePair;
    return inner(x) and lambda(x);
}
");

            Expect(true, new PValueKeyValuePair(1, 2));
            Expect(false, 1);
        }

        [Test]
        public void ConditionalExpressionVsKvpPriority()
        {
            Compile(@"
function main(x,y,z) = if(x) x else y:z;
");

            Expect(true, true, 1, 2);
        }

        [Test]
        public void KvpSelfPriority()
        {
            Compile(@"
function main(x,y,z) = (x : y : z).Key;
");

            Expect(1, 1, 2, 3);
        }

        private class Callable : IIndirectCall
        {
            private readonly Func<StackContext, PValue[], PValue> _impl;

            public Callable(Func<StackContext, PValue[], PValue> impl)
            {
                if (impl == null)
                    throw new ArgumentNullException("impl");
                _impl = impl;
            }

            #region Implementation of IIndirectCall

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                return _impl(sctx, args);
            }

            #endregion
        }

        [Test]
        public void ReturnModes()
        {
            Compile(
                @"
function ret_exit()
{
    return 5;
}

function ret_yield()
{
    yield 6;
}

function ret_continue()
{
    continue;
}

function ret_break()
{
    break;
}
");

            _testReturnMode("ret_exit", ReturnMode.Exit, 5);
            _testReturnMode("ret_yield", ReturnMode.Continue, 6);
            _testReturnMode("ret_continue", ReturnMode.Continue, PType.Null);
            _testReturnMode("ret_break", ReturnMode.Break, PType.Null);
        }

        private void _testReturnMode(string id, ReturnMode mode, PValue retVal)
        {
            var fctx = target.Functions[id].CreateFunctionContext(engine);
            engine.Process(fctx);
            Assert.AreEqual(fctx.ReturnMode, mode,
                "Return mode for function " + id + " does not match.");
            Assert.IsTrue((bool) retVal.Equality(fctx, fctx.ReturnValue).Value,
                "Return value for function " + id + " does not match.");
        }

        [Test]
        public void FunctionCompositionSyntax()
        {
            Compile(
                @"
function closed(x,y) 
{   
    var f = x then y;
    return f.(null);
}

function partialLeft(x,y) 
{   
    var f = (? then y);
    f = f.(x);
    return f.(null);
}

function partialRight(x,y) 
{   
    var f = (x then ?);
    f = f.(y);
    return f.(null);
}

function partialFull(x,y) 
{   
    var f = (? then ?);
    f = f.(x,y);
    return f.(null);
}

function chainedPrio(x,y,z) 
{   
    var f = x then y then z;
    return f.(null);
}
");

            var x =
                sctx.CreateNativePValue(
                    new Callable(
                        (stackContext, args) => "x" + args[0].CallToString(stackContext) + "x"));
            var y =
                sctx.CreateNativePValue(
                    new Callable(
                        (stackContext, args) => "y" + args[0].CallToString(stackContext) + "y"));
            var z =
                sctx.CreateNativePValue(
                    new Callable(
                        (stackContext, args) => "z" + args[0].CallToString(stackContext) + "z"));

            ExpectNamed("closed", "yxxy", x, y);
            ExpectNamed("partialLeft", "yxxy", x, y);
            ExpectNamed("partialRight", "yxxy", x, y);
            ExpectNamed("partialFull", "yxxy", x, y);
            ExpectNamed("chainedPrio", "zyxxyz", x, y, z);
        }

        [Test]
        public void PartialInitialization2()
        {
            var ldr = Compile(@"
var x = 5;

function main(y)
{
    return y + x;
}
");

            Expect(11, 6);

            Compile(ldr,
                @"
var x = 17;
var z = 9;

function main2(x)
{
    return z + main(x);
}
");

            ExpectNamed("main2", 20 + 9, 3);

            Compile(ldr, @"
var x = 22;
var z = 20;
");

            ExpectNamed("main2", 20 + 22 + 4, 4);
        }

        [Test]
        public void ArgsFallback()
        {
            CompileInvalid(
                @"
function main(args)
{
    foreach(var arg in var args)
        args += arg;

    return args;
}
",
                "main", PFunction.ArgumentListId, "0", "local");
        }

        [Test]
        public void ParamDefaultNull()
        {
            Compile(@"
function main(x,y)
{
    return y;
}
");

            ExpectNull("main", "z");
        }

        [Test]
        public void VariableDefaultNull()
        {
            Compile(@"
function main(x)
{
    var y;
    return y;
}");

            ExpectNull("main", "z");
        }

        [Test]
        public void LocalRef()
        {
            Compile(
                @"
function interpolate(x,y,t, ref result)
{
    if(y < x)
        interpolate(y,x,t,->result);
    else
        result = x+(y-x)*t;
}

function main(x,t)
{
    var y = x*1.5;
    interpolate(y,x,t,->y);
    return y;
}
");

            var x = 22.5;
            var y = x*1.5;
            var t = 0.75;

            Expect((x + (y - x)*t), x, t);
        }

        [Test]
        public void GlobalRef()
        {
            Assert.That(Runtime.WrapPVariableMethod, Is.Not.Null);
            Assert.That(Runtime.LoadGlobalVariableReferenceAsPValueMethod, Is.Not.Null);

            Compile(
                @"
var result;

function interpolate(x,y,t, ref result)
{
    if(y < x)
        interpolate(y,x,t,->result);
    else
        result = x+(y-x)*t;
}

function main(x,t)
{
    var y = x*1.5;
    interpolate(y,x,t,result = ?);
    return result;
}
");

            var x = 22.5;
            var y = x*1.5;
            var t = 0.75;

            Expect((x + (y - x)*t), x, t);
        }

        [Test]
        public void RealArithmetic()
        {
            Compile(
                @"function main(x)
{
    var y = x * 2.5;
    var z = y / 1.4;
    var a = z^y;
    return a;
}");

            var x = Math.PI;
            var y = x*2.5;
            var z = y/1.4;
            var a = Math.Pow(z, y);
            Expect(a, x);
        }

        [Test]
        public void AsmLdrApp()
        {
            Compile(
                @"
function foo(x) = 2*x;
function main(x)
{
    return asm(ldr.app).Functions[""foo""].(x);
}");
            Expect(4, 2);
        }

        [Test]
        public void DynamicTypeIsArray()
        {
            Compile(@"
function main(x,type)
{
    return x is Object<(type + ""[]"")>;
}
");
            Expect(false, 4, "System.String");
            Expect(true, sctx.CreateNativePValue(new[] {1, 2, 3}), "System.Int32");
        }

        [Test]
        public void PostIncDecGlobal()
        {
            Compile(
                @"
var i = 0;
function main(x)
{
    if(x mod 2 == 0)
        return i++;
    else
        return i--;
}
");
            var i = 0;
            Expect(i++, 2);
            Expect(i++, 4);
            Expect(i--, 3);
            Expect(i++, -2);
        }

        [Test]
        public void PreIncDecGlobal()
        {
            Compile(
                @"
var i = 0;
function main(x)
{
    if(x mod 2 == 0)
        return ++i;
    else
        return --i;
}
");
            var i = 0;
            Expect(++i, 2);
            Expect(++i, 4);
            Expect(--i, 3);
            Expect(++i, 4);
        }

        [Test]
        public void StaticSet()
        {
            engine.RegisterAssembly(typeof (StaticClassMock).Assembly);

            Compile(
                @"
function main(y,x)
[ Add Prx::Tests to Import; ]
{
    for(var i = 0; i < y; i++)
    {
        ::StaticClassMock.SomeProperty = x;
    }
    if(::StaticClassMock.SomeProperty is not null)
        return ::StaticClassMock.SomeProperty;
    else
        return 5;
}
");
            var x = "500";
            Expect(x, 10, x);
        }

        [Test]
        public void BitwiseOperators()
        {
            Compile(
                @"
function main(x,y,z)
{
    var a = x | y | z;
    var b = x & y;
    var c = y & z;
    var d = x & y & z;
    var e = x xor y;
    var f = x & y | z;
    var g = x | y & z;
    return [a,b,c,d,e,f,g];
}
");

            var x = 27;
            var y = 0x113;
            var z = 0x0FFFA;

            var a = x | y | z;
            var b = x & y;
            var c = y & z;
            var d = x & y & z;
            var e = x ^ y;
            var f = x & y | z;
            var g = x | y & z;

            Expect(new List<PValue> {a, b, c, d, e, f, g}, x, y, z);
        }


        [Test]
        public void LazyAndOptimization()
        {
            Compile(
                @"
var x = true;
var y = false;

function main(z)
{
    var a = x and 1;
    var b = x and 0;
    var c = true and 1;
    var d = true and 0;
    var e = y and 1;
    var f = y and 0;
    var g = false and 1;
    var h = false and 0;
    var i = x and z;
    var j = y and z;
    var k = true and z;
    var l = false and z;
    
    var s = ""$(a)$(b)$(c)$(d)$(e)$(f)$(g)$(h)$(i)$(j)$(k)$(l)"";

    foreach(var p in [a,b,c,d,e,f,g,h,i,j,k,l])
        if(p is not Bool)
        {
            s += "" Detected non-Bool value"";
            break;
        }

    return s;
}");

            const string prefix = "TrueFalseTrueFalseFalseFalseFalseFalse";
            const string valueEqTrue = "TrueFalseTrueFalse";
            const string valueEqFalse = "FalseFalseFalseFalse";

            Expect(prefix + valueEqTrue, true);
            Expect(prefix + valueEqFalse, false);
            Expect(prefix + valueEqTrue, 6);
            Expect(prefix + valueEqFalse, 0);
            Expect(prefix + valueEqFalse, PType.Null);
        }

        /// <summary>
        ///     This test checks whether optimizations of lazy logical expression 
        ///     can alter semantics (e.g., conversion to bool still necessary)
        /// </summary>
        [Test]
        public void LazyOrOptimization()
        {
            Compile(
                @"
var x = true;
var y = false;

function main(z)
{
    var a = x or 1;
    var b = x or 0;
    var c = true or 1;
    var d = true or 0;
    var e = y or 1;
    var f = y or 0;
    var g = false or 1;
    var h = false or 0;
    var i = x or z;
    var j = y or z;
    var k = true or z;
    var l = false or z;
    
    var s = ""$(a)$(b)$(c)$(d)$(e)$(f)$(g)$(h)$(i)$(j)$(k)$(l)"";

    foreach(var p in [a,b,c,d,e,f,g,h,i,j,k,l])
        if(p is not Bool)
        {
            s += "" Detected non-Bool value"";
            break;
        }

    return s;
}");

            const string prefix = "TrueTrueTrueTrueTrueFalseTrueFalse";
            const string valueEqTrue = "TrueTrueTrueTrue";
            const string valueEqFalse = "TrueFalseTrueFalse";

            Expect(prefix + valueEqTrue, true);
            Expect(prefix + valueEqFalse, false);
            Expect(prefix + valueEqTrue, 6);
            Expect(prefix + valueEqFalse, 0);
            Expect(prefix + valueEqFalse, PType.Null);
        }

        [Test]
        public void StringEscapeCollision()
        {
            Compile(
                @"
function main(s)
{
    var es = s.Escape;
    var ues = es.Unescape;
    return ""$s:$ues:$es"";
}
");

            //‰ = U+00E4

            //Simple
            _expectRoundtrip("X‰x", "X\\xE4x");

            //Collision
            _expectRoundtrip("A‰E0", "A\\u00E4E0");
        }

        private void _expectRoundtrip(string text, string escaped)
        {
            Expect(string.Format("{0}:{0}:{1}", text, escaped), text);
        }

        [Test]
        public void NullStringEscapeSequence()
        {
            Compile(
                @"
function main(x,y)
{
    var z = x;
    var z\ = y;
    var z\t = z\;

    return ""$z\&_$z\t;$z&:"" + ""\&"".Length;
}

function main_vs(x,y)
{
    var z = x;
    var z\ = y;
    var z\t = z\;

    return @""$z\&_$z\t;$z&:"" + ""\&"".Length;
}

function unharmed(x,y)
{
    var z\ = x == y;
    return z\&&true;
}
");

            const string expected = "A_B;A&:0";
            const string x = "A";
            const string y = "B";
            Expect(expected, x, y);
            ExpectNamed("main_vs", expected, x, y);
            ExpectNamed("unharmed", true, x, x);
            ExpectNamed("unharmed", false, x, y);
        }

        [Test]
        public void SingleQuotes()
        {
            Compile(
                @"
function al'gebra_f(x'') = x'' + 6'000'';

function main(x,x')
{
    var al'gebra = al'gebra_f(x');
    return ""$x $al'gebra $x':"" + 54'08.9;
}
");

            Expect("A 7000 1000:5408.9", "A", 1000);
        }

        [Test]
        public void ObjectCreationOptimizeReorder()
        {
            engine.RegisterAssembly(typeof (StaticClassMock).Assembly);
            Compile(
                @"
function main()
{
    var x = ""xXx"";
    var obj = new Prx::Tests::ConstructEcho(-1,x);
    println(obj);
    return obj.ToString;
}
");

            Expect("-1-xXx");
        }

        [Test]
        public void DuplicatingJustEffectBlockExpression()
        {
            var ldr =
                Compile(@"
var s;
function main()[is volatile;]
{
    s = ""BEGIN--"";
}
");
            var pos = new SourcePosition("file", -1, -2);
            var mn = ldr.ParentApplication.Module.Name;
            var ct = ldr.FunctionTargets["main"];
            ct.Function.Code.RemoveAt(ct.Function.Code.Count - 1);
            var block = new AstScopedBlock(new SourcePosition("file", -1, -2),ct.Ast);

            var assignStmt = ct.Factory.Call(pos, EntityRef.Variable.Global.Create("s",mn),PCall.Set);
            assignStmt.Arguments.Add(new AstConstant("file", -1, -2, "stmt."));
            var incStmt = ct.Factory.ModifyingAssignment(NoSourcePosition.Instance,
                                                         assignStmt,
                                                         BinaryOperator.Addition);

            var assignExpr = ct.Factory.Call(pos, EntityRef.Variable.Global.Create("s", mn), PCall.Set);
            assignExpr.Arguments.Add(new AstConstant("file", -1, -2, "expr."));
            var incExpr = ct.Factory.ModifyingAssignment(pos, assignExpr, BinaryOperator.Addition);

            block.Statements.Add(incStmt);
            block.Expression = incExpr;

            Assert.That(block,Is.InstanceOf<AstExpr>(),string.Format("{0} is expected to handle emission of effect code.", block));
            block.EmitEffectCode(ct);

            var sourcePosition = new SourcePosition("file", -1, -2);
            ct.EmitLoadGlobal(sourcePosition, "s", null);
            ct.Emit(sourcePosition, OpCode.ret_value);

            if (CompileToCil)
                Prexonite.Compiler.Cil.Compiler.Compile(ldr, target, StaticLinking);

            Expect("BEGIN--stmt.expr.");
        }

        #region Helper

        #endregion
    }
}

namespace Prx.Tests
{
    public static class StaticClassMock
    {
        public static string SomeProperty { get; set; }
    }

    public class ConstructEcho
    {
        public int Index { get; set; }
        public string X { get; set; }

        public ConstructEcho(int index, string x)
        {
            Index = index;
            X = x;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Index, X);
        }
    }
}