

using System;
using System.Threading;
using NUnit.Framework;
using Prexonite;
using Prexonite.Types;
using Prx.Tests;

namespace PrexoniteTests.Tests;

[Parallelizable(ParallelScope.Fixtures | ParallelScope.Self)]
[TestFixture(Description = "General type system checks")]
public class TypeSystem
{
    Engine engine = null!;
    StackContext sctx = null!;

    [OneTimeSetUp]
    public void SetupTypeSystemEngine()
    {
        engine = new();
        sctx = new TestStackContext(engine, new());
    }

    [OneTimeTearDown]
    public void TeardownTypeSystemEngine()
    {
        engine = null!;
        sctx = null!;
    }

    [Test(Description = "Does a number of set and get calls on a mock object")]
    public void TestFieldAccess()
    {
        //obj.Subject = 55;
        //obj.Count = obj.Subject;
        var test = new TestObject();
        var obj = engine.CreateNativePValue(test);
        Assert.AreEqual(obj.Type, PType.Object[typeof (TestObject)]);
        Assert.AreSame(test, obj.Value);

        //Set obj.Subject = 55
        obj.DynamicCall(
            sctx,
            [55],
            PCall.Set,
            "Subject");
        Assert.AreSame(test, obj.Value);
        Assert.AreEqual(test.Subject, ((TestObject) obj.Value!).Subject);

        //Get res = obj.Subject
        var res = obj.DynamicCall(
            sctx,
            [],
            PCall.Get,
            "Subject");
        Assert.AreEqual(test.Subject, res.Value);

        //Set obj.Count = res
        obj.DynamicCall(
            sctx,
            [res],
            PCall.Set,
            "Count");
        Assert.AreEqual(55, test.Count);

        //Get res = obj.Count
        res = obj.DynamicCall(
            sctx,
            [],
            PCall.Get,
            "Count");
        Assert.AreEqual(55, test.Count);
        Assert.AreEqual(55, (int) res.Value!);
    }

    [Test(Description = "Test the implicit [basic] to [PValue] conversion operators")]
    public void TestImplicitPValueConversion()
    {
        PValue obj;

        obj = 55;
        Assert.AreSame(PType.Int, obj.Type);

        obj = 5.5;
        Assert.AreSame(PType.Real, obj.Type);

        obj = true;
        Assert.AreSame(PType.Bool, obj.Type);

        obj = "Hello World";
        Assert.AreSame(PType.String, obj.Type);
    }

    [Test(Description = "PType creation from type name.")]
    public void TestPTypeCreationUsingClrObjects_name()
    {
        var res =
            sctx.ConstructPType(
                nameof(Object),
                [PType.Object.CreatePValue(typeof (int))]);
        Assert.AreEqual(PType.Object[typeof (int)], res);
    }

    [Test(Description = "PType creation from type")]
    public void TestPTypeCreationUsingClrObjects_type()
    {
        var res =
            sctx.ConstructPType(
                typeof (ObjectPType),
                [PType.Object.CreatePValue(typeof (DateTime))]);
        Assert.AreEqual(PType.Object[typeof (DateTime)], res);
    }

    [Test(Description = "PType creation from type as ClrPType")]
    public void TestPTypeCreationUsingClrObjects_clrtype()
    {
        var res =
            sctx.ConstructPType(
                PType.Object[typeof (ObjectPType)],
                [PType.Object.CreatePValue(typeof (Thread))]);
        Assert.AreEqual(PType.Object[typeof (Thread)], res);
    }

    [Test(Description = "Creation of a ClrPType from a fqTypeName")]
    public void TestTypeResolving()
    {
        var res = new ObjectPType(sctx, "System.Threading.Thread");
        Assert.AreEqual(PType.Object[typeof(Thread)], res);
    }

    [Test(Description = "PType creation from type simple expression")]
    public void TestPTypeCreationUsingExpression_simple()
    {
        var res = sctx.ConstructPType("Object(\"System.Threading.Thread\")");
        Assert.AreEqual(PType.Object[typeof (Thread)], res);
    }

    [Test]
    public void NativePValue()
    {
        var str = "Hello ";
        var nStr = engine.CreateNativePValue(str);
        Assert.IsInstanceOf(typeof (StringPType), nStr.Type);
        Assert.AreSame(str, nStr.Value);
    }

    [Test]
    public void TestStringEscape()
    {
        var sEscaped = @"This is a \n followed by a \ttab and an umlaut: \x" +
            ((int) 'ä').ToString("X") +
            " escape sequence.";
        var sUnescaped = "This is a \n followed by a \ttab and an umlaut: ä escape sequence.";

        var escaped = PType.String.CreatePValue(sEscaped);
        Assert.IsTrue(
            escaped.TryDynamicCall(sctx, [], PCall.Get, "unescape", out var unescaped));
        Assert.AreEqual(sUnescaped, unescaped!.Value as string);
        Assert.IsTrue(
            unescaped.TryDynamicCall(sctx, [], PCall.Get, "escape", out escaped));
        Assert.AreEqual(sEscaped, escaped!.Value as string);
    }
}