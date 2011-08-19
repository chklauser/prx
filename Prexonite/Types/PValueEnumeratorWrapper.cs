#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Prexonite.Types
{
    /// <summary>
    /// An enumerator proxy that returns the values instead of PValue objects of an <see cref="IEnumerable{PValue}"/>
    /// </summary>
    public sealed class PValueEnumeratorWrapper : PValueEnumerator
    {
        #region Class

        private readonly IEnumerator<PValue> _baseEnumerator;

        /// <summary>
        /// Creates a new proxy for the IEnumerator of the supplied <paramref name="enumerable"/>.
        /// </summary>
        /// <param name="enumerable">An IEnumerable.</param>
        public PValueEnumeratorWrapper(IEnumerable<PValue> enumerable)
            : this(enumerable.GetEnumerator())
        {
        }

        /// <summary>
        /// Creates a new prox for the supplied enumerator.
        /// </summary>
        /// <param name="baseEnumerator">An IEnumerator</param>
        public PValueEnumeratorWrapper(IEnumerator<PValue> baseEnumerator)
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
        public override PValue Current
        {
            get { return _baseEnumerator.Current; }
        }

        #endregion

        #region IDisposable Members

        // Dispose() calls Dispose(true)

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            // free managed resources 
            if (_baseEnumerator != null)
                _baseEnumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Moves on to the next value.
        /// </summary>
        /// <returns>True if that next value exists; false otherwise.</returns>
        public override bool MoveNext()
        {
            return _baseEnumerator.MoveNext();
        }

        /// <summary>
        /// Resets the base enumerator.
        /// </summary>
        /// <remarks>Some enumerators may not support the <see cref="IEnumerator.Reset"/> method.</remarks>
        /// <exception cref="NotSupportedException">The base enumerator does not support resetting.</exception>
        public override void Reset()
        {
            _baseEnumerator.Reset();
        }

        #endregion

        #region IObject Members

        #endregion
    }
}