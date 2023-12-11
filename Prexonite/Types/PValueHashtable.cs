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

using System.Diagnostics;

#endregion

namespace Prexonite.Types;

/// <summary>
///     Unordered table of key-value pairs.
/// </summary>
[DebuggerNonUserCode]
public class PValueHashtable : Dictionary<PValue, PValue>
{
    /// <summary>
    ///     Adds a new key-value pair to the hashtable.
    /// </summary>
    /// <param name = "pair">The pair to add.</param>
    public void Add(KeyValuePair<PValue, PValue> pair)
    {
        Add(pair.Key, pair.Value);
    }

    /// <summary>
    ///     Adds a new key-value pair to the hashtable. Duplicate keys are overwritten.
    /// </summary>
    /// <param name = "pair">The pair to add.</param>
    public void AddOverride(KeyValuePair<PValue, PValue> pair)
    {
        AddOverride(pair.Key, pair.Value);
    }

    /// <summary>
    ///     Adds a new key-value pair to the hashtable. Duplicate keys are overwritten.
    /// </summary>
    /// <param name = "key">The key of the key-value pair to add.</param>
    /// <param name = "value">The value of the key-value pair to add.</param>
    public void AddOverride(PValue key, PValue value)
    {
        if (ContainsKey(key))
            this[key] = value;
        else
            Add(key, value);
    }

    /// <summary>
    ///     Provides access to the object type of this class.
    /// </summary>
    public static ObjectPType ObjectType { get; } = new(typeof (PValueHashtable));

    /// <summary>
    ///     Creates a new instance of PValueHashTable.
    /// </summary>
    public PValueHashtable()
    {
    }

    /// <summary>
    ///     Creates a new instance of PValueHashTable.
    /// </summary>
    /// <remarks>
    ///     This overload initializes the backing store with a certain capacity.
    /// </remarks>
    public PValueHashtable(int capacity)
        : base(capacity)
    {
    }

    /// <summary>
    ///     Creates a new instance of PValueHashTable.
    /// </summary>
    /// <remarks>
    ///     This overload initialized the table with the supplied dictionary.
    /// </remarks>
    public PValueHashtable(IDictionary<PValue, PValue> dictionary)
        : base(dictionary)
    {
    }

    /// <summary>
    ///     Checks whether an object is equal to the current PValueHashtable
    /// </summary>
    /// <param name = "obj">The object to check for equality.</param>
    /// <returns>True if this PValueHashtable and the object are equal, False otherwise.</returns>
    public override bool Equals(object? obj)
    {
        if (obj != null && (obj is PValueHashtable || obj is Dictionary<PValue, PValue>))
            return base.Equals(obj);
        return false;
    }

    public override int GetHashCode()
    {
        var hash = 1;
        foreach (var pair in this)
            hash =
                hash ^ pair.Key.GetHashCode() ^ pair.Value.GetHashCode();

        return hash;
    }

    /// <summary>
    ///     Similar to <see cref = "Dictionary{TKey,TValue}.GetEnumerator" /> but 
    ///     returns <see cref = "PValueKeyValuePair" /> instances.
    /// </summary>
    /// <returns>An IEnumerator that returns <see cref = "PValueKeyValuePair" /> instances.</returns>
    public IEnumerable<PValueKeyValuePair> PValueKeyValuePairs()
    {
        foreach (var pair in this)
            yield return pair;
    }

    internal IEnumerable<PValue> GetPValueEnumerator()
    {
        foreach (var pair in this)
            yield return PType.Object.CreatePValue(new PValueKeyValuePair(pair));
    }

    public static explicit operator PValue(PValueHashtable pvht)
    {
        return new(pvht, PType.Hash);
    }
}