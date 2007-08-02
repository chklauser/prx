using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Types;

namespace Prx.Tests
{
    [TestFixture]
    public class VMTests
    {
        #region Setup

        private Engine engine;
        private TestStackContext sctx;
        private Application target;
        private LoaderOptions options;

        [SetUp()]
        public void SetupCompilerEngine()
        {
            engine = new Engine();
            target = new Application("testApplication");
            sctx = new TestStackContext(engine, target);
            options = new LoaderOptions(engine, target);
        }

        [TearDown()]
        public void TeardownCompilerEngine()
        {
            engine = null;
            sctx = null;
            target = null;
            options = null;
        }

        #endregion

        [Test]
        public void Basic()
        {
            const string input1 = @"
function test1
{
    var x = 5 + 5;
}
";
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            PFunction test1 = target.Functions["test1"];
            FunctionContext fctx = new FunctionContext(engine, test1);
            PVariable x = fctx.LocalVariables["x"];
            Assert.IsTrue(x.Value == null || x.Value.Value == null, "variable x must be null in some way.");
            engine.Stack.AddLast(fctx);
            engine.Process();
            Assert.AreEqual(0, engine.Stack.Count, "Machine stack is expected to be empty after execution.");
            Assert.AreEqual(PType.BuiltIn.Int, x.Value.Type.ToBuiltIn());
            Assert.AreEqual(10, (int) x.Value.Value);
        }

        [Test]
        public void IncDecrement()
        {
            const string input1 = @"
function test1(x)
{
    x++;    
    x = 2*x++;
    return = x--;
}
";
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation");

            Random rnd = new Random();
            int x0 = rnd.Next(1, 200);
            int x = x0;
            x++;
            x = 2*x;
            int expected = x--;

            FunctionContext fctx = target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
            engine.Stack.AddLast(fctx);
            engine.Process();

            Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn());
            Assert.AreEqual(expected, (int) fctx.ReturnValue.Value, "Return value is expected to be " + expected + ".");

            Assert.AreEqual(x, (int) fctx.LocalVariables["x"].Value.Value, "Value of x is supposed to be " + x + ".");
        }

        [Test]
        public void LateReturn()
        {
            const string input1 = @"
function test1(x)
{
    x*=2;
    return = x-2;
    x+=55;
}
";
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors during compilation");

            Random rnd = new Random();
            int x0 = rnd.Next(1, 200);
            int x = x0;
            x *= 2;
            int expected = x - 2;
            x += 55;

            FunctionContext fctx = target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
            engine.Stack.AddLast(fctx);
            engine.Process();

            Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn());
            Assert.AreEqual(expected, (int) fctx.ReturnValue.Value, "Return value is expected to be " + expected + ".");

            Assert.AreEqual(x, (int) fctx.LocalVariables["x"].Value.Value, "Value of x is supposed to be " + x + ".");
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
    return = y+x;
    //dummy     
}
";
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            Random rnd = new Random();
            PValue result;
            int expected;

            //Test simple
            int v0 = rnd.Next(1, 100);
            expected = 2*v0;

            result = target.Functions["twice"].Run(engine, new PValue[] {v0});
            Assert.AreEqual(PType.BuiltIn.Int, result.Type.ToBuiltIn(), "Result is expected to be an integer. (twice)");
            Assert.AreEqual(expected, (int) result.Value);

            //Test complicated            
            int x0 = rnd.Next(1, 100);
            int y0 = rnd.Next(1, 100);
            int z = x0*y0;
            int x1 = z - x0;
            int y1 = x1 + z;
            expected = y1 + x1;

            result = target.Functions["complicated"].Run(engine, new PValue[] {x0, y0});
            Assert.AreEqual(PType.BuiltIn.Int, result.Type.ToBuiltIn(),
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
            Random rnd = new Random();
            int expected,
                J;
            J = rnd.Next(1, 1000);

            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            target.Variables["J"].Value = J;
            Assert.AreEqual(0, ldr.ErrorCount);

            Console.WriteLine(target.StoreInString());

            //Expectation
            int x0 = rnd.Next(1, 589);
            J = 0;
            J = (7*x0 + 2 + J);
            expected = (x0 + 2 + J)/J;

            FunctionContext fctx = target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
            engine.Stack.AddLast(fctx);
            engine.Process();
            Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn());
            Assert.AreEqual(expected, (int) fctx.ReturnValue.Value);
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            Console.WriteLine(target.StoreInString());

            Random rnd = new Random();
            int expected;
            int J0 = 0;
            int x0 = rnd.Next(1, 300);
            int x1 = 2 + J0 + J0;
            int J1 = 2 + (7*x1) + J0;
            expected = (2 + x1 + J1)/J1;

            FunctionContext fctx = target.Functions["test1"].CreateFunctionContext(engine, new PValue[] {x0});
            engine.Stack.AddLast(fctx);
            engine.Process();
            Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn());
            Assert.AreEqual(expected, (int) fctx.ReturnValue.Value);

            Assert.AreEqual(PType.BuiltIn.Int, target.Variables["J"].Value.Type.ToBuiltIn());
            Assert.AreEqual(J1, (int) target.Variables["J"].Value.Value);
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            for (int n = 1; n <= 6; n++)
            {
                Console.WriteLine("\nFib(" + n + ") do ");
                int expected = Fibonacci(n);
                FunctionContext fctx = target.Functions["fib"].CreateFunctionContext(engine, new PValue[] {n});
                engine.Stack.AddLast(fctx);
                engine.Process();
                Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn(), "Result must be a ~Int");
                Assert.AreEqual(expected, (int) fctx.ReturnValue.Value,
                                "Fib(" + n + ") = " + expected + " and not " + (int) fctx.ReturnValue.Value);
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            for (int n = 1; n <= 6; n++)
            {
                Console.WriteLine("\nFib(" + n + ") do ");
                int expected = Fibonacci(n);
                FunctionContext fctx = target.Functions["fib"].CreateFunctionContext(engine, new PValue[] {n});
                engine.Stack.AddLast(fctx);
                engine.Process();
                Assert.AreEqual(PType.BuiltIn.Int, fctx.ReturnValue.Type.ToBuiltIn(), "Result must be a ~Int");
                Assert.AreEqual(expected, (int) fctx.ReturnValue.Value,
                                "Fib(" + n + ") = " + expected + " and not " + (int) fctx.ReturnValue.Value);
            }
        }

        [DebuggerNonUserCode]
        private static int Fibonacci(int n)
        {
            return
                n <= 2
                    ?
                1
                    : Fibonacci(n - 1) + Fibonacci(n - 2);
        }

        [Test]
        public void WhileLoop()
        {
            _compile(
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

            Random rnd = new Random();
            int M = rnd.Next(1, 13);
            int iterations = rnd.Next(3, 10);
            int sum = 0;
            for (int i = 0; i < iterations; i++)
                sum += M*i + 12;
            int expected = sum;

            _expectNamed("main", expected, M, iterations);
        }

        [Test]
        public void ForLoop()
        {
            _compile(
                @"
var theList;

function getNextElement does
{
    if(static index < theList.Count) return theList[index++]; else return null;
}

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
            StringBuilder buffer = new StringBuilder();
            int max = 20;
            List<string> aList = new List<string>(new string[]
                                                      {
                                                          _generateRandomString(5),
                                                          _generateRandomString(10),
                                                          _generateRandomString(15),
                                                          _generateRandomString(3),
                                                          _generateRandomString(5)
                                                      });

            foreach (string elem in aList)
                if (buffer.Length + elem.Length < max) buffer.Append(elem);

            _expect(buffer.ToString(), engine.CreateNativePValue(aList), max);
        }

        [Test]
        public void StaticClrCalls()
        {
            _compile(@"
entry main;
function main(rawInteger)
{
    return System::Int32.Parse(rawInteger);
}
");
            Random rnd = new Random();
            int expected = rnd.Next(1, 45);
            _expect(expected, expected.ToString());
        }

        [Test]
        public void Conditions()
        {
            _compile(
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
            const string TT = "1212";
            const string Tx = "11112";
            const string xT = "2112";
            const string xx = "11122";

            Console.WriteLine("// TRUE  - TRUE ");
            _expect(TT, true, true);
            Console.WriteLine("// TRUE  - FALSE");
            _expect(Tx, true, false);
            Console.WriteLine("// FALSE - TRUE ");
            _expect(xT, false, true);
            Console.WriteLine("// FALSE - FALSE");
            _expect(xx, false, false);
        }

        [Test]
        public void IndexAccess()
        {
            _compile(
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

            string str = Guid.NewGuid().ToString("N").Substring(0, 3);
            Random rnd = new Random();
            int idx = rnd.Next(0, str.Length);
            StringBuilder buffer = new StringBuilder();
            foreach (char ch in str.ToCharArray())
                buffer.Append(ch.ToString() + ' ');
            buffer.Append("--" + str[idx]);
            string expect = buffer.ToString();
            _expect(expect, str, idx);
        }

        [Test]
        public void NonRecursiveTailCall()
        {
            options.RegisterCommands = true;
            _compile(
                @"
var buffer;
function print(text) does buffer.Append(text);
function main
{
    var args;
    buffer = new System::Text::StringBuilder(args[0]);
    print(args[1]);
    print(args[2]);
}
");
            string a = Guid.NewGuid().ToString("N");
            string b = Guid.NewGuid().ToString("N");
            string c = Guid.NewGuid().ToString("N");
            string expect = a + b + c;
            _expectNull("main", a, b, c);
            Assert.AreEqual(PType.Object[typeof(StringBuilder)], target.Variables["buffer"].Value.Type,
                            "buffer has not the expected type.");
            Assert.AreEqual(expect, target.Variables["buffer"].Value.Value.ToString());
        }

        [Test]
        public void Commands()
        {
            options.RegisterCommands = true;
            engine.Commands.AddUserCommand("conRev",
                                           new DelegatePCommand(delegate(StackContext localSctx, PValue[] args)
                                                                {
                                                                    StringBuilder sb = new StringBuilder();
                                                                    for (int i = args.Length - 1; i > -1; i--)
                                                                        sb.Append(args[i].CallToString(localSctx));
                                                                    return (PValue) sb.ToString();
                                                                }));

            string[] list = new string[] {"the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"};

            engine.Commands.AddUserCommand("theList",
                                           new DelegatePCommand(
                                               delegate(StackContext localSctx, PValue[] args) { return localSctx.CreateNativePValue(list); }));
            _compile(
                @"function main = conRev(theList[0], theList[1], theList[2], theList[3], theList[4], theList[5], theList[6], theList[7], theList[8]);");

            StringBuilder buffer = new StringBuilder();
            for (int i = list.Length - 1; i > -1; i--)
                buffer.Append(list[i]);

            _expect(buffer.ToString());
        }

        public class SomeSortOfList : IEnumerable<string>
        {
            public SomeSortOfList(string input)
            {
                _input = input;
            }

            private string _input;

            #region IEnumerable<string> Members

            public IEnumerator<string> GetEnumerator()
            {
                string[] words = _input.Split(new char[] {' ', '\t', '\n', '\r'});

                foreach (string word in words)
                    if (word.Length > 0)
                        yield return word[0].ToString().ToUpperInvariant();

                yield return ">>";

                foreach (string word in words)
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

            internal string printList()
            {
                StringBuilder buffer = new StringBuilder();
                foreach (string s in this)
                {
                    buffer.Append(' ');
                    buffer.Append(s);
                }
                return buffer.ToString();
            }

            internal int countList()
            {
                IEnumerator<string> e = GetEnumerator();
                while (e.MoveNext())
                    if (e.Current == ">>")
                        break;
                int cnt = 0;
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
            _compile(@"
function main(lst)
{
    var i = 0;
    foreach(var e in lst)
        i++;
    return i;
}
");
            _expect(5,PType.List.CreatePValue(new PValue[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void Foreach()
        {
            SomeSortOfList lst = new SomeSortOfList("The quick brown fox jumps over the lazy dog");
            _compile(
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

            _expectNamed("printList", lst.printList(), sctx.CreateNativePValue(lst));
            _expectNamed("countList", lst.countList(), sctx.CreateNativePValue(lst));
        }

        [Test]
        public void GlobalVarInit()
        {
            _compile(
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
            StringBuilder buffer = new StringBuilder();
            StringBuilder HW = new StringBuilder("Hello World");
            Random rnd = new Random();
            int x = rnd.Next(0, HW.Length + 1);
            int xi = x >= HW.Length ? HW.Length - 1 : x;
            for (int i = 0; i < xi; i++)
            {
                HW.Insert(i, i.ToString());
                buffer.Append(">");
                buffer.Append(HW.ToString());
            }
            string expect = buffer.ToString();

            _expect(expect, x);
        }

        [Test]
        public void PartialInitialization()
        {
            Loader ldr =
                _compile(
                    @"

Add System::Text to Import;

var buffer = new ::Text::StringBuilder;
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

    return = buffer.ToString; 
}
");

            _expect("#1=1o1;", 1);

            //Continue compilation using the same loader
            _compile(ldr,
                     @"
var L2 = ""2p2"";

function \init [ Import { System, System::Text }; ] does
{
    L1 = ""1o2"";
    buffer = new ::StringBuilder;
}
");
            _expect("#1=1o2;#2=2p2;", 2);

            //Continue compilation using a different loader
            _compile(
                @"
var L3 = ""3z3"";
var L2 = ""2m3"";
var L1 = ""1k3"";

declare var buffer;
function \init does buffer = new ::StringBuilder;
");

            _expect("#1=1k3;#2=2m3;#3=3z3;", 3);
        }

        [Test]
        public void UselessBuildBlock()
        {
            Loader ldr = _compile(@"
    var myGlob; var initGlob;
");

            _compile(ldr,
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
            _expect("ELLO" + (55*77) + "init");
        }

        [Test]
        public void References()
        {
            _compile(
                @"
function foldl(ref f, var left, var lst) does // (b -> a -> b) -> b -> a -> [b]
{
    foreach (var right in lst) left = f(left,right);
    return left;
}

function map(ref f, var lst) does // (a -> b) -> [a] -> [b]
{
    var nlst = new List;
    return = nlst;
    foreach(var e in lst) nlst.Add = f(e);
}

var tuple\lst;
function tuple(x)
{
    static idx;
    declare tuple\lst as lst;

    if(idx == null)
        idx = 0;

    return = ~List.Create(x, lst[idx++]);
    unless(idx < lst.Count)
        idx = null;
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

function main()                           // IO() -> IO()
{
    //Create [1..10]
    var lst = new List;
    for(var i = 1; until i == 11; i++)
        lst.Add = i;
    
    var bin = map(->binary, lst); // 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024
    var bin\sum = foldl(->add, 0, bin); // 2024

    chain = ~List.Create( -> twice, -> twice); //*4
    var bin\quad = map(->chained, bin); // 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096

    var twi = map(->twice, lst); // 2, 4, 6, 8, 10, 12, 14, 16, 18, 20

    tuple\lst = twi;
    var tup\bin_twi = map(->tuple, bin); // (2,2), (4,4), (6,8), (8,16), (10,32), (12,64), (14,128), (16,256), (18,512), (20,1024)

    ->reduce\f = ->sub;
    var tup\bin_twi\sub = map(->reduce, tup\bin_twi); // 0, 0, -2, -8, -22, -52, -114, -240, -494, -1004
    
    var tup\bin_twi\sub\sum = foldl(->add, 0 , tup\bin_twi\sub); // 1936
    
    var bin\quad\sum = foldl(->add, 0, bin\quad); // 8184

    return  (bin\quad\sum - tup\bin_twi\sub\sum)~Int; // 6248
}
");
            _expect(6248);
        }

        [Test]
        public void Typecheck()
        {
            _compile(
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
            string rs = _generateRandomString(3);
            _expect(rs + rs, rs);

            List<PValue> lst = new List<PValue>(new PValue[]
                                                    {
                                                        _generateRandomString(2), _generateRandomString(3),
                                                        _generateRandomString(4)
                                                    });
            string ls = "";
            foreach (PValue e in lst)
                ls += e.Value as string;
            _expect(ls, (PValue) lst);

            StringBuilder sb = new StringBuilder(_generateRandomString(5));
            _expect(sb.ToString(), engine.CreateNativePValue(sb));
        }

        [Test]
        public void ClosureCreation()
        {
            _compile(@"
function clo1 = x => 2*x;

function clo2(a)
{
    return x => a*x;
}
");

            Random rnd = new Random();

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
        }

        [Test]
        public void Lambda()
        {
            _compile(
                @"
function map(ref f, lst)
{
    var nlst = new List;
    foreach(var x in lst) nlst.Add = f(x);
    return nlst;
}

function foldl(ref f, var left, var lst)
{
    foreach(var x in lst)
        left = f(left, x);
    return left;   
}

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
            string expected;
            int[] lst = new int[10];
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                lst[i] = rnd.Next(4, 49);
                int twi = 2*lst[i];
                int factors = twi/10;
                int rests = twi%10;
                sb.Append(" (" + factors + "," + rests + ")");
            }
            expected = sb.ToString();

            List<PValue> plst = new List<PValue>();
            foreach (int x in lst)
                plst.Add(x);

            _expect(expected, PType.List.CreatePValue(plst));
        }

        [Test]
        public void Currying()
        {
            _compile(
                @"
function curry(ref f) = a => b => f(a,b);

function uncurry(ref f) = (a, b) =>
{
    ref bc = f(a);
    return bc(b);
};

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
            Random rnd = new Random();
            int s = rnd.Next(2, 9);
            List<PValue> plst = new List<PValue>();
            int head = -1;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                int c = rnd.Next(11, 99);
                if (head < 0)
                    head = c;
                plst.Add(c);
                int d = c + s;
                int compared = d - head;
                sb.Append(" ");
                sb.Append(compared.ToString());
            }
            string expect = sb.ToString();

            _expect(expect, PType.List.CreatePValue(plst), s);
        }

        [Test]
        public void NestedFunctions()
        {
            _compile(
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
            Random rnd = new Random();
            int[] ps = new int[] {1, 2, 10, 27, 26, 57, 60, 157, rnd.Next(1, 190), rnd.Next(1, 190), rnd.Next(1, 190)};
            foreach (int p in ps)
            {
                int goo;
                if (p%10 == 0)
                    goo = 2*p;
                else
                    goo = 2 + p;

                string q;
                string koo;
                q = goo.ToString();
                if (p <= 50)
                    koo = q.Length > 1 ? q + "koo" : q;
                else
                {
                    q = q + q;
                    koo = q.Length%2 != 0 ? q + "q" : q;
                }

                _expect(koo, p);
            }
        }

        [Test]
        public void DeDereference()
        {
            _compile(
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
            Random rnd = new Random();
            int m = rnd.Next(3, 500);
            int expected = 2 + (2*m);

            _expect(expected, m);
        }

        [Test]
        public void ExplicitIndirectCall()
        {
            _compile(
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
            Random rnd = new Random();
            double sum = 0.0;
            List<PValue> xlst = new List<PValue>(),
                         ylst = new List<PValue>();
            for (int i = 0; i < 10; i++)
            {
                int x = rnd.Next(3, 50);
                int y = rnd.Next(6, 34);

                xlst.Add(x);
                ylst.Add(y);

                double d = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                sum += d;
            }
            _expect(sum, PType.List.CreatePValue(xlst), PType.List.CreatePValue(ylst));
        }

        [Test]
        public void ConditionalExpression()
        {
            _compile(@"
function abs(x) = x > 0 ? x : -x;
function max(a,b) = a > b ? a : b;
var rnd = new System::Random;
function randomImplementation(fa, fb) = (a,b) => rnd.Next(0,2) mod 2 == 0 ? fa.(a,b) : fb.(a,b);
function sum(lst)
{
    var s = 0;
    foreach(var e in lst)
        s += e;
    return s;
}

function main(lst, limit)
{
    ref min = randomImplementation((a,b) => max(a,b) == a ? b : a, (a,b) => a < b ? a : b);
    return -sum(mapall(a => min(a, limit), lst));
}
");
            Random rnd = new Random();
            List<PValue> lst = new List<PValue>();
            int sum = 0;
            for(int i = 0; i < 10; i++)
            {
                int e = rnd.Next(-5, 6);
                lst.Add(e);
                if(e < 0)
                    sum += e;
            }

            lst.Add(4);
            lst.Add(1);
            lst.Add(0);
            lst.Add(-2);
            sum -= 2;
            _expect(-sum, PType.List.CreatePValue(lst));
        }

        [Test]
        public void NestedConditionalExpressions()
        {
            _compile(@"
function main(xs)
{
    var ys = [];
    foreach(var x in xs)
        ys[] = 
            x mod 2 == 0
            ?   x > 5
                ?   x
                :   x*2
            :   x < 10
                ?   (x+1) / 2
                :   x+2
        ;
    
    var s = 0;
    foreach(var y in ys)   
        s+=y;
    return s;
}
");
            List<PValue> xs = new List<PValue>(
                new PValue[]
                    {
                        12, //=> 12
                        4,  //=> 8
                        5,  //=> 3
                        13  //=> 15
                    });

            _expect(12+8+3+15, PType.List.CreatePValue(xs));
        }

        [Test]
        public void GlobalRefAssignment()
        {
            _compile(
                @"
var theList;

function accessor(index) = v => 
{
    return = theList[index];
    if(v != null)
        theList[index] = v;
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
            List<PValue> lst = new List<PValue>();
            int av = 0;
            for(int i = 0; i < 10; i++)
            {
                lst.Add(i);
                int k = i != 0 ? i != 1 ? i : 7 : 4;
                av += k;
            }
            av = av/10;
            _expect("f4::" + av + "::s7", PType.List.CreatePValue(lst));          
        }

        [Test]
        public void DataStructure()
        {
            _compile(@"
function chain(lst, serial)
{
    var c = new Structure<""IsSerial"", ""Functions"">;
    return = c;
    c.IsSerial = serial != null ? serial : true;
    c.Functions = lst != null ? lst : new List();
    function Invoke(this, prev)
    {
        var res = prev;
        if(this.IsSerial)
        {
            foreach(var f in this.Functions)
                prev = f.(prev);
            return prev;
        }
        else
        {
            var nlst = new List();
            foreach(var f in this.Functions)
                nlst[] = f.(prev);
            return nlst;
        }
    }

    c.\(""Invoke"",true) = ->Invoke;
    c.\(""IndirectCall"",true) = ->Invoke;
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
            int seed = (new Random()).Next(400, 500);
            int expected = seed;
            expected = expected + 2;
            expected = expected*2;
            expected = expected%3;
            expected = (int) (Math.Sqrt(expected)*10);

            _expect("The answer is: " + expected,seed);
        }

        [Test]
        public void ListConcat()
        {

            _compile(@"

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
            List<PValue> lst = new List<PValue>();
            StringBuilder sbo = new StringBuilder();
            StringBuilder sbe = new StringBuilder();
            for(int i = 0; i < 10; i++)
            {
                lst.Add(i);
                if(i % 2 == 0)
                    sbe.Append(i);
                else
                    sbo.Append(i);
            }
            _expect(string.Concat(sbe.ToString(), sbo.ToString()),new PValue[] { PType.List.CreatePValue(lst) });
        }

        [Test]
        public void CoroutineSimple()
        {
            _compile(@"
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
            _expect("abc","a","b","c");
        }

        [Test]
        public void CoroutineFunction()
        {
            _compile(@"
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

            _expect("cde","a","b","c","d","e","f","g");
        }

        [Test]
        public void CoroutineComplex()
        {
            _compile(@"
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

function chain()
{
    var filters = var args;
    return lst =>
    {
        foreach(ref filter in filters)
            lst = filter(lst);
        return lst;
    };
}

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
            List<PValue> lst = new List<PValue>();
            StringBuilder buffer = new StringBuilder();
            int nums = 0;
            for(int i = 0; i < 20; i++)
            {
                lst.Add(i);
                if(i < 2)
                    continue;
                if(i*3 % 2 != 0)
                    continue;
                if(nums > 2)
                    continue;
                buffer.Append(i*3);
                nums++;
            }
            _expect(buffer.ToString(), lst.ToArray());
        }

        [Test]
        public void MapCommandImplementation()
        {
            _compile(@"
declare command mapall as map;

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

            _expect("01234", 1, 2, 3, 4, 5);
        }

        [Test]
        public void FoldLCommandImplementation()
        {
            _compile(@"
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

            _expect(13, 4, 5, 6, 7);
        }

        [Test]
        public void CallCommandImplementation()
        {
            _compile(@"
function sum()
{
    var s = 0;
    foreach(var x in var args)
        s += x~Int;
    return s;
}

function main()
{
    return call(->call, ->sum, var args);
}
");
            int
                a = 3,
                b = 7,
                c = 9,
                d = 13,
                e = 14,
                f = 99,
                g = 101;

            _expect(a + b + c + d + e + f + g, PType.List.CreatePValue(new PValue[] {a, b, c}),
                    PType.List.CreatePValue(new PValue[] {d, e}), f, PType.List.CreatePValue(new PValue[] {g}));
        }

        /// <summary>
        /// Makes sure that coroutines do not return an unnecessary null at the end.
        /// </summary>
        [Test]
        public void CoroutineNoNull()
        {
            _compile(@"
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
            _expect("=>0\n=>1\n=>2\n=>3\n");
        }

        [Test]
        public void CoroutineRecursive()
        {
            _compile(@"
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

            List<PValue> args = new List<PValue>();
            List<PValue> sub1 = new List<PValue>();
            List<PValue> sub2 = new List<PValue>();
            List<PValue> sub21 = new List<PValue>();

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

            _expect("1.2.3.4.5.6.7.8.9.10.11.12.13.14",PType.List.CreatePValue(args));
        }

        [Test]
        public void CoroutineFib()
        {
            _compile(@"
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

            _expectNamed("fib",Fibonacci(6),6);
        }

        [Test]
        public void UnusedTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()
{
    var xs = [0];
    try
    {
        for(var i = 0; i < 5; i++)
            xs[] = i;
    }
    catch(var exc)
    {
        xs[] = exc;
    }
    finally
    {
        xs[] = ""--"";
    }

    return tos(xs);
}
");

            _expect("001234--");
        }

        [Test]
        public void UnusedSimpleTry()
        {
            _compile(
                @"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()
{
    var xs = [0];
    try
    {
        for(var i = 0; i < 5; i++)
            xs[] = i;
    }
    xs[] = ""--"";

    return tos(xs);
}
");

            _expect("001234--");
        }

        [Test]
        public void IgnoreTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()
{
    var xs = [0];
    for(var i = 1; i < 6; i++)
        try
        {
            xs[] = i;
            if(i == 3)
                throw i; //Should be ignored
        }
    return tos(xs);
}
");

            _expect("012345");
        }

        [Test]
        public void FinallyTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

var xs = [0];
function main()
{
    for(var i = 1; i < 6; i++)
        try
        {
            if(i == 3)
                throw i;
        }
        finally
        {
            xs[] = i;
        }
    return tos(xs);
}
");
            try
            {
                _expect("012345");
            }
            catch(Exception exc)
            {
                Assert.AreEqual("3",exc.Message);
            }

            PValue pxs = target.Variables["xs"].Value;
            Assert.IsInstanceOfType(typeof(ListPType),pxs.Type,"xs must be a ~List.");
            List<PValue> xs = (List<PValue>) pxs.Value;
            Assert.AreEqual("0", xs[0].CallToString(sctx));
            Assert.AreEqual("1", xs[1].CallToString(sctx));
            Assert.AreEqual("2", xs[2].CallToString(sctx));
            Assert.AreEqual("3", xs[3].CallToString(sctx));
        }

        [Test]
        public void CatchTry()
        {
            _compile(@"
function main()
{
    var xs = [0];
    for(var i = 1; i < 4; i++)
        try
        {
            if(i == 2)
                throw i;
            xs[] = i;
        }
        catch(var exc)
        {
            println(""(""+exc+"")"");
            xs[] = 2;
        }
    return xs.ToString;
}
");
            _expect("[ 0, 1, 2, 3 ]");
        }

        [Test]
        public void CatchFinallyTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()
{
    var xs = [0];
    for(var i = 1; i < 6; i++)
        try
        {
            if(i == 3)
                throw i; //Should be ignored
        }
        catch(var exc)
        {   
            xs[] = exc.Message;
        }
        finally
        {
            xs[] = i;
        }
    return tos(xs);
}
");
            _expect("0123345");
        }

        [Test]
        public void NestedTries()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()
{
    var xs = [0];
    for(var i = 1; i < 6; i++)
        try
        {
            try
            {
                if(i mod 2 == 0)
                    throw i; //Should be ignored
            }
            catch(var exc)
            {
                if(exc.Message == ""4"")
                    throw exc;
            }
        }
        catch
        {
            xs[] = i;
        }
        finally
        {
            xs[] = i;
        }
    return tos(xs);
}
");
            _expect("0123445");
        }

        [Test]
        public void CrossFunctionTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function process(i) does
    if(i == 3)
        throw i;

function main()
{
    var xs = [0];
    for(var i = 1; i < 6; i++)
        try
        {
            process(i);
        }
        catch(var exc)
        {   
            xs[] = exc.Message;
        }
        finally
        {
            xs[] = i;
        }
    return tos(xs);
}
");
            _expect("0123345");
        }

        [Test]
        public void HandledSurfaceTry()
        {
            _compile(@"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function process(i) does try
{
    if(i == 3)
        throw i;
}
catch(var exc)
{
    throw exc.Message + ""i"";
}

function main()
{
    var xs = [0];
    for(var i = 1; i < 6; i++)
        try
        {
            process(i);
        }
        catch(var exc)
        {   
            xs[] = exc.Message;
        }
        finally
        {
            xs[] = i;
        }
    return tos(xs);
}
");
            _expect("01233i45");
        }

        [Test]
        public void Hashes()
        {
            _compile(@"
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
            PValue[] xs =
                new PValue[]
                    {
                        2, //4
                        3, //5
                        8, //10
                    };
            _expect(4 + 5 + 10, xs);
        }

        [Test]
        public void NestedFunctionCrossReference()
        {
            _compile(@"
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

            _expect("bs.Closure(main\\nested\\A0)b", "s");
        }

        [Test]
        public void CrossForeachTryCatch()
        {
            _compile(@"
coroutine mayFail
{
    yield 1;
    yield 2;
    throw ""I failed"";
    yield 3;
}

function main(sum)
{
    try
	{
        ref mightFail = mayFail;
		foreach(var sourceFile in [4,5,6])
		{
			sum+=sourceFile + mightFail;
		}
	}
	catch(var exc)
	{
        println(exc);
        sum*=2;
	}
	finally
	{
        sum*=10;        
	}
    return sum;
}
");
            _expect((1+4+2+5+1)*20,1);
        }

        [Test]
        public void StructureToString()
        {
            _compile(@"
function main(x)
{
    var s = new Structure<""value"", ""r"", ""ToString"">;
    s.value = x;
    s.\(""ToString"",true) = this => this.value;
    return s~String;
}
");
            _expect("xzzxy","xzzxy");
        }

        [Test]
        public void UnbindCommandImplementation()
        {
            _compile(@"
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
            _expect(expected);
        }

        [Test]
        public void GlobalCode()
        {
            _compile(@"
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
            List<PValue> lst = new List<PValue>(4);
            lst.Add(new PValueKeyValuePair("apple",1));
            lst.Add(new PValueKeyValuePair("pencil", 5));
            lst.Add(new PValueKeyValuePair("juice",2));
            lst.Add(new PValueKeyValuePair("apple",2));

            _expect(3*3*2+5*1+2*4,PType.List.CreatePValue(lst));
        }

        [Test]
        public void CoalescenceOperator()
        {
            _compile(@"
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

            _expect(".6.2.4.4.?.1");
        }

        [Test]
        public void LoopExpressions()
        {
            _compile(
                @"
function main(s)
{
    var words = for(var i = 0; i < s.Length; i += 2)
                {
                    if(i == s.Length - 1)
                        yield s.Substring(i,1);
                    else
                        yield s.Substring(i,2);
                };
    
    return foldl    ( (l,r) => l + ""-"" + r, words.Count,  foreach(var word in words) 
                                                                yield word[0].ToUpper + word[1].ToLower;
                    );
}
");

            _expect("5-Bl-Oo-Dh-Ou-Nd","BloodHound");
        }

        [Test]
        public void HarmlessTryFinally()
        {
            _compile(@"
function main
{
    var r;
    try
    {
        r = ""NO_ERROR"";
    }
    finally
    {
        r += "", REALLY"";
    }
    return r;
}
");
            _expect("NO_ERROR, REALLY");
        }

        [Test]
        public void TryCatchInFinally()
        {
            _compile(@"
function mightFail(x)
{
    throw ""I don't like $x."";
}

var sb = new System::Text::StringBuilder;

function print does foreach(sb.Append in var args);
function println does foreach(sb.AppendLine in var args);

function main()
{
    
    try
    {
        var xs = 
            foreach(var a in var args) 
                yield mightFail(a);
            ;
    }
    finally
    {
        xs = 
            foreach(var a in var args)
                yield ""NP($a)"";
            ;
    }
    catch(var exc)
    {
        print = ""EXC($(exc.Message))"";
    }

    print(foldl((l,r) => l + "" "" + r, "" BEGIN"", xs)); 
    return sb.ToString;
}
");

            List<PValue> xs = new List<PValue>();
            xs.Add(4);
            xs.Add("Hello");
            xs.Add(3.4);

            _expect("EXC(I don't like 4.) BEGIN NP(4) NP(Hello) NP(3.4)",xs.ToArray());
        }

        [Test]
        public void LeftAppendArgument()
        {
            _compile(@"
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

            _expect("BEGIN 3:abc 5:hello 3:123", ":", "ab", "abc", "hello", "12", "123", "8965");
        }

        [Test]
        public void RightAppendArgument()
        {
            _compile(@"
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

            _expect("BEGIN 3:abc 5:hello 3:123", ":", "ab", "abc", "hello", "12", "123", "8965");
        }

        [Test]
        public void List_Sort()
        {
            _compile(@"
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

            _expect(",A,A,a,F,f,f,G,g,H,h,X,x");
        }

        [Test]
        public void CompilerHook()
        {
            _compile(@"
//In some library
declare function debug;

Import
{
    System,
    Prexonite,
    Prexonite::Types,
    Prexonite::Compiler,
    Prexonite::Compiler::Ast
};

function ast(type) [is hidden;]
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

build does hook (t => 
{
    var body = t.Ast;
    if(t.$Function.Id == ""main"")
    {
        //Append a return statement
        var ret = ast(""Return"", ::ReturnVariant.Exit);
        ret.Expression = ast(""GetSetMemberAccess"", ::PCall.Get, 
            ast(""GetSetSymbol"", ::PCall.Get, ""sb"", ::SymbolInterpretations.GlobalObjectVariable), ""ToString"");
        t.Ast.Add(ret);
    }

    function replace_debug(block)
    {
        for(var i = 0; i < block.Count; i++)
        {
            var stmt = block[i];
            if( stmt is ::AstGetSetSymbol && 
                stmt.Interpretation~Int == ::SymbolInterpretations.$Function~Int &&
                stmt.Id == ""debug"")
            {
                //Found a call to debug
                block[i] = ast(""AsmInstruction"", new ::Instruction(::OpCode.nop));
                for(var j = 0; j < stmt.Arguments.Count; j++)
                {
                    var arg = stmt.Arguments[j];
                    if(arg is ::AstGetSetSymbol)
                    {
                        var printlnCall = ast(""GetSetSymbol"", ::PCall.Get, ""println"", ::SymbolInterpretations.$Function);
                        var concatCall  = ast(""GetSetSymbol"", ::PCall.Get, ""concat"", ::SymbolInterpretations.$Command);
                        concatCall.Arguments.Add(ast(""Constant"",""DEBUG $(arg.Id) = ""));
                        concatCall.Arguments.Add(arg);
                        printlnCall.Arguments.Add(concatCall);

                        block.Insert(i,printlnCall);
                        i += 1;
                    }//end if                    
                }//end for

                //Recursively replace 'debug' in nested blocks.
                try
                {
                    foreach(var subBlock in stmt.Blocks)
                        replace_debug(subBlock);
                }
                catch(var exc)
                {
                    //ignore
                }//end catch
            }//end if
        }//end for
    }

    replace_debug(t.Ast);
});

//Emulation
var sb = new System::Text::StringBuilder;
function print(s)   does sb.Append(s);
function println(s) does sb.AppendLine(s);

//The main program
function main(a)
{
    var x = 3;
    var y = a;
    var z = 5*y+x;

    debug(x,y);
    debug(z);
}
");

            _expect("DEBUG x = 3\r\nDEBUG y = 4\r\nDEBUG z = 23\r\n",4);
        }

        #region Helper

        private static string _generateRandomString(int length)
        {
            return _generateRandomString().Substring(0, length);
        }

        private static string _generateRandomString()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static void _compile(Loader ldr, string input)
        {
            try
            {
                ldr.LoadFromString(input);
            }
            finally
            {
                foreach (string s in ldr.Errors)
                {
                    Console.Error.WriteLine(s);
                }
            }
            Assert.AreEqual(0, ldr.ErrorCount, "Errors detected during compilation.");
            Console.WriteLine(ldr.StoreInString());
        }

        private Loader _compile(string input)
        {
            Loader ldr = new Loader(options);
            _compile(ldr, input);
            return ldr;
        }

        private void _expect<T>(T expectedReturnValue, params PValue[] args)
        {
            _expectReturnValue(target.Meta[Application.EntryKey], expectedReturnValue, args);
        }

        private void _expectNamed<T>(string functionId, T expectedReturnValue, params PValue[] args)
        {
            _expectReturnValue(functionId, expectedReturnValue, args);
        }

        private void _expectReturnValue<T>(string functionId, T expectedReturnValue, PValue[] args)
        {
            PValue expected = engine.CreateNativePValue(expectedReturnValue);
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");
            FunctionContext fctx = target.Functions[functionId].CreateFunctionContext(engine, args);
            engine.Stack.AddLast(fctx);
            engine.Process();
            Assert.AreEqual(expected.Type, fctx.ReturnValue.Type,
                            string.Format("Return type is expected to be of type {0} and not {1}. Returned {2}.", 
                            expected.Type, fctx.ReturnValue.Type, fctx.ReturnValue));
            Assert.AreEqual(expected.Value, fctx.ReturnValue.Value,
                            "Return value is expected to be " + expected + " and not " +
                            fctx.ReturnValue);
        }

        private void _expectNull(params PValue[] args)
        {
            _expectReturnValue<object>(target.Meta[Application.EntryKey], null, args);
        }

        private void _expectNull(string functionId, params PValue[] args)
        {
            _expectReturnValue<object>(functionId, null, args);
        }

        private PValue _getReturnValueNamed(string functionId, params PValue[] args)
        {
            return _getReturnValueNamed_(functionId, args);
        }

        private PValue _getReturnValueNamed_(string functionId, PValue[] args)
        {
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");
            FunctionContext fctx = target.Functions[functionId].CreateFunctionContext(engine, args);
            engine.Stack.AddLast(fctx);
            engine.Process();
            return fctx.ReturnValue;
        }

        private PValue _getReturnValue(params PValue[] args)
        {
            return _getReturnValueNamed_(target.Meta[Application.EntryKey], args);
        }

        #endregion
    }
}