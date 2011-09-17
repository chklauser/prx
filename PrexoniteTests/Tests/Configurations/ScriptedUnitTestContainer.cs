using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Prexonite;
using Prexonite.Compiler;

using NUnit.Framework;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace PrexoniteTests.Tests.Configurations
{
    public abstract class ScriptedUnitTestContainer
    {
        public Application Application { get; set; }
        public Engine Engine { get; set; }
        public Loader Loader { get; set; }
        public StackContext Root { get; set; }

        public const string ListTestsId = @"test\list_test";
        public const string RunTestId = @"test\run_test";
        public const string PrexoniteUnitTestFramework = @"psr\test.pxs";

        protected abstract UnitTestConfiguration Runner { get; }

        public void SetUpLoader()
        {
            Application = new Application(this.GetType().Name);
            Engine = new Engine();
            Loader = new Loader(Engine, Application);
            Root = new NullContext(Engine, Application, new string[0]);

            var slnPath = Environment.CurrentDirectory;
            while (Directory.Exists(slnPath) && !File.Exists(Path.Combine(slnPath, "Prexonite.sln")))
                slnPath = Path.Combine(slnPath,@"..\");

            if (Directory.Exists(slnPath))
            {
                var psrTestsPath = Path.GetFullPath(Path.Combine(slnPath, @"PrexoniteTests\psr-tests"));
                Console.WriteLine("inferred psr-tests path: " + psrTestsPath, "Engine.Path");
                Engine.Paths.Add(psrTestsPath);
                var prxPath = Path.GetFullPath(Path.Combine(slnPath, @"Prx"));
                Console.WriteLine("inferred prx path: " + prxPath,"Engine.Path");
                Engine.Paths.Add(prxPath);
            }
            else
            {
                Console.WriteLine("CANNOT INFER psr-tests PATH: " + slnPath, "Engine.Path");
            }
        }

        protected void LoadUnitTestingFramework()
        {
            RequireFile(PrexoniteUnitTestFramework);
        }

        public void RequireFile(string path)
        {
            var fileInfo = Loader.ApplyLoadPaths(path);
            if (fileInfo == null)
                throw new FileNotFoundException("Cannot find required script file.", path);
            if (!Loader.LoadedFiles.Contains(fileInfo.FullName))
                Loader.LoadFromFile(fileInfo.FullName);
        }

        protected void RunUnitTest(string testCaseId)
        {
            Console.WriteLine("----  begin of stored representation   ----");
            Console.WriteLine(Loader.StoreInString());
            Console.WriteLine("---- SNIP end of stored representation ----");

            foreach (var error in Loader.Errors)
                Console.WriteLine("Error: {0}", error);
            foreach (var warning in Loader.Warnings)
                Console.WriteLine("Warning: {0}", warning);
            foreach (var info in Loader.Infos)
                Console.WriteLine("Info: {0}", info);


            Assert.That(Loader.ErrorCount,Is.EqualTo(0),"Errors during compilation");

            var tc = Application.Functions[testCaseId];
            Assert.That(tc,Is.Not.Null,"Test case " + testCaseId + " not found.");

            var rt = Application.Functions[RunTestId];
            Assert.That(rt,Is.Not.Null);

            var resP = rt.Run(Engine, new[] {PType.Null, Root.CreateNativePValue(tc)});
            var success = (bool)resP.DynamicCall(Root,new PValue[0], PCall.Get,"Key").Value;
            if(success)
                return;

            var eObj = resP
                .DynamicCall(Root, new PValue[0], PCall.Get, "Value")
                .DynamicCall(Root, new PValue[0], PCall.Get, "e")
                .Value;
            var e = eObj as Exception;
            if(e != null)
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
    }
}
