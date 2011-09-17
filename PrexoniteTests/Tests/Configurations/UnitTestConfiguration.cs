using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests.Configurations
{
    public abstract class UnitTestConfiguration
    {
        public class InMemory : UnitTestConfiguration
        {
        }

        public class FromStored : UnitTestConfiguration
        {
            public override void PrepareTestCompilation(ScriptedUnitTestContainer runner)
            {
                base.PrepareTestCompilation(runner);
                using(var buffer = new MemoryStream(512*1024))
                {
                    //we don't need to wrap reader/writer in using because 
                    // we can dispose of the buffer directly
                    var writer = new StreamWriter(buffer, Encoding.UTF8);
                    var reader = new StreamReader(buffer, Encoding.UTF8);

                    runner.Loader.Store(writer);
                    writer.Flush();
                    runner.SetUpLoader(); //throws away old engine,loader,application; creates new one
                    buffer.Seek(0, SeekOrigin.Begin);
                    runner.Loader.LoadFromReader(reader);
                }
            }
        }

        protected  UnitTestConfiguration()
        {
            Linking = FunctionLinking.FullyStatic;
            CompileToCil = false;
        }

        public FunctionLinking Linking { get; set; }
        public bool CompileToCil { get; set; }

        public virtual void SetupTestFile(ScriptedUnitTestContainer runner, IEnumerable<string> filesUnderTest)
        {
            foreach (var fut in filesUnderTest)
                runner.RequireFile(fut);
        }

        public virtual void PrepareExecution(ScriptedUnitTestContainer runner)
        {
            if (CompileToCil)
                Compiler.Compile(runner.Application, runner.Engine, Linking);
        }

        public virtual void PrepareTestCompilation(ScriptedUnitTestContainer runner)
        {
            // do nothing
        }
    }
}
