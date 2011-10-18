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
using Prexonite.Compiler.Macro.Commands;
using PrexoniteTests.Tests;

namespace Prx.Tests
{
    public abstract partial class VMTests : VMTestsBase
    {
        [Test]
        public void CallSubMacroCommandNested()
        {
            CompileInvalid(
                @"
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
",
                CallSub.Alias, "expression");

            //var xs = new List<PValue> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            //Expect("[ 4, 10, 16 ]", (PValue)xs);
        }

        [Test]
        public void CallSubMacroCommandTopLevel()
        {
            Compile(
                @"
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
        public void CallSubMinimal()
        {
            Compile(
                @"
function f(x)
{
    return x;
}

function main()
{
    call\sub(f(?),[2]);   
    return 2; 
}
");

            Expect(2);
        }

        [Test]
        public void CallSubMinimalReturn()
        {
            Compile(
                @"
function f()
{
    return 1;
}

function main()
{
    var zs = [];
    asm{nop nop nop}
    zs[] = call\sub(f(?));   
    asm{nop nop nop}
    return zs[0];
}
");

            Expect(1);
        }

        [Test]
        public void CallSubOfPartial()
        {
            CompileInvalid(
                @"
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
",
                CallSub.Alias, "expression");

            //var xs = new List<PValue> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            //Expect("[ 4, 10, 16 ]", (PValue)xs, 6);
            //Expect("[ 4, 10 ]", (PValue)xs, 4);
        }

        [Test]
        public void CaptureUnmentionedMacroVariable()
        {
            Compile(
                @"
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
            Assert.AreEqual(clo.Meta[PFunction.SharedNamesKey].List[0].Text,
                MacroAliases.ContextAlias);

            Expect(15, new PValue[0]);
        }

        [Test]
        public void MacroTransport()
        {
            Compile(
                @"
macro echo(lst)
{
    lst = macro\unpack(lst);
    return new Prexonite::Compiler::Ast::AstConstant(context.Invocation.File,
        context.Invocation.Line,context.Invocation.Column,sum(lst) + lst.Count); 
}

macro gen(n)
{
    n = n.Constant~Int;
    var id = macro\pack(1.to(n) >> all);
    var idConst = new Prexonite::Compiler::Ast::AstConstant(context.Invocation.File,
        context.Invocation.Line,context.Invocation.Column, id);
    return call\macro([echo(idConst)]);
}

function main()
{
    return gen(4);
}

function main2()
{
    return gen(5);
}
");

            Expect(1 + 2 + 3 + 4 + 4);
            ExpectNamed("main2", 1 + 2 + 3 + 4 + 5 + 5);
        }

        [Test]
        public void CallMacroOnFunction()
        {
            Compile(
                @"
macro __append(con)
{
    var c = con.Constant + ""__"";

    if(context.IsJustEffect)
        c += ""je"";

    if(context.Call~Int == Prexonite::Types::PCall.Set~Int)
        c += ""="";

    var con = new Prexonite::Compiler::Ast::AstConstant(context.Invocation.File,
        context.Invocation.Line,context.Invocation.Column,c);
    return con;
}

macro __surround(con, idx)
{
    var idx = idx.Constant~Int;
    con.Constant = ""__"" + con.Constant;
    return [ call\macro([__append],[con])
           , call\macro([__append(con)])
           , call\macro([__append = con])
           , call\macro([__append,true],[con])
           , call\macro([__append,false],[con])
           , call\macro([__append(con),true])
           , call\macro([__append(con),false])
           , call\macro([__append = con, true])
           , call\macro([__append = con, false]) ][idx];
}

function main(x,y)
{
    return  [ x + __surround(""xXx"", 0) + y
            , x + __surround(""xXx"", 1) + y
            , x + __surround(""xXx"", 2) + y
            , x + __surround(""xXx"", 3) + y
            , x + __surround(""xXx"", 4) + y
            , x + __surround(""xXx"", 5) + y
            , x + __surround(""xXx"", 6) + y
            , x + __surround(""xXx"", 7) + y
            , x + __surround(""xXx"", 8) + y];
}
");

            Expect(new List<PValue>
                {
                    "a__xXx__b",
                    "a__xXx__b",
                    "a__xXx__=b",
                    "a__xXx__jeb",
                    "a__xXx__b",
                    "a__xXx__jeb",
                    "a__xXx__b",
                    "a__xXx__je=b",
                    "a__xXx__=b"
                }, "a", "b");

            if (CompileToCil)
            {
                var surround = target.Functions["__surround"];
                Assert.That(surround, Is.Not.Null, "Function __surround does not exist.");
                Assert.That(surround.Meta[PFunction.VolatileKey].Switch,
                    Is.False,
                    string.Format("Function {0} is volatile. Reason: {1}", surround.Id,
                        surround.Meta[PFunction.DeficiencyKey].Text));
            }
        }


        [Test]
        public void PartialCallMacroOnFunction()
        {
            Compile(
                @"
macro __append(con)
{
    var c = con.Constant + ""__"";

    if(context.IsJustEffect)
        c += ""je"";

    if(context.Call~Int == Prexonite::Types::PCall.Set~Int)
        c += ""="";

    var con = new Prexonite::Compiler::Ast::AstConstant(context.Invocation.File,
        context.Invocation.Line,context.Invocation.Column,c);
    return con;
}

macro __surround\create_pa(con, idx)
{
    var idx = idx.Constant~Int;
    con.Constant = ""__"" + con.Constant;
    var f = [ call\macro([?],[?])                       // 20
            , call\macro([__append(?0)])
            , call\macro([__append = ?0])                // 22
            , call\macro([__append,?],[con])
            , call\macro([?,?],[?])
            , call\macro([__append(con),?])             // 25
            , call\macro([__append(?0),?])
            , call\macro([__append = ?0, ?])             // 27
            , call\macro([__append = con, ?]) ][idx];

    var fc = macro\pack(f);
    return new Prexonite::Compiler::Ast::AstConstant(context.Invocation.File,
        context.Invocation.Line,context.Invocation.Column,fc);
}

macro __surround(con, idx)
{
    var i = idx.Constant~Int;                                           //37
    var fc = call\macro([__surround\create_pa(con, idx)]).Expression;
    var f = macro\unpack(fc.Constant~Int);
    if(i == 0)
        return f.(macro\reference(__append),con);
    else if(i == 1)
        return f.(con);
    else if(i == 2)
        return f.(con);
    else if(i == 3)
        return f.(true);
    else if(i == 4)
        return f.(macro\reference(__append),false,con);
    else if(i == 5)                                                 // 50
        return f.(true);
    else if(i == 6)
        return f.(con, false);
    else if(i == 7)
        return f.(con,true);
    else if(i == 8)
        return f.(false);
    else
        throw ""ZOMG! (invalid index to macro __surround)"";
} // 60

function main(x,y)
{
    return  [ x + __surround(""xXx"", 0) + y
            , x + __surround(""xXx"", 1) + y
            , x + __surround(""xXx"", 2) + y
            , x + __surround(""xXx"", 3) + y
            , x + __surround(""xXx"", 4) + y
            , x + __surround(""xXx"", 5) + y
            , x + __surround(""xXx"", 6) + y // 70
            , x + __surround(""xXx"", 7) + y // 71
            , x + __surround(""xXx"", 8) + y]; // 72
}
");

            Expect(new List<PValue>
                {
                    "a__xXx__b",
                    "a__xXx__b",
                    "a__xXx__=b",
                    "a__xXx__jeb",
                    "a__xXx__b",
                    "a__xXx__jeb",
                    "a__xXx__b",
                    "a__xXx__je=b",
                    "a__xXx__=b"
                }, "a", "b");
        }
    }
}