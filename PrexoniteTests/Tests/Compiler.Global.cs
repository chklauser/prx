using System;
using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;

namespace Prx.Tests
{
    [TestFixture]
    public class CompilerGlobal : Compiler
    {
        #region Metadata

        [Test]
        public void Empty()
        {
            _compile(
                @"
//Hello World

/* this is // a 
multi line // comment */

//*/ this is single line

///* this too

/* multiline
 multiline 
//*/// single line
");
        }

        [Test()]
        public void Metadata()
        {
            Loader ldr =
                _compile(
                    @"
//This is a text fixture for metadata

//Simple
Name	metadata\Fixture;
Description Not_yet_very_useful;
Version	6;
TheSwitch True;

Debugging enabled;
Optimization disabled;
Is Cached;
Is Not Final;

//The following metadata is checked in other tests

Add System to Import;
Add System::Text to Import;
Add {
	System::Xml,
	Prexonite,
	System::Xml::Xsl
} to Imports;
");

            Assert.AreEqual(0, ldr.ErrorCount);

            Assert.AreEqual("metadata\\Fixture", target.Meta["Id"].Text);
            Assert.AreEqual("Not_yet_very_useful", target.Meta["deScRiPtIoN"].Text);
            Assert.AreEqual("6", target.Meta["Version"].Text);
            Assert.AreEqual(true, target.Meta["theswitch"].Switch);

            Assert.IsTrue(target.Meta["debugging"]);
            Assert.IsFalse(target.Meta["optimization"]);
            Assert.IsTrue(target.Meta["caChed"]);
            Assert.IsFalse(target.Meta["finaL"]);
        }

        [Test()]
        public void NestedMetadata()
        {
            const string input1 =
                "FirstList { \"first String\", firstId, true, 55 } ;" +
                "SecondList { \"second String\", { \"first }}Nested{ string\", secondNestedId }, thirdId }; ";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);

            Assert.AreEqual(0, ldr.ErrorCount);

            //First list
            Assert.IsTrue(target.Meta.ContainsKey("firstList"), "firstList missing");
            Assert.IsTrue(target.Meta["firstList"].IsList, "firstList should be a list");
            Assert.AreEqual("first String", target.Meta["firstList"].List[0].Text);
            Assert.AreEqual("firstId", target.Meta["firstList"].List[1].Text);
            Assert.AreEqual(true, target.Meta["firstList"].List[2].Switch);
            Assert.AreEqual("55", target.Meta["firstList"].List[3].Text);

            //Second list
            Assert.IsTrue(target.Meta.ContainsKey("secondList"), "secondList missing");
            Assert.IsTrue(target.Meta["secondList"].IsList, "secondList should be a list");
            Assert.AreEqual("second String", target.Meta["secondList"].List[0].Text);
            Assert.IsTrue(
                target.Meta["secondList"].List[1].IsList, "second element should be a list");
            Assert.AreEqual(
                "first }}Nested{ string", target.Meta["secondList"].List[1].List[0].Text);
            Assert.AreEqual("secondNestedId", target.Meta["secondList"].List[1].List[1].Text);
            Assert.AreEqual("thirdId", target.Meta["secondList"].List[2].Text);
        }

        [Test()]
        public void AddingMetadata()
        {
            const string input1 =
                "MyList { elem1, elem2, elem3 };";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);

            Assert.AreEqual(0, ldr.ErrorCount);

            Assert.IsTrue(target.Meta.ContainsKey("MyList"), "MyList missing");
            Assert.IsTrue(target.Meta["MyList"].IsList, "MyList should be a list");
            Assert.AreEqual(3, target.Meta["MyList"].List.Length);
            Assert.AreEqual("elem2", target.Meta["MyList"].List[1].Text);

            const string input2 =
                "Add elem4 to MyList;";
            ldr.LoadFromString(input2);

            Assert.AreEqual(0, ldr.ErrorCount);

            Assert.IsTrue(target.Meta.ContainsKey("MyList"), "MyList missing after modification");
            Assert.IsTrue(
                target.Meta["MyList"].IsList, "MyList should be a list, even after modification");
            Assert.AreEqual(4, target.Meta["MyList"].List.Length);
            Assert.AreEqual("elem4", target.Meta["MyList"].List[3].Text);

            const string input3 =
                "Add { elem5, elem6 } to MyList;";
            ldr.LoadFromString(input3);

            Assert.AreEqual(0, ldr.ErrorCount);

            Assert.IsTrue(
                target.Meta.ContainsKey("MyList"), "MyList missing after 2nd modification");
            Assert.IsTrue(
                target.Meta["MyList"].IsList, "MyList should be a list, even after 2nd modification");
            Assert.AreEqual(6, target.Meta["MyList"].List.Length);
            Assert.AreEqual("elem6", target.Meta["MyList"].List[5].Text);

            const string input4 =
                @"Import 
{
    System,
    System::Text
};

Add System::Xml to Imports;
";
            ldr.LoadFromString(input4);
            Assert.AreEqual(0, ldr.ErrorCount, "There were errors during compilator of input4");
            Assert.IsTrue(target.Meta.ContainsKey("Import"), "Import missing");
            Assert.IsTrue(
                target.Meta["Import"].IsList, "Import should be a list after 2nd modification");
            Assert.AreEqual(3, target.Meta["Import"].List.Length);
            Assert.AreEqual("System", target.Meta["Import"].List[0].Text);
            Assert.AreEqual("System.Text", target.Meta["Import"].List[1].Text);
            Assert.AreEqual("System.Xml", target.Meta["Import"].List[2].Text);
        }

        [Test()]
        public void Declare()
        {
            const string input =
                @"declare function f\1; " +
                @"declare var go\1; " +
                @"declare ref gf\1, gf\2; " +
                @"declare function if\1;";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input);
            Assert.AreEqual(0, ldr.ErrorCount);
            SymbolTable<SymbolEntry> symbols = ldr.Symbols;

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "f\\1"), symbols[@"f\1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "go\\1"),
                symbols[@"go\1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "gf\\1"),
                symbols[@"gf\1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "gf\\2"),
                symbols[@"gf\2"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "if\\1"), symbols[@"if\1"]);
        }

        [Test()]
        public void Redeclare()
        {
            const string input =
                "declare function name1; " +
                "declare var name2; " +
                "declare ref name1, name2; " +
                "declare function name1;";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input);
            Assert.AreEqual(0, ldr.ErrorCount);
            SymbolTable<SymbolEntry> symbols = ldr.Symbols;

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "name1"), symbols["name1"]);
            Assert.AreNotEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "name2"),
                symbols["name2"]);
            Assert.AreNotEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "name1"),
                symbols["name1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "name2"),
                symbols["name2"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "name1"), symbols["name1"]);

            const string input2 =
                "declare function name1; " +
                "declare function name2; ";

            ldr.LoadFromString(input2);
            Assert.AreEqual(0, ldr.ErrorCount);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "name1"), symbols["name1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.Function, "name2"), symbols["name2"]);
        }

        [Test()]
        public void DefineGlobal()
        {
            //First declare the variables

            const string input1 =
                "declare var name1; " +
                "declare ref name2; " +
                "declare var value;";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(
                0, ldr.ErrorCount, "The compiler reported errors in the first chunk of code.");
            SymbolTable<SymbolEntry> symbols = ldr.Symbols;

            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "name1"),
                symbols["name1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "name2"),
                symbols["name2"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "value"),
                symbols["value"]);

            //Then define them
            const string input2 =
                "var name1; " +
                "ref name2 [ Description NotUseful; ]; " +
                "var name3;";

            ldr.LoadFromString(input2);
            Assert.AreEqual(
                0, ldr.ErrorCount, "The compiler reported errors in the second chunk of code.");
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "name1"),
                symbols["name1"]);
            Assert.IsNotNull(target.Variables["name1"]);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalReferenceVariable, "name2"),
                symbols["name2"]);
            Assert.IsNotNull(target.Variables["name2"]);
            Assert.AreEqual("NotUseful", target.Variables["name2"].Meta["description"].Text);
            Assert.AreEqual(
                new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, "name3"),
                symbols["name3"]);
            Assert.IsNotNull(target.Variables["name3"]);
        }

        [Test()]
        public void Unicode()
        {
            _compile(@"
Name �berreden;
Description ""K�nste des �berredens von Kr�hen."";
");

            Assert.AreEqual("�berreden", target.Meta["Name"].Text);
            Assert.AreEqual("K�nste des �berredens von Kr�hen.", target.Meta["Description"].Text);
        }

        #endregion

        #region Definitions

        [Test]
        public void ExpressionFunctions()
        {
            const string input1 = @"
function twice(x) = 2*x;
function megabyte = 1024*1024;
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            //check "twice"
            List<Instruction> actual = target.Functions["twice"].Code;
            List<Instruction> expected = getInstructions(@"
ldc.int 2
ldloc x
mul
ret.value
");

            Console.Write(target.StoreInString());

            Assert.AreEqual(
                expected.Count,
                actual.Count,
                "Expected and actual instruction count missmatch in twice.");

            for (int i = 0; i < actual.Count; i++)
                Assert.AreEqual(
                    expected[i],
                    actual[i],
                    String.Format(
                        "Twice: Instructions at address {0} do not match ({1} != {2})",
                        i,
                        expected[i],
                        actual[i]));

            //check "megabyte"
            actual = target.Functions["megabyte"].Code;
            expected = getInstructions(@"
ldc.int 1048576
ret.value
");

            Assert.AreEqual(
                expected.Count,
                actual.Count,
                "Expected and actual instruction count missmatch in megabyte.");

            for (int i = 0; i < actual.Count; i++)
                Assert.AreEqual(
                    expected[i],
                    actual[i],
                    String.Format(
                        "Megabyte: Instructions at address {0} do not match ({1} != {2})",
                        i,
                        expected[i],
                        actual[i]));
        }

        #endregion

        #region Assembler

        [Test]
        public void AssemblerOptimization()
        {
            _compile(
                @"
function main does asm
{
    var x

//Constant Jump Target Propagation
    
            ldloc   x
            jump.t  a

            ldloc   x
            jump.f  b

            ldloc   x
            jump    c

            nop
            nop

label   a   jump    a2
label   b   jump    b2
label   c   jump    c2

label   a2  nop+a
label   b2  nop+b
label   c2  jump    c3

            nop
            nop

label   c3  nop+c

            nop
            nop
    
            jump    CJRI

//Conditional Jump ReInversion

label  CJRI jump.f  d
            jump    de
label   d   nop+d
            nop
label   de  nop+de

            nop
            nop

            jump    e
            jump    ee
label   e   nop+e
            nop
label   ee  nop+ee

            nop
            nop

            jump.t  f
            jump    fe
label   f   nop+f
            nop
label   fe  nop+fe

            nop
            nop

}
");

            _expect(
                @"
            var x

            ldloc   x
            jump.t  a2

            ldloc   x
            jump.f  b2

            ldloc   x
            jump    c3

            nop
            nop

label   a   jump    a2
label   b   jump    b2
label   c   jump    c2

label   a2  nop+a
label   b2  nop+b
label   c2  jump    c3

            nop
            nop

label   c3  nop+c

            nop
            nop

//Conditional Jump ReInversion

            jump.t  de
            nop+d
            nop
label   de  nop+de

            nop
            nop

            jump    e
            jump    ee
label   e   nop+e
            nop
label   ee  nop+ee

            nop
            nop

            jump.f  fe
            nop+f
            nop
label   fe  nop+fe

            nop
            nop
");
        }

        [Test()]
        public void EmptyAsmFunction()
        {
            const string input1 =
                @"
//minimalistic
function func1 does asm {} 

//normal
function func2(param1, param2, param3) does asm
{  /* a comment */  } 

//*bling-bling*
function func3(param1, param2)
    [ Add System::IO to Imports; ] does asm
{ 

    }";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            //func1
            Assert.IsTrue(target.Functions.Contains("func1"), "Func1 is not in the function table");
            Assert.AreEqual(SymbolInterpretations.Function, ldr.Symbols["func1"].Interpretation);
            Assert.AreEqual(0, target.Functions["func1"].Parameters.Count);

            Assert.IsTrue(target.Functions.Contains("func2"), "Func2 is not in the function table");
            Assert.AreEqual(SymbolInterpretations.Function, ldr.Symbols["func2"].Interpretation);
            Assert.AreEqual(3, target.Functions["func2"].Parameters.Count);
            Assert.AreEqual("param2", target.Functions["func2"].Parameters[1]);

            Assert.IsTrue(target.Functions.Contains("func3"), "Func3 is not in the function table");
            Assert.AreEqual(SymbolInterpretations.Function, ldr.Symbols["func3"].Interpretation);
            Assert.AreEqual(2, target.Functions["func3"].Parameters.Count);
            Assert.AreEqual("param2", target.Functions["func3"].Parameters[1]);
            Assert.AreEqual(1, target.Functions["func3"].ImportedNamespaces.Count);
        }

        [Test()]
        public void AsmLocalVariableDeclaration()
        {
            const string input1 = @"
function func1() does asm
{
    var loc1
    ref loc2
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            Assert.IsTrue(target.Functions["func1"].Variables.Contains("loc1"));
            Assert.AreEqual(
                SymbolInterpretations.LocalObjectVariable,
                ldr.FunctionTargets["func1"].Symbols["loc1"].Interpretation);

            Assert.IsTrue(target.Functions["func1"].Variables.Contains("loc2"));
            Assert.AreEqual(
                SymbolInterpretations.LocalReferenceVariable,
                ldr.FunctionTargets["func1"].Symbols["loc2"].Interpretation);
        }

        [Test()]
        public void AsmNullInstructions()
        {
            const string input1 =
                @"
function func1() does asm
{
    //Instructions in the null group
    nop             //0
    ldc.null        //1
    nop             //2
    nop             //3
    neg             //4
    not             //5
    add             //6
    sub             //7
    mul             //8
    div             //9
    mod             //10
    pow             //11
    ceq             //12
    cne             //13
    cgt             //14
    cge             //15
    clt             //16
    cle             //17
    or              //18
    and             //19
    xor             //20
    check.arg       //21
    cast . arg      //22
    ldr.eng         //23
    ldr. app        //24
    ret.exit        //25
    ret.break       //26
    ret.continue    //27
    ret.set         //28
    ret.value       //29

    //Aliases
    continue        //30
    break           //31
    ret             //32
    exit            //33
}
";

            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            List<Instruction> code = target.Functions["func1"].Code;
            int i = 0;
#if DEBUG //nops are stripped in release mode
            Assert.AreEqual(OpCode.nop, code[i++].OpCode);
#endif
            Assert.AreEqual(OpCode.ldc_null, code[i++].OpCode);
#if DEBUG
            Assert.AreEqual(OpCode.nop, code[i++].OpCode);
            Assert.AreEqual(OpCode.nop, code[i++].OpCode);
#endif
            Assert.AreEqual(OpCode.neg, code[i++].OpCode);
            Assert.AreEqual(OpCode.not, code[i++].OpCode);
            Assert.AreEqual(OpCode.add, code[i++].OpCode);
            Assert.AreEqual(OpCode.sub, code[i++].OpCode);
            Assert.AreEqual(OpCode.mul, code[i++].OpCode);
            Assert.AreEqual(OpCode.div, code[i++].OpCode);
            Assert.AreEqual(OpCode.mod, code[i++].OpCode);
            Assert.AreEqual(OpCode.pow, code[i++].OpCode);
            Assert.AreEqual(OpCode.ceq, code[i++].OpCode);
            Assert.AreEqual(OpCode.cne, code[i++].OpCode);
            Assert.AreEqual(OpCode.cgt, code[i++].OpCode);
            Assert.AreEqual(OpCode.cge, code[i++].OpCode);
            Assert.AreEqual(OpCode.clt, code[i++].OpCode);
            Assert.AreEqual(OpCode.cle, code[i++].OpCode);
            Assert.AreEqual(OpCode.or, code[i++].OpCode);
            Assert.AreEqual(OpCode.and, code[i++].OpCode);
            Assert.AreEqual(OpCode.xor, code[i++].OpCode);
            Assert.AreEqual(OpCode.check_arg, code[i++].OpCode);
            Assert.AreEqual(OpCode.cast_arg, code[i++].OpCode);
            Assert.AreEqual(OpCode.ldr_eng, code[i++].OpCode);
            Assert.AreEqual(OpCode.ldr_app, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_exit, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_break, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_continue, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_set, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_value, code[i++].OpCode);

            //Aliases
            Assert.AreEqual(OpCode.ret_continue, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_break, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_value, code[i++].OpCode);
            Assert.AreEqual(OpCode.ret_exit, code[i++].OpCode);
        }

        [Test()]
        public void AsmIdInstructions()
        {
            const string input1 =
                @"
var glob1;
function func1 does asm
{
    var    loc1
    ldc.string      ""Hello World""
    ldr.func        func1
    ldr.glob        glob1
    ldr.loc         loc1
    ldr.type        Int
    ldloc           loc1
    stloc           loc1
    ldglob          glob1
    stglob          glob1
    check.const     Int
    cast.const      Int
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "There were errors during compilation.");

            List<Instruction> code = target.Functions["func1"].Code;
            int i = 0;

            Assert.AreEqual(OpCode.ldc_string, code[i].OpCode);
            Assert.AreEqual("Hello World", code[i++].Id);

            Assert.AreEqual(OpCode.ldr_func, code[i].OpCode);
            Assert.AreEqual("func1", code[i++].Id);

            Assert.AreEqual(OpCode.ldr_glob, code[i].OpCode);
            Assert.AreEqual("glob1", code[i++].Id);

            Assert.AreEqual(OpCode.ldr_loc, code[i].OpCode);
            Assert.AreEqual("loc1", code[i++].Id);

            Assert.AreEqual(OpCode.ldr_type, code[i].OpCode);
            Assert.AreEqual("Int", code[i++].Id);

            Assert.AreEqual(OpCode.ldloc, code[i].OpCode);
            Assert.AreEqual("loc1", code[i++].Id);

            Assert.AreEqual(OpCode.stloc, code[i].OpCode);
            Assert.AreEqual("loc1", code[i++].Id);

            Assert.AreEqual(OpCode.ldglob, code[i].OpCode);
            Assert.AreEqual("glob1", code[i++].Id);

            Assert.AreEqual(OpCode.stglob, code[i].OpCode);
            Assert.AreEqual("glob1", code[i++].Id);

            Assert.AreEqual(OpCode.check_const, code[i].OpCode);
            Assert.AreEqual("Int", code[i++].Id);

            Assert.AreEqual(OpCode.cast_const, code[i].OpCode);
            Assert.AreEqual("Int", code[i++].Id);
        }

        [Test()]
        public void AsmSpecialInstructions()
        {
            const string input1 =
                @"
function func1(param1)  [ key value; ] does asm
{
    rot.2,3
    swap
    ldc.real    -2.53e-3
    ldc.real    2.5
    ldc.bool    false
    ldc.bool    TrUe
    indarg.3
    inda.0
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "There were errors during compilation.");

            List<Instruction> code = target.Functions["func1"].Code;
            int i = 0;

            Assert.AreEqual(OpCode.rot, code[i].OpCode);
            Assert.AreEqual(2, code[i].Arguments);
            Assert.AreEqual(3, (int) code[i++].GenericArgument);

            Assert.AreEqual(OpCode.rot, code[i].OpCode);
            Assert.AreEqual(1, code[i].Arguments);
            Assert.AreEqual(2, (int) code[i++].GenericArgument);

            Assert.AreEqual(OpCode.ldc_real, code[i].OpCode);
            Assert.AreEqual(-2.53e-3, (double) code[i++].GenericArgument);

            Assert.AreEqual(OpCode.ldc_real, code[i].OpCode);
            Assert.AreEqual(2.5, (double) code[i++].GenericArgument);

            Assert.AreEqual(OpCode.ldc_bool, code[i].OpCode);
            Assert.AreEqual(0, code[i++].Arguments);

            Assert.AreEqual(OpCode.ldc_bool, code[i].OpCode);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.indarg, code[i].OpCode);
            Assert.AreEqual(3, code[i++].Arguments);

            Assert.AreEqual(OpCode.indarg, code[i].OpCode);
            Assert.AreEqual(0, code[i++].Arguments);
        }

        [Test()]
        public void AsmIdArgInstructions()
        {
            const string input1 =
                @"
function func1 does asm
{
    newobj.1    ""Object(\""System.Text.StringBuilder\"")""
    newtype.1   ""Object""
    get.3       ToString
    set.2       __\defaultIndex
    new.1       ""Object(\""System.DateTime\"")""
    sget.10     ""Object(\""System.Console\"")::WriteLine""
    sset.1      ""Object(\""System.Console\"")::ForegroundColor""
    indloc.2    aFunction
    indglob.3   anotherFunction
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "There were errors during compilation.");

            List<Instruction> code = target.Functions["func1"].Code;
            int i = 0;

            Assert.AreEqual(OpCode.newobj, code[i].OpCode);
            Assert.AreEqual("Object(\"System.Text.StringBuilder\")", code[i].Id);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.newtype, code[i].OpCode);
            Assert.AreEqual("Object", code[i].Id);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.get, code[i].OpCode);
            Assert.AreEqual("ToString", code[i].Id);
            Assert.AreEqual(3, code[i++].Arguments);

            Assert.AreEqual(OpCode.set, code[i].OpCode);
            Assert.AreEqual("__\\defaultIndex", code[i].Id);
            Assert.AreEqual(2, code[i++].Arguments);

            Assert.AreEqual(OpCode.newobj, code[i].OpCode);
            Assert.AreEqual("Object(\"System.DateTime\")", code[i].Id);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.sget, code[i].OpCode);
            Assert.AreEqual("Object(\"System.Console\")::WriteLine", code[i].Id);
            Assert.AreEqual(10, code[i++].Arguments);

            Assert.AreEqual(OpCode.sset, code[i].OpCode);
            Assert.AreEqual("Object(\"System.Console\")::ForegroundColor", code[i].Id);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.indloc, code[i].OpCode);
            Assert.AreEqual("aFunction", code[i].Id);
            Assert.AreEqual(2, code[i++].Arguments);

            Assert.AreEqual(OpCode.indglob, code[i].OpCode);
            Assert.AreEqual("anotherFunction", code[i].Id);
            Assert.AreEqual(3, code[i++].Arguments);
        }

        [Test()]
        public void AsmIntInstructions()
        {
            const string input1 =
                @"
function func1 does asm
{
    ldc.int 1           //0
    ldc.int -58416325   //1
    pop     1           //2
    pop     10          //3
    dup     2           //4
    dup     10          //5
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            List<Instruction> code = target.Functions["func1"].Code;
            int i = 0;

            Assert.AreEqual(OpCode.ldc_int, code[i].OpCode);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.ldc_int, code[i].OpCode);
            Assert.AreEqual(-58416325, code[i++].Arguments);

            Assert.AreEqual(OpCode.pop, code[i].OpCode);
            Assert.AreEqual(1, code[i++].Arguments);

            Assert.AreEqual(OpCode.pop, code[i].OpCode);
            Assert.AreEqual(10, code[i++].Arguments);

            Assert.AreEqual(OpCode.dup, code[i].OpCode);
            Assert.AreEqual(2, code[i++].Arguments);

            Assert.AreEqual(OpCode.dup, code[i].OpCode);
            Assert.AreEqual(10, code[i++].Arguments);
        }

        [Test()]
        public void AsmLabelsAndJumps()
        {
            const string input1 =
                @"
function func1 does asm
{
    jump        14
    ldc.bool    true
    jump.t      14
    jump.f      5
    
    //a while loop
    var i
    ldc.int     5
    stloc       i
    label       while0
    ldloc       i
    ldc.int     0
    cgt
    jump.f      endwhile0
    ldloc       i
    sget.1      ""Object(\""System.Console\"")::WriteLine""
    dec         i
    jump        while0
    label       endwhile0
}
";
            LoaderOptions opt = new LoaderOptions(engine, target);
            opt.UseIndicesLocally = false;
            Loader ldr = new Loader(opt);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation.");

            List<Instruction> code = target.Functions["func1"].Code;

            Assert.AreEqual(OpCode.jump, code[0].OpCode);
            Assert.AreEqual(14, code[0].Arguments);

            Assert.AreEqual(OpCode.jump_t, code[2].OpCode);
            Assert.AreEqual(14, code[2].Arguments);

            Assert.AreEqual(OpCode.jump_f, code[3].OpCode);
            Assert.AreEqual(5, code[3].Arguments);

            //The while loop
            Assert.AreEqual(OpCode.jump, code[13].OpCode);
            Assert.AreEqual(6, code[13].Arguments);

            Assert.AreEqual(OpCode.jump_f, code[9].OpCode);
            Assert.AreEqual(14, code[9].Arguments);
        }

        [Test()]
        public void AsmBugNullFollwedByInteger()
        {
            _compile(@"
function main does asm
{
    add
    pop 1    
}
");
        }

        #endregion

        [Test]
        public void GlobalVariableShadowId()
        {
            var ldr = _compile(@"
var a;

var as b, c;

var d as e, f;

");

            Assert.IsTrue(target.Variables.ContainsKey("a"),"Variable a must exist.");
            Assert.IsFalse(target.Variables.ContainsKey("b"),"No Variable b must exist.");
            Assert.IsFalse(target.Variables.ContainsKey("c"), "No Variable c must exist.");
            Assert.IsTrue(target.Variables.ContainsKey("d"), "Variable d must exist.");
            Assert.IsFalse(target.Variables.ContainsKey("e"), "No Variable e must exist.");
            Assert.IsFalse(target.Variables.ContainsKey("f"), "No Variable f must exist.");


            Assert.IsTrue(ldr.Symbols.ContainsKey("a"),"Symbol a must exist.");
            var a = ldr.Symbols["a"];
            Assert.IsTrue(a.Interpretation == SymbolInterpretations.GlobalObjectVariable,"Symbol a must be global object variable.");

            Assert.IsTrue(ldr.Symbols.ContainsKey("b"), "Symbol b must exist.");
            var b = ldr.Symbols["b"];
            Assert.IsTrue(b.Interpretation == SymbolInterpretations.GlobalObjectVariable, "Symbol b must be global object variable.");
            Assert.IsTrue(target.Variables.ContainsKey(b.Id),"Symbol b must point to a physical variable.");

            Assert.IsTrue(ldr.Symbols.ContainsKey("c"), "Symbol c must exist.");
            var c = ldr.Symbols["c"];
            Assert.IsTrue(c.Interpretation == SymbolInterpretations.GlobalObjectVariable, "Symbol c must be global object variable.");
            Assert.IsTrue(target.Variables.ContainsKey(c.Id), "Symbol c must point to a physical variable.");   
            Assert.IsTrue(b.Id == c.Id, "Symbols b and c must point to the same variable.");

            Assert.IsTrue(ldr.Symbols.ContainsKey("d"), "Symbol d must exist.");
            var d = ldr.Symbols["d"];
            Assert.IsTrue(d.Interpretation == SymbolInterpretations.GlobalObjectVariable, "Symbol d must be global object variable.");

            Assert.IsTrue(ldr.Symbols.ContainsKey("e"), "Symbol e must exist.");
            var e = ldr.Symbols["e"];
            Assert.IsTrue(e.Interpretation == SymbolInterpretations.GlobalObjectVariable, "Symbol e must be global object variable.");
            Assert.IsTrue(target.Variables.ContainsKey(e.Id), "Symbol e must point to a physical variable.");

            Assert.IsTrue(ldr.Symbols.ContainsKey("f"), "Symbol f must exist.");
            var f = ldr.Symbols["f"];
            Assert.IsTrue(f.Interpretation == SymbolInterpretations.GlobalObjectVariable, "Symbol f must be global object variable.");
            Assert.IsTrue(target.Variables.ContainsKey(f.Id), "Symbol f must point to a physical variable.");
            Assert.IsTrue(e.Id == f.Id, "Symbols e and f must point to the same variable.");

            Assert.IsTrue(e.Id == "d","Symbols e and f must point to variable d");
        }
    }
}