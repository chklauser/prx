// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    ///     Makes a function (or any stack context) behave like a coroutine.
    /// </summary>
    /// <seealso cref = "PFunction" />
    /// <seealso cref = "FunctionContext" />
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Coroutine")]
    public class Coroutine : IEnumerable<PValue>,
                             IObject,
                             IIndirectCall
    {
        #region Class

        private StackContext _corctx;

        /// <summary>
        ///     Creates a new coroutine wrapper around the supplied stack context.
        /// </summary>
        /// <param name = "corctx">The stack context to be treated like a coroutine.</param>
        /// <exception cref = "ArgumentNullException"><paramref name = "corctx" /> is null.</exception>
        public Coroutine(StackContext corctx)
        {
            if (corctx == null)
                throw new ArgumentNullException("corctx");
            _corctx = corctx;
        }

        /// <summary>
        ///     Returns a description of the underlying function context.
        /// </summary>
        /// <returns>A description of the underlying function context.</returns>
        public override string ToString()
        {
            return "coroutine " + _corctx;
        }

        #endregion

        #region IEnumerable<PValue> Members

        ///<summary>
        ///    Returns an enumerator that iterates through the collection.
        ///</summary>
        ///<returns>
        ///    A <see cref = "T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public IEnumerator<PValue> GetEnumerator()
        {
            return new PValueEnumeratorWrapper(_internalEnumerator());
        }

        private IEnumerator<PValue> _internalEnumerator()
        {
            while (IsValid)
            {
                var result = Execute(_corctx.ParentEngine);
                if (!IsValid)
                    break;
                yield return result;
            }
        }

        #endregion

        #region IEnumerable Members

        ///<summary>
        ///    Returns an enumerator that returns all results of the coroutine one by one.
        ///</summary>
        ///<returns>
        ///    An <see cref = "T:System.Collections.IEnumerator"></see> object that can be used to iterate through the results of the coroutine.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IObject Members

        /// <summary>
        ///     Tries to dispatch a dynamic (instance) call to a <see cref = "Coroutine" /> member.
        /// </summary>
        /// <param name = "sctx">The stack context in which to perform the call.</param>
        /// <param name = "args">The arguments to be passed to the call.</param>
        /// <param name = "call">Indicates whether the call is a get or a set call.</param>
        /// <param name = "id">The id of the memeber to be called.</param>
        /// <param name = "result">The result returned by the member call.</param>
        /// <returns>True if a member has been called; false otherwise.</returns>
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            switch (id.ToLower())
            {
                case "indirectcall":
                case "execute":
                case "run":
                    result = Execute(sctx);
                    break;
                case "isvalid":
                    result = IsValid;
                    break;
                case "getenumerator":
                    result = PType.Object.CreatePValue(GetEnumerator());
                    break;
                case "all":
                    result = PType.List.CreatePValue(All());
                    break;
                default:
                    result = null;
                    break;
            }
            return result != null;
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        ///     Hands the control over to the coroutine.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute the coroutine.</param>
        /// <param name = "args"><strong>Ignored</strong>. The current version of Prexonite does 
        ///     not allow you to pass additional arguments to the coroutine once it is running.</param>
        /// <returns>The PValue generated by the coroutine</returns>
        /// <remarks>
        ///     This method returns <see cref = "PType.Null" /> PValue if the coroutine is 
        ///     no longer valid (<see cref = "IsValid" />).
        /// </remarks>
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return Execute(sctx);
        }

        #endregion

        #region Execution

        /// <summary>
        ///     Indicates whether the coroutine reference is still valid.
        /// </summary>
        /// <remarks>
        ///     A coroutine becomes invalid once the end of the 
        ///     underlying routine or a <strong>break</strong> or <strong>return</strong> statement has been reached.
        /// </remarks>
        public bool IsValid
        {
            get { return _corctx != null; }
        }

        /// <summary>
        ///     Hands the control over to the coroutine.
        /// </summary>
        /// <param name = "sctx">The stack context in which to execute the coroutine.</param>
        /// <returns>The PValue generated by the coroutine</returns>
        /// <remarks>
        ///     This method returns <see cref = "PType.Null" /> PValue if the coroutine is no longer valid (<see cref = "IsValid" />).
        /// </remarks>
        public PValue Execute(StackContext sctx)
        {
            return Execute(sctx.ParentEngine);
        }

        /// <summary>
        ///     Hands the control over to the coroutine.
        /// </summary>
        /// <param name = "eng">The engine in which to execute the coroutine.</param>
        /// <returns>The PValue generated by the coroutine</returns>
        /// <remarks>
        ///     This method returns <see cref = "PType.Null" /> PValue if the coroutine is no longer valid (<see cref = "IsValid" />).
        /// </remarks>
        public PValue Execute(Engine eng)
        {
            if (!IsValid)
                return PType.Null.CreatePValue();

            var ret = eng.Process(_corctx);
            if (_corctx.ReturnMode != ReturnMode.Continue)
            {
                _corctx = null;
                return PType.Null.CreatePValue();
            }
            else
            {
                return ret;
            }
        }

        #endregion

        /// <summary>
        ///     The meta key used to mark designated coroutine functions.
        /// </summary>
        public const string IsCoroutineKey = @"_\iscoroutine";

        /// <summary>
        ///     Returns a list with all results returned by the coroutine.
        /// </summary>
        /// <returns>Do not use with infinite lists!</returns>
        public List<PValue> All()
        {
            return new List<PValue>(this);
        }
    }
}