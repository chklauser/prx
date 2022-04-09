#nullable enable

using System;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests.Configurations;

internal abstract class UnitTestConfiguration : IDisposable
{
    public class InMemory : UnitTestConfiguration
    {
    }

    protected UnitTestConfiguration()
    {
        Linking = FunctionLinking.FullyStatic;
        CompileToCil = false;
    }

    public FunctionLinking Linking { get; init; }
    public bool CompileToCil { get; init; }

    private bool _configured;
    private ModuleCache? _cache;

    public ModuleCache Cache
    {
        get
        {
            if (!_configured)
            {
                throw new InvalidOperationException("Must call Configure before accessing the module cache.");
            }
            return _cache ??= ModuleCache.LeaseFor(CompileToCil, Linking);
        }
    }

    /// <summary>
    /// Executed as the last step of loading, immediately before the actual test methods are executed.
    /// </summary>
    /// <param name="runner">The container under which the test is being executed.</param>
    private void _prepareExecution(ScriptedUnitTestContainer runner)
    {
        if (CompileToCil)
            Compiler.Compile(runner.Application, runner.Engine, Linking);
    }

    protected void LoadUnitTestingFramework(ScriptedUnitTestContainer container)
    {
        Cache.Describe(container.Loader,new TestDependency
        {
            ScriptName = ScriptedUnitTestContainer.PrexoniteUnitTestFramework
        });
    }

// ReSharper disable InconsistentNaming
    internal void Configure(TestModel model, ScriptedUnitTestContainer container)
// ReSharper restore InconsistentNaming
    {
        // We can be certain that CompileToCil and Linking are set
        _configured = true;

        // describe units under test
        foreach (var unit in model.UnitsUnderTest)
            Cache.Describe(container.Loader, unit);

        // describe unit testing framework
        LoadUnitTestingFramework(container);

        // describe unit testing extensions
        foreach(var extension in model.TestDependencies)
            Cache.Describe(container.Loader, extension); 

        // describe test suite
        var suiteDependencies =
            model.UnitsUnderTest
                .Append(model.TestDependencies)
                .Select(d => d.ScriptName)
                .Append(ScriptedUnitTestContainer.PrexoniteUnitTestFramework)
                .ToArray();
        var suiteDescription = new TestDependency
        {
            ScriptName = model.TestSuiteScript, Dependencies = suiteDependencies
        };
        Cache.Describe(container.Loader, suiteDescription);

        // Finally instantiate the test suite application(s)
        var (application, target) = Cache.Load(model.TestSuiteScript);
        container.Application = application;
        container.PrintCompound();

        if (!target.IsSuccessful)
        {
            container.OneTimeSetupLog.WriteLine("The target {0} failed to build. Working directory: {1}", target.Name, Environment.CurrentDirectory);

            if(target.Exception != null)
                container.OneTimeSetupLog.WriteLine(target.Exception);

            foreach (var error in target.Messages.Where(m => m.Severity == MessageSeverity.Error))
                container.OneTimeSetupLog.WriteLine("Error: {0}", error);
            foreach (var warning in target.Messages.Where(m => m.Severity == MessageSeverity.Warning))
                container.OneTimeSetupLog.WriteLine("Warning: {0}", warning);
            foreach (var info in target.Messages.Where(m => m.Severity == MessageSeverity.Info))
                container.OneTimeSetupLog.WriteLine("Info: {0}", info);

            TestContext.WriteLine(container.OneTimeSetupLog);
            Assert.Fail("The target {0} failed to build. Working directory: {1}", target.Name, Environment.CurrentDirectory);
        }

        _prepareExecution(container);
    }

    public void Dispose()
    {
        if(!_configured)    
            return;
        if (_cache is { } cache)
        {
            ModuleCache.ReturnTo(CompileToCil, Linking, cache);
        }
    }
}