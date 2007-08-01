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

        private IEnumerator<PValue> _baseEnumerator;

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

        /// <summary>
        /// Disposes the base enumerator
        /// </summary>
        public void Dispose()
        {
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
        /// <param name="id">The name of the member to call.</param>
        /// <param name="result">The PValue returned by the member call.</param>
        /// <returns>True if the call was successful; false otherwise.</returns>
        /// <remarks>Since none of the instance members take any arguments, <paramref name="args"/> must have length 0.</remarks>
        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;
            if (args == null)
                throw new ArgumentNullException("args"); 
            if (args.Length != 0)
                return false;

            switch(id.ToLower())
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