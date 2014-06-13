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
#region

using System;
using System.Collections.Generic;

#endregion

namespace Prexonite.Types
{
    /// <summary>
    ///     Pair of PValues
    /// </summary>
    //[System.Diagnostics.DebuggerNonUserCode()]
    public class PValueKeyValuePair : IObject
    {
        private static readonly ObjectPType _objectType =
            new ObjectPType(typeof (PValueKeyValuePair));

        /// <summary>
        ///     A static reference to the object type of this class.
        /// </summary>
        public static ObjectPType ObjectType
        {
            get { return _objectType; }
        }

        private readonly PValue _key;

        /// <summary>
        ///     Provides access to the value stored as the "Key".
        /// </summary>
        public PValue Key
        {
            get { return _key; }
        }

        private readonly PValue _value;

        /// <summary>
        ///     Provides access to the value stored as the "Value".
        /// </summary>
        public PValue Value
        {
            get { return _value; }
        }

        /// <summary>
        ///     Creates a new PValueKeyValuePair.
        /// </summary>
        /// <param name = "key">The key.</param>
        /// <param name = "value">The value.</param>
        public PValueKeyValuePair(PValue key, PValue value)
        {
            _key = key ?? PType.Null.CreatePValue();
            _value = value ?? PType.Null.CreatePValue();
        }

        /// <summary>
        ///     Creates a new PValueKeyValuePair.
        /// </summary>
        /// <param name = "pair">The key-value pair.</param>
        public PValueKeyValuePair(KeyValuePair<PValue, PValue> pair)
            : this(pair.Key, pair.Value)
        {
        }

        #region IObject Members

        /// <summary>
        ///     Tries to handle prexonite object member calls.
        /// </summary>
        /// <param name = "sctx">The stack context of the call.</param>
        /// <param name = "args">The arguments for the call.</param>
        /// <param name = "call">Indicates the mode of call.</param>
        /// <param name = "id">The id of the member to call (empty for the default member).</param>
        /// <param name = "result">The result returned by the call.</param>
        /// <returns>True, if the call succeeded, false otherwise.</returns>
        /// <remarks>
        ///     <paramref name = "result" /> is not defined if the function returns false.
        /// </remarks>
        /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            result = null;

            PValue arg0;

            switch (id.ToLowerInvariant())
            {
                case "":
                    if (args.Length == 1)
                    {
                        if (args[0].TryConvertTo(sctx, PType.Int, out arg0))
                        {
                            var i = (int) arg0.Value;
                            if (i == 0)
                                result = _key;
                            else
                                result = i == 1 ? _value : PType.Null.CreatePValue();
                        }
                    }
                    break;
                case "key":
                    result = _key;
                    break;
                case "value":
                    result = _value;
                    break;
                case "equals":
                    if (args.Length == 1)
                    {
                        if (args[0].TryConvertTo(sctx, _objectType, out arg0))
                        {
                            var pair = (PValueKeyValuePair) arg0.Value;
                            result = _key.Equals(pair._key) && _value.Equals(pair._value);
                        }
                    }
                    break;
                case "tostring":
                    result = String.Concat(_key.CallToString(sctx), ": ", _value.CallToString(sctx));
                    break;
            }

            return result != null;
        }

        #endregion

        /// <summary>
        ///     Checks if this instance is equal to the supplied object.
        /// </summary>
        /// <param name = "obj">The object to check for equality.</param>
        /// <returns>True if the two object are equal, False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            PValue okey;
            PValue ovalue;

            if (obj is PValueKeyValuePair)
            {
                var pvkvp = (PValueKeyValuePair) obj;
                okey = pvkvp.Key;
                ovalue = pvkvp.Value;
            }
            else if (obj is KeyValuePair<PValue, PValue>)
            {
                var kvp = (KeyValuePair<PValue, PValue>) obj;
                okey = kvp.Key;
                ovalue = kvp.Value;
            }
            else
                return false;

            return _key.Equals(okey) && _value.Equals(ovalue);
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode() ^ _value.GetHashCode();
        }

        public override string ToString()
        {
            return String.Concat(_key.ToString(), ": ", _value.ToString());
        }

        /// <summary>
        ///     Implicitly converts a PValueKeyValuePair to an ordinary KeyValuePair.
        /// </summary>
        /// <param name = "pvkvp">The key-value pair.</param>
        /// <returns>An ordinary key-value pair</returns>
        public static implicit operator KeyValuePair<PValue, PValue>(PValueKeyValuePair pvkvp)
        {
            return new KeyValuePair<PValue, PValue>(pvkvp._key, pvkvp._value);
        }

        /// <summary>
        ///     Implicitly converts an ordinary key-value pair to a PValueKeyValuePair.
        /// </summary>
        /// <param name = "kvp">The key-value pair.</param>
        /// <returns>A PValueKeyValuePair.</returns>
        public static implicit operator PValueKeyValuePair(KeyValuePair<PValue, PValue> kvp)
        {
            return new PValueKeyValuePair(kvp);
        }
    }
}