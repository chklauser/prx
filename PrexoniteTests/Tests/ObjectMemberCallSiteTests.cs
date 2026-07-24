using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Types;
using CilCompiler = Prexonite.Compiler.Cil.Compiler;

namespace PrexoniteTests.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class ObjectMemberCallSiteTests : VMTestsBase
{
    public ObjectMemberCallSiteTests()
    {
        CompileToCil = false;
    }

    [Test]
    public void InterpreterCallSiteDistinguishesExplicitlyConvertedArguments()
    {
        Compile(
            """
            function invokeProbe(target, value) = target.Select(value, 2, 3);

            function main(target, value) =
                [invokeProbe(target, value),
                 invokeProbe(target, value~Int),
                 invokeProbe(target, value)];
            """
        );

        Expect(
            result =>
            {
                var values = (List<PValue>)result.Value!;
                Assert.That(
                    values.Select(value => value.Value),
                    Is.EqualTo(
                        new[] { "automatic conversion", "type locked", "automatic conversion" }
                    )
                );
            },
            engine.CreateNativePValue(new OverloadProbe()),
            PType.Int.CreatePValue(1)
        );

        var function = target.Functions["invokeProbe"]!;
        var callOffset = function.Code.FindIndex(instruction =>
            instruction.OpCode == OpCode.get && instruction.Id == "Select"
        );
        Assert.That(callOffset, Is.GreaterThanOrEqualTo(0));
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(callOffset),
            Is.InstanceOf<DynamicObjectMemberCallSite>()
        );
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(callOffset)!.BindingCount,
            Is.EqualTo(2),
            "The polymorphic site should bind once for unlocked Int and once for locked Int."
        );
    }

    [Test]
    public void ExplicitConversionCreatesALockedCopy()
    {
        Compile("function main(value) = value~Int;");
        var original = PType.Int.CreatePValue(1);

        PValue? result = null;
        Expect(value => result = value, original);

        Assert.That(original.IsTypeLocked, Is.False);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsTypeLocked, Is.True);
        Assert.That(result.Type, Is.SameAs(original.Type));
        Assert.That(result.Value, Is.EqualTo(original.Value));
        Assert.That(result, Is.Not.SameAs(original));
    }

    [Test]
    public void ExplicitConversionOfAConstantIsNotFoldedAway()
    {
        Compile("function main(target) = target.Select(1~Int, 2, 3);");

        Assert.That(
            target.Functions["main"]!.Code.Any(instruction =>
                instruction.OpCode == OpCode.cast_const
            ),
            Is.True
        );
        Expect("type locked", engine.CreateNativePValue(new OverloadProbe()));
    }

    [Test]
    public void InterpreterCallSiteDistinguishesPTypeTagsForTheSameClrType()
    {
        Compile("function main(target, value) = target.Accept(value);");
        var probe = engine.CreateNativePValue(new OverloadProbe());

        Expect("accepted", probe, PType.Int.CreatePValue(1));
        Expect("accepted", probe, PType.Object[typeof(int)].CreatePValue(1));
        Expect("accepted", probe, PType.Int.CreatePValue(2));

        var function = target.Functions["main"]!;
        var callOffset = function.Code.FindIndex(instruction =>
            instruction.OpCode == OpCode.get && instruction.Id == "Accept"
        );
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(callOffset)!.BindingCount,
            Is.EqualTo(2),
            "Int and Object<Int> must produce separate rules despite wrapping the same CLR type."
        );
    }

    [Test]
    public void InterpreterStaticCallSiteDistinguishesExplicitConversions()
    {
        engine.RegisterAssembly(typeof(ObjectMemberCallSiteStaticProbe).Assembly);
        Compile(
            """
            function invokeProbe(value) =
                PrexoniteTests::Tests::ObjectMemberCallSiteStaticProbe.Select(value, 2, 3);

            function main(value) =
                [invokeProbe(value), invokeProbe(value~Int), invokeProbe(value)];
            """
        );

        Expect(
            result =>
            {
                var values = (List<PValue>)result.Value!;
                Assert.That(
                    values.Select(value => value.Value),
                    Is.EqualTo(
                        new[] { "automatic conversion", "type locked", "automatic conversion" }
                    )
                );
            },
            PType.Int.CreatePValue(1)
        );

        var function = target.Functions["invokeProbe"]!;
        var callOffset = function.Code.FindIndex(instruction =>
            instruction.OpCode == OpCode.sget && instruction.Id!.EndsWith("::Select")
        );
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(callOffset)!.BindingCount,
            Is.EqualTo(2)
        );
    }

    public sealed class OverloadProbe
    {
        public string Select(double value, int second, int third)
        {
            return "automatic conversion";
        }

        public string Select(int value, double second, double third)
        {
            return "type locked";
        }

        public string Accept(int value)
        {
            return "accepted";
        }
    }
}

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class ObjectMemberCallSiteCilCompatibilityTests : VMTestsBase
{
    [Test]
    public void CilExplicitConversionStillLocksOverloadResolution()
    {
        Compile("function main(target) = target.Select(1~Int, 2, 3);");

        Expect(
            "type locked",
            engine.CreateNativePValue(new ObjectMemberCallSiteTests.OverloadProbe())
        );
    }

    [Test]
    public void CilCallSiteDistinguishesExplicitlyConvertedArguments()
    {
        Compile(
            """
            function invokeProbe(target, value) = target.Select(value, 2, 3);

            function main(target, value) =
                [invokeProbe(target, value),
                 invokeProbe(target, value~Int),
                 invokeProbe(target, value)];
            """
        );

        Expect(
            result =>
            {
                var values = (List<PValue>)result.Value!;
                Assert.That(
                    values.Select(value => value.Value),
                    Is.EqualTo(
                        new[] { "automatic conversion", "type locked", "automatic conversion" }
                    )
                );
            },
            engine.CreateNativePValue(new ObjectMemberCallSiteTests.OverloadProbe()),
            PType.Int.CreatePValue(1)
        );

        var function = target.Functions["invokeProbe"]!;
        Assert.That(function.HasCilImplementation, Is.True);
        Assert.That(
            function
                .ObjectMemberCallSites.GetExisting(_callOffset(function, OpCode.get, "Select"))!
                .BindingCount,
            Is.EqualTo(2)
        );
    }

    [Test]
    public void CilCallSiteDistinguishesPTypeTagsForTheSameClrType()
    {
        Compile("function main(target, value) = target.Accept(value);");
        var probe = engine.CreateNativePValue(new ObjectMemberCallSiteTests.OverloadProbe());

        Expect("accepted", probe, PType.Int.CreatePValue(1));
        Expect("accepted", probe, PType.Object[typeof(int)].CreatePValue(1));
        Expect("accepted", probe, PType.Int.CreatePValue(2));

        var function = target.Functions["main"]!;
        Assert.That(
            function
                .ObjectMemberCallSites.GetExisting(_callOffset(function, OpCode.get, "Accept"))!
                .BindingCount,
            Is.EqualTo(2)
        );
    }

    [Test]
    public void CilStaticCallSiteDistinguishesExplicitConversions()
    {
        engine.RegisterAssembly(typeof(ObjectMemberCallSiteStaticProbe).Assembly);
        Compile(
            """
            function invokeProbe(value) =
                PrexoniteTests::Tests::ObjectMemberCallSiteStaticProbe.Select(value, 2, 3);

            function main(value) =
                [invokeProbe(value), invokeProbe(value~Int), invokeProbe(value)];
            """
        );

        Expect(
            result =>
            {
                var values = (List<PValue>)result.Value!;
                Assert.That(
                    values.Select(value => value.Value),
                    Is.EqualTo(
                        new[] { "automatic conversion", "type locked", "automatic conversion" }
                    )
                );
            },
            PType.Int.CreatePValue(1)
        );

        var function = target.Functions["invokeProbe"]!;
        var callOffset = function.Code.FindIndex(instruction =>
            instruction.OpCode == OpCode.sget && instruction.Id!.EndsWith("::Select")
        );
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(callOffset)!.BindingCount,
            Is.EqualTo(2)
        );
    }

    [Test]
    public void CilObjectOverrideDoesNotCreateAReflectionCallSite()
    {
        Compile("function main(target) = target.answer();");
        var callable = new MemberCallable { Name = "callsite override" };
        callable.Expect("answer", [], returns: PType.Int.CreatePValue(42));

        Expect(42, engine.CreateNativePValue(callable));

        var function = target.Functions["main"]!;
        Assert.That(
            function.ObjectMemberCallSites.GetExisting(_callOffset(function, OpCode.get, "answer")),
            Is.Null
        );
        callable.AssertCalledAll();
    }

    [Test]
    public void CilCallSitesCanBeDisabledForDiagnosticComparison()
    {
        var loader = new Loader(options);
        loader.LoadFromString("function main(target) = target.Accept(1);");
        Assert.That(loader.ErrorCount, Is.Zero);
        var function = target.Functions["main"]!;
        function.Meta[CilCompiler.DisableObjectMemberCallSitesKey] = true;
        CilCompiler.Compile(target, engine);

        Expect(
            "accepted",
            engine.CreateNativePValue(new ObjectMemberCallSiteTests.OverloadProbe())
        );

        Assert.That(
            function.ObjectMemberCallSites.GetExisting(_callOffset(function, OpCode.get, "Accept")),
            Is.Null
        );
    }

    static int _callOffset(PFunction function, OpCode opCode, string id)
    {
        var callOffset = function.Code.FindIndex(instruction =>
            instruction.OpCode == opCode && instruction.Id == id
        );
        Assert.That(callOffset, Is.GreaterThanOrEqualTo(0));
        return callOffset;
    }
}

public static class ObjectMemberCallSiteStaticProbe
{
    public static string Select(double value, int second, int third)
    {
        return "automatic conversion";
    }

    public static string Select(int value, double second, double third)
    {
        return "type locked";
    }
}
