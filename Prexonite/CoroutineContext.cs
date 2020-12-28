// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    ///     Integrates suspendable .NET managed code into the Prexonite stack via the IEnumerator interface.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Coroutine")]
    public class CoroutineContext : StackContext, IDisposable
    {
        public override string ToString()
        {
            return $"Managed Coroutine({_coroutine})";
        }

        public CoroutineContext(StackContext sctx, IEnumerator<PValue> coroutine)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));

            _coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));

            ParentEngine = sctx.ParentEngine;
            ParentApplication = sctx.ParentApplication;
            ImportedNamespaces = sctx.ImportedNamespaces;
        }

        public CoroutineContext(StackContext sctx, IEnumerable<PValue> coroutine)
            : this(sctx, coroutine.GetEnumerator())
        {
        }

        private readonly IEnumerator<PValue> _coroutine;

        private PValue _returnValue;

        /// <summary>
        ///     Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine { get; }

        /// <summary>
        ///     The parent application.
        /// </summary>
        public override Application ParentApplication { get; }

        public override SymbolCollection ImportedNamespaces { get; }

        /// <summary>
        ///     Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCycle(StackContext lastContext)
        {
            var moved = _coroutine.MoveNext();
            if (moved)
            {
                if (_coroutine.Current != null)
                    _returnValue = _coroutine.Current;
                ReturnMode = ReturnMode.Continue;
            }
            else
            {
                ReturnMode = ReturnMode.Break;
            }
            return false; //remove the context from the stack (for now)
        }

        /// <summary>
        ///     Tries to handle the supplied exception.
        /// </summary>
        /// <param name = "exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public override bool TryHandleException(Exception exc)
        {
            return false;
        }

        /// <summary>
        ///     Represents the return value of the context.
        ///     Just providing a value here does not mean that it gets consumed by the caller.
        ///     If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public override PValue ReturnValue => _returnValue ?? PType.Null.CreatePValue();

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _coroutine?.Dispose();
                }
            }
            _disposed = true;
        }

        ~CoroutineContext()
        {
            Dispose(false);
        }

        #endregion
    }
}