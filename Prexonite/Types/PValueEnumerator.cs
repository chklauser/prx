/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Prexonite.Types
{
    /// <summary>
    /// An enumerator proxyy that returns the values instead of PValue objects of an <see cref="IEnumerable{PValue}"/>
    /// </summary>
    public class PValueEnumerator : IEnumerator<PValue>,
                                    IObject
    {
        #region Class

        private readonly IEnumerator<PValue> _baseEnumerator;

        /// <summary>
        /// Creates a new proxy for the IEnumerator of the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">An IEnumerable.</param>
        public PValueEnumerator(IEnumerable<PValue> enumerable)
            : this(enumerable.GetEnumerator())
        {
        }

        /// <summary>
        /// Creates a new prox for the supplied enumerator.
        /// </summary>
        /// <param name="baseEnumerator">An IEnumerator</param>
        public PValueEnumerator(IEnumerator<PValue> baseEnumerator)
        {
            if (baseEnumerator == null)
                throw new ArgumentNullException("baseEnumerator");
            _baseEnumerator = baseEnumerator;
        }

        #endregion

        #region IEnumerator<PValue> Members

        /// <summary>
        /// Returns the current element
        /// </summary>
        public PValue Current
        {
            get { return _baseEnumerator.Current; }
        }

        #endregion

        #region IDisposable Members

        // Dispose() calls Dispose(true)
        /// <summary>
        /// Releases all managed and unmanaged resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if(!disposing)
                return;
            // free managed resources 
            if(_baseEnumerator != null)
                _baseEnumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Returns the current PValue (as an object)
        /// </summary>
        object IEnumerator.Current
        {
            get { return _baseEnumerator.Current; }
        }

        /// <summary>
        /// Moves on to the next value.
        /// </summary>
        /// <returns>True if that next value exists; false otherwise.</returns>
        public bool MoveNext()
        {
            return _baseEnumerator.MoveNext();
        }

        /// <summary>
        /// Resets the base enumerator.
        /// </summary>
        /// <remarks>Some enumerators may not support the <see cref="IEnumerator.Reset"/> method.</remarks>
        /// <exception cref="NotSupportedException">The base enumerator does not support resetting.</exception>
        public void Reset()
        {
            _baseEnumerator.Reset();
        }

        #endregion

        #region IObject Members

        /// <summary>
        /// Dynamically calls members of <see cref="PValueEnumerator"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to call the memeber.</param>
        /// <param name="args">The array of arguments to be passed to the member call.</param>
        /// <param name="call">The call method. (ignored)</param>
        /// <param name="id">The name of the member to call.</param>
        /// <param name="result">The PValue returned by the member call.</param>
        /// <returns>True if the call was successful; false otherwise.</returns>
        /// <remarks>Since none of the instance members take any arguments, <paramref name="args"/> must have length 0.</remarks>
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Length != 0)
                return false;

            switch (id.ToLower())
            {
                case "movenext":
                    result = MoveNext();
                    break;
                case "current":
                    result = Current;
                    break;
                case "reset":
                    Reset();
                    result = PType.Null.CreatePValue();
                    break;
                case "dispose":
                    Dispose();
                    result = PType.Null.CreatePValue();
                    break;
            }

            return result != null;
        }

        #endregion
    }
}