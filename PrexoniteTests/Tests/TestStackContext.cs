using System;
using Prexonite;
using Prexonite.Types;

namespace Prx.Tests
{
    public class TestStackContext : StackContext
    {
        private Engine _engine;
        private PFunction _implementation;

        public TestStackContext(Engine engine, Application app)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (app == null)
                throw new ArgumentNullException("app");
            _engine = engine;
            _implementation = new PFunction(app);
        }

        public override Engine ParentEngine
        {
            get { return _engine; }
        }

        public override PFunction Implementation
        {
            get { return _implementation; }
        }

        protected override bool PerformNextCylce()
        {
            return false;
        }

        public override PValue ReturnValue
        {
            get { return PType.Null.CreatePValue(); }
        }

        public override bool HandleException(Exception exc)
        {
            return false;
        }
    }
}