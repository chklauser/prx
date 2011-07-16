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
using PrexoniteTests.Tests;

namespace Prx.Tests
{
    public abstract partial class VMTests : VMTestsBase
    {
        [Test]
        public void UnusedTry()
        {
            Compile(
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

            Expect("001234--");
        }

        [Test]
        public void UnusedSimpleTry()
        {
            Compile(
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

            Expect("001234--");
        }

        [Test]
        public void IgnoreTry()
        {
            Compile(
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
    for(var i = 1; i < 6; i++)
        try
        {
            xs[] = i;
            if(i == 3)
                throw i; //Should be ignored
        }catch(var exc){}
    return tos(xs);
}
");

            Expect("012345");
        }

        [Test]
        public void FinallyTry()
        {
            Compile(
                @"
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
                Expect("012345");
            }
            catch (Exception exc)
            {
                Assert.AreEqual("3", exc.Message);
            }

            var pxs = target.Variables["xs"].Value;
            Assert.IsInstanceOf(typeof(ListPType), pxs.Type, "xs must be a ~List.");
            var xs = (List<PValue>)pxs.Value;
            Assert.AreEqual("0", xs[0].CallToString(sctx));
            Assert.AreEqual("1", xs[1].CallToString(sctx));
            Assert.AreEqual("2", xs[2].CallToString(sctx));
            Assert.AreEqual("3", xs[3].CallToString(sctx));
        }

        [Test]
        public void CatchTry()
        {
            Compile(
                @"
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
            Expect("[ 0, 1, 2, 3 ]");
        }

        [Test]
        public void CatchFinallyTry()
        {
            Compile(
                @"
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
            Expect("0123345");
        }

        [Test]
        public void NestedTries()
        {
            Compile(
                @"
function foldl(ref f, var left, var xs)
{
    foreach(var right in xs)
        left = f(left,right);
    return left;
}
function tos(xs) = foldl((a,b) => a + b,"""",xs);

function main()  [store_debug_implementation enabled;]
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
        catch(var exc)
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
            try
            {
                Expect("0123445");
            }
            catch (Exception exc)
            {
                Assert.Fail(exc.Message, exc);
            }
        }

        [Test]
        public void CrossFunctionTry()
        {
            Compile(
                @"
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
            Expect("0123345");
        }

        [Test]
        public void HandledSurfaceTry()
        {
            Compile(
                @"
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
            Expect("01233i45");
        }

        [Test]
        public void CrossForeachTryCatch()
        {
            Compile(
                @"
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
            Expect((1 + 4 + 2 + 5 + 1) * 20, 1);
        }

        [Test]
        public void HarmlessTryFinally()
        {
            Compile(
                @"
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
            Expect("NO_ERROR, REALLY");
        }

        [Test]
        public void TryCatchInFinally()
        {
            Compile(
                @"
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

            var xs = new List<PValue> { 4, "Hello", 3.4 };

            Expect("EXC(I don't like 4.) BEGIN NP(4) NP(Hello) NP(3.4)", xs.ToArray());
        }

        [Test]
        public void ReturnFromForeach()
        {
            Compile(@"
function main(xs)
{
    foreach(var x in xs)
        if(x > 5)
            return x;
    return -1;
}
");
            var xs = (PValue)new List<PValue> { 1, 2, 3, 4, 7, 15 };

            Expect(7, xs);
        }

        [Test]
        public void ForeachLastInConditionCil()
        {
            Compile(@"
function main(cond, xs)
{
    var z = 0;
    if(cond)
    {
        foreach(var x in xs)
            z += x;
    }
    else
    {
        z = 5;
    }
    return z;
}
");

            if (CompileToCil)
            {
                var main = target.Functions["main"];
                Assert.IsFalse(main.Meta[PFunction.VolatileKey], "main must not be volatile.");
                Assert.IsFalse(main.Meta.ContainsKey(PFunction.DeficiencyKey), "main must not have a deficiency");
                Assert.IsTrue(main.HasCilImplementation, "main must have CIL implementation.");
            }

            Expect(6, true, (PValue)new List<PValue> { 1, 2, 3 });
        }

        [Test]
        public void ReturnContinueFormTryFinally()
        {
            Compile(@"
function main()
{
    try
    {
        continue;
    } 
    finally
    {

    }
}
");

            var func = target.Functions["main"];

            var emptyArgV = new PValue[0];
            var emptyEnvironment = new PVariable[0];

            if (CompileToCil)
            {
                var nullContext = new NullContext(engine, target, new List<string>());
                Assert.IsTrue(func.HasCilImplementation, "main must have CIL implementation.");
                ReturnMode returnMode;
                PValue value;
                func.CilImplementation(
                    func, nullContext, emptyArgV, emptyEnvironment, out value, out returnMode);
                Assert.AreEqual(value.Type, PType.Null);
                Assert.AreEqual(returnMode, ReturnMode.Continue);
            }

            var fctx = func.CreateFunctionContext(engine, emptyArgV, emptyEnvironment);
            engine.Process(fctx);
            Assert.AreEqual(fctx.ReturnValue.Type, PType.Null);
            Assert.AreEqual(fctx.ReturnMode, ReturnMode.Continue);
        }

        [Test]
        public void JumpToAfterEmptyFinally()
        {
            Compile(@"
function main()
{
    try
    {
        goto after;
        goto fin;
    } 
    finally
    {
        fin:
    }
after:
}
");

            var func = target.Functions["main"];


            if (CompileToCil)
            {
                Assert.IsTrue(func.HasCilImplementation, "main must have CIL implementation.");
            }

            ExpectNull(new PValue[0]);
        }

        [Test]
        public void ReturnFromFinally()
        {
            Compile(@"
var t = """";
function trace x = t += x;

function main(x)
{
    try {
        trace(""t"");
    } finally {
        if(x)
            yield t;
        else 
            trace(""n"");
    }

    return t;
}
");

            var mainTable = target.Functions["main"].Meta;

            if (CompileToCil)
            {
                Assert.IsTrue(mainTable[PFunction.VolatileKey].Switch, "return from finally is illegal in CIL");
                Assert.IsTrue(mainTable[PFunction.DeficiencyKey].Text.Contains("SEH"),
                              "deficiency must be related to SEH.");
            }
            Expect("tn", false);
        }

        [Test]
        public void BreakFromProtected()
        {
            Compile(
                @"
var t = """";
function trace x=t+=x;
function main()
{
    try{
        trace(""t"");
        break;
    }catch(var exc) {
        trace(""c"");
    }
    
    return t;
}
");

            ExpectNull();
            Assert.IsTrue((bool)((PValue)"t").Equality(sctx, target.Variables["t"].Value).Value, "trace does not match");
        }

        [Test]
        public void TryAsLastStatement()
        {
            Compile(
                @"
var t;
function main(x)
{
    try {
        t = ""t"";  
    } finally {
        t += ""f"";
    }
}");

            ExpectNull();
            Assert.IsTrue((bool)target.Variables["t"].Value.Equality(sctx, "tf").Value, "Unexpected trace");
        }

        [Test]
        public void EndFinallies()
        {
            Compile(
                @"
var t = """";
function trace x=t+=x;
function main(x,y)
{
    try {
        trace(""t1"");
    } finally {
        goto endfinally1;
        trace(""f1"");
endfinally1:
    }

    trace(""e1"");

    try {
        trace(""t2"");
    } finally {
        if(x)
            goto endfinally2;    
        trace(""f2"");
endfinally2:
    }

    trace(""e2"");
    try {
        trace(""t3"");
    } finally {
        if(not y)
            goto endfinally3;    
        trace(""f3"");
endfinally3:
    }

    trace(""e3"");

    return t;
}
");
            if (CompileToCil)
            {
                Assert.IsNotNull(target.Functions["main"], "function main must exist.");
                Assert.IsFalse(target.Functions["main"].Meta[PFunction.VolatileKey].Switch,
                    "should compile to cil successfully");
            }
            Expect("t1e1t2e2t3e3", true, false);
        }

        [Test]
        public void ReturnFromCatch()
        {
            Compile
                (@"

var lastCode = -1;
var buffer = new System::Text::StringBuilder;
function green f => f.();
var errors = [];
var ldrErrors = [""error1"",""error2""];

function main()
{
    try 
    {
        var exc = null;
        lastCode = buffer.ToString; //Save the code for error reporting            
    } 
    catch(exc)
    {
        //Exceptions are truly exceptional, so they should be printed
        // out right away.
        green = () =>
		{
			println(exc);
			exc = null;
			foreach(var err in errors)
				println(err);
		};
		return false;
    }
    finally
    {
        //Save errors for review and clean up.
        buffer.Length = 0;
        errors = ~List.CreateFromList(ldrErrors);            
    }
    println(errors.Count);
    return errors.Count == 0;
}");

            Expect(false);
        }
    }
}
