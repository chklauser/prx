using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite
{

    /// <summary>
    /// Integrates suspendable .NET managed code into the Prexonite stack via the IEnumerator interface.
    /// </summary>
    public class CoroutineContext : StackContext, IDisposable
    {

        public override string ToString()
        {
            return "Managed Coroutine";
        } 

        public CoroutineContext(StackContext sctx, IEnumerator<PValue> coroutine)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (coroutine == null)
                throw new ArgumentNullException("coroutine");

            _coroutine = coroutine;

            parentEngine = sctx.ParentEngine;
            parentApplication = sctx.ParentApplication;
            importedNamespaces = sctx.ImportedNamespaces;
        }

        public CoroutineContext(StackContext sctx, IEnumerable<PValue> coroutine)
            : this(sctx, coroutine.GetEnumerator())
        {
        }

        private IEnumerator<PValue> _coroutine;

        private Engine parentEngine;
        private Application parentApplication;
        private SymbolCollection importedNamespaces;
        private PValue returnValue;

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
            bool moved = _coroutine.MoveNext();
            if (moved)
            {
                if (_coroutine.Current != null)
                    returnValue = _coroutine.Current;
                ReturnMode = ReturnModes.Continue;
            }
            else
            {
                ReturnMode = ReturnModes.Break;
            }
            return false; //remove the context from the stack (for now)
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
            get { return returnValue ?? PType.Null.CreatePValue(); }
        }

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if(_coroutine != null)
                        _coroutine.Dispose();
                }
            }
            disposed = true;
        }

        ~CoroutineContext()
        {
            Dispose(false);
        }

        #endregion

    }
}
