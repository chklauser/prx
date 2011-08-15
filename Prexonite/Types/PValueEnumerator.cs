using System;
using System.Collections;
using System.Collections.Generic;

namespace Prexonite.Types
{
    public abstract class PValueEnumerator : IEnumerator<PValue>, IObject
    {
        /// <summary>
        /// Returns the current element
        /// </summary>
        public abstract PValue Current { get; }

        /// <summary>
        /// Returns the current PValue (as an object)
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Releases all managed and unmanaged resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Moves on to the next value.
        /// </summary>
        /// <returns>True if that next value exists; false otherwise.</returns>
        public abstract bool MoveNext();

        /// <summary>
        /// Resets the base enumerator.
        /// </summary>
        /// <remarks>Some enumerators may not support the <see cref="IEnumerator.Reset"/> method.</remarks>
        /// <exception cref="NotSupportedException">The base enumerator does not support resetting.</exception>
        public virtual void Reset()
        {
            throw new NotSupportedException(GetType().Name + " does not support System.Collections.IEnumerator.Reset()");
        }

        /// <summary>
        /// Dynamically calls members of <see cref="PValueEnumeratorWrapper"/>.
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
    }
}