using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests
{
    [TestFixture,Explicit]
    public class VMTestsCILStaticallyLinked : Prx.Tests.VMTests
    {
        public VMTestsCILStaticallyLinked()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyStatic;
        }
        
    }

    [TestFixture,Explicit]
    public class VMTestsCILDynamicallyLinked : Prx.Tests.VMTests
    {
        public VMTestsCILDynamicallyLinked()
        {
            CompileToCil = true;
            StaticLinking = FunctionLinking.FullyIsolated;
        }
    }
}
