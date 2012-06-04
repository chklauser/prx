using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrexoniteTests.Tests.Configurations
{
    class TestModel
    {
        public string TestSuiteScript { get; set; }
        public TestDependency[] UnitsUnderTest { get; set; }
        public TestDependency[] TestDependencies { get; set; }
    }

    public class TestDependency
    {
        public string ScriptName { get; set; }
        public string[] Dependencies { get; set; }
    }
}
