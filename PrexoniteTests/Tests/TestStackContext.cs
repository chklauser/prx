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

        public PFunction Implementation
        {
            get { return _implementation; }
        }

        public override PValue ReturnValue
        {
            get { return PType.Null.CreatePValue(); }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get
            {
                return _implementation.ParentApplication;
            }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get
            {
                return _implementation.ImportedNamespaces;
            }
        }

        public override bool TryHandleException(Exception exc)
        {
            return false;
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCylce(StackContext lastContext)
        {
            return false;
        }
    }
}