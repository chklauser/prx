using System;
using System.Collections.Generic;

namespace Prexonite.Types
{

    /// <summary>
    /// Pair or PValues
    /// </summary>
    //[System.Diagnostics.DebuggerNonUserCode()]
    public class PValueKeyValuePair : IObject
    {
        private static readonly ObjectPType _objectType = new ObjectPType(typeof(PValueKeyValuePair));

        /// <summary>
        /// A static reference to the object type of this class.
        /// </summary>
        public static ObjectPType ObjectType
        {
            get { return _objectType; }
        }


        private PValue _key;

        /// <summary>
        /// Provides access to the value stored as the "Key".
        /// </summary>
        public  PValue Key
        {
            get { return _key; }
        }

        private PValue _value;

        /// <summary>
        /// Provides access to the value stored as the "Value".
        /// </summary>
        public PValue Value
        {
            get { return _value; }
        }


        /// <summary>
        /// Creates a new PValueKeyValuePair.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public PValueKeyValuePair(PValue key, PValue value)
        {
            _key = key ?? PType.Null.CreatePValue();
            _value = value ?? PType.Null.CreatePValue();
        }

        /// <summary>
        /// Creates a new PValueKeyValuePair.
        /// </summary>
        /// <param name="pair">The key-value pair.</param>
        public PValueKeyValuePair(KeyValuePair<PValue, PValue> pair)
            : this(pair.Key, pair.Value)
        {
        }

        #region IObject Members

        /// <summary>
        /// Tries to handle prexonite object member calls.
        /// </summary>
        /// <param name="sctx">The stack context of the call.</param>
        /// <param name="args">The arguments for the call.</param>
        /// <param name="id">The id of the member to call (empty for the default member).</param>
        /// <param name="result">The result returned by the call.</param>
        /// <returns>True, if the call succeeded, false otherwise.</returns>
        /// <remarks><paramref name="result"/> is not defined if the function returns false.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null.</exception>
        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            result = null;

            PValue arg0;

            switch(id.ToLowerInvariant())
            {
                case "":
                    if(args.Length == 1)
                    {
                        if(args[0].TryConvertTo(sctx, PType.Int, out arg0))
                        {
                            int i = (int) arg0.Value;
                            if (i == 0)
                                result = _key;
                            else if (i == 1)
                                result = _value;
                            else
                                result = PType.Null.CreatePValue();
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
                    if(args.Length == 1)
                    {
                        if(args[0].TryConvertTo(sctx, _objectType, out arg0))
                        {
                            PValueKeyValuePair pair = (PValueKeyValuePair) arg0.Value;
                            result = _key.Equals(pair._key) && _value.Equals(pair._value);
                        }
                    }
                    break;
                case "tostring":
                    result = String.Concat(_key.CallToString(sctx), ": ", _value.CallToString(sctx));
                    break;
                default:
                    if(!_objectType.TryDynamicCall(sctx, sctx.CreateNativePValue(this),args, PCall.Get,id, out result))
                        result = null;
                    break;
            }

            return result != null;
        }

        #endregion

        /// <summary>
        /// Checks if this instance is equal to the supplied object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>True if the two object are equal, False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            PValue okey;
            PValue ovalue;

            if(obj is PValueKeyValuePair)
            {
                PValueKeyValuePair pvkvp = (PValueKeyValuePair) obj;
                okey = pvkvp.Key;
                ovalue = pvkvp.Value;
            }
            else if(obj is KeyValuePair<PValue, PValue>)
            {
                KeyValuePair<PValue, PValue> kvp = (KeyValuePair<PValue, PValue>) obj;
                okey = kvp.Key;
                ovalue = kvp.Value;
            }
            else
                return false;

            return _key.Equals(okey) && _value.Equals(ovalue);
        }

        public override int GetHashCode()
        {
            return PType._CombineHashes(_key.GetHashCode(), _value.GetHashCode());
        }

        public override string ToString()
        {
            return String.Concat(_key.ToString(), ": ", _value.ToString());
        }

        /// <summary>
        /// Implicitly converts a PValueKeyValuePair to an ordinary KeyValuePair.
        /// </summary>
        /// <param name="pvkvp">The key-value pair.</param>
        /// <returns>An ordinary key-value pair</returns>
        public static implicit operator KeyValuePair<PValue, PValue>(PValueKeyValuePair pvkvp)
        {
            return new KeyValuePair<PValue, PValue>(pvkvp._key, pvkvp._value);
        }

        /// <summary>
        /// Implicitly converts an ordinary key-value pair to a PValueKeyValuePair.
        /// </summary>
        /// <param name="kvp">The key-value pair.</param>
        /// <returns>A PValueKeyValuePair.</returns>
        public static implicit operator PValueKeyValuePair(KeyValuePair<PValue, PValue> kvp)
        {
            return new PValueKeyValuePair(kvp);
        }
    }
}
