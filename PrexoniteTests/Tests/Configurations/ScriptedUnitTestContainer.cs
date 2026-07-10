using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests.Configurations;

[Parallelizable(ParallelScope.Fixtures)]
abstract class ScriptedUnitTestContainer : IDisposable
{
    public Application Application { get; set; } = null!;
    public Engine Engine { get; private set; } = null!;
    public Loader Loader { get; private set; } = null!;

    public StackContext Root { get; set; } = null!;

    public const string ListTestsId = @"test\list_test";
    public const string RunTestId = @"test\run_test";
    public static readonly string PrexoniteUnitTestFramework = Path.Combine("psr", "test.pxs");
    public const string DumpRequestFlag = "request_dump";

    protected abstract UnitTestConfiguration Runner { get; }

    // NOTE: the RunUnitTest method relies on the fact that StringWriter.ToString() prints the contents.
    // If you change the TextWriter implementation, you need to account for that.
    public TextWriter OneTimeSetupLog { get; } = new StringWriter();
    bool _oneTimeSetupPrinted;

    public void Initialize()
    {
        Application = new(ApplicationName);
        Engine = new();
        Loader = new(Engine, Application);

        Root = new NullContext(Engine, Application, []);

        var slnPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        while (
            slnPath != null
            && Directory.Exists(slnPath)
            && !File.Exists(Path.Combine(slnPath, "prx.sln"))
            && !File.Exists(Path.Combine(slnPath, "prx.slnx"))
        )
            slnPath = Path.Combine(slnPath, @".." + Path.DirectorySeparatorChar);

        if (slnPath != null && Directory.Exists(slnPath))
        {
            var psrTestsPath = Path.GetFullPath(
                Path.Combine(slnPath, nameof(PrexoniteTests), "psr-tests")
            );
            OneTimeSetupLog.WriteLine("inferred psr-tests path: " + psrTestsPath, "Engine.Path");
            Engine.Paths.Add(psrTestsPath);

            var prxPath = Path.GetFullPath(Path.Combine(slnPath, nameof(Prx)));
            OneTimeSetupLog.WriteLine("inferred prx path: " + prxPath, "Engine.Path");
            Engine.Paths.Add(prxPath);
        }
        else
        {
            OneTimeSetupLog.WriteLine("CANNOT INFER solution PATH: " + slnPath, "Engine.Path");
        }
    }

    public string ApplicationName => GetType().Name;

    protected void RunUnitTest(string testCaseId)
    {
        if (!_oneTimeSetupPrinted)
        {
            TestContext.WriteLine(OneTimeSetupLog);
            _oneTimeSetupPrinted = true;
        }

        var tc = Application.Functions[testCaseId];
        Assert.That(tc, Is.Not.Null, "Test case " + testCaseId + " not found.");
        if (tc == null)
            throw new InvalidOperationException("tc is null");
        if (Runner.CompileToCil)
        {
            Assert.That(
                tc.CilImplementation,
                Is.Not.Null,
                "Test case " + testCaseId + " should have a CIL implementation."
            );
        }
        else
        {
            Assert.That(
                tc.CilImplementation,
                Is.Null,
                "Test case " + testCaseId + " should not have a CIL implementation."
            );
        }

        var rt = _findRunFunction();
        Assert.That(
            rt,
            Is.Not.Null,
            "Test case run function (part of testing framework) not found. Was looking for {0}.",
            RunTestId
        );
        if (rt == null)
            throw new InvalidOperationException("rt is null");

        var resP = rt.Run(Engine, [PType.Null, Root.CreateNativePValue(tc)]);
        var success = (bool)resP.DynamicCall(Root, [], PCall.Get, "Key").Value!;
        if (success)
            return;

        var eObj = resP.DynamicCall(Root, [], PCall.Get, "Value")
            .DynamicCall(Root, [], PCall.Get, "e")
            .Value;
        if (eObj is Exception e)
        {
            throw e;
        }
        else
        {
            TestContext.WriteLine("Test failed. Result:");
            TestContext.WriteLine(eObj);
            Assert.Fail("Test failed");
        }
    }

    /// <summary>
    /// Prints a stored representation of each application in the compound that has its "request_dump" flag set.
    /// </summary>
    public void PrintCompound()
    {
        var tasks = Application
            .Compound.Where(app => app.Meta[DumpRequestFlag].Switch)
            .Select(app => new KeyValuePair<ModuleName, Task<ITarget>>(
                app.Module.Name,
                Runner.Cache.BuildAsync(app.Module.Name)
            ))
            .ToDictionary(k => k.Key, k => k.Value);
        var printedRepresentation = false;
        foreach (var (name, targetTask) in tasks)
        {
            printedRepresentation = true;
            var target = targetTask.Result;

            OneTimeSetupLog.WriteLine();
            OneTimeSetupLog.WriteLine(
                "##################################  begin of stored representation for {0} ",
                name
            );

            var opt = new LoaderOptions(Engine, new(target.Module), target.Symbols)
            {
                ReconstructSymbols = false,
                RegisterCommands = false,
                StoreSymbols = true,
            };
            var ldr = new Loader(opt);
            ldr.Store(OneTimeSetupLog);

            OneTimeSetupLog.WriteLine(
                "##################################    end of stored representation for {0} ----------",
                name
            );
        }

        if (printedRepresentation)
        {
            TestContext.WriteLine("---- SNIP end of stored representation ----");
        }
    }

    PFunction? _findRunFunction()
    {
        return Application
            .Compound.Select(app =>
            {
                return app.Functions.TryGetValue(RunTestId, out var func) ? func : null;
            })
            .SingleOrDefault(f => f != null);
    }

    public void Dispose()
    {
        ((UnitTestConfiguration?)Runner)?.Dispose();
    }
}
