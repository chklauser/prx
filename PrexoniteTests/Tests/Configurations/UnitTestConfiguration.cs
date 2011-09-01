using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests.Configurations
{
    public abstract class UnitTestConfiguration
    {
        public abstract void SetupTestFile(ScriptedUnitTestContainer runner,  IEnumerable<string> filesUnderTest);
        public abstract void PrepareExecution(ScriptedUnitTestContainer runner);

        public class InMemory : UnitTestConfiguration
        {
            public FunctionLinking Linking { get; set; }
            public bool CompileToCil { get; set; }

            public InMemory()
            {
                Linking = FunctionLinking.FullyStatic;
                CompileToCil = false;
            }

            #region Overrides of ConfigurationRunner

            public override void SetupTestFile(ScriptedUnitTestContainer runner, IEnumerable<string> filesUnderTest)
            {
                foreach (var fut in filesUnderTest)
                    runner.RequireFile(fut);
            }

            public override void PrepareExecution(ScriptedUnitTestContainer runner)
            {
                if (CompileToCil)
                    Compiler.Compile(runner.Application, runner.Engine, Linking);
            }

            #endregion
        }
    }
}
