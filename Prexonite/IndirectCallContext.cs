using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite
{
    public class IndirectCallContext : StackContext
    {

        public IndirectCallContext(StackContext parent, IIndirectCall callable, PValue[] args)
            : this(parent.ParentEngine,parent.ParentApplication,parent.ImportedNamespaces,callable,args)
        {
        }

        public IndirectCallContext(Engine parentEngine, Application parentApplication, ICollection<string> importedNamespaces, IIndirectCall callable, PValue[] args)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine");
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication");
            if (importedNamespaces == null)
                throw new ArgumentNullException("importedNamespaces");
            if (callable == null)
                throw new ArgumentNullException("callable");
            if (args == null)
                throw new ArgumentNullException("args"); 

            _engine = parentEngine;
            _application = parentApplication;
            _importedNamespaces = (importedNamespaces as SymbolCollection) ?? new SymbolCollection(importedNamespaces);
            _callable = callable;
            _arguments = args;
        }

        public IIndirectCall Callable
        {
            get { return _callable; }
        }
        private readonly  IIndirectCall _callable;

        public PValue[] Arguments
        {
            get { return _arguments; }
        }
        private readonly PValue[] _arguments;

        private readonly Engine _engine;
        private readonly Application _application;
        private readonly SymbolCollection _importedNamespaces;
        private PValue _returnValue = PType.Null;

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine
        {
            get { return _engine; }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get
            {
                return _application;
            }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get
            {
                return _importedNamespaces;
            }
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCylce(StackContext lastContext)
        {
            LinkedList<StackContext> stack = _engine.Stack;

            StackContext sctx = this;

            //Remove this context if possible (IndirectCallContext should be transparent)
            if(stack.Count > 1)
            {
                stack.RemoveLast();
                sctx = stack.Last.Value;
            }

            _returnValue = _callable.IndirectCall(sctx, _arguments);
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
            get
            {
                return _returnValue ?? PType.Null;
            }
        }
    }
}
