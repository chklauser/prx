using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class VMTestsCILStaticallyLinked : Prx.Tests.VMTests
    {
        public VMTestsCILStaticallyLinked()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
        
    }

    [TestFixture]
    public class VMTestsCILDynamicallyLinked : Prx.Tests.VMTests
    {
        public VMTestsCILDynamicallyLinked()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }
}
