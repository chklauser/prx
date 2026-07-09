
#define UseCil

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Symbolic;
using Prexonite.Types;
using Prx.Tests;
using Compiler = Prexonite.Compiler.Cil.Compiler;

namespace PrexoniteTests.Tests;

[Parallelizable(ParallelScope.Fixtures)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class VMTestsBase
{
    public const string StoreDebugImplementationKey = "store_debug_implementation";
    protected Engine engine = null!;
    protected TestStackContext sctx = null!;
    protected Application target = null!;
    protected LoaderOptions options = null!;

    /// <summary>
    /// Indicates whether to skip the <see cref="Loader.Store(System.Text.StringBuilder)"/> call on compilation.
    /// </summary>
    public bool SkipStore { get; set; }

    public bool CompileToCil { get; set; }
    protected FunctionLinking StaticLinking { get; set; }

    public VMTestsBase()
    {
#if UseCil
        CompileToCil = true;
#else
            CompileToCil = false;
#endif
        StaticLinking = FunctionLinking.FullyStatic;
    }

    [SetUp]
    public virtual void SetupCompilerEngine()
    {
        engine = new();
        target = new("testApplication");
        sctx = new(engine, target);
        options = new(engine, target);
        SkipStore = false;
    }

    [TearDown]
    public void TeardownCompilerEngine()
    {
        engine = null!;
        sctx = null!;
        target = null!;
        options = null!;
    }

    protected static string GenerateRandomString(int length)
    {
        return GenerateRandomString()[..length];
    }

    protected static string GenerateRandomString()
    {
        return Guid.NewGuid().ToString("N");
    }

    protected void Compile(Loader ldr, string input)
    {
        _compile(ldr, input);
        Assert.AreEqual(0, ldr.ErrorCount, "Errors detected during compilation.");
    }

    protected Loader CompileInvalid(string input, params string[] keywords)
    {
        var ldr = new Loader(options);
        CompileInvalid(ldr, input, keywords);
        return ldr;
    }

    protected void CompileInvalid(Loader ldr, string input, params string[] keywords)
    {
        _compile(ldr, input);
        Assert.AreNotEqual(0, ldr.Errors.Count(m => m.Severity == MessageSeverity.Error),
            "Errors expected, but none were raised.");
        foreach (var keyword in keywords)
        {
            var word = keyword;
            Assert.IsTrue(ldr.Errors.Any(m => m.Text.Contains(word)),
                "Expected keyword " + word + " in one of the error messages.");
        }
    }

    void _compile(Loader ldr, string input)
    {
        try
        {
            ldr.LoadFromString(input);
            if (CompileToCil)
                Compiler.Compile(ldr.ParentApplication, ldr.ParentEngine, StaticLinking);
        }
        finally
        {
            foreach (var s in ldr.Errors)
                TestContext.WriteLine("ERROR: " + s);
            foreach (var s in ldr.Warnings)
                TestContext.WriteLine("WARNING: " + s);
            foreach (var s in ldr.Infos)
                TestContext.WriteLine("INFO: " + s);

            if(!SkipStore)
                TestContext.WriteLine(ldr.StoreInString());
        }
    }

    protected Loader Compile(string input)
    {
        var ldr = new Loader(options);
        Compile(ldr, input);
        return ldr;
    }

    protected Loader Store(Loader ldr)
    {
        var sb = new StringBuilder();
        ldr.Store(sb);


        //Create a new engine
        SetupCompilerEngine();

        ldr = new(options);
        try
        {
            ldr.LoadFromString(sb.ToString());
        }
        finally
        {
            foreach (var s in ldr.Errors)
            {
                TestContext.Error.WriteLine(s);
            }
        }
        Assert.AreEqual(0, ldr.ErrorCount, "Errors detected while loading stored code.");
        TestContext.WriteLine(ldr.StoreInString());
        return ldr;
    }

    protected Loader CompileStore(Loader loader, string input)
    {
        Compile(loader, input);
        return Store(loader);
    }

    protected Loader CompileStore(string input)
    {
        return Store(Compile(input));
    }

    protected void Expect<T>(T expectedReturnValue, params ReadOnlySpan<PValue> args)
    {
        ExpectReturnValue(target.Meta[Application.EntryKey], expectedReturnValue, args);
    }

    protected void Expect(Action<PValue> assertion, params ReadOnlySpan<PValue> args)
    {
        ExpectReturnValue(target.Meta[Application.EntryKey], assertion, args);
    }

    protected void ExpectNamed<T>(string functionId, T expectedReturnValue, params ReadOnlySpan<PValue> args)
    {
        ExpectReturnValue(functionId, expectedReturnValue, args);
    }

    protected void ExpectReturnValue<T>(string functionId, T expectedReturnValue, ReadOnlySpan<PValue> args)
    {
        ExpectReturnValue(functionId, rv =>
        {
            var expected = engine.CreateNativePValue(expectedReturnValue);
            AssertPValuesAreEqual(expected, rv);
        }, args);
    }

    protected void ExpectReturnValue(string functionId, Action<PValue> assertion, ReadOnlySpan<PValue> args)
    {
        if (assertion == null)
            throw new ArgumentNullException(nameof(assertion));

        foreach (var value in args)
        {
            if ((PValue?)value == null) throw new ArgumentException("Arguments must not contain naked CLR null references. Use `PType.Null`.");
        }

        if (!target.Functions.Contains(functionId))
            throw new PrexoniteException("Function " + functionId + " cannot be found.");

        var func = target.Functions[functionId];
        Assert.That(func, Is.Not.Null);
        if (func!.Meta[StoreDebugImplementationKey].Switch)
            Compiler.StoreDebugImplementation(target, engine);

        PValue rv;
        try
        {
            rv = func.Run(engine, args);
        }
        catch (AccessViolationException)
        {
            TestContext.WriteLine(
                "Detected AccessViolationException. Trying to store debug implementation (Repeats CIL compilation)");
            Compiler.StoreDebugImplementation(target, engine);
            throw;
        }
        catch (InvalidProgramException)
        {
            TestContext.WriteLine(
                "Detected InvalidProgramException. Trying to store debug implementation (Repeats CIL compilation)");
            Compiler.StoreDebugImplementation(target, engine);
            throw;
        }

        assertion(rv);
    }

    public void AssertPValuesAreEqual(PValue expected, PValue rv)
    {
        Assert.AreEqual(
            expected.Type,
            rv.Type,
            $"Return value is expected to be of type {expected.Type} and not {rv.Type}. Returned {rv}.");
        if (expected.Type == PType.List)
        {
            var expectedL = (List<PValue>) expected.Value!;
            var rvL = (List<PValue>) rv.Value!;
            Assert.AreEqual(expectedL.Count, rvL.Count,
                $"Returned list differs in length. Elements returned {rvL.ToEnumerationString()}");

            for (var i = 0; i < expectedL.Count; i++)
            {
                var expectedLi = expectedL[i];
                var rvLi = rvL[i];
                AssertPValuesAreEqual(expectedLi, rvLi);
            }
        }
        else
        {
            Assert.AreEqual(
                expected.Value,
                rv.Value,
                "Return value is expected to be " + expected + " and not " +
                rv);
        }
    }

    protected void ExpectNull(params PValue[] args)
    {
        var functionId = target.Meta[Application.EntryKey];
        ExpectReturnValue(functionId, _buildIsNullAssertion(functionId), args);
    }

    protected void ExpectNull(string functionId, params PValue[] args)
    {
        ExpectReturnValue(functionId, _buildIsNullAssertion(functionId), args);
    }

    static Action<PValue> _buildIsNullAssertion(string functionId)
    {
        return v =>
            Assert.That(v.IsNull, Is.True,
                $"Value returned from {functionId} is expected to be a null reference, was {v} instead.");
    }

    protected PValue GetReturnValueNamed(string functionId, params PValue[] args)
    {
        return GetReturnValueNamedExplicit(functionId, args);
    }

    protected PValue GetReturnValueNamedExplicit(string functionId, PValue[] args)
    {
        if (target.Functions[functionId] is not {} func)
            throw new PrexoniteException("Function " + functionId + " cannot be found.");
        var fctx = func.CreateFunctionContext(engine, args);
        engine.Stack.AddLast(fctx);
        return engine.Process();
    }

    protected PValue GetReturnValue(params PValue[] args)
    {
        return GetReturnValueNamedExplicit(target.Meta[Application.EntryKey], args);
    }

    protected Symbol LookupSymbolEntry(ISymbolView<Symbol> store,string symbolicId)
    {
        Assert.IsTrue(store.TryGet(symbolicId, out var symbol),
            $"Expected to find symbol {symbolicId} but there is no such entry.");
        return symbol!;
    }

    protected void BoolTable4(Func<bool, bool, bool, bool, string> main, PValue pTrue,
        PValue pFalse)
    {
        Expect(main(false, false, false, false), pFalse, pFalse, pFalse, pFalse);
        Expect(main(false, false, false, true), pFalse, pFalse, pFalse, pTrue);
        Expect(main(false, false, true, false), pFalse, pFalse, pTrue, pFalse);
        Expect(main(false, false, true, true), pFalse, pFalse, pTrue, pTrue);
        Expect(main(false, true, false, false), pFalse, pTrue, pFalse, pFalse);
        Expect(main(false, true, false, true), pFalse, pTrue, pFalse, pTrue);
        Expect(main(false, true, true, false), pFalse, pTrue, pTrue, pFalse);
        Expect(main(false, true, true, true), pFalse, pTrue, pTrue, pTrue);

        Expect(main(true, false, false, false), pTrue, pFalse, pFalse, pFalse);
        Expect(main(true, false, false, true), pTrue, pFalse, pFalse, pTrue);
        Expect(main(true, false, true, false), pTrue, pFalse, pTrue, pFalse);
        Expect(main(true, false, true, true), pTrue, pFalse, pTrue, pTrue);
        Expect(main(true, true, false, false), pTrue, pTrue, pFalse, pFalse);
        Expect(main(true, true, false, true), pTrue, pTrue, pFalse, pTrue);
        Expect(main(true, true, true, false), pTrue, pTrue, pTrue, pFalse);
        Expect(main(true, true, true, true), pTrue, pTrue, pTrue, pTrue);
    }
}