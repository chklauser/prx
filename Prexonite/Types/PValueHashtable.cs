using System.Collections.Generic;
using System.Diagnostics;

namespace Prexonite.Types
{
    /// <summary>
    /// Unordered table of key-value pairs.
    /// </summary>
    [DebuggerNonUserCode]
    public class PValueHashtable : Dictionary<PValue, PValue>
    {
        private static readonly ObjectPType _objectType = new ObjectPType(typeof(PValueHashtable));
        
        /// <summary>
        /// Adds a new key-value pair to the hashtable.
        /// </summary>
        /// <param name="pair">The pair to add.</param>
        public void Add(KeyValuePair<PValue, PValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Adds a new key-value pair to the hashtable. Duplicate keys are overwritten.
        /// </summary>
        /// <param name="pair">The pair to add.</param>
        public void AddOverride(KeyValuePair<PValue, PValue> pair)
        {
            AddOverride(pair.Key, pair.Value);
        }

        /// <summary>
        /// Adds a new key-value pair to the hashtable. Duplicate keys are overwritten.
        /// </summary>
        /// <param name="key">The key of the key-value pair to add.</param>
        /// <param name="value">The value of the key-value pair to add.</param>
        public void AddOverride(PValue key, PValue value)
        {
            if(ContainsKey(key))
                this[key] = value;
            else
                Add(key, value);
        }

        /// <summary>
        /// Provides access to the object type of this class.
        /// </summary>
        public static ObjectPType ObjectType
        {
            get { return _objectType; }
        }


        /// <summary>
        /// Creates a new instance of PValueHashTable.
        /// </summary>
        public PValueHashtable()
        {
        }

        /// <summary>
        /// Creates a new instance of PValueHashTable.
        /// </summary>
        /// <remarks>
        /// This overload initializes the backing store with a certain capacity.
        /// </remarks>
        public PValueHashtable(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Creates a new instance of PValueHashTable.
        /// </summary>
        /// <remarks>
        /// This overload initialized the table with the supplied dictionary.
        /// </remarks>
        public PValueHashtable(IDictionary<PValue, PValue> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Checks whether an object is equal to the current PValueHashtable
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>True if this PValueHashtable and the object are equal, False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj != null && (obj is PValueHashtable || obj is Dictionary<PValue, PValue>))
                return base.Equals(obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            foreach (KeyValuePair<PValue, PValue> pair in this)
                hash =
                    PType._CombineHashes(
                        hash, PType._CombineHashes(pair.Key.GetHashCode(), pair.Value.GetHashCode()));

            return hash;
        }

        /// <summary>
        /// Similar to <see cref="Dictionary{TKey,TValue}.GetEnumerator"/> but 
        /// returns <see cref="PValueKeyValuePair"/> instances.
        /// </summary>
        /// <returns>An IEnumerator that returns <see cref="PValueKeyValuePair"/> instances.</returns>
        public IEnumerable<PValueKeyValuePair> PValueKeyValuePairs()
        {
            foreach (KeyValuePair<PValue, PValue> pair in this)
                yield return pair;
        }

        internal IEnumerable<PValue> GetPValueEnumerator()
        {
            foreach (KeyValuePair<PValue, PValue> pair in this)
                yield return PType.Object.CreatePValue(new PValueKeyValuePair(pair));
        }
    }
}
