using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PrexoniteTests.Tests
{
    [TestFixture,Explicit]
    public class VMTestsInterpreted : Prx.Tests.VMTests
    {
        public VMTestsInterpreted()
        {
            CompileToCil = false;
        }
    }
}
