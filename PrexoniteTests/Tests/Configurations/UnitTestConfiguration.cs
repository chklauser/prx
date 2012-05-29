// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Symbol = Prexonite.Compiler.Symbolic.Symbol;

namespace PrexoniteTests.Tests.Configurations
{
    public abstract class UnitTestConfiguration
    {
        public class InMemory : UnitTestConfiguration
        {
        }

        public class FromStored : UnitTestConfiguration
        {
            public FromStored()
            {
                ModularCompilation = false;
            }

            public override void PrepareTestCompilation(ScriptedUnitTestContainer container)
            {
                base.PrepareTestCompilation(container);
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
            ModularCompilation = true;
        }

        public FunctionLinking Linking { get; set; }
        public bool CompileToCil { get; set; }
        public bool ModularCompilation { get; set; }

        public virtual void SetupDependencies(ScriptedUnitTestContainer runner,
            IEnumerable<string> dependencies)
        {
            SetupUnitsUnderTest(runner, dependencies);
        }

        /// <summary>
        /// Loads dependencies into the application. Called just after <see cref="ScriptedUnitTestContainer.Initialize"/> and before <see cref="ScriptedUnitTestContainer.LoadUnitTestingFramework"/>.
        /// </summary>
        /// <param name="container">The container under which the test is being executed.</param>
        /// <param name="dependencies">A list of dependencies for this test.</param>
        public virtual void SetupUnitsUnderTest(ScriptedUnitTestContainer container,
            IEnumerable<string> dependencies)
        {
            var originalApp = container.Application;

            foreach (var fut in dependencies)
            {
                if(ModularCompilation)
                {
                    // TODO use build system to implement inter-module dependencies
                    Module module;
                    IEnumerable<SymbolInfo> symbols;
                    if(ModuleCache.TryGetModule(fut, out module, out symbols))
                    {
                        Application.Link(container.Loader.ParentApplication, new Application(module));
                    }
                    else
                    {
                        //For linking the apps together, we need to preserve them
                        var prevApp = container.Application;

                        //Create the module, application and loader for this next dependency
                        module = Module.Create( new ModuleName(
                            container.ApplicationName + "." + Path.GetFileNameWithoutExtension(fut), 
                            new Version()));
                        var app = container.Application = new Application(module);

                        var ldrStore = SymbolStore.Create(conflictUnionSource: container.ImportedSymbols.SelectMany(x => x));
                        var ldrOptions = new LoaderOptions(container.Engine, app, ldrStore);
                        var ldr = container.Loader = new Loader(ldrOptions);

                        //Link them together, both physically and symbolically
                        Application.Link(app,prevApp);

                        //Now, we're ready to load the file into the fresh module
                        container.RequireFile(fut);

                        //Store the module for future test cases
                        symbols = ModuleCache.Provide(fut, ldr);
                    }

                    container.ImportedSymbols.Add(symbols);
                }
                else //no modular compilation
                {
                    container.RequireFile(fut);
                }
            }

            if(ModularCompilation)
            {
                //restore original environment
                container.Application = originalApp;
            }
        }

        /// <summary>
        /// Executed after prerequisites (including testing framework) are compiled, but before
        /// actual test is compiled.
        /// </summary>
        /// <param name="container">The container under which the test is being executed.</param>
        public virtual void PrepareTestCompilation(ScriptedUnitTestContainer container)
        {
        }

        /// <summary>
        /// Executed as the last step of loading, immediately before the actual test methods are executed.
        /// </summary>
        /// <param name="runner">The container under which the test is being executed.</param>
        public virtual void PrepareExecution(ScriptedUnitTestContainer runner)
        {
            if (CompileToCil)
                Compiler.Compile(runner.Application, runner.Engine, Linking);
        }
    }
}