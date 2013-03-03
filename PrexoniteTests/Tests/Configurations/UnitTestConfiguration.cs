// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Internal
{
}

namespace PrexoniteTests.Tests.Configurations
{
    internal abstract class UnitTestConfiguration
    {
        public class InMemory : UnitTestConfiguration
        {
        }

        public class FromStored : UnitTestConfiguration
        {
            public FromStored()
            {
                throw new NotSupportedException("Store round-tripping is not currently implemented.");
            }

            internal override void Configure(TestModel model, ScriptedUnitTestContainer container)
            {
                // Rewire the units under test to point to stored representations
                var originalUnits = model.UnitsUnderTest.ToList();
                var storedNameMap = originalUnits.Select(
                    td =>
                        {
                            var ext = Path.GetExtension(td.ScriptName);
                            var extLen = ext == null ? 0 : ext.Length;
                            var baseName = td.ScriptName.Substring(td.ScriptName.Length - extLen);
                            return string.Format("{0}~-stored{1}", baseName, ext);
                        });


                // Configure the test as per usual
                base.Configure(model, container);
            }

            public void PrepareTestCompilation(ScriptedUnitTestContainer container)
            {
                using (var buffer = new MemoryStream(512*1024))
                {
                    //we don't need to wrap reader/writer in using because 
                    // we can dispose of the buffer directly
                    var writer = new StreamWriter(buffer, Encoding.UTF8);
                    var reader = new StreamReader(buffer, Encoding.UTF8);

                    container.Loader.Store(writer);
                    writer.Flush();
                    container.Initialize();
                    //throws away old engine,loader,application; creates new one
                    buffer.Seek(0, SeekOrigin.Begin);
                    container.Loader.LoadFromReader(reader);
                }
            }
        }

        protected UnitTestConfiguration()
        {
            Linking = FunctionLinking.FullyStatic;
            CompileToCil = false;
        }

        public FunctionLinking Linking { get; set; }
        public bool CompileToCil { get; set; }

        /// <summary>
        /// Executed as the last step of loading, immediately before the actual test methods are executed.
        /// </summary>
        /// <param name="runner">The container under which the test is being executed.</param>
        public virtual void PrepareExecution(ScriptedUnitTestContainer runner)
        {
            if (CompileToCil)
                Compiler.Compile(runner.Application, runner.Engine, Linking);
        }

        public virtual void LoadUnitTestingFramework(ScriptedUnitTestContainer container)
        {
            ModuleCache.Describe(container.Loader,new TestDependency
                {
                    ScriptName = ScriptedUnitTestContainer.PrexoniteUnitTestFramework
                });
        }

// ReSharper disable InconsistentNaming
        internal virtual void Configure(TestModel model, ScriptedUnitTestContainer container)
// ReSharper restore InconsistentNaming
        {
            // describe units under test
            foreach (var unit in model.UnitsUnderTest)
                ModuleCache.Describe(container.Loader, unit);

            // describe unit testing framework
            LoadUnitTestingFramework(container);

            // describe unit testing extensions
            foreach(var extension in model.TestDependencies)
                ModuleCache.Describe(container.Loader, extension);

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
            ModuleCache.Describe(container.Loader, suiteDescription);

            // Finally instantiate the test suite application(s)
            var result = ModuleCache.Load(model.TestSuiteScript);
            container.Application = result.Item1;
            container.PrintCompound();

            ITarget target = result.Item2;
            if (!target.IsSuccessful)
            {
                Console.WriteLine("The target {0} failed to build.", target.Name);

                if(target.Exception != null)
                    Console.WriteLine(target.Exception);

                foreach (var error in target.Messages.Where(m => m.Severity == MessageSeverity.Error))
                    Console.WriteLine("Error: {0}", error);
                foreach (var warning in target.Messages.Where(m => m.Severity == MessageSeverity.Warning))
                    Console.WriteLine("Warning: {0}", warning);
                foreach (var info in target.Messages.Where(m => m.Severity == MessageSeverity.Info))
                    Console.WriteLine("Info: {0}", info);

                Assert.Fail("The target {0} failed to build.", target.Name);
            }

            PrepareExecution(container);
        }
    }
}