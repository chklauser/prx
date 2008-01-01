using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite
{
    public class NullContext : StackContext
    {

        public NullContext(StackContext parentCtx)
            : this(parentCtx.ParentEngine, parentCtx.ParentApplication,parentCtx.ImportedNamespaces)
        {
        }

        public NullContext(Engine parentEngine, Application parentApplication, ICollection<string> importedNamespaces)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine");
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication");
            if (importedNamespaces == null)
                throw new ArgumentNullException("importedNamespaces");

            this.parentEngine = parentEngine;
            this.parentApplication = parentApplication;
            this.importedNamespaces = (importedNamespaces as SymbolCollection) ?? new SymbolCollection(importedNamespaces);
        }

        private Engine parentEngine;
        private Application parentApplication;
        private SymbolCollection importedNamespaces;

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine
        {
            get { return parentEngine; }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get { return parentApplication; }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get { return importedNamespaces; }
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCylce(StackContext lastContext)
        {
            return false;
        }

        /// <summary>
        /// Tries to handle the supplied exception.
        /// </summary>
        /// <param name="exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public override bool TryHandleException(Exception exc)
        {
            return false;
        }

        /// <summary>
        /// Represents the return value of the context.
        /// Just providing a value here does not mean that it gets consumed by the caller.
        /// If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public override PValue ReturnValue
        {
            get { return PType.Null; }
        }
    }
}