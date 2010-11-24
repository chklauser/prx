using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class VMTestsCIL : Prx.Tests.VMTests
    {
        public VMTestsCIL()
        {
            CompileToCil = true;
        }
        
    }
}
