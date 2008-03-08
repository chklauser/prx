using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Prexonite.Types;
using System.Reflection;

namespace Prexonite
{
    [DebuggerStepThrough]
    public class CilFunctionContext : StackContext
    {

        public  static CilFunctionContext New(StackContext caller, PFunction originalImplementation)
        {
            if (originalImplementation == null)
                throw new ArgumentNullException("originalImplementation");
            return New(caller, originalImplementation.ImportedNamespaces);
        }

        public static CilFunctionContext New(StackContext caller, SymbolCollection importedNamespaces)
        {
            if (caller == null)
                throw new ArgumentNullException("caller"); 

            if(importedNamespaces == null)
                importedNamespaces = new SymbolCollection();

            return new CilFunctionContext(caller.ParentEngine, caller.ParentApplication, importedNamespaces);
        }

        public  static CilFunctionContext New(StackContext caller)
        {
            return New(caller, (SymbolCollection) null);
        }

        private static readonly MethodInfo _NewMethod =
            typeof(CilFunctionContext).GetMethod(
                "New", new Type[] {typeof(StackContext), typeof(PFunction)});
        internal static MethodInfo NewMethod
        {
            get
            {
                return _NewMethod;
            }
        }

        private CilFunctionContext(Engine parentEngine, Application parentApplication, SymbolCollection importedNamespaces)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine"); 
            this.parentEngine = parentEngine;
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication"); 
            this.parentApplication = parentApplication;
            if (importedNamespaces == null)
                throw new ArgumentNullException("importedNamespaces"); 
            this.importedNamespaces = importedNamespaces;
        }

        private readonly Engine parentEngine;
        private readonly Application parentApplication;
        private readonly SymbolCollection importedNamespaces;

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
