using System;
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    /// Integrates suspendable .NET managed code into the Prexonite stack via the IEnumerator interface.
    /// </summary>
    public class CooperativeContext : StackContext, IDisposable
    {

        public override string ToString()
        {
            return String.Format("Cooperative managed method({0})", _existingMethod);
        } 

        public CooperativeContext(StackContext sctx, Func<Action<PValue>,IEnumerable<bool>> methodCtor)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (methodCtor == null)
                throw new ArgumentNullException("methodCtor");

            _methodCtor = methodCtor;
            _parentEngine = sctx.ParentEngine;
            _parentApplication = sctx.ParentApplication;
            _importedNamespaces = sctx.ImportedNamespaces;
        }

        private IEnumerator<bool> _method
        {
            [DebuggerStepThrough]
            get
            {
                if (_existingMethod != null)
                {
                    return _existingMethod;

                }
                else
                {
                    _existingMethod = _methodCtor(v => _returnValue = v).GetEnumerator();
                    _methodCtor = null;
                    return _existingMethod;
                }
            }
        }

        private Func<Action<PValue>, IEnumerable<bool>> _methodCtor;
        private IEnumerator<bool> _existingMethod;

        private readonly Engine _parentEngine;
        private readonly Application _parentApplication;
        private readonly SymbolCollection _importedNamespaces;
        private PValue _returnValue;

        /// <summary>
        /// Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine
        {
            get { return _parentEngine; }
        }

        /// <summary>
        /// The parent application.
        /// </summary>
        public override Application ParentApplication
        {
            get { return _parentApplication; }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get { return _importedNamespaces; }
        }

        /// <summary>
        /// Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCylce(StackContext lastContext)
        {
            return _method.MoveNext() && _method.Current;
        }

        /// <summary>
        /// Tries to handle the supplied exception.
        /// </summary>
        /// <param name="exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public override bool TryHandleException(Exception exc)
        {
            if (ExceptionHandler != null)
                return ExceptionHandler(exc);
            else
                return false;
        }

        public Func<Exception,bool> ExceptionHandler { get; set; }

        /// <summary>
        /// Represents the return value of the context.
        /// Just providing a value here does not mean that it gets consumed by the caller.
        /// If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public override PValue ReturnValue
        {
            get { return _returnValue ?? PType.Null.CreatePValue(); }
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _dispose(true);
        }

        private void _dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if(_existingMethod != null)
                        _existingMethod.Dispose();
                }
            }
            _disposed = true;
        }

        ~CooperativeContext()
        {
            _dispose(false);
        }

        #endregion

    }
}