#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler.Build;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests.Configurations;

[NonParallelizable]
public abstract class V2UnitTestContainer
{
    private const string RunTestMetaEntryId = "psr.test.run_test";

    private static readonly ModuleName TestFrameworkModuleName =
        new("psr.test", new Version(0, 0));

    private (Application app, ITarget target, PFunction runTestsFunction, Engine engine)? _testSuite;

    [PublicAPI] public string TestSuitePath { get; }
    public bool CompileToCil { get; }

    protected V2UnitTestContainer(string testSuiteFileName, bool compileToCil)
    {
        CompileToCil = compileToCil;
        TestSuitePath = Path.GetFullPath(Path.Combine(
            ModuleCacheV2.SolutionPath, "PrexoniteTests", "psr-tests", "_2", testSuiteFileName
        ));
    }

    [PublicAPI]
    public Application TestSuiteApplication => 
        _testSuite?.app 
        ?? throw new PrexoniteException($"Test suite application for {TestSuitePath} was not initialized.");
        
    [PublicAPI]
    public ITarget TestSuiteTarget => 
        _testSuite?.target
        ?? throw new PrexoniteException($"Test suite target for {TestSuitePath} was not initialized.");
        
    [PublicAPI]
    public PFunction TestSuiteRunFunction => 
        _testSuite?.runTestsFunction
        ?? throw new PrexoniteException($"Test suite run function for {TestSuitePath} was not initialized.");
        
    [PublicAPI]
    public Engine TestSuiteEngine => 
        _testSuite?.engine
        ?? throw new PrexoniteException($"Test suite engine for {TestSuitePath} was not initialized.");

    [OneTimeSetUp]
    public void SetUpApplication()
    {
        var (app, target) = ModuleCacheV2.Load(TestSuitePath, CompileToCil);

        if (!target.IsSuccessful)
        {
            TestContext.WriteLine($"Loading {TestSuitePath} was not successful (cil={CompileToCil})");
            foreach (var msg in target.Messages)
            {
                TestContext.WriteLine(msg);
            }

            if (target.Exception != null)
            {
                throw new AssertionException("Errors while loading the test suite.", target.Exception);
            }
            else
            {
                Assert.Fail("Errors while loading the test suite.");
            }
        }

        var testModule = TestFrameworkModuleName;
        if(!(app.Compound.TryGetApplication(testModule, out var testFrameworkApp)
               && testFrameworkApp.Meta.TryGetValue(RunTestMetaEntryId, out var runTestEntry)
               && testFrameworkApp.TryGetFunction(runTestEntry.Text, testModule, out var runTestFunction)))
        {
            throw new PrexoniteException($"Cannot find function indicated by meta entry " +
                $"{RunTestMetaEntryId} in module {testModule}");
        }

        _testSuite = (app, target, runTestFunction, ModuleCacheV2.CreateEngine());
    }

    public class TestUiIntegration : IObject
    {
            
            
        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = PType.Null;
            (string Id, Application ParentApplication) testCase;
            switch (id.ToUpperInvariant())
            {
                case "BEGIN_RUNNING":
                    testCase = extract(sctx, args[0]);
                    Trace.WriteLine($"begin running test case [{sctx.ParentApplication.Module.Name}]" +
                        $".{testCase.Id}");
                    return true;
                case "SUCCESS":
                    testCase = extract(sctx,
                        args[0].TryDynamicCall(sctx, new PValue[0], PCall.Get, "test", out var testCaseValue)
                            ? testCaseValue
                            : throw new PrexoniteException("Expected test result to have a member called `test`."));
                    Trace.WriteLine($"test [{sctx.ParentApplication.Module.Name}].{testCase.Id} successful");
                    return true;
                case "FAILURE":
                    testCase = extract(sctx,
                        args[0].TryDynamicCall(sctx, new PValue[0], PCall.Get, "test", out testCaseValue)
                            ? testCaseValue
                            : throw new PrexoniteException("Expected test result to have a member called `test`."));
                    var exceptionObj =
                        args[0].TryDynamicCall(sctx, new PValue[0], PCall.Get, "e", out var exceptionValue)
                            ? exceptionValue.Value
                            : throw new PrexoniteException(
                                "Expected failed test result to have a member called `e`");
                    if (exceptionObj is Exception e)
                    {
                        // TODO don't throw exception here but store it in the UI
                        // Don't re-throw the exception because that would overwrite the stack trace.
                        throw new AssertionException(
                            $"Test [{testCase.ParentApplication.Module.Name}].{testCase.Id} failed.", e);
                    }
                    else
                    {
                        throw new AssertionException(
                            $"Test [{testCase.ParentApplication.Module.Name}].{testCase.Id} failed: " +
                            $"{exceptionObj}");
                    }
                default:
                    return false;
            }
        }

        private static readonly PValue[] Empty = new PValue[0];
            
        private static (string Id, Application ParentApplication) extract(StackContext sctx, PValue funcValue)
        {
            if (funcValue.Value is PFunction f)
            {
                return (f.Id, f.ParentApplication);
            }

            return (
                (string) funcValue.DynamicCall(sctx, Empty, PCall.Get, "Id").ConvertTo(sctx, PType.String).Value,
                funcValue.DynamicCall(sctx, Empty, PCall.Get, "ParentApplication").ConvertTo<Application>(sctx)
            );
        }
    }
        
    public void RunTestCase(string testCaseName)
    {
        // GIVEN
        Assert.IsTrue(TestSuiteApplication.TryGetFunction(testCaseName, null, out var testCase),
            "Could not find test case {0} in {1}", testCaseName, TestSuiteApplication.Module.Name);
        var ui = TestSuiteEngine.CreateNativePValue(new TestUiIntegration());
        var testCaseValue = TestSuiteEngine.CreateNativePValue(testCase);
            
        // WHEN
        // result~(Bool:Structure)
        var result = TestSuiteRunFunction.Run(TestSuiteEngine, new[] {ui, testCaseValue});
            
        // THEN
        // (the UI callback should actually raise an exception in case of a test error)
        var ctx = new NullContext(TestSuiteEngine, TestSuiteApplication, Enumerable.Empty<string>());
        Assert.IsTrue(result.TryDynamicCall(ctx, new PValue[0], PCall.Get, "Key", out var successfulValue), 
            "Expected result of test run function on [{1}].{2} to have a `Key` member. Got: {0}", result,
            TestSuiteApplication.Module.Name, testCaseName);
        Assert.AreEqual(true, successfulValue.Value, "Expected test [{0}].{1} to be successful.",
            TestSuiteApplication.Module.Name, testCaseName);
    }
        
}