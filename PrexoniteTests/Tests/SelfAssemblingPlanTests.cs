﻿// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using NUnit.Framework;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace PrexoniteTests.Tests;

[TestFixture]
public class SelfAssemblingPlanTests
{
    protected static readonly TraceSource Trace = new("PrexoniteTests.Tests.SelfAssemblingPlan");

    private string _basePath;

    protected ISelfAssemblingPlan Sam;

    protected IDisposable MockFile(string path, string content)
    {
        var handle = new MockFileHandle(new FileInfo(Path.Combine(_basePath, path)), this);
        var fileDir = handle.File.Directory;
        Debug.Assert(fileDir != null, "handle.File.Directory != null");
        fileDir.Create();
        using var sw = new StreamWriter(handle.File.ToString(),false,Encoding.UTF8);
        sw.Write(content);
        sw.Flush();
        sw.Close();
        return handle;
    }

    private class MockFileHandle : IDisposable
    {
        [NotNull]
        public FileInfo File { get; }

        [NotNull]
        public SelfAssemblingPlanTests Instance { get; }

        public MockFileHandle([NotNull]FileInfo file, [NotNull]SelfAssemblingPlanTests instance)
        {
            File = file;
            Instance = instance;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                    
            }
            _tryDelete();
        }

        private void _tryDelete()
        {
            try
            {
                File.Delete();
            }
            catch (IOException e)
            {
                Trace.TraceEvent(TraceEventType.Error, 0, $"Cannot delete file {File} because of {e}");
            }
        }

        ~MockFileHandle()
        {
            Dispose(false);
        }
    }

    private string _prototypePath;

    [OneTimeSetUp]
    public void Init()
    {
        _prototypePath = Path.Combine(Path.GetTempPath(), "PrexoniteTests.SelfAssemblingPlanTests");
    }

    [SetUp]
    public void Prepare()
    {
        _basePath = Path.Combine(_prototypePath, Guid.NewGuid().ToString("N"));
        if (Directory.Exists(_basePath))
        {
            Trace.TraceEvent(TraceEventType.Warning, 0,
                "Expected temporary directory at {0} not to exist. Using it anyway.", _basePath);
        }
        Sam = Plan.CreateSelfAssembling();
        Sam.SearchPaths.Add(_basePath);
    }

    private void _tryTearDown()
    {
        try
        {
            Directory.Delete(_basePath, recursive: true);
        }
        catch (Exception e)
        {
            Trace.TraceEvent(TraceEventType.Error, 0, "Exception during tear down (deletion of temp dir): {0}", e);
            TestContext.WriteLine(e);
        }
    }

    [TearDown]
    public void TearDown()
    {
        _tryTearDown();
    }

    [OneTimeTearDown]
    public void LastDitchEffort()
    {
        try
        {
            Directory.Delete(_prototypePath,recursive:true);
        }
        catch (Exception e)
        {
            Trace.TraceEvent(TraceEventType.Error, 0, "Exception during fixture tear down (last ditch effort, deletion of temp dir): {0}", e);
            TestContext.WriteLine(e);
        }
    }

    [Test]
    public void EmptyInMemory()
    {
        var desc = Sam.AssembleAsync(Source.FromString(""), CancellationToken.None).Result;
        Assert.That(desc,Is.Not.Null);
        Assert.That(desc.BuildMessages,Is.Empty,"Should not have build (error) messages");
    }

    [Test]
    public void Empty()
    {
        const string path = "empty.pxs";
        using (MockFile(path,""))
        {
            var desc = Sam.AssembleAsync(Source.FromFile(Path.Combine(_basePath,path),Encoding.UTF8), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
        }
    }

    [Test]
    public void ExtractModuleNameInMemory()
    {
        var desc = Sam.AssembleAsync(Source.FromString("name the_module/5.4.3.2;"), CancellationToken.None).Result;
        Assert.That(desc, Is.Not.Null);
        Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
        Assert.That(desc.Name,Is.EqualTo(new ModuleName("the_module",new Version(5,4,3,2))));
    }

    [Test]
    public void ExtractModuleName()
    {
        const string path = "unrelated_name.pxs";
        using (MockFile(path,"name the_module/5.4.3.2;"))
        {
            var desc = Sam.AssembleAsync(Source.FromFile(Path.Combine(_basePath,path), Encoding.UTF8), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("the_module", new Version(5, 4, 3, 2))));
        }
    }

    [Test]
    public void SinglePathDependency()
    {
        const string path = "find_me.pxs";
        using (MockFile(path, "name the_module/5.4.3.2;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    ""./find_me.pxs""
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0,0))));
            var theModuleName = new ModuleName("the_module", new Version(5, 4, 3, 2));
            Assert.That(desc.Dependencies.Count,Is.GreaterThanOrEqualTo(1),"Primary should have at least one dependency.");
            Assert.That(desc.Dependencies,Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault,Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }

    [Test]
    public void SingleModuleNameDependency()
    {
        const string path = "found.pxs";
        using (MockFile(path, "name found;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    found
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));
            var theModuleName = new ModuleName("found", new Version(0,0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault, Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }

    [Test]
    public void SingleModuleDottedNameDependency()
    {
        const string path = "hay/stack.pxs";
        using (MockFile(path, "name hay::stack;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    hay::stack
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));
            var theModuleName = new ModuleName("hay.stack", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault, Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }

    [Test]
    public void DeeplyDottedModuleNameDependency()
    {
        const string path = "hay/stack/lazy/impl.pxs";
        using (MockFile(path, "name hay::stack::lazy::impl;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    hay::stack::lazy::impl
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));
            var theModuleName = new ModuleName("hay.stack.lazy.impl", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault, Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }


    [Test]
    public void CommonPrefixDottedModuleNameDependency()
    {
        const string path = "hay.stack/lazy/impl.pxs";
        using (MockFile(path, "name hay::stack::lazy::impl;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    hay::stack::lazy::impl
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));
            var theModuleName = new ModuleName("hay.stack.lazy.impl", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault, Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }

    [Test]
    public void FlatDottedModuleNameDependency()
    {
        const string path = "hay.stack.lazy.impl.pxs";
        using (MockFile(path, "name hay::stack::lazy::impl;"))
        {
            var desc = Sam.AssembleAsync(Source.FromString(@"
name finder;
references {
    hay::stack::lazy::impl
};
"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));
            var theModuleName = new ModuleName("hay.stack.lazy.impl", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(theModuleName),
                $"Primary is expected to depend on {theModuleName}.");
            var firstOrDefault = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(theModuleName));
            Assert.That(firstOrDefault, Is.Not.Null, $"Expected a target description of {theModuleName} in SAM.");
        }
    }

    [Test]
    public void DoubleModuleNameDependency()
    {
        const string pathFound = "found.pxs";
        const string pathLost = "lost.pxs";
        using (MockFile(pathFound, "name found;"))
        using (MockFile(pathLost,"name lost;"))
        {
            var desc =
                Sam.AssembleAsync(
                    Source.FromString(@"name finder;references{found,lost};"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));

            // Does the primary module depend on lost and found?
            var foundModuleName = new ModuleName("found", new Version(0, 0));
            var lostModuleName = new ModuleName("lost", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(2), "Primary should have at least two dependencies.");
            Assert.That(desc.Dependencies, Contains.Item(foundModuleName),
                $"Primary is expected to depend on {foundModuleName}.");
            Assert.That(desc.Dependencies, Contains.Item(lostModuleName),
                $"Primary is expected to depend on {lostModuleName}.");

            // Does SAM contain the found module
            var foundTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(foundModuleName));
            Assert.That(foundTarget, Is.Not.Null, $"Expected a target description of {foundModuleName} in SAM.");
// ReSharper disable PossibleNullReferenceException
            Assert.That(foundTarget.BuildMessages, Is.Empty,
                $"{foundModuleName} should not have build (error) messages");
// ReSharper restore PossibleNullReferenceException

            // does SAM contain the lost module?
            var lostTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(lostModuleName));
            Assert.That(lostTarget, Is.Not.Null, $"Expected a target description of {lostModuleName} in SAM.");
// ReSharper disable PossibleNullReferenceException
            Assert.That(lostTarget.BuildMessages, Is.Empty,
                $"{lostModuleName} should not have build (error) messages");
// ReSharper restore PossibleNullReferenceException
        }
    }

    [Test]
    public void TransitiveDependency()
    {
        const string pathFound = "found.pxs";
        const string pathLost = "lost.pxs";
        using (MockFile(pathFound, "name found;references{lost}"))
        using (MockFile(pathLost, "name lost;"))
        {
            var desc =
                Sam.AssembleAsync(
                    Source.FromString(@"name finder;references{found};"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));

            // Does the primary module depend on lost?
            var foundModuleName = new ModuleName("found", new Version(0, 0));
            var lostModuleName = new ModuleName("lost", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(foundModuleName),
                $"Primary is expected to depend on {foundModuleName}.");

            // Does SAM contain the found module?
            var foundTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(foundModuleName));
            Assert.That(foundTarget, Is.Not.Null, $"Expected a target description of {foundModuleName} in SAM.");
            Debug.Assert(foundTarget != null);

            // Does found depend on lost?
            // ReSharper disable PossibleNullReferenceException
            Assert.That(foundTarget.Dependencies, Contains.Item(lostModuleName), string.Format("{1} is expected to depend on {0}.", lostModuleName, foundModuleName));
            Assert.That(foundTarget.BuildMessages, Is.Empty,
                $"{foundModuleName} should not have build (error) messages");

            // does SAM contain the lost module?
            var lostTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(lostModuleName));
            Assert.That(lostTarget, Is.Not.Null, $"Expected a target description of {lostModuleName} in SAM.");
            Assert.That(lostTarget.BuildMessages, Is.Empty,
                $"{lostModuleName} should not have build (error) messages");
            // ReSharper restore PossibleNullReferenceException
        }
    }

    [Test]
    public void DependencyRelativeToSearchPath()
    {
        const string pathFound = "stack/found.pxs";
        const string pathLost = "stack/lost.pxs";
        using (MockFile(pathFound, @"name stack::found;references{stack::lost}"))
        using (MockFile(pathLost, @"name stack::lost;"))
        {
            var emptyCount = Sam.TargetDescriptions.Count;
            var desc =
                Sam.AssembleAsync(
                    Source.FromString(@"name finder;references{stack::found};"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));

            // Does the primary module depend on lost and found?
            var foundModuleName = new ModuleName("stack.found", new Version(0, 0));
            var lostModuleName = new ModuleName("stack.lost", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(foundModuleName),
                $"Primary is expected to depend on {foundModuleName}.");

            // Does SAM contain the found module
            var foundTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(foundModuleName));
            Assert.That(foundTarget, Is.Not.Null, $"Expected a target description of {foundModuleName} in SAM.");
            // ReSharper disable PossibleNullReferenceException
            Assert.That(foundTarget.BuildMessages, Is.Empty,
                $"{foundModuleName} should not have build (error) messages");
            // ReSharper restore PossibleNullReferenceException
            Assert.That(foundTarget.Dependencies, Contains.Item(lostModuleName),
                $"found module expected to depend on {lostModuleName}.");

            // does SAM contain the lost module?
            var lostTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(lostModuleName));
            Assert.That(lostTarget, Is.Not.Null, $"Expected a target description of {lostModuleName} in SAM.");
            // ReSharper disable PossibleNullReferenceException
            Assert.That(lostTarget.BuildMessages, Is.Empty,
                $"{lostModuleName} should not have build (error) messages");
            // ReSharper restore PossibleNullReferenceException

            Assert.That(Sam.TargetDescriptions.Count,Is.EqualTo(emptyCount+3), "There should be exactly three new modules: finder, lost and found.");
        }
    }

    [Test]
    public void FindProvided()
    {
        const string pathFound = "found.pxs";
        const string pathLost = "lost.pxs";

        // Provide the module lost to the SAM ahead of time, fully resolved
        var lostModuleName = new ModuleName("lost", new Version(0, 0));
        Sam.TargetDescriptions.Add(new ManualTargetDescription(lostModuleName, Source.FromString("name: lost;"),
            pathLost, Enumerable.Empty<ModuleName>()));

        // Have module found short-circuit when resolving lost
        using (MockFile(pathFound, "name found;references{lost}"))
        {
            var desc =
                Sam.AssembleAsync(
                    Source.FromString(@"name finder;references{found};"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));

            // Does the primary module depend on lost?
            var foundModuleName = new ModuleName("found", new Version(0, 0));
                
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(foundModuleName),
                $"Primary is expected to depend on {foundModuleName}.");

            // Does SAM contain the found module?
            var foundTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(foundModuleName));
            Assert.That(foundTarget, Is.Not.Null, $"Expected a target description of {foundModuleName} in SAM.");
            Debug.Assert(foundTarget != null);

            // Does found depend on lost?
            // ReSharper disable PossibleNullReferenceException
            Assert.That(foundTarget.Dependencies, Contains.Item(lostModuleName), string.Format("{1} is expected to depend on {0}.", lostModuleName, foundModuleName));
            Assert.That(foundTarget.BuildMessages, Is.Empty,
                $"{foundModuleName} should not have build (error) messages");

            // does SAM contain the lost module?
            var lostTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(lostModuleName));
            Assert.That(lostTarget, Is.Not.Null, $"Expected a target description of {lostModuleName} in SAM.");
            Assert.That(lostTarget.BuildMessages, Is.Empty,
                $"{lostModuleName} should not have build (error) messages");
            // ReSharper restore PossibleNullReferenceException
        }
    }

    [Test]
    public void DiamondDependency()
    {
        const string pathFound = "found.pxs";
        const string pathLost = "lost.pxs";
        const string pathBase = "base.pxs";
        using (MockFile(pathFound, "name found;references{base}"))
        using (MockFile(pathLost, "name lost;references{base}"))
        using(MockFile(pathBase,"name base;"))
        {
            var desc =
                Sam.AssembleAsync(
                    Source.FromString(@"name finder;references{lost,found};"), CancellationToken.None).Result;
            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.BuildMessages, Is.Empty, "Should not have build (error) messages");
            Assert.That(desc.Name, Is.EqualTo(new ModuleName("finder", new Version(0, 0))));

            // Does the primary module depend on lost and found?
            var foundModuleName = new ModuleName("found", new Version(0, 0));
            var lostModuleName = new ModuleName("lost", new Version(0, 0));
            var baseModuleName = new ModuleName("base", new Version(0, 0));
            Assert.That(desc.Dependencies.Count, Is.GreaterThanOrEqualTo(1), "Primary should have at least one dependency.");
            Assert.That(desc.Dependencies, Contains.Item(foundModuleName),
                $"Primary is expected to depend on {foundModuleName}.");
            Assert.That(desc.Dependencies, Contains.Item(lostModuleName),
                $"Primary is expected to depend on {lostModuleName}.");

            // Does SAM contain the found module?
            var foundTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(foundModuleName));
            Assert.That(foundTarget, Is.Not.Null, $"Expected a target description of {foundModuleName} in SAM.");
            Debug.Assert(foundTarget != null);

            // Does found depend on base?
            // ReSharper disable PossibleNullReferenceException
            Assert.That(foundTarget.Dependencies, Contains.Item(baseModuleName), string.Format("{1} is expected to depend on {0}.", baseModuleName, foundModuleName));
            Assert.That(foundTarget.BuildMessages, Is.Empty,
                $"{foundModuleName} should not have build (error) messages");

            // does SAM contain the lost module?
            var lostTarget = Sam.TargetDescriptions.FirstOrDefault(td => td.Name.Equals(lostModuleName));
            Assert.That(lostTarget, Is.Not.Null, $"Expected a target description of {lostModuleName} in SAM.");
            Assert.That(lostTarget.BuildMessages, Is.Empty,
                $"{lostModuleName} should not have build (error) messages");
            // ReSharper restore PossibleNullReferenceException

            Assert.That(lostTarget.Dependencies, Contains.Item(baseModuleName), string.Format("{1} is expected to depend on {0}.", baseModuleName, lostTarget));
            Assert.That(lostTarget.BuildMessages, Is.Empty, $"{lostTarget} should not have build (error) messages");
        }
    }
}