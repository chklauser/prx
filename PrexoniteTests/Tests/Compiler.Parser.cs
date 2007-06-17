using System;
using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prx.Tests
{
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            //No instructions have been emitted
            Assert.AreEqual(0, ldr.FunctionTargets["func0"].Function.Code.Count);

            CompilerTarget tar = ldr.FunctionTargets["func0"];

            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj0"), tar.Symbols["obj0"]);
            Assert.IsTrue(tar.Function.Variables.Contains("obj0"));
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj1"), tar.Symbols["obj1"]);
            Assert.IsTrue(tar.Function.Variables.Contains("obj1"));
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalObjectVariable, "obj2"), tar.Symbols["obj2"]);
            Assert.IsTrue(tar.Function.Variables.Contains("obj2"));

            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "func1"), tar.Symbols["func1"]);
            Assert.IsTrue(tar.Function.Variables.Contains("func1"));
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "ifunc0"),
                            tar.Symbols["ifunc0"]);
            Assert.IsTrue(tar.Function.Variables.Contains("ifunc0"));
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.LocalReferenceVariable, "cor0"), tar.Symbols["cor0"]);
            Assert.IsTrue(tar.Function.Variables.Contains("cor0"));

            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "gobj0"), tar.Symbols["gobj0"]);
            Assert.IsFalse(tar.Function.Variables.Contains("gobj0"),
                           "\"declare var <id>;\" only declares a global variable.");
            Assert.IsFalse(ldr.Options.TargetApplication.Variables.ContainsKey("gobj0"),
                           "\"global <id>;\" only declares a global variable.");
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "gobj1"), tar.Symbols["gobj1"]);
            Assert.IsFalse(tar.Function.Variables.Contains("gobj1"),
                           "\"declare var <id>;\" only declares a global variable.");
            Assert.IsFalse(ldr.Options.TargetApplication.Variables.ContainsKey("gobj1"),
                           "\"declare var <id>;\" only declares a global variable.");

            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "func0\\static\\sobj0"),
                            tar.Symbols["sobj0"]);
            Assert.IsTrue(ldr.Options.TargetApplication.Variables.ContainsKey("func0\\static\\sobj0"));
            Assert.AreEqual(new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "func0\\static\\sobj1"),
                            tar.Symbols["sobj1"]);
            Assert.IsTrue(ldr.Options.TargetApplication.Variables.ContainsKey("func0\\static\\sobj1"));
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            //Check AST
            AstBlock block = ldr.FunctionTargets["func0"].Ast;

            //label begin
            int i = 0;
            Assert.IsInstanceOfType(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("begin", (block[i] as AstExplicitLabel).Label);

            //instruction
            i++;
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            Assert.AreEqual(SymbolInterpretations.Function, (block[i] as AstGetSetSymbol).Interpretation);
            Assert.AreEqual("instruction", (block[i] as AstGetSetSymbol).Id);
            Assert.AreEqual(PCall.Get, (block[i] as AstGetSetSymbol).Call);
            Assert.AreEqual(0, (block[i] as AstGetSetSymbol).Arguments.Count);

            //goto fith
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("fifth", (block[i] as AstExplicitGoTo).Destination);

            //label third
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("third", (block[i] as AstExplicitLabel).Label);

            //instruction
            i++;
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            Assert.AreEqual(SymbolInterpretations.Function, (block[i] as AstGetSetSymbol).Interpretation);
            Assert.AreEqual("instruction", (block[i] as AstGetSetSymbol).Id);
            Assert.AreEqual(PCall.Get, (block[i] as AstGetSetSymbol).Call);
            Assert.AreEqual(0, (block[i] as AstGetSetSymbol).Arguments.Count);

            //label fourth
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("fourth", (block[i] as AstExplicitLabel).Label);

            //goto sixth
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("sixth", (block[i] as AstExplicitGoTo).Destination);

            //label fifth
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("fifth", (block[i] as AstExplicitLabel).Label);

            //goto fourth
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("fourth", (block[i] as AstExplicitGoTo).Destination);

            //label sixth
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitLabel), block[i]);
            Assert.AreEqual("sixth", (block[i] as AstExplicitLabel).Label);

            //goto begin
            i++;
            Assert.IsInstanceOfType(typeof(AstExplicitGoTo), block[i]);
            Assert.AreEqual("begin", (block[i] as AstExplicitGoTo).Destination);

            Console.WriteLine(target.StoreInString());
        }

        [Test]
        public void Arguments()
        {
            const string input1 =
                @"
function func0
{
    declare function func1;
    func1;
    func1(1);
    func1(""2"", true, 3.41);
    func1(func1(func1(55)));
}
";
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            //Check AST
            int i = 0;
            AstGetSetSymbol symbol;

            AstBlock block = ldr.FunctionTargets["func0"].Ast;

            //func1;
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            symbol = (AstGetSetSymbol) block[i];
            Assert.AreEqual("func1", symbol.Id);
            Assert.AreEqual(SymbolInterpretations.Function, symbol.Interpretation);
            Assert.AreEqual(0, symbol.Arguments.Count, "First function call should have no arguments");
            i++;
            //func1(1);
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            symbol = (AstGetSetSymbol) block[i];
            Assert.AreEqual("func1", symbol.Id);
            Assert.AreEqual(SymbolInterpretations.Function, symbol.Interpretation);
            Assert.AreEqual(1, symbol.Arguments.Count, "First function call should have 1 argument");
            Assert.IsInstanceOfType(typeof(AstConstant), symbol.Arguments[0]);
            Assert.AreEqual(1, (int) ((AstConstant) symbol.Arguments[0]).Constant);
            i++;
            //func1("2", true, 3.41);
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            symbol = (AstGetSetSymbol) block[i];
            Assert.AreEqual("func1", symbol.Id);
            Assert.AreEqual(SymbolInterpretations.Function, symbol.Interpretation);
            Assert.AreEqual(3, symbol.Arguments.Count, "First function call should have 3 arguments");
            Assert.IsInstanceOfType(typeof(AstConstant), symbol.Arguments[0]);
            Assert.AreEqual("2", (string) ((AstConstant) symbol.Arguments[0]).Constant);
            Assert.IsInstanceOfType(typeof(AstConstant), symbol.Arguments[1]);
            Assert.AreEqual(true, (bool) ((AstConstant) symbol.Arguments[1]).Constant);
            Assert.IsInstanceOfType(typeof(AstConstant), symbol.Arguments[2]);
            Assert.AreEqual(3.41, (double) ((AstConstant) symbol.Arguments[2]).Constant);
            i++;
            //func1(func1(func1(55)));
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), block[i]);
            symbol = (AstGetSetSymbol) block[i];
            Assert.AreEqual("func1", symbol.Id);
            Assert.AreEqual(SymbolInterpretations.Function, symbol.Interpretation);
            Assert.AreEqual(1, symbol.Arguments.Count, "First function call should have 1 argument");
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), symbol.Arguments[0]);
            AstGetSetSymbol sub = symbol.Arguments[0] as AstGetSetSymbol;
            Assert.AreEqual(1, sub.Arguments.Count);
            Assert.IsInstanceOfType(typeof(AstGetSetSymbol), sub.Arguments[0]);
            sub = sub.Arguments[0] as AstGetSetSymbol;
            Assert.AreEqual(1, sub.Arguments.Count);
            Assert.IsInstanceOfType(typeof(AstConstant), sub.Arguments[0]);
            Assert.AreEqual(55, (int) ((AstConstant) sub.Arguments[0]).Constant);
        }

        [Test]
        public void Expressions()
        {
            const string input1 =
                @"
declare function func1;

function func0
{
    var x;
    var y;
    x = 1;
    y = 2 * x + 1;
    x += 0 + y * 1;
}";

            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            List<Instruction> actual = target.Functions["func0"].Code;
            List<Instruction> expected =
                getInstructions(
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
stloc    x"
                    );

            Console.Write(target.StoreInString());

            Assert.AreEqual(expected.Count, actual.Count, "Expected and actual instruction count missmatch.");

            for (int i = 0; i < actual.Count; i++)
                Assert.AreEqual(expected[i], actual[i],
                                String.Format("Instructions at address {0} do not match ({1} != {2})", i,
                                              expected[i], actual[i]));
        }

        [Test]
        public void IncrementDecrement()
        {
            const string input1 =
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
    
}";

            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            List<Instruction> actual = target.Functions["func0"].Code;
            List<Instruction> expected =
                getInstructions(
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
stloc    x"
                    );

            Console.Write(target.StoreInString());

            Assert.AreEqual(expected.Count, actual.Count, "Expected and actual instruction count missmatch.");

            for (int i = 0; i < actual.Count; i++)
                Assert.AreEqual(expected[i], actual[i],
                                String.Format("Instructions at address {0} do not match ({1} != {2})", i,
                                              expected[i], actual[i]));
        }

        [Test]
        public void ReturnStatements()
        {
            const string input1 =
                @"
function func0
{
    var x;
    var y;
    return;
    return x;
    return = 6 + 7;
    break;
    continue;
}";

            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            List<Instruction> actual = target.Functions["func0"].Code;
            List<Instruction> expected =
                getInstructions(@"
ret.exit
ldloc   x
ret.value
ldc.int 13
ret.set
ret.break
ret.continue
");
            Console.Write(target.StoreInString());

            Assert.AreEqual(expected.Count, actual.Count, "Expected and actual instruction count missmatch.");

            for (int i = 0; i < actual.Count; i++)
                Assert.AreEqual(expected[i], actual[i],
                                String.Format("Instructions at address {0} do not match ({1} != {2})", i,
                                              expected[i], actual[i]));
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
}
");

            _expect(@"test\static",
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

            _expect(
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

    asm nop+end;
}
");
            _expect(
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
Beginning:; asm nop+Beginning;
    if (u And ( v Or ( ( w Or x ) And ( y And z) ) ))
        action1;
    else if( ( u Or v Or w ) And ( x Or y Or z ))
        action2;
    else
        goto Beginning;

    asm nop+Loop;

    do action1; while ( ( u And v ) Or x);
}
");
            _expect(
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
    asm nop+AndAA;

    if (x And y)
        action1;
    else
        action2;

    asm nop+OrAJ;

    if (x Or y)
        action1;
    else 
        goto AndAA;

    NotAndOrJA:;
    asm nop+NotAndOrJA;

    unless (x And ( y Or z ))
        goto AndAA;
    else
        action2;

    asm nop+OrAndJJ;
    
    if (x Or ( y And z ))
        goto NotAndOrJA;
    else
        goto AndAA;

    asm nop+END;
}
");
            _expect(
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

            _expect(
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

            _expect(@"
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
}
");
            _expect(@"
 ldloc   str
 ldloc   idx
 get.1   """"
@func.1  println
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

    if(3 == x) asm inc x;
}
");
            _expect(
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

            _expect(
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
            string asmInput =
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
            _expect("main", asmInput);
            _expect("main_extended", asmInput);
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
            _expect(
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
}
");
            _expect(
                @"
ldc.string  ""Hello""
@cmd.1       print
ldc.string  ""Hello2""
@cmd.1       println
ldc.string  ""The good old echo""
@cmd.1      println
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
    
    asm nop+COMPLICATED;

    var buffer = new ::StringBuilder;
    foreach(buffer.Append in lst.ToArray);
}
");
            List<Instruction> code = target.Functions["main"].Code;
            Assert.IsTrue(code.Count > 18, "Resulting must be longer than 18 instructions");
            string enum1 = code[3].Id ?? "No_ID_at_2";
            string enum2 = code[25].Id ?? "No_ID_at_18";
            _expect(
                @"
var lst
var buffer
var enum1
var enum2

                        ldloc   lst
                        get.0   GetEnumerator
                        cast.const  ""Object(\""System.Collections.IEnumerator\"")""
                        stloc   " +
                enum1 + @"
                        jump    enum1\continue

label enum1\begin       ldloc   " + enum1 +
                @"
                        get.0   Current
                        stloc   e

                        ldloc   e
                        @cmd.1  println

label enum1\continue    ldloc   " +
                enum1 +
                @"
                        get.0   MoveNext
                        jump.t  enum1\begin
label enum1\break       ldloc   "+enum1+@"
                       @cmd.1   dispose
                        leave   endTry
                        exception
                        throw
label endTry            nop+COMPLICATED
                                    
                        newobj.0    ""Object(\""StringBuilder\"")""
                        stloc   buffer
                        ldloc   lst
                        get.0   ToArray
                        
                        get.0   GetEnumerator
                        cast.const  ""Object(\""System.Collections.IEnumerator\"")""
                        stloc   " +
                enum2 + @"
                        jump    enum2\continue

label enum2\begin       ldloc   buffer
                        ldloc   " +
                enum2 +
                @"
                        get.0   Current
                        set.1   Append
label enum2\continue    ldloc   " +
                enum2 + @"
                        get.0   MoveNext
                        jump.t  enum2\begin
                        ldloc   " + enum2 + @"
                       @cmd.1   dispose
                        leave   endTry2
                        exception
                        throw
label endTry2
");
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
}
");
            _expect(@"
ldglob  ekoe
@cmd.1   print
");
        }

        [Test]
        public void References()
        {
            _compile(
                @"
declare function f, g, h;

ref gprime = ->g;

function main
{
    var hprime = ->h;
    ref fprime = ->f;

    ref primes = ~List.Create(->f, ->gprime, hprime);    
    ref hprime;

    print = fprime;
    print = gprime;
    print = hprime;
    print = primes;
}
");
            _expect(
                @"
var fprime
//declare var gprime
var hprime

ldr.func    h
stloc       hprime
ldr.func    f
stloc       fprime

ldr.func    f
ldglob      gprime
ldloc       hprime
sget.3      ""List::Create""
stloc       primes

indloc.0    fprime
@cmd.1      print
indglob.0   gprime
@cmd.1      print
indloc.0    hprime
@cmd.1      print
indloc.0    primes
@cmd.1      print
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
}
");
            _expect(
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
    var w = x => { var d = x+2; return = d * 4; };
}
");
            _expect(@"main\nested\0", @"ldc.int 5861 ret.value");
            _expect(@"main\nested\1", @"ldc.int 2 ldloc x mul ret.value");
            _expect(@"main\nested\2", @"ldloc x ldloc y add indloc.1 z ret.value");
            _expect(@"main\nested\3", @"var d ldloc x ldc.int 2 add stloc d ldloc d ldc.int 4 mul ret.set");
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
    var u = () => { ref a; a; };
}
");

            _expect(@"main\nested\0", @"
ldloc   x
ldloc   a
add
ret.value
");
            PFunction func = target.Functions[@"main\nested\0"];
            Assert.AreEqual(1, func.Meta[PFunction.SharedNamesKey].List.Length);
            Assert.AreEqual("a", func.Meta[PFunction.SharedNamesKey].List[0].Text);

            _expect(@"main\nested\1", @"
@indloc.0  a
");
            func = target.Functions[@"main\nested\1"];
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
}
");

            _expectSharedVariables(@"main\nested\N10", "a");
            _expectSharedVariables(@"main\nested\N21");

            _expect(
                @"
var         a
var         N1
var         N2

newclo      main\nested\N10
stloc       N1

ldc.int     4
@indloc.1   N1

newclo      main\nested\N21
stloc       N2

ldc.int     5
newclo      main\nested\2
@indloc.2   N2
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
}
");
            _expect(
                @"
newclo  main\nested\0
stloc   f
ldloc   f
stloc   fobj
ldr.loc f
stloc   fvar
newclo  main\nested\1
indloc.1 fvar

indloc.0 fvar
stloc fobj2

@indloc.0 fobj
@indloc.0 f
@indloc.0 fobj2
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
}
");

            _expect(
                @"
newclo      main\nested\0
stloc       fobj
newclo      main\nested\1
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
");
        }

        [Test]
        public void StringConcatenation()
        {
            _compile(@"
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
}
");

            _expect(@"
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
cmd.3       concat
stloc       x

ldc.string  ""a""
ldloc       b
ldloc       c
ldc.string  ""ad""
cmd.4       concat
stloc       x
");
        }

        [Test]
        public void StringInterpolationIdentifier()
        {
            _compile(@"
function main()
{
    var x; var a; var b; var c;

    x = ""I think the first parameter is $a, while the second seems like ($b) $c"";    
}
");
            _expect(@"
var x,a,b,c

ldc.string  ""I think the first parameter is ""
ldloc       a
ldc.string  "", while the second seems like (""
ldloc       b
ldc.string  "") ""
ldloc       c
cmd.6       concat
stloc       x
");
        }

        [Test]
        public void StringInterpolationExpression()
        {
            _compile(@"
function main()
{
    var x; var a; var b; var c;

    x = ""I think the first parameter is $(a) while the second seems like $(c.Substring(5,4).Length~String) ($(b.()))"";
}
");

            _expect(@"
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
cmd.7       concat
stloc       x
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
}
");
            _expect(@"
ldc.string ""AB""
cmd.0       a
ldc.string  ""CD""
cmd.3       concat
@cmd.1      print
");


        }

        [Test]
        public void ConditionalExpression()
        {
            _compile(@"
function max(a,b) = a > b ? a : b;
function maxv(a,b) = 
    if(a > b) 
        a 
    else 
        b;

function main(x)
{
    x = x mod 2 == 0 ? (x > 0 ? x : -x) : max(x,2);
    return x is Null ? 0 : x == """" ? -1 : x.Length;
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
                jump    endif
label else      ldloc   b
label endif     ret.value
";
            _expect("max", emax);
            _expect("maxv", emax);

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
                check.const     ""Null""
                jump.f          else3
                ldc.int         0
                jump            endif3
label else3     ldloc           x
                ldc.string      """"
                ceq
                jump.f          else4
                ldc.int         -1
                jump            endif4
label else4     ldloc           x
                get.0           Length
label endif4    //optimized://  jump            endif3
label endif3    ret.value
";

            _expect("main", emain);
            _expect("mainv", emain);
        }

        [Test]
        public void StringInterpolationWithStrings()
        {
            _compile(@"
function main(x)
{
    declare command transform;
    return ""There is $(transform(""no"")) spoon"";
}   
");
            _expect(@"
ldc.string  ""There is ""
ldc.string  ""no""
cmd.1       transform
ldc.string  "" spoon""
cmd.3       concat
ret.value
");
        }

        [Test]
        public void ProceduralStructureDefinition()
        {
            _compile(@"
function main(a,b,c)
{
    var str = new Structure<""a"",""b"",""r"",""c"">();
}
");
            _expect(@"
newobj.0    ""Structure(\""a\"",\""b\"",\""r\"",\""c\"")""
stloc       str
");
        }

        [Test]
        public void OneDividedByX()
        {
            _compile(@"
function main(x) = 1 / x;
");
            _expect(@"
ldc.int 1
ldloc   x
div
ret.value
");
        }

        [Test]
        public void ListLiteral()
        {
            _compile(@"
function main()
{
    var x = [];
    var y = [1,[4,5],8,[10],11];
}
");

            _expect(@"
var x,y
newobj.0 ""List""
stloc   x
ldc.int 1
ldc.int 4
ldc.int 5
sget.2  ""List::Create""
ldc.int 8
ldc.int 10
sget.1  ""List::Create""
ldc.int 11
sget.5  ""List::Create""
stloc   y
");
        }

        [Test]
        public void CoroutineCreation()
        {
            _compile(@"
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
}
");
            _expect(@"
var lst, oneToFive, even
ldr.func    subrange
ldloc       lst
ldc.int     1
ldc.int     5
newcor.3
stloc       oneToFive
newclo      main\nested\0
newcor.0
stloc       even      
");

            _expect(@"subrange",@"
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
            _compile(@"
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
}
");
            _expect(@"
ldglob  A
@cmd.1  println
");
        }

        [Test]
        public void CoroutineFunctionDefinitions()
        {
            _compile(@"
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

            _expect(@"
var skip
newclo  main\nested\skip0
stloc   skip

ldc.int 1
ldc.int 2
ldc.int 3
sget.3  ""List::Create""
ldc.int 1
indloc.2 skip
newclo  main\nested\1
func.2  where
ret.value
");

            _expect("where",@"
newclo  where\nested\0
newcor.0
ret.value
");
            _expect(@"main\nested\skip0",@"
newclo  main\nested\skip0\nested\0
newcor.0
ret.value
");
        }

        [Test]
        public void TryCatchFinallySimple()
        {
            _compile(@"
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
            _expect(@"
var 
    handle,
    exc

label beginTry      func.0  createHandle
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
label endTry
");
        }

        [Test]
        public void TryCatchFinallyNested()
        {
            _compile(@"
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
        catch
        {
            log(""FATAL"");
            shutdown;
        }
    }
}
");
            _expect(@"
var 
    handle,
    exc

label beginTry      func.0  open
                    stloc   handle
                   @func.0  fail
label beginFinally  
label beginTry2     ldloc   handle
                   @func.1  close

label beginFinally2 leave   endTry2
label beginCatch2   ldc.string
                            ""FATAL""
                   @func.1  log
                   @func.0  shutdown
label endTry2       leave   endTry
label beginCatch    exception
                    stloc   exc
                    ldloc   exc
                   @func.1  log
label endTry
");
        }

        [Test]
        public void TryFinally()
        {
            _compile(@"
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
            _expect(@"
var 
    handle,
    exc

label beginTry      func.0  createHandle
                    stloc   handle
//...work
                   @func.0  fail
label beginFinally
                    ldloc   handle
                   @func.1  closeHandle
                    leave   endTry
label beginCatch
                    exception
                    throw
label endTry
");
        }

        [Test]
        public void Throw()
        {
            _compile(@"
function main()
{
    throw ""There must be a mistake!"";
    throw new System::Exception(""There IS a mistake!"");
    throw 3;
}
");
            _expect(@"
ldc.string  ""There must be a mistake!""
throw

ldc.string  ""There IS a mistake!""
newobj.1    ""Object(\""System.Exception\"")""
throw

ldc.int     3
throw
");
        }

        [Test]
        public void Using()
        {
            _compile(@"
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
            _expect(@"
var h
label beginTry  ldr.loc h
                func.0  createHandle
               @func.2  handle
                ldloc   h
                ldc.int 15
               @func.2  write
label beginFinally
                ldr.loc h
                func.1  handle
               @cmd.1   dispose
                leave   endTry
label beginCatch
                exception
                throw
label endTry                    
");
        }

        [Test]
        public void KeyValuePairs()
        {
            _compile(@"
function main()
{
    var x; var y; var z;
    var a; var b; var c;
    x = a: b;
    y = a: b: c;
    z = (a: b): (c: a): b;
}
");

            _expect(@"
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
");
        }

        [Test]
        public void HashLiteral()
        {
            _compile(@"

function main()
{
    var a; var b; var c; var d;
    var hset = {    a: b, 
                    c: d, 
                    5: a, 
                    ""hello"": ""ciao"",
                    (a > b ? a : b): (c > d ? c : d)
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
}
");

            _expect(@"
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
");
        }
    }
}