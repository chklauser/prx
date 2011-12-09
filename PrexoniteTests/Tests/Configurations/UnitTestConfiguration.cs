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
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;

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

            public override void PrepareTestCompilation(ScriptedUnitTestContainer runner)
            {
                base.PrepareTestCompilation(runner);
                using (var buffer = new MemoryStream(512*1024))
                {
                    //we don't need to wrap reader/writer in using because 
                    // we can dispose of the buffer directly
                    var writer = new StreamWriter(buffer, Encoding.UTF8);
                    var reader = new StreamReader(buffer, Encoding.UTF8);

                    runner.Loader.Store(writer);
                    writer.Flush();
                    runner.SetUpLoader();
                    //throws away old engine,loader,application; creates new one
                    buffer.Seek(0, SeekOrigin.Begin);
                    runner.Loader.LoadFromReader(reader);
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

        public static ModuleName BuiltIn = new ModuleName("prx-built-in",Engine.PrexoniteVersion);

        /// <summary>
        /// Loads dependencies into the application. Called just after <see cref="ScriptedUnitTestContainer.SetUpLoader"/> and before <see cref="ScriptedUnitTestContainer.LoadUnitTestingFramework"/>.
        /// </summary>
        /// <param name="runner">The runner under which the test is being executed.</param>
        /// <param name="dependencies">A list of dependencies for this test.</param>
        public virtual void SetupTestFile(ScriptedUnitTestContainer runner,
            IEnumerable<string> dependencies)
        {
            var originalApp = runner.Application;
            var originalLoader = runner.Loader;

            foreach (var key in originalLoader.Symbols.Keys
                .Where(k => originalLoader.Symbols[k].Module == null).ToArray())
                originalLoader.Symbols[key] = originalLoader.Symbols[key].WithModule(BuiltIn);

            foreach (var fut in dependencies)
            {
                if(ModularCompilation)
                {
                    Module module;
                    IDictionary<string, SymbolEntry> symbols;
                    if(ModuleCache.TryGetModule(fut, out module, out symbols))
                    {
                        Application.Link(runner.Loader.ParentApplication, new Application(module));
                        runner.Loader.ImportSymbols(symbols);
                        if(originalLoader != runner.Loader)
                            originalLoader.ImportSymbols(symbols);
                    }
                    else
                    {
                        //For linking the apps together, we need to preserve them
                        var prevApp = runner.Application;
                        var prevLdr = runner.Loader;

                        //Create the module, application and loader for this next dependency
                        module = Module.Create( new ModuleName(
                            runner.ApplicationName + "." + Path.GetFileNameWithoutExtension(fut), 
                            new Version()));
                        var app = runner.Application = new Application(module);
                        var ldr = runner.Loader = new Loader(runner.Engine, app);

                        //Link them together, both physically and symbolically
                        Application.Link(app,prevApp);
                        ldr.ImportSymbols(prevLdr.Symbols); //import all symbols from before

                        //Now, we're ready to load the file into the fresh module
                        runner.RequireFile(fut);

                        //Cleanup: mark symbols with correct module name
                        MarkSymbols(module, ldr);

                        //Store the module for future test cases
                        symbols = ModuleCache.Provide(fut, ldr);
                        originalLoader.ImportSymbols(symbols);
                    }
                }
                else //no modular compilation
                {
                    runner.RequireFile(fut);
                }
            }

            if(ModularCompilation)
            {
                //restore original environment
                runner.Application = originalApp;
                runner.Loader = originalLoader;
            }
        }

        private static void MarkSymbols(Module module, Loader ldr)
        {
            var ks =
                ldr.Symbols
                    .Where(kvp => kvp.Value.Module == null)
                    .Select(kvp => kvp.Key)
                    .ToList();
            foreach (var k in ks)
                ldr.Symbols[k] = ldr.Symbols[k].WithModule(module.Name);
        }

        /// <summary>
        /// Executed after prerequisites (including testing framework) are compiled, but before
        /// actual test is compiled.
        /// </summary>
        /// <param name="runner">The runner under which the test is being executed.</param>
        public virtual void PrepareTestCompilation(ScriptedUnitTestContainer runner)
        {
            // do nothing
        }

        /// <summary>
        /// Executed as the last step of loading, immediately before the actual test methods are executed.
        /// </summary>
        /// <param name="runner">The runner under which the test is being executed.</param>
        public virtual void PrepareExecution(ScriptedUnitTestContainer runner)
        {
            if (CompileToCil)
                Compiler.Compile(runner.Application, runner.Engine, Linking);
        }
    }
}