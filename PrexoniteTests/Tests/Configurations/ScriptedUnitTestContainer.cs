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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests.Configurations
{
    internal abstract class ScriptedUnitTestContainer
    {
        public Application Application { get; set; }
        public Engine Engine { get; set; }
        public Loader Loader { get; set; }

        public List<string> Dependencies { get; set; }

        public StackContext Root { get; set; }

        public const string ListTestsId = @"test\list_test";
        public const string RunTestId = @"test\run_test";
        public const string PrexoniteUnitTestFramework = @"psr\test.pxs";
        public const string DumpRequestFlag = "request_dump";

        protected abstract UnitTestConfiguration Runner { get; }

        public void Initialize()
        {
            Application = new Application(ApplicationName);
            Engine = new Engine();
            Loader = new Loader(Engine, Application);

            Dependencies = new List<string>();
            Root = new NullContext(Engine, Application, new string[0]);

            var slnPath = Environment.CurrentDirectory;
            while (Directory.Exists(slnPath) && !File.Exists(Path.Combine(slnPath, "Prexonite.sln")))
                slnPath = Path.Combine(slnPath, @".." + Path.DirectorySeparatorChar);

            if (Directory.Exists(slnPath))
            {
                var psrTestsPath =
                    Path.GetFullPath(Path.Combine(slnPath, @"PrexoniteTests\psr-tests"));
                Console.WriteLine("inferred psr-tests path: " + psrTestsPath, "Engine.Path");
                Engine.Paths.Add(psrTestsPath);

                var prxPath = Path.GetFullPath(Path.Combine(slnPath, @"Prx"));
                Console.WriteLine("inferred prx path: " + prxPath, "Engine.Path");
                Engine.Paths.Add(prxPath);
            }
            else
            {
                Console.WriteLine("CANNOT INFER solution PATH: " + slnPath, "Engine.Path");
            }
        }

        public string ApplicationName
        {
            get { return GetType().Name; }
        }

        protected void RunUnitTest(string testCaseId)
        {
            Console.WriteLine("---- SNIP end of stored representation ----");

            var tc = Application.Functions[testCaseId];
            Assert.That(tc, Is.Not.Null, "Test case " + testCaseId + " not found.");

            var rt = _findRunFunction();
            Assert.That(rt, Is.Not.Null,
                        "Test case run function (part of testing framework) not found. Was looking for {0}.", RunTestId);

            var resP = rt.Run(Engine, new[] {PType.Null, Root.CreateNativePValue(tc)});
            var success = (bool) resP.DynamicCall(Root, new PValue[0], PCall.Get, "Key").Value;
            if (success)
                return;

            var eObj = resP
                .DynamicCall(Root, new PValue[0], PCall.Get, "Value")
                .DynamicCall(Root, new PValue[0], PCall.Get, "e")
                .Value;
            var e = eObj as Exception;
            if (e != null)
            {
                throw e;
            }
            else
            {
                Console.WriteLine("Test failed. Result:");
                Console.WriteLine(eObj);
                Assert.Fail("Test failed");
            }
        }

        /// <summary>
        /// Prints a stored representation of each application in the compound that has its "request_dump" flag set.
        /// </summary>
        public void PrintCompound()
        {
            var tasks =
                Application.Compound.Where(app => app.Meta[DumpRequestFlag].Switch).Select(
                    app =>
                    new KeyValuePair<ModuleName, Task<ITarget>>(app.Module.Name, ModuleCache.BuildAsync(app.Module.Name)))
                    .ToDictionary(k => k.Key, k => k.Value);
            foreach (var entry in tasks)
            {
                var name = entry.Key;
                var target = entry.Value.Result;

                Console.WriteLine();
                Console.WriteLine("##################################  begin of stored representation for {0} ",name);

                var opt = new LoaderOptions(Engine, new Application(target.Module), target.Symbols)
                    {ReconstructSymbols = false, RegisterCommands = false, StoreSymbols = true};
                var ldr = new Loader(opt);
                ldr.Store(Console.Out);

                Console.WriteLine("##################################    end of stored representation for {0} ----------", name);
            }
        }

        private PFunction _findRunFunction()
        {
            return Application.Compound.Select(app =>
                {
                    PFunction func;
                    return app.Functions.TryGetValue(RunTestId, out func) ? func : null;
                }).SingleOrDefault(f => f != null);
        }
    }
}