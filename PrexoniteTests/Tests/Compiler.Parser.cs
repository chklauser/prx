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

using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Types;
using Prx.Tests;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class CompilerParser : Compiler
    {
        [Test]
        public void VariableDeclarations()
        {
            const string input1 =
                @"
function func0
{
    var obj0;
    var obj1;
    var obj2;

    ref func1;
    ref ifunc0;
    ref cor0;

    declare var gobj0;    
    declare var gobj1;
    
    static sobj0;
    static sobj1;
}
";
            var opt = new LoaderOptions(engine, target) {UseIndicesLocally = false};
            var ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            foreach (var message in ldr.Errors)
            {
                TestContext.WriteLine(message);
            }
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            // We allow up to two instructions (`return sobj1`) to accomodate the
            //  relaxed treatment of variable declarations in Prexonite 2
            Assert.That(ldr.FunctionTargets["func0"].Code.Count, Is.LessThanOrEqualTo(2));

            var tar = ldr.FunctionTargets["func0"];

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj0", null),
                LookupSymbolEntry(tar.Symbols, "obj0"));
            Assert.IsTrue(tar.Function.Variables.Contains("obj0"));
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj1", null),
                LookupSymbolEntry(tar.Symbols, "obj1"));
            Assert.IsTrue(tar.Function.Variables.Contains("obj1"));
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj2", null),
                LookupSymbolEntry(tar.Symbols, "obj2"));
            Assert.IsTrue(tar.Function.Variables.Contains("obj2"));

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "func1", null),
                LookupSymbolEntry(tar.Symbols, "func1"));
            Assert.IsTrue(tar.Function.Variables.Contains("func1"));
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "ifunc0", null),
                LookupSymbolEntry(tar.Symbols, "ifunc0"));
            Assert.IsTrue(tar.Function.Variables.Contains("ifunc0"));
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "cor0", null),
                LookupSymbolEntry(tar.Symbols, "cor0"));
            Assert.IsTrue(tar.Function.Variables.Contains("cor0"));

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "gobj0", target.Module.Name),
                LookupSymbolEntry(tar.Symbols, "gobj0"));
            Assert.IsFalse(
                tar.Function.Variables.Contains("gobj0"),
                "\"declare var <id>;\" only declares a global variable.");
            Assert.IsFalse(
                ldr.Options.TargetApplication.Variables.ContainsKey("gobj0"),
                "\"global <id>;\" only declares a global variable.");
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "gobj1", target.Module.Name),
                LookupSymbolEntry(tar.Symbols, "gobj1"));
            Assert.IsFalse(
                tar.Function.Variables.Contains("gobj1"),
                "\"declare var <id>;\" only declares a global variable.");
            Assert.IsFalse(
                ldr.Options.TargetApplication.Variables.ContainsKey("gobj1"),
                "\"declare var <id>;\" only declares a global variable.");

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "func0\\static\\sobj0", target.Module.Name),
                LookupSymbolEntry(tar.Symbols, "sobj0"));
            Assert.IsTrue(
                ldr.Options.TargetApplication.Variables.ContainsKey("func0\\static\\sobj0"));
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "func0\\static\\sobj1", target.Module.Name),
                LookupSymbolEntry(tar.Symbols, "sobj1"));
            Assert.IsTrue(
                ldr.Options.TargetApplication.Variables.ContainsKey("func0\\static\\sobj1"));
        }

        [Test]
        public void ExplicitGotos()
        {
            const string input1 =
                @"
declare function instruction;

function func0
{
begin:  instruction;        //1
        goto    fifth;      //2
third : instruction;        //3
fourth: goto    sixth;      //4
fifth:  goto    fourth;     //5
sixth:  goto    begin;      //6
}

function instruction {}
";
            var opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            var ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            //Check AST
            var block = ldr.FunctionTargets["func0"].Ast;

            //label begin
            var i = 0;
            Assert.IsInstanceOf(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("begin", ((AstExplicitLabel) block[i]).Label);

            //instruction
            i++;
            Assert.IsInstanceOf(typeof(AstIndirectCall), block[i]);
            Assert.AreEqual(PCall.Get, ((AstGetSet) block[i]).Call);
            Assert.AreEqual(0, ((AstGetSet) block[i]).Arguments.Count);

            //goto fith
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("fifth", ((AstExplicitGoTo) block[i]).Destination);

            //label third
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("third", ((AstExplicitLabel) block[i]).Label);

            //instruction
            i++;
            Assert.IsInstanceOf(typeof(AstIndirectCall), block[i]);
            Assert.AreEqual(PCall.Get, ((AstGetSet)block[i]).Call);
            Assert.AreEqual(0, ((AstGetSet)block[i]).Arguments.Count);

            //label fourth
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("fourth", ((AstExplicitLabel) block[i]).Label);

            //goto sixth
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("sixth", ((AstExplicitGoTo) block[i]).Destination);

            //label fifth
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("fifth", ((AstExplicitLabel) block[i]).Label);

            //goto fourth
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("fourth", ((AstExplicitGoTo) block[i]).Destination);

            //label sixth
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("sixth", ((AstExplicitLabel) block[i]).Label);

            //goto begin
            i++;
            Assert.IsInstanceOf(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("begin", ((AstExplicitGoTo) block[i]).Destination);

            TestContext.WriteLine(target.StoreInString());
        }

        [Test]
        public void Arguments()
        {
            const string input1 =
                @"
function main
{
    declare function func1;
    func1;
    func1(1);
    func1(""2"", true, 3.41);
    func1(func1(func1(55)));
}
";
            _compile(input1);
            Expect(@"
@func.0 func1

ldc.int 1
@func.1 func1
ldc.string ""2""
ldc.bool true
ldc.real 3.41
@func.3 func1

ldc.int 55
 func.1 func1
 func.1 func1
 func.1 func1
 ret.val
");
        }

        [Test]
        public void Expressions()
        {
            const string input1 =
                @"
function main
{
    var x;
    var y;
    x = 1;
    y = 2 * x + 1;
    x += 0 + y * 1;
}";

            _compile(@input1);

            Expect(
                @"
ldc.int  1
stloc    x
ldc.int  2
ldloc    x
mul
ldc.int  1
add
stloc    y
ldloc    x
ldloc    y
add
dup      1
stloc    x
ret.val
");
        }

        [Test]
        public void IncrementDecrement()
        {
            _compile(
                @"
function func0
{
    var x;
    var y;
    x = 1;
    y = 2 * x++ + 1;
    y++;
    x = --y;
    x = 1+(++y)+1;
    return null;
}");
   
                Expect("func0",
                    @"
//x = 1
ldc.int  1
stloc    x
//y = 2 * x++ + 1
ldc.int  2
ldloc    x
inc      x
mul
ldc.int  1
add
stloc    y
//y++
inc      y
//x = --y;
dec      y
ldloc    y
stloc    x
//x = 1+(++y)+1
ldc.int  1
inc      y
ldloc    y
add
ldc.int  1
add
stloc    x

ldnull
ret.val"
                    );
        }

        [Test]
        public void ReturnStatements()
        {
            _compile(
                @"
function func0
{
    var x;
    var y;
    return;
    return x;
    break;
    continue;
}");


            var actual = target.Functions["func0"].Code;
            var expected =
                GetInstructions(
                    @"
ret.exit
ldloc   x
ret.value
ret.break
ret.continue
");
            TestContext.Write(target.StoreInString());

            Assert.AreEqual(
                expected.Count, actual.Count, "Expected and actual instruction count missmatch.");

            for (var i = 0; i < actual.Count; i++)
                Assert.AreEqual(
                    expected[i],
                    actual[i],
                    $"Instructions at address {i} do not match ({expected[i]} != {actual[i]})");
        }

        [Test]
        public void StaticCallAssign()
        {
            _compile(@"
function test\static_assign
{
    ::System::Console.WriteLine = ""Hi"";
}
");
            Expect(@"test\static_assign", @"
 ldc.string ""Hi""
 dup 1
 sset.1 ""Object(\""System.Console\"")::WriteLine""
 ret.value
");
        } 
        
        [Test]
        public void StaticCalls()
        {
            _compile(
                @"
Import System;
Add System::Text to Imports;

function test\static
{
    //CLR
    ::System::Console.WriteLine(""Hello World"");

    //CLR
    System::Console.ReadLine;

    //Prexonite (not CLR)
    var x = ~String.Escape(@""""""\""); // ~String('\)

    var pass;
    var raw;
    println(~String.Format(""\tmeasured:\t{0:0.00} ms\n"" + 
                ""\t\t\t{1:0.00} s\n""+
                ""\tpass:\t\t{2:0.00} ms\n"" +
                ""\t\t\t{3:0.00} micros"", raw, raw / 1000.0, pass,pass * 1000.0));
    
    //CLR over Prexonite
    ~Object<""System.Console"">.WriteLine(""Bye!"");

    return null;
}
");

            Expect(
                @"test\static",
                @"
 ldc.string  ""Hello World""
@sget.1      ""Object(\""System.Console\"")::WriteLine""
@sget.0      ""Object(\""System.Console\"")::ReadLine""
 var  x
 ldc.string  ""\""\\""
 sget.1      ""String::Escape""
 stloc       x
 
 ldc.string ""\tmeasured:\t{0:0.00} ms\n\t\t\t{1:0.00} s\n\tpass:\t\t{2:0.00} ms\n\t\t\t{3:0.00} micros""
 ldloc raw
 ldloc raw
 ldc.real 1000.0
 div
 ldloc pass
 ldloc pass
 ldc.real 1000.0
 mul
 sget.5     ""String::Format""
@cmd.1      println

 ldc.string  ""Bye!""
@sget.1      ""Object(\""System.Console\"")::WriteLine""

 ldnull
 ret.val
");
        }

        [Test]
        public void Conditions()
        {
            _compile(
                @"
entry conditions;
declare function action1, action2;
function conditions
{
    var x; //Required to prevent constant folding
    var y;
    //Simple:
    if(x)
        action1;

    //Simple #2
    unless(x)
        {action1;}
    else
        action2;

    //Constant
    if(true and true)
        action1;
    else
        action2;

    //Complex
    action1(""===========COMPLEX============"");
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
    if(x){}else action2;
    
    if(not x){}else;
}
");

            Expect(
                @"
var x
var y

//Simple:
 ldloc   x
 jump.f  endif1
@func.0  action1
 label   endif1
    
//Simple #2
 ldloc   x
 jump.t  else2
@func.0  action1
 jump    endif2
 label   else2
@func.0  action2
 label   endif2

//Constant
@func.0  action1

//Complex
 ldc.string  ""===========COMPLEX============""
@func.1      action1

 ldloc   x
 jump.f  else3
     ldloc   y
     jump.t  else4
        @func.0  action1
     jump    endif4
     label   else4
        @func.0  action2
     label   endif4
 jump    endif3
 label   else3
    @func.0  action1
    @func.0  action2
label   endif3

//Redundant blocks/conditions
ldloc   x
jump.t  endif5
@func.0  action2
label   endif5  
");
        }

        [Test]
        public void LazyConditions1dot5()
        {
            _compile(
                @"
function main
{
    declare function action1, action2, action3;

    var u; var v; var w; var x; var y; var z;

    if (u And ( v Or ( ( w Or x ) And ( y And z) ) ))
        action1;
    else if ( ( u Or v ) And ( x Or y ) )
        action2;
    else
        action3;

    asm { nop+end };
}
");
            Expect(
                @"
var u var v var w var x var z

                ldloc   u
                jump.f  else0
                ldloc   v
                jump.t  if0
                ldloc   w
                jump.t  next0
                ldloc   x
                jump.f  else0
label next0     ldloc   y
                jump.f  else0
                ldloc   z
                jump.f  else0
label if0      @func.0 action1
                jump    endif0
label else0     ldloc   u
                jump.t  next1
                ldloc   v
                jump.f  else1
label next1      ldloc   x
                jump.t  if1
                ldloc   y
                jump.f  else1
label if1      @func.0  action2
                jump    endif1
label else1    @func.0  action3

label endif0
label endif1
                nop+end
");
        }

        [Test]
        public void LazyConditions2()
        {
            _compile(
                @"
declare function action1, action2;
function main
{
    var u; var v; var w; var x; var y; var z; 
Beginning:; asm {nop+Beginning};
    if (u And ( v Or ( ( w Or x ) And ( y And z) ) ))
        action1;
    else if( ( u Or v Or w ) And ( x Or y Or z ))
        action2;
    else
        goto Beginning;

    asm { nop+Loop }

    do action1; while ( ( u And v ) Or x);
}
");
            Expect(
                @"
var u
var v
var w
var x
var y
var z
 
label   beginning       nop+beginning
                        ldloc   u
                        jump.f  else0
                        ldloc   v
                        jump.t  if0
                        ldloc   w
                        jump.t  next\logical0
                        ldloc   x
                        jump.f  else0
label   next\logical0   ldloc   y
                        jump.f  else0
                        ldloc   z
                        jump.f  else0

label   if0            @func.0  action1
                        jump    endif0
label   else0           ldloc   u
                        jump.t  next\logical1
                        ldloc   v
                        jump.t  next\logical1
                        ldloc   w
                        jump.f  beginning
label   next\logical1   ldloc   x
                        jump.t  if1
                        ldloc   y
                        jump.t  if1
                        ldloc   z
                        jump.f  beginning
label   if1            @func.0  action2
                        jump    endif0

label   endif0          nop+Loop
label   begin\while0   @func.0  action1
                        ldloc   u
                        jump.f  next\logical2
                        ldloc   v
                        jump.t  begin\while0
label   next\logical2   ldloc   x
                        jump.t  begin\while0
");
        }

        [Test]
        public void LazyConditions()
        {
            _compile(
                @"
declare function action1, action2;
function main
{
    var x;
    var y;
    var z;

    AndAA:;
    asm { nop+AndAA };

    if (x And y)
        action1;
    else
        action2;

    asm { nop+OrAJ };

    if (x Or y)
        action1;
    else 
        goto AndAA;

    NotAndOrJA:;
    asm { nop+NotAndOrJA };

    unless (x And ( y Or z ))
        goto AndAA;
    else
        action2;

    asm { nop+OrAndJJ };
    
    if (x Or ( y And z ))
        goto NotAndOrJA;
    else
        goto AndAA;

    asm { nop+END };
}
");
            Expect(
                @"
var     x
var     y
var     z
        
label   AndAA   nop+AndAA

                ldloc   x
                jump.f  else0
                ldloc   y
                jump.f  else0
               @func.0 action1
                jump    endif0
label   else0  @func.0 action2

label   endif0  nop+OrAJ

                ldloc   x
                jump.t  if1
                ldloc   y
                jump.t  if1
                jump    AndAA
label   if1    @func.0  action1
                
label NotAndOrJA nop+NotAndOrJA

                ldloc   x
                jump.f  AndAA
                ldloc   y
                jump.t  else2
                ldloc   z
                jump.t  else2
                jump    AndAA
label   else2  @func.0  action2                

                nop+OrAndJJ
    
                ldloc   x
                jump.t  NotAndOrJA
                ldloc   y
                jump.f  AndAA
                ldloc   z
                jump.f  AndAA
                jump    NotAndOrJA

/*
    if x Or ( y And z ) do
        goto NotAndOrJA;
    else
        goto AndAA;
*/
                
                nop+END
");
        }

        [Test]
        public void ConditionalExplicitJumps()
        {
            _compile(
                @"
declare function ________________, someAction;

function main(x)
{
    if(x)
        goto secondLevel;
    
    ________________;

    secondLevel:
    
    if(x)
        goto thirdLevel;
    else
        goto fourthLevel;

    ________________;

    thirdLevel:

    unless(x)
        someAction;
    else
        goto fourthLevel;

    ________________;

    fourthLevel:

    if(not x)
        goto fifthLevel;
    else
        someAction;

    ________________;

    fifthLevel:;
}
");

            Expect(
                @"
 ldloc   x
 jump.t  secondLevel
@func.0  ________________
 label   secondLevel
 ldloc   x
 jump.t  thirdLevel
 jump    fourthLevel
@func.0  ________________
 label   thirdLevel
 ldloc   x
 jump.t  fourthLevel
@func.0  someAction
@func.0  ________________
 label   fourthLevel
 ldloc   x
 jump.f  fifthLevel
@func.0  someAction
@func.0  ________________
 label   fifthLevel
");
        }

        [Test]
        public void StringConcat()
        {
            _compile(@"
function main(id)
{
    return ""Hello "" + id;
}
");

            Expect(@"
ldc.string  ""Hello ""
ldloc       id
add
ret.value
");
        }

        [Test]
        public void IndexAccess()
        {
            _compile(
                @"
function println(text) does ::Console.WriteLine(text);

function main(str, idx)
{
    println(str[idx]);
    return null;
}
");
            Expect(@"
 ldloc   str
 ldloc   idx
 get.1   """"
@func.1  println
 ldc.null
 ret.val
");
        }

        [Test]
        public void InlineAssembler()
        {
            _compile(
                @"
function main
{
    var x;
    asm {
        ldc.int 4
        ldc.int 3
        mul
        stloc   x
    }

    if(x > 5) asm
    {
        ldloc   x
        ldc.int 6
        sub
        stloc   x
    }

    if(3 == x) asm { inc x }
}
");
            Expect(
                @"
var x
ldc.int 4
ldc.int 3
mul
stloc   x

ldloc   x
ldc.int 5
cgt
jump.f  endif1
ldloc   x
ldc.int 6
sub
stloc   x
label   endif1

ldc.int 3
ldloc   x
ceq
jump.f  endif2
inc     x
label   endif2
");
        }

        [Test]
        public void WhileLoop()
        {
            _compile(
                @"
declare function action1, action2, __;

function main does
{
    var i = 0;
    var j = 0;
    //Conventional
    while(i++ < 5)
    {
        action1;
        do action2; while (j++ < 5);
    }
    
    __; //Until and short syntax

    until(i == 0)
    {
        action1;
        if(i == 1) continue;
        if (i-- mod 2 == 0) action2; else break;
    }

    __; //Invert

    while(not (var x))
        action1;

    __; //Constant no loop

    until(true) //false
        action2;

    __; //Constant infinite loop
    while(true)
    {
        if (action1) break; else continue;
        action2;        
    }
}
");

            Expect(
                @"
var i
var j
var x
ldc.int 0
stloc   i
ldc.int 0
stloc   j

//Conventional
jump    while0\continue
label   while0\begin
@func.0 action1
    label   while1\continue
   @func.0 action2
    ldloc   j
    inc     j
    ldc.int 5
    clt
    jump.t  while1\continue
    label   while1\break
label   while0\continue
ldloc   i
inc     i
ldc.int 5
clt
jump.t  while0\begin
label   while0\break

@func.0 __
//Until and short syntax
                        jump    while2\continue
label   while2\begin   @func.0 action1
                        ldloc   i
                        ldc.int 1
                        ceq
                        jump.t  while2\continue
label   endif0          ldloc   i
                        dec     i
                        ldc.int 2
                        mod
                        ldc.int 0
                        ceq
                        jump.f  while2\break
                       @func.0 action2
label   endif1          jump    while2\continue
label   while2\continue ldloc   i
                        ldc.int 0
                        ceq
                        jump.f  while2\begin
label   while2\break   @func.0 __

//Invert
                        jump    while3\continue
label   while3\begin   @func.0 action1
label   while3\continue ldloc   x
                        jump.f  while3\begin
label   while3\break   @func.0 __
//Constant no loop
                       @func.0 __
//Constant infinite loop
label   while5\continue
label   while5\begin
                        func.0  action1
                        jump.t  while5\break
                        jump    while5\begin
                       @func.0 action2
                        jump    while5\continue
label   while5\break   
");
        }

        [Test]
        public void ForLoopSimple()
        {
            _compile(
                @"
declare function action;

function main does
    for(var i = 0; i < 5; i++)
        action;

function main_extended does
    for(var i = 0; while i < 5; i++)
        action;
");
            var asmInput =
                @"
                var i
                ldc.int 0
                stloc   i
                jump    cond
label begin    @func.0  action
label cont      inc     i
label cond      ldloc   i
                ldc.int 5
                clt
                jump.t  begin
label end       
";
            Expect("main", asmInput);
            Expect("main_extended", asmInput);
        }

        [Test]
        public void ForLoopComplex()
        {
            _compile(
                @"
    declare function print, getNextElement;
    declare var max;
    function main does
    {
        var cnt = 0;
        for(var element; 
            do element = getNextElement;
            until element == null Or element.Directive == ""break""
           )
         {
            var desc = element.CreateDescription;
            if(cnt + desc.Length > max)
                continue;
            if(element.IsBold)
                print(""#B#"");            
            cnt += desc.Length;
            print(desc + ""\n"");            
         }
    }
");
            Expect(
                @"
var cnt
var element
var desc
                    ldc.int 0
                    stloc   cnt
                    jump    continue
label   begin
                    //Begin Block
                    ldloc   element
                    get.0   CreateDescription
                    stloc   desc
                    ldloc   cnt
                    ldloc   desc
                    get.0   Length
                    add
                    ldglob  max
                    cgt
                    jump.t  continue
                    ldloc   element
                    get.0   IsBold
                    jump.f  endif
                    ldc.string  ""#B#""
                   @func.1  print
label   endif       ldloc   cnt
                    ldloc   desc
                    get.0   Length
                    add
                    stloc   cnt
                    ldloc   desc
                    ldc.string  ""\n""
                    add
                   @func.1  print
                    //End Block
label   continue    func.0  getNextElement
                    stloc   element
label   condition   ldloc   element
                    ldc.null
                    ceq
                    jump.t  break
                    ldloc   element
                    get.0   Directive
                    ldc.string  ""break""
                    ceq
                    jump.f  begin
label   break
");
        }

        [Test]
        public void Commands()
        {
            _compile(
                @"
declare command print, println;

declare command println as echo;

function main
{
    print(""Hello"");
    println = ""Hello2"";
    echo(""The good old echo"");
    return null;
}
");
            Expect(
                @"
ldc.string  ""Hello""
@cmd.1       print
ldc.string  ""Hello2""
@cmd.1       println
ldc.string  ""The good old echo""
@cmd.1      println
ldc.null
ret.val
");
        }

        [Test]
        public void Foreach()
        {
            _compile(
                @"
function main
[ Import System::Text; ]
{
    var lst;
    foreach(var e in lst)
        println = e;
    
    //asm nop+COMPLICATED;

    var buffer = new ::StringBuilder;
    foreach(buffer.Append in lst.ToArray);
}
");
            var code = target.Functions["main"].Code;
            Assert.IsTrue(code.Count > 26, "Resulting must be longer than 18 instructions");
            var enum1 = code[3].Id ?? "No_ID_at_3";
            var enum2 = code[24].Id ?? "No_ID_at_23";

            Expect(
                string.Format(
                    @"
var lst
var buffer
var enum1
var enum2

                        ldloc   lst
                        get.0   GetEnumerator
                        cast.const  ""Object(\""System.Collections.IEnumerator\"")""
                        stloc   {0}
                        try
                        jump    enum1\continue

label enum1\begin       ldloc   {0}
                        get.0   Current
                        stloc   e

                        ldloc   e
                        @cmd.1  println

label enum1\continue    ldloc   {0}
                        get.0   MoveNext
                        jump.t  enum1\begin
label enum1\break       ldloc   {0}
                       @cmd.1   dispose
                        leave   endTry
label endTry            nop
                                    
                        newobj.0    ""Object(\""StringBuilder\"")""
                        stloc   buffer
                        ldloc   lst
                        get.0   ToArray
                        
                        get.0   GetEnumerator
                        cast.const  ""Object(\""System.Collections.IEnumerator\"")""
                        stloc   {1}
                        try
                        jump    enum2\continue

label enum2\begin       ldloc   buffer
                        ldloc   {1}
                        get.0   Current
                        set.1   Append
label enum2\continue    ldloc   {1}
                        get.0   MoveNext
                        jump.t  enum2\begin
                        ldloc   {1}
                       @cmd.1   dispose
                        leave   endTry2
label endTry2           nop
",
                    enum1,
                    enum2));
        }

        [Test]
        public void EmptyBuildBlock()
        {
            _compile(
                @"
BuildOptimization Enabled;

build {

}

Name EmptyBuild;

var ekoe = ""Hammer"";

declare command print;

function main()
{
    print=ekoe;
    return null;
}
");
            Expect(@"
ldglob  ekoe
@cmd.1   print
ldc.null
ret.val
");
        }

        [Test]
        public void References()
        {
            _compile(
                @"
declare function f, g, h;

ref g' = ->g;

function main
{
    var h' = ->h;
    ref f' = ->f;

    ref primes = ~List.Create(->f, ->g', h');    
    ref h';

    print = f';
    print = g';
    print = h';
    print = primes;

    return null;
}
");
            Expect(
                @"
var f'
//declare var g'
var h'

ldr.func    h
stloc       h'
ldr.func    f
stloc       f'

ldr.func    f
ldglob      g'
ldloc       h'
sget.3      ""List::Create""
stloc       primes

indloc.0    f'
@cmd.1      print
indglob.0   g'
@cmd.1      print
indloc.0    h'
@cmd.1      print
indloc.0    primes
@cmd.1      print

ldc.null
ret.val
");
        }

        [Test]
        public void Typecheck()
        {
            _compile(
                @"
function main(var arg)
{   
    if(not arg is System::Text::StringBuilder) return false;
    if(arg.ToString is String) print = arg;
    return null;
}
");
            Expect(
                @"
ldloc       arg
check.const ""Object(\""System.Text.StringBuilder\"")""
jump.t      else1
ldc.bool    false
ret.value   
label       else1
ldloc       arg
get.0       ToString
check.const String
jump.f      else2
ldloc       arg
@cmd.1      print
label       else2
ldc.null
ret.val
");
        }

        [Test]
        public void IndependantLambda()
        {
            _compile(
                @"
var g;
var h;
var j;

function main()
{
    var a;
    var b;
    var c;
    
    
    var t = ( ) => 5861;
    var u = (x) => 2*x;
    var v = (x, var y, ref z) => z(x + y);
    var w = x => { var d = x+2; return d * 4; };
}
");
            Expect(@"main\0", @"ldc.int 5861 ret.value");
            Expect(@"main\1", @"ldc.int 2 ldloc x mul ret.value");
            Expect(@"main\2", @"ldloc x ldloc y add indloc.1 z ret.val");
            Expect(
                @"main\3",
                @"var d ldloc x ldc.int 2 add stloc d ldloc d ldc.int 4 mul ret.val");
        }

        [Test]
        public void DependantLambda()
        {
            _compile(
                @"
var g;
function main()
{
    var a;

    var t = x => x + a;
    var u = () => { ref a; a; return null; };
}
");

            Expect(@"main\0", @"
ldloc   x
ldloc   a
add
ret.value
");
            var func = target.Functions[@"main\0"];
            Assert.AreEqual(1, func.Meta[PFunction.SharedNamesKey].List.Length);
            Assert.AreEqual("a", func.Meta[PFunction.SharedNamesKey].List[0].Text);

            Expect(@"main\1", @"
@indloc.0  a
ldnull
ret.val
");
            func = target.Functions[@"main\1"];
            Assert.AreEqual(1, func.Meta[PFunction.SharedNamesKey].List.Length);
            Assert.AreEqual("a", func.Meta[PFunction.SharedNamesKey].List[0].Text);
        }

        [Test]
        public void NestedFunction()
        {
            _compile(
                @"
function main
{
    var a;
    function N1
    {
        var na = 2*a;
        return na-1;
    }

    N1(4);

    function N2(x, ref f) = f(x);

    N2(5, x => 2*x);
    return null;
}
");

            _expectSharedVariables(@"main\N10", "a");
            _expectSharedVariables(@"main\N21");

            Expect(
                @"
var         a
var         N1
var         N2

newclo      main\N10
stloc       N1

ldc.int     4
@indloc.1   N1

ldr.func    main\N21 //no need to create a closure
stloc       N2

ldc.int     5
ldr.func    main\2 //no need to create a closure
@indloc.2   N2

ldc.null
ret.val
");
        }

        [Test]
        public void DeDereference()
        {
            _compile(
                @"
function main()
{
    ref f = x => 2+x;
    ref fobj = ->f;
    ref fvar = ->->f;
    fvar = x => 2*x;
    
    ref fobj2 = fvar;

    fobj(); //x => 2+x
    f();    //x => 2*x
    fobj2();//x => 2*x

    return null;
}
");
            Expect(
                @"
ldr.func main\0 //no need for closure here
stloc   f

ldloc   f
stloc   fobj

ldr.loc f
stloc   fvar

ldr.func main\1
@indloc.1    fvar

indloc.0 fvar
stloc fobj2

@indloc.0 fobj
@indloc.0 f
@indloc.0 fobj2

ldc.null
ret.val
");
        }

        [Test]
        public void ExplicitIndirectCall()
        {
            _compile(
                @"
function main()
{
    var threshold;
    var fobj = x => 2*x;
    var gobj = () => 3*threshold;

    println( fobj.(15) );           //Optimized
    println( gobj.() );             //Optimized
    println( ->fobj.Value.(10) );
    return null;
}
");

            Expect(
                @"
ldr.func    main\0 //no need for a closure here
stloc       fobj
newclo      main\1
stloc       gobj

ldc.int     15
indloc.1    fobj
@cmd.1      println

indloc.0    gobj
@cmd.1      println

ldr.loc     fobj
get.0       Value
ldc.int     10
indarg.1
@cmd.1      println

ldc.null
ret.val
");
        }

        [Test]
        public void StringConcatenation()
        {
            _compile(
                @"
function main()
{
    var x; var a; var b; var c;
    x = ""a"";
    x =   a   +   b;
    x = ""a"" + ""b"";
    x = ""a"" +   b;
    x =   a   + ""b"";
    x = ""a"" +   b   + ""c"";
    x = ""a"" +   b   +   c   + ""a"" + ""d"";

    return null;
}
");

            Expect(
                @"
var x,a,b,c
ldc.string  ""a""
stloc       x

ldloc   a
ldloc   b
add
stloc   x

ldc.string  ""ab""
stloc   x

ldc.string  ""a""
ldloc       b
add
stloc       x

ldloc       a
ldc.string  ""b""
add
stloc       x

ldc.string  ""a""
ldloc       b
ldc.string  ""c""
cmd.3       string_concat
stloc       x

ldc.string  ""a""
ldloc       b
ldloc       c
ldc.string  ""ad""
cmd.4       string_concat
stloc       x

ldc.null
ret.val
");
        }

        [Test]
        public void StringInterpolationIdentifier()
        {
            _compile(
                @"
function main()
{
    var x; var a; var b; var c;

    x = ""I think the first parameter is $a, while the second seems like ($b) $c"";    
    return null;
}
");
            Expect(
                @"
var x,a,b,c

ldc.string  ""I think the first parameter is ""
ldloc       a
ldc.string  "", while the second seems like (""
ldloc       b
ldc.string  "") ""
ldloc       c
cmd.6       string_concat
stloc       x
ldc.null
ret.val
");
        }

        [Test]
        public void StringInterpolationExpression()
        {
            _compile(
                @"
function main()
{
    var x; var a; var b; var c;

    x = ""I think the first parameter is $(a) while the second seems like $(c.Substring(5,4).Length~String) ($(b.()))"";
    return null;
}
");

            Expect(
                @"
var x,a,b,c

ldc.string  ""I think the first parameter is ""
ldloc       a
ldc.string  "" while the second seems like ""
ldloc       c
ldc.int     5
ldc.int     4
get.2       Substring
get.0       Length
cast.const  ""String""
ldc.string  "" (""
indloc      b
ldc.string  "")""
cmd.7       string_concat
stloc       x
ldc.null
ret.val
");
        }

        [Test]
        public void StringInterpolationExpressionSimple()
        {
            _compile(@"
declare command a;
function main()
{
    print = ""AB$(a)CD"";
    return null;
}
");
            Expect(
                @"
ldc.string ""AB""
cmd.0       a
ldc.string  ""CD""
cmd.3       string_concat
@cmd.1      print
ldc.null
ret.val
");
        }

        [Test]
        public void ConditionalExpression()
        {
            _compile(
                @"
function max(a,b) = if(a > b) a else b;
function maxv(a,b) = 
    if(a > b) 
        a 
    else 
        b;

function main(x)
{
    x = if(x mod 2 == 0) (if(x > 0) x else -x) else max(x,2);
    return if(x is Null) 0 else if(x == """") -1 else x.Length;
}

function mainv(x)
{
    x = 
        if(x mod 2 == 0) 
            if(x > 0) 
                x 
            else 
                -x 
        else 
            max(x,2);
    return 
        if(x is Null) 
            0 
        else if(x == """") 
            -1 
        else 
            x.Length;
}
");
            const string emax =
                @"
                ldloc   a
                ldloc   b
                cgt
                jump.f  else
                ldloc   a
                ret.value
label else      ldloc   b
label endif     ret.value
";
            Expect("max", emax);
            Expect("maxv", emax);

            const string emain =
                @"
                ldloc   x
                ldc.int 2
                mod
                ldc.int 0
                ceq
                jump.f  else1
                ldloc   x
                ldc.int 0
                cgt
                jump.f  else2
                ldloc   x
                jump    endif1
label else2     ldloc   x
                neg
label endif2    jump    endif1
label else1     ldloc   x
                ldc.int 2
                func.2   max
label endif1
stloc           x

//return x is Null ? 0 : x == """" ? -1 : x.Length;

                ldloc           x
                check.null
                jump.f          else3
                ldc.int         0
                ret.value
label else3     ldloc           x
                ldc.string      """"
                ceq
                jump.f          else4
                ldc.int         -1
                ret.value
label else4     ldloc           x
                get.0           Length
label endif4    //optimized://  jump            endif3
label endif3    ret.value
";

            Expect("main", emain);
            Expect("mainv", emain);
        }

        [Test]
        public void ConditionalExpressionInverted()
        {
            _compile(
                @"
declare var a,b,c;

function main
{
    var x = 
        if(not a) 
            b 
        else
            c;

    var y = 
        unless(b) 
            a 
        else 
            a+b;

    var z = 
        unless(not (a and b))
            c 
        else 
            a-b;

    return null;
}
");

            Expect(
                @"
var x,y,z

ldglob  a
jump.t  xe
ldglob  b
jump    sx
label   xe
ldglob  c
label   sx
stloc   x

ldglob  b
jump.t  ye
ldglob  a
jump    sy
label   ye
ldglob  a
ldglob  b
add
label   sy
stloc   y

ldglob  a
jump.f  ze
ldglob  b
jump.f  ze
ldglob  c
jump    sz
label   ze
ldglob  a
ldglob  b
sub
label   sz
stloc   z

ldc.null
ret.val
");
        }

        [Test]
        public void StringInterpolationWithStrings()
        {
            _compile(
                @"
function main(x)
{
    declare command transform;
    return ""There is $(transform(""no"")) spoon"";
}   
");
            Expect(
                @"
ldc.string  ""There is ""
ldc.string  ""no""
cmd.1       transform
ldc.string  "" spoon""
cmd.3       string_concat
ret.value
");
        }

        [Test]
        public void ProceduralStructureDefinition()
        {
            _compile(
                @"
function main(a,b,c)
{
    var str = new Structure<""a"",""b"",""r"",""c"">();
    return null;
}
");
            Expect(
                @"
newobj.0    ""Structure(\""a\"",\""b\"",\""r\"",\""c\"")""
stloc       str
ldnull
ret.val
");
        }

        [Test]
        public void OneDividedByX()
        {
            _compile(@"
function main(x) = 1 / x;
");
            Expect(@"
ldc.int 1
ldloc   x
div
ret.value
");
        }

        [Test]
        public void ListLiteral()
        {
            _compile(
                @"
function main()
{
    var x = [];
    var y = [1,[4,5],8,[10],11];
    return null;
}
");

            Expect(
                @"
var x,y
cmd.0   list
stloc   x
ldc.int 1
ldc.int 4
ldc.int 5
cmd.2   list
ldc.int 8
ldc.int 10
cmd.1   list
ldc.int 11
cmd.5   list
stloc   y
ldnull
ret.val
");
        }

        [Test]
        public void CoroutineCreation()
        {
            _compile(
                @"
function subrange(lst, index, count)
{
    for(var i = index; i < index + count; i++)
        yield lst[i];
}

function main()
{
    var lst;
    ref oneToFive = coroutine -> subrange for (lst,1,5);
    var even = coroutine () => 
    {
        foreach(var x in lst)
            yield x;
    };
    return null;
}
");
            Expect(
                @"
var lst, oneToFive, even
ldr.func    subrange
ldloc       lst
ldc.int     1
ldc.int     5
newcor.3
stloc       oneToFive
newclo      main\0
newcor.0
stloc       even     
ldnull
ret.val 
");

            Expect(
                @"subrange",
                @"
var lst, index, count,i
ldloc   index
stloc   i
jump    condition
label   begin
ldloc   lst
ldloc   i
get.1   """"
ret.set
yield
label continue
inc     i
label condition
ldloc   i
ldloc   index
ldloc   count
add
clt
jump.t  begin
");
        }

        [Test]
        public void BugIllegalHide()
        {
            _compile(
                @"
var A = 5;

function legalFunction()
{
    foreach(var a in [1,2,3])
        print = a;
}

function main()
{
    println(A); //Refers to the global variable.
                //Due to a bug, the local variable in 'legalFunction' hides the global 'A'.
    return null;
}
");
            Expect(@"
ldglob  A
@cmd.1  println
ldnull
ret.val
");
        }

        [Test]
        public void CoroutineFunctionDefinitions()
        {
            _compile(
                @"
coroutine where(lst, ref predicate) does
    foreach(var e in lst)
        if(predicate(e))
            yield(e);

function main()
{
    coroutine skip(lst, num)
    {
        foreach(var e in lst)
            if(num-- > 0)
                yield e;
    }

    return where(skip([1,2,3],1),x=>x mod 2==0);
}
");

            Expect(
                @"
var skip
ldr.func  main\skip0  //the nested function does not need to be a closure, so it is not
stloc   skip
ldc.int 1
ldc.int 2
ldc.int 3
cmd.3   list
ldc.int 1
indloc.2 skip
ldr.func main\1 //no need for a closure
func.2  where
ret.val
");

            Expect("where", @"
newclo  where\0
newcor.0
ret.value
");
            Expect(
                @"main\skip0", @"
newclo  main\skip0\0
newcor.0
ret.value
");
        }

        [Test]
        public void TryCatchFinallySimple()
        {
            _compile(
                @"
declare function
    createHandle,
    fail,
    log,
    closeHandle;

function main()
{
    var handle;
    try
    {
        handle = createHandle;
        //...work
        fail;
    }
    catch(var exc)
    {
        log(exc);
    }
    finally
    {
        closeHandle(handle);
    }
}
");
            Expect(
                @"
var 
    handle,
    exc

label beginTry      try
                    func.0  createHandle
                    stloc   handle
//...work
                   @func.0  fail
label beginFinally
                    ldloc   handle
                   @func.1  closeHandle
                    leave   endTry
label beginCatch
                    exception
                    stloc   exc
                    ldloc   exc
                   @func.1  log
label endTry        nop
");
        }

        [Test]
        public void TryCatchFinallyNested()
        {
            _compile(
                @"
declare function
    open,
    close,
    log,
    shutdown,
    fail;

function main()
{
    try
    {
        var handle = open;
        fail;
    }
    catch(var exc)
    {
        log(exc);
    }
    finally
    { 
        try
        {
            close(handle);
        }
        catch(var exc)
        {
            log(""FATAL"");
            shutdown;
        }
    }
}
");
            Expect(
                @"
var 
    handle,
    exc

label beginTry      try
                    func.0  open
                    stloc   handle
                   @func.0  fail
label beginFinally  
label beginTry2     try
                    ldloc   handle
                   @func.1  close

label beginFinally2 leave   endTry2
label beginCatch2   exception
                    stloc exc
                    ldc.string
                            ""FATAL""
                   @func.1  log
                   @func.0  shutdown
label endTry2       nop
                    leave   endTry
label beginCatch    exception
                    stloc   exc
                    ldloc   exc
                   @func.1  log
label endTry        nop
");
        }

        [Test]
        public void TryFinally()
        {
            _compile(
                @"
declare function
    createHandle,
    fail,
    log,
    closeHandle;

function main()
{
    var handle;
    try
    {
        handle = createHandle;
        //...work
        fail;
    }
    finally
    {
        closeHandle(handle);
    }
}
");
            Expect(
                @"
var 
    handle,
    exc

label beginTry      try
                    func.0  createHandle
                    stloc   handle
//...work
                   @func.0  fail
label beginFinally
                    ldloc   handle
                   @func.1  closeHandle
                    leave   endTry
label endTry        nop
");
        }

        [Test]
        public void Throw()
        {
            _compile(
                @"
function main()
{
    throw ""There must be a mistake!"";
    throw new System::Exception(""There IS a mistake!"");
    throw 3;
    return null;
}
");
            Expect(
                @"
ldc.string  ""There must be a mistake!""
throw

ldc.string  ""There IS a mistake!""
newobj.1    ""Object(\""System.Exception\"")""
throw

ldc.int     3
throw

ldnull
ret.value
");
        }

        [Test]
        public void Using()
        {
            _compile(
                @"
declare function
    createHandle,
    write,
    handle;

function main
{
    var h;
    using(handle(->h) = createHandle)
        write(h,15);
}
");

            var code = target.Functions["main"].Code;
            Assert.IsTrue(code.Count > 6, "Resulting must be longer than 6 instructions");
            var using1 = code[6].Id ?? "No_ID_at_6";

            Expect(string.Format(
                @"
var h,{0}
label beginTry  try
                ldr.loc h
                func.0  createHandle
                dup 1
                rot.2,3
               @func.2  handle
                stloc   {0}
                ldloc   h
                ldc.int 15
               @func.2  write
label beginFinally
                ldloc   {0}
               @cmd.1   dispose
                leave   endTry
label endTry    nop
",using1));
        }

        [Test]
        public void KeyValuePairs()
        {
            _compile(
                @"
function main()
{
    var x; var y; var z;
    var a; var b; var c;
    x = a: b;
    y = a: b: c;
    z = (a: b): (c: a): b;

    return null;
}
");

            Expect(
                @"
var x, y, z

ldloc   a
ldloc   b
cmd.2   pair
stloc   x

ldloc   a
ldloc   b
ldloc   c
cmd.2   pair
cmd.2   pair
stloc   y

ldloc   a
ldloc   b
cmd.2   pair
ldloc   c
ldloc   a
cmd.2   pair
ldloc   b
cmd.2   pair
cmd.2   pair
stloc   z

ldnull
ret.val
");
        }

        [Test]
        public void HashLiteral()
        {
            _compile(
                @"

function main()
{
    var a; var b; var c; var d;
    var hset = {    a: b, 
                    c: d, 
                    5: a, 
                    ""hello"": ""ciao"",
                    (if(a > b) a else b): (if(c > d) c else d)
               };
    var people = 
    {
        ""V"":
        {
            ""Name"": ""Veronica"",
            ""Age"": 23
        },

        ""A"":
        {
            ""Name"": ""Abraham"",
            ""Age"": 53
        }
    };

    return null;
}
");

            Expect(
                @"
var hset, people

ldloc   a
ldloc   b
cmd.2   pair
ldloc   c
ldloc   d
cmd.2   pair
ldc.int 5
ldloc   a
cmd.2   pair
ldc.string ""hello""
ldc.string ""ciao""
cmd.2   pair
ldloc   a
ldloc   b
cgt
jump.f  else1
ldloc   a
jump    endif1      label   else1
ldloc   b           label   endif1
ldloc   c
ldloc   d
cgt
jump.f  else2
ldloc   c
jump    endif2      label   else2
ldloc   d           label   endif2
cmd.2   pair
sget.5  ""Hash::Create""
stloc   hset

ldc.string ""V""
ldc.string ""Name""
ldc.string ""Veronica""
cmd.2   pair
ldc.string ""Age""
ldc.int 23
cmd.2   pair
sget.2  ""Hash::Create""
cmd.2   pair

ldc.string ""A""
ldc.string ""Name""
ldc.string ""Abraham""
cmd.2   pair
ldc.string ""Age""
ldc.int 53
cmd.2   pair
sget.2  ""Hash::Create""
cmd.2   pair

sget.2  ""Hash::Create""
stloc   people

ldnull
ret.value
");
        }

        [Test]
        public void KeyValuePairArrayLiteral()
        {
            _compile(
                @"
function main()
{
    var lst = 
    [
        ""apple"": 5,
        ""orange"": 6,
        ""apple"": 8
    ];

    return null;
}
");
            Expect(
                @"
var lst

ldc.string  ""apple""
ldc.int 5
cmd.2   pair

ldc.string  ""orange""
ldc.int 6
cmd.2   pair

ldc.string  ""apple""
ldc.int 8
cmd.2   pair

cmd.3  list

stloc lst

ldnull
ret.val
");
        }

        [Test]
        public void ReferenceDeclarationLiteral()
        {
            // Note: the behaviour of the expression (ref h) has been changed.
            //  It is no longer equivalent to ->h but instead always means the same as var h 
            //  (without changing re-declaring h, of course)
            //  By extension (ref ref h) == (ref h)
            _compile(@"
function main
{
    print(ref h);
    print(ref ref g);
    ref ref ref var k = ref h;
    print(k);
    return var o;
}
");

            Expect(@"
var h
ldloc h
@cmd.1  print

ldloc g
@cmd.1 print

ldloc h
stloc k

indloc k
indarg.0 
indarg.0
@cmd.1 print

ldloc o
ret.val
");
        }

        [Test]
        public void GlobalCode()
        {
            _compile(@"
var g;

{ g = 4; }

function main does
{
    println = g;
    return null;
}
");

        }

        [Test]
        public void StatementConcatenation()
        {
            _compile(
                @"
declare var a, b, c;

function main does
    declare var g; and
    g = a or b and c; and
    println(g); and
    return null;
");

            Expect(
                @"
            ldglob      a
            jump.t      tr
            ldglob      b
            jump.f      fa
            ldglob      c
            jump.t      tr
label fa    ldc.bool    false
            jump        set
label tr    ldc.bool    true
label set   stglob      g
            ldglob      g
            @cmd.1      println

            ldnull
            ret.val
");
        }

        [Test]
        public void AssemblerExpressions()
        {
            //Now this really *IS* Voodoo magic
            _compile(
                @"
function main
{
    var eng = asm ( ldc.int 6 ldc.int 8 add );
    var funcs = asm ( ldr.app get.0 Functions );
    return null;
}
");

            Expect(
                @"
var eng, funcs

ldc.int 6 
ldc.int 8 
add
stloc eng

ldr.app
get.0 Functions
stloc funcs

ldnull
ret.val
");
        }

        [Test]
        public void CoalescenceOperator()
        {
            _compile(
                @"
function main()
{
    var a; var b; var c;

    var x = a ?? b;
    var y = a + b ?? (if(a) b ?? c else c ) ?? null ?? c;

    return null;
}
");

            Expect(
                @"
var a,b,c,x,y

ldloc   a
dup     1
check.null
jump.f  endc0
pop     1
ldloc   b
label   endc0
stloc   x

                ldloc   a
                ldloc   b
                add
                dup     1
                check.null
                jump.f  endc1
                pop     1
                ldloc   a
                jump.f  else
                ldloc   b
                dup     1
                check.null
                jump.f  endc2
                pop     1
                ldloc   c
                jump    endif
label   else    ldloc   c                
label   endif
label   endc2   dup     1
                check.null
                jump.f  endc1
                pop     1
                ldloc   c
                
label   endc1   stloc   y

                ldnull
                ret.val                
");
        }

        [Test]
        public void CoalescenceAssignment()
        {
            _compile(
                @"
function main()
{
    var a;
    var b;
    var c;

    if(a is Null)
        a = b;

    c ??= b;

    return null;
}
");

            Expect(
                @"
var a,b,c

ldloc   a
check.null
jump.f  endif0
ldloc   b
stloc   a
label   endif0

ldloc   c
check.null
jump.f  endif1
ldloc   b
stloc   c
label   endif1
ldnull
ret.val
");
        }

        [Test]
        public void AppendLeftArguments()
        {
            _compile(
                @"
function main()
{
    print << 4;
    println << (6,9);
    call << (concat << (3,7), 15);
    return null;
}
");

            Expect(
                @"
ldc.int 4
@cmd.1  print
ldc.int 6
ldc.int 9
@cmd.2  println
ldc.int 3
ldc.int 7
cmd.2   concat
ldc.int 15
@cmd.2  call\perform
ldnull
ret.val
");
        }

        [Test]
        public void AppendRightArguments()
        {
            _compile(
                @"
function main()
{
    var a; var b; var c;
    (4) >> print;
    (6,9) >> println;
    ( 3 >> concat(7), 15 ) >> call;
    println = 5 >> call;
    return null;
}   
");

            Expect(
                @"
ldc.int 4
@cmd.1  print
ldc.int 6
ldc.int 9
@cmd.2  println
ldc.int 7
ldc.int 3
cmd.2   concat
ldc.int 15
@cmd.2  call\perform
ldc.int 5
cmd.1   call\perform
@cmd.1  println
ldnull
ret.val
");
        }

        [Test]
        public void AppendBothArguments()
        {
            _compile(
                @"
function main(x)
{
    var r;
    r = x >> call << 5;
    r = x * 8 >> call << 5: x;
    return null;
}
");

            Expect(
                @"
var r
ldloc   x
ldc.int 5
cmd.2   call\perform
stloc   r

ldloc   x
ldc.int 8
mul
ldc.int 5
ldloc   x
cmd.2   pair
cmd.2   call\perform
stloc   r

ldnull
ret.val
");
        }

        [Test]
        public void AppendTraceRight()
        {
            _compile(
                @"
function main(app, f, ref trace){
   println(app.Compound >> trace(f) >> all);
   app.Compound         >> trace(f)        >> all >> println;
}");

            Expect(
                @"
 ldloc f
 ldloc app
 get.0 Compound
 indloc.2 trace
 cmd.1 all
 @cmd.1 println

 ldloc f
 ldloc app
 get.0 Compound
 indloc.2 trace
 cmd.1 all
 cmd.1 println
 ret.val
");
        }

       
        /* not supported at the moment
        [Test]
        public void AppendLeftDirect()
        {
            _compile(@"
function main()
{
    var a;
    println << a << 5;

    println << (->a).() << 5;
}
");

            _expect(@"
var a

ldc.int 5
dup     1
stloc   a
@cmd.1  println

ldc.int 5
indloc.1 a
@cmd.1  println
");
        } //*/

        [Test]
        public void OptimizeSquare()
        {
            //Since operators can now be overloaded, this optimization does no longer apply
            //This test ensures that the optimization does NOT take place
            _compile(@"
function main()
{
    var x;
    return x^2;
}
");

            Expect(@"
var x
ldloc   x
ldc.int 2   //what you would expect
pow
ret
");
        }

        [Test]
        public void SingleModifyingAssign()
        {
            _compile(@"
function main()
{
    var a;
    a += 3;
}
");
            Expect(@"
ldloc a
ldc.int 3
add
dup 1
stloc a
ret.val
");
        }

        [Test]
        public void CastAssign()
        {
            _compile(@"
function main()
{
    var a;
    var x = a;
    var y = a;
    var z = a;

    x~=String; //no whitespace
    y ~= Int; //normal whitespace
    z ~ = Real; // ~ and = are two distinct operators. There is no '~=' operator  

    return x+y+z;
}
");

            Expect(@"
var a,x,y,z

ldloc   a
stloc   x
ldloc   a
stloc   y
ldloc   a
stloc   z

ldloc   x
cast.const  String
stloc   x

ldloc   y
cast.const  Int
stloc   y

ldloc   z
cast.const  Real
stloc   z

ldloc   x
ldloc   y
add
ldloc   z
add
ret
");
        }

        [Test]
        public void CastAssignSingle()
        {
            _compile(@"
function main(a, b)
{
    var x = b;
    x ~ = Object<""System.Version"">;
    return x;
}
");

            Expect(@"
var a,b,x

ldloc b
stloc x

ldloc x
cast.const ""Object(\""System.Version\"")""
stloc x

ldloc x
ret
");
        }

        [Test]
        public void SimpleAssignExpr()
        {
            _compile(@"
function main()
{
    var a;
    var b;
    var c = b = a;
    var d = a + 5*(b = c);
    var s;
    var e = s.M = d;
    var f = System::Console.WriteLine = 5;
    var g = f.(8) = a;
    
    return null;
}
");

            Expect(@"
var a,b,c,d,e,f,g,s

ldloc   a
dup     1
stloc   b
stloc   c

ldloc   a
ldc.int 5
ldloc   c
dup     1
stloc   b
mul
add
stloc   d

ldloc   s
ldloc   d
dup     1
rot.2,3
set.1   M
stloc   e

ldc.int 5
dup     1
sset.1  ""Object(\""System.Console\"")::WriteLine""
stloc   f

ldc.int 8
ldloc   a
dup     1
rot.2,3
@indloc.2 f
stloc   g

ldnull
ret.val
");
        }

        [Test]
        public void ComplexAssignExpr()
        {
            _compile(@"
function main()
{
    var a;
    var b = a *= 5;
    var c ~= String;
    var d = b ??= a;

    return null;
}
");
            Expect(@"
var a,b,c,d

            ldloc   a
            ldc.int 5
            mul
            dup     1
            stloc   a
            stloc   b

            ldloc   c
            cast.const  ""String""
            stloc   c

            ldloc   b
            check.null
            jump.f  el
            ldloc   a
            dup     1
            stloc   b
            jump    end
label   el  ldloc   b
label end   stloc   d
    
            ldnull
            ret.val
");
        }

        [Test]
        public void DirectTailRecursion()
        {
            _compile(@"
function fac n r =
    if(n == 1)
        r
    else
        fac(n-1, n*r);
");

            Expect("fac", @"
ldloc   n
ldc.int 1
ceq
jump.f  else
ldloc   r
ret.value

label else

ldloc   n
ldc.int 1
sub

ldloc   n
ldloc   r
mul

stloc   r
stloc   n

jump    0
");
        }

        [Test]
        public void ComplexModifyAssign()
        {
            _compile(@"
function main()
{
    var a;
    var b;
    a.Member -= b;
    return null;
}
");

            Expect(@"
var a,b
ldloc   a
ldloc   a
get.0   Member
ldloc   b
sub
//dup 1
set.1   Member
//rot.2,3
ldc.null
ret.value
");
        }

        [Test]
        public void AppendRightStatement()
        {
            _compile(@"
function main(lst)
{
    lst >>
        where(x => x > 5) >>
        foldl((a,b) => a + b, 1);
    return null;
}
");

            Expect(@"
var lst

ldr.func main\1
ldc.int 1
ldr.func main\0
ldloc   lst
cmd.2   where
@cmd.3   foldl

ldnull
ret.val
");
        }

        [Test]
        public void ReturnUnlessOptimization()
        {
            _compile(@"
function main(x)
{
    return unless(x) 1 else 0;
}");

            Expect(@"
ldloc x
jump.t elseBranch
ldc.int 1
ret
label elseBranch
ldc.int 0
ret
");
        }

        [Test]
        public void PostIncrementFunctionEmulation()
        {
            _compile(@"
function propLike = 0;

function main()
{
    return ""work"" * propLike++;
}
");

            Expect(@"
ldc.string work
func.0 propLike
func.0 propLike
ldc.int 1
add
@func.1 propLike
mul
ret.value
");
        }

        [Test]
        public void NullcheckInConditionalExpression()
        {
            _compile(@"
function main(x)
{
    var n = if(x is null) 1 else 0;
    var i = if(x is not null) 1 else 0;
    asm{nop}
    return if(x is not null) 1 else 0;
}
");

            Expect(@"
var n,i

ldloc x
check.null
jump.f isNotNull
ldc.int 1
jump endn
label isNotNull
ldc.int 0
label endn
stloc n

ldloc x
check.null
jump.t isNull
ldc.int 1
jump endi
label isNull
ldc.int 0
label endi
stloc i

nop

ldloc x
check.null
jump.t isNull2
ldc.int 1
ret
label isNull2
ldc.int 0
ret
");
        }

        [Test]
        public void Bug0()
        {
            _compile(
                @"
declare function f;

function main does foreach(var arg in var args)
{
    var t = f(arg);
    t.() = t.().M;
}
");

            Expect(string.Format(@"
var args,arg,t,{0}
                        ldloc   args
                        get.0   GetEnumerator
                        cast.const ""Object(\""System.Collections.IEnumerator\"")""
                        stloc   {0}
                        try
                        jump    continueForeach
label   beginForeach    ldloc   {0}
                        get.0   Current
                        stloc   arg
                        ldloc   arg
                        func.1  f
                        stloc   t
                        indloc.0    t
                        get.0   M
                        @indloc.1    t
label continueForeach   ldloc   {0}
                        get.0   MoveNext
                        jump.t  beginForeach
                        ldloc   {0}
                        @cmd.1  dispose
                        leave   end
label   end             nop //this nop ensures compatibility with CIL
", target.Functions["main"].Code[3].Id));
        }

        [Test]
        public void ValidFunctionScope()
        {
            _compile(@"function main=()=>{};function f2(){}");
            Assert.That(target.Functions.Contains("f2"),Is.True,"Expected that function f2 exists.");
        }

        [Test]
        public void Bug26CommentsAtEnd()
        {
            //cannot reproduce as unit test so far
            _compile(@"
//PXS_

function get_routine(name) = asm(ldr.app).Functions[name] ?? () => {};

function as setup does get_routine(""setup"").();
function as teardown does get_routine(""teardown"").();

var TEST_KEY = ""test"";

function run\test(test)
{
	var result = null;
	try {
		setup();		
		test.();
		teardown();
	} 
	catch(var e)
	{
		result = e;
	}
	
	return result;
}

function main as run_all()
{
	if(var args.count == 0)
		args = asm(ldr.app).Functions;
	
	var tests =
		args 
		>> map(t => if(t is String) asm(ldr.app).Functions[t] else t)
		>> where(t => t is not null and t.Meta[TEST_KEY].Switch)
		>> all;
	
	var n = tests.Count;
	println(""Running $(tests.Count) tests."");
	var i = 1;
	var failed = [];
	foreach(var test in tests)
	{
		println(""Running test $i/$n: "",$test.Id);
		var exc = run\test(test);
		if(exc is not null)
		{
			failed[] = test;
			println(""\tFAILED"");
			println(exc);
		}
		else 
		{
			println(""\tSUCCESS"");
		}
		
		println();
		i++;
	}
}

{ 
	CompileToCil;
	if(->run\test.Meta[""volatile""].Switch)
		throw ""run\\test was not compiled to CIL. This is necessary in order to ensure testing integrity."";
}

//

/* */
");
        }

        [Test]
        public void GlobalShadowId()
        {
            var ldr = _compile(@"
function f as alias1, alias2(){}

function as alias3, alias4{}
");
            Assert.IsTrue(target.Functions.Contains("f"),"Function f not defined");
            var entryF = LookupSymbolEntry(ldr.Symbols, "F");
            Assert.IsNotNull(entryF,"No symbol table entry for `f` exists");
            Assert.IsTrue(entryF.Interpretation == SymbolInterpretations.Function,"Symbol f is not declared as a function");
            Assert.IsTrue(target.Functions.Contains(entryF.InternalId));

            var alias1 = LookupSymbolEntry(ldr.Symbols, "alias1");
            Assert.IsNotNull(alias1, "No symbol table entry for `alias1` exists");
            Assert.IsTrue(alias1.Interpretation == SymbolInterpretations.Function, "Symbol alias1 is not declared as a function");
            Assert.IsTrue(target.Functions.Contains(alias1.InternalId));
            Assert.AreSame(target.Functions[entryF.InternalId], target.Functions[alias1.InternalId]);
            Assert.IsFalse(target.Functions.Contains("alias1"));

            var alias2 = LookupSymbolEntry(ldr.Symbols, "alias2");
            Assert.IsNotNull(alias2, "No symbol table entry for `alias2` exists");
            Assert.IsTrue(alias2.Interpretation == SymbolInterpretations.Function, "Symbol alias2 is not declared as a function");
            Assert.IsTrue(target.Functions.Contains(alias2.InternalId));
            Assert.AreSame(target.Functions[entryF.InternalId], target.Functions[alias2.InternalId]);
            Assert.IsFalse(target.Functions.Contains("alias2"));

            var alias3 = LookupSymbolEntry(ldr.Symbols, "alias3");
            Assert.IsNotNull(alias3, "No symbol table entry for `alias3` exists");
            Assert.IsTrue(alias3.Interpretation == SymbolInterpretations.Function, "Symbol alias3 is not declared as a function");
            Assert.IsTrue(target.Functions.Contains(alias3.InternalId));
            Assert.IsFalse(target.Functions.Contains("alias3"));

            var alias4 = LookupSymbolEntry(ldr.Symbols, "alias4");
            Assert.IsNotNull(alias4, "No symbol table entry for `alias4` exists");
            Assert.IsTrue(alias4.Interpretation == SymbolInterpretations.Function, "Symbol alias4 is not declared as a function");
            Assert.IsTrue(target.Functions.Contains(alias4.InternalId));
            Assert.IsFalse(target.Functions.Contains("alias4"));

            Assert.AreSame(target.Functions[alias3.InternalId], target.Functions[alias4.InternalId]);
        }

        [Test]
        public void LocalShadowId()
        {
            var ldr = _compile(@"
function main()
{
    function f as alias1, alias2(){}
    function as alias3, alias4(){}
}
");
            var maint = ldr.FunctionTargets["main"];
            var main = target.Functions["main"];

            Assert.IsTrue(main.Variables.Contains("f"), "Variable f must be physically present.");
            Assert.IsFalse(main.Variables.Contains("alias1"), "There must be no variable named alias1");
            Assert.IsFalse(main.Variables.Contains("alias2"), "There must be no variable named alias2");
            Assert.IsFalse(main.Variables.Contains("alias3"), "There must be no variable named alias3");
            Assert.IsFalse(main.Variables.Contains("alias4"), "There must be no variable named alias4");

            // `f`
            Assert.IsTrue(maint.Symbols.Contains("f"));
            var f = LookupSymbolEntry(maint.Symbols, "f");
            Assert.AreEqual(f.Interpretation, SymbolInterpretations.LocalReferenceVariable);

            // `alias1`
            Assert.IsTrue(maint.Symbols.Contains("alias1"));
            var alias1 = LookupSymbolEntry(maint.Symbols, "alias1");
            Assert.AreEqual(alias1.Interpretation, SymbolInterpretations.LocalReferenceVariable);
            Assert.AreEqual(f.InternalId, alias1.InternalId);

            // `alias2`
            Assert.IsTrue(maint.Symbols.Contains("alias2"));
            var alias2 = LookupSymbolEntry(maint.Symbols, "alias2");
            Assert.AreEqual(alias2.Interpretation, SymbolInterpretations.LocalReferenceVariable);
            Assert.AreEqual(f.InternalId, alias2.InternalId);

            // `alias3`
            Assert.IsTrue(maint.Symbols.Contains("alias3"));
            var alias3 = LookupSymbolEntry(maint.Symbols, "alias3");
            Assert.AreEqual(alias3.Interpretation, SymbolInterpretations.LocalReferenceVariable);

            // `alias4`
            Assert.IsTrue(maint.Symbols.Contains("alias4"));
            var alias4 = LookupSymbolEntry(maint.Symbols, "alias4");
            Assert.AreEqual(alias4.Interpretation, SymbolInterpretations.LocalReferenceVariable);
            Assert.AreEqual(alias3.InternalId, alias4.InternalId);
        }

        [Test]
        public void PrxShowPrompt()
        {
            _compile(@"
declare function runInDifferentColor, readline;

function showPrompt(q) does
{
	declare var prompt\color as color;
	var originalColor;
    if(q == null)
        q = ""PRX> "";
    runInDifferentColor( () => print(q), 
        if(color != null) color else ::Console.ForegroundColor);
	return readline;
}
");

        }

        [Test]
        public void NestedDerivedCapture()
        {
            _compile(@"
function parent
{
    var x;
    coroutine nested = x;
    lazy nested = x;
}
");
        }

        [Test]
        public void GetCopyOfAstUnknown()
        {
            var unr = new AstUnresolved("file", 1, 2, "the_id");
            unr.Arguments.Add(new AstNull("file2",2,3));
            unr.Arguments.RightAppend(new AstNull("file3",3,4));
            unr.Arguments.ReleaseRightAppend();
            unr.Call = PCall.Set;

            var copy = unr.GetCopy();
            Assert.AreEqual(2, unr.Arguments.Count);
            Assert.IsInstanceOf(typeof(AstNull), copy.Arguments[0]);
            Assert.AreEqual("file3",copy.Arguments[1].File);
            Assert.AreEqual(PCall.Set,unr.Call);
            Assert.AreNotSame(unr,copy);
        }

        [Test]
        public void ArgumentListDuplicate()
        {
            //If the argument list of a AstGetSet complex contains successive references to the same node, it will 
            //  emit a duplicate instructions.
            //This shouldn't happen during ordinary operation, as the nodes may be equal, but not identical
            _compile(@"
declare command side_effect, target;
function main does
    target(side_effect, side_effect);
");

            Expect(@"
cmd.0 side_effect
cmd.0 side_effect
cmd.2 target
ret
");
        }

        [Test]
        public void NoClosureForSimpleLambda()
        {
            _compile(@"
function main = x => x.(16)+5; //more complicated than id or something partially applicable
");

            Expect(@"
ldr.func    main\0
ret
");
        }


        [Test]
        public void ObjectCreationFallback()
        {
            _compile(@"
declare function make_foo as create_foo;

function main(x,y)
{
    return new foo(x,y);
}

function make_foo(){}
");
            Expect(@"
ldloc x
ldloc y
func.2 make_foo
ret
");
        }

        [Test]
        public void ObjectCreationFallbackWithTypeArgs()
        {
            _compile(@"
declare function make_foo as create_foo;
function make_foo()[pxs\supportsTypeArguments]{}
function main(x,y)
{
    return new foo<1,""a"">(x,y);
}


");
            Expect(@"
ldc.int 2
ldc.int 1
ldc.string ""a""
ldloc x
ldloc y
func.5 make_foo
ret
");
        }

        [Test]
        public void CustomTypeConversion()
        {
            _compile(@"
function convert_to_foo as to_foo(){};
function main(x) {
    return x~foo;
}
");
            Expect(@"
ldloc x
func.1 convert_to_foo
ret.value
");
        }

        [Test]
        public void CustomTypeConversionWithTypeArgs()
        {
            _compile(@"
function convert_to_foo as to_foo()[pxs\supportsTypeArguments]{};
function main(x) {
    return x~foo<1, ""a"">;
}
");
            Expect(@"
ldc.int 2
ldc.int 1
ldc.string ""a""
ldloc x
func.4 convert_to_foo
ret.value
");
        }

        [Test]
        public void CustomTypeCheck()
        {
            _compile(@"
function check_foo as is_foo(){}
function main(x) {
    println(x is not foo);
    return x is foo;
}
");
            Expect(@"
ldloc x
func.1 check_foo
not
@cmd.1 println
ldloc x
func.1 check_foo
ret.value
");
        }

        [Test]
        public void CustomTypeCheckWithTypeArgs()
        {
            _compile(@"
function check_foo as is_foo()[pxs\supportsTypeArguments]{}
function main(x) {
    println(x is not foo<3, ""a"">);
    return x is foo<1, ""b"">;
}
");
            Expect(@"
ldc.int 2
ldc.int 3
ldc.string ""a""
ldloc x
func.4 check_foo
not
@cmd.1 println
ldc.int 2
ldc.int 1
ldc.string ""b""
ldloc x
func.4 check_foo
ret.value
");
        }

        [Test]
        public void CustomTypeStaticCall()
        {
            _compile(@"
function static_foo as static_call_foo(){}
function main(x,y) {
    ~foo.the_property = x;
    return ~foo.the_method(x,y);
}
");
            Expect(@"
 ldc.string ""the_property""
 ldloc x
@func.2 static_foo
 
 ldc.string ""the_method""
 ldloc x
 ldloc y
 func.3 static_foo
 ret.value
");
        }

        [Test]
        public void CustomTypeStaticCallWithTypeArgs()
        {
            _compile(@"
function static_foo as static_call_foo()[pxs\supportsTypeArguments]{}
function main(x,y) {
    ~foo<1, ""a"">.the_property = x;
    return ~foo<3, ""b"">.the_method(x,y);
}
");
            Expect(@"
 ldc.int 2
 ldc.int 1
 ldc.string a
 ldc.string ""the_property""
 ldloc x
@func.5 static_foo
 
 ldc.int 2
 ldc.int 3
 ldc.string b
 ldc.string ""the_method""
 ldloc x
 ldloc y
 func.6 static_foo
 ret.value
");
        }

        [Test]
        public void LocalAliasDeclaration()
        {
            _compile(@"
var g;
function main()
{
    declare g as f;
    declare( h = sym ""g"" );
    return f + h;
}
");

            Expect(@"
ldglob g
ldglob g
add
ret
");
        }

        [Test]
        public void ErrorSymbolOnlyGeneratesOneMessage()
        {
            var loader = CompileWithErrors(@"
declare(
    invalid = error(pos(""some-file"",1,2),""T.example"",""Zhee-Error-Message"",null)
);

function main = invalid;
");

            Assert.That(loader.ErrorCount,Is.EqualTo(1));
            var message = loader.Errors[0];
            Assert.That(message.MessageClass,Is.EqualTo("T.example"));
            Assert.That(message.Text,Is.EqualTo("Zhee-Error-Message"));
        }

    }
}
