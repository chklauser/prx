using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler
{
    public class SourceMapping : IDictionary<int, ISourcePosition>
    {
        #region Representation

        private readonly List<ISourcePosition> _positionTable = new List<ISourcePosition>();
        private int _validCount;

        #endregion


        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<int, ISourcePosition>> GetEnumerator()
        {
            var index = 0;
            foreach (var sourcePosition in _positionTable)
            {
                if (sourcePosition != null)
                    yield return new KeyValuePair<int, ISourcePosition>(index, sourcePosition);

                index++;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<int,ISourcePosition>>

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        void ICollection<KeyValuePair<int, ISourcePosition>>.Add(KeyValuePair<int, ISourcePosition> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
        public void Clear()
        {
            _positionTable.Clear();
            _validCount = 0;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        bool ICollection<KeyValuePair<int, ISourcePosition>>.Contains(KeyValuePair<int, ISourcePosition> item)
        {
            ISourcePosition pos;
            return TryGetValue(item.Key, out pos) && pos.SourcePositionEquals(item.Value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination 
        /// of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. 
        /// The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> 
        /// is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> 
        /// is multidimensional.-or-The number of elements in the source 
        /// <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space 
        /// from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// -or-Type cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(KeyValuePair<int, ISourcePosition>[] array, int arrayIndex)
        {
            this.ToArray().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        bool ICollection<KeyValuePair<int, ISourcePosition>>.Remove(KeyValuePair<int, ISourcePosition> item)
        {
            ISourcePosition pos;
            if (TryGetValue(item.Key, out pos) && pos.SourcePositionEquals(item.Value))
            {
                return Remove(item.Key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return _validCount; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<KeyValuePair<int, ISourcePosition>>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IDictionary<int,ISourcePosition>

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="instructionOffset">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="instructionOffset"/> is null.</exception>
        public bool ContainsKey(int instructionOffset)
        {
            return instructionOffset < _positionTable.Count && _positionTable[instructionOffset] != null;
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="instructionOffset">The object to use as the key of the element to add.</param><param name="value">The object to use as the value of the element to add.</param><exception cref="T:System.ArgumentNullException"><paramref name="instructionOffset"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public void Add(int instructionOffset, ISourcePosition value)
        {
            if (instructionOffset < _positionTable.Count)
            {
                var insertDelta = value == null ? 0 : +1;
                var removeDelta = _positionTable[instructionOffset] == null ? 0 : -1;

                _positionTable[instructionOffset] = value;

                _validCount += insertDelta + removeDelta;
            }
            else
            {
                _positionTable.Capacity = Math.Max(2,Math.Max(instructionOffset + 1, _positionTable.Capacity * 2));
                for (var i = _positionTable.Count; i < _positionTable.Capacity; i++)
                    _positionTable.Add(null);
                Debug.Assert(instructionOffset < _positionTable.Count);
                Add(instructionOffset, value);
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="instructionOffset"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <param name="instructionOffset">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="instructionOffset"/> is null.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public bool Remove(int instructionOffset)
        {
            if (instructionOffset < _positionTable.Count)
            {
                if (_positionTable[instructionOffset] != null)
                {
                    _positionTable[instructionOffset] = null;
                    _validCount--;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void RemoveRange(int instructionOffset, int count)
        {
            if(count < 0)
            {
                RemoveRange(instructionOffset + count, -count);
                return;
            }
            else if (count == 0)
            {
                return;
            }

            if(instructionOffset < _positionTable.Count)
            {
                count = Math.Min(instructionOffset + count, _positionTable.Count) - instructionOffset;
                Debug.Assert(instructionOffset + count <= _positionTable.Count,"Removal range not clamped to backing storage index range.");
                _positionTable.RemoveRange(instructionOffset, count);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="instructionOffset">The key whose value to get.</param><param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="instructionOffset"/> is null.</exception>
        public bool TryGetValue(int instructionOffset, out ISourcePosition value)
        {
            if (instructionOffset < _positionTable.Count)
            {
                value = _positionTable[instructionOffset];
                return value != null;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <param name="instructionOffset">The key of the element to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="instructionOffset"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="instructionOffset"/> is not found.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.</exception>
        public ISourcePosition this[int instructionOffset]
        {
            get
            {
                ISourcePosition pos;
                if (TryGetValue(instructionOffset, out pos))
                    return pos;
                else
                    throw new KeyNotFoundException("The source mapping does not contain an entry for instruction " + instructionOffset);
            }
            set
            {
                Remove(instructionOffset);
                Add(instructionOffset, value);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        ICollection<int> IDictionary<int, ISourcePosition>.Keys
        {
            get { return Enumerable.Range(0, _positionTable.Count).Where(i => _positionTable[i] != null).ToList(); }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        ICollection<ISourcePosition> IDictionary<int, ISourcePosition>.Values
        {
            get { return _positionTable.Where(pos => pos != null).ToList(); }
        }

        #endregion

        #region Serialization

        public const string SourceMappingKey = "SourceCode";

        public static SourceMapping Load(IHasMetaTable subject)
        {
            return Load(subject.Meta);
        }

        public static SourceMapping Load(MetaTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            MetaEntry rootEntry;

            //Check if there is source information available
            if (!table.TryGetValue(SourceMappingKey, out rootEntry))
                return new SourceMapping();

            var source = new SourceMapping();
            var root = rootEntry.List;

            //Read file
            var file = root.Length < 1 ? "~unknown~" : root[0].Text;

            //Read source locations
            foreach (var metaEntry in root.Skip(1))
            {
                //Extract values from entry
                var entry = metaEntry.List;
                var instructionOffsetRaw = entry.Length >= 1 ? entry[0].Text : null;
                var lineRaw = entry.Length >= 2 ? entry[1].Text : null;
                var colRaw = entry.Length >= 3 ? entry[2].Text : null;
                var thisFile = entry.Length >= 4 ? entry[3].Text : file;

                //Convert text to values
                int instructionOffset;
                if (instructionOffsetRaw == null || !Int32.TryParse(instructionOffsetRaw, out instructionOffset))
                    continue;

                int line;
                if (lineRaw == null || !Int32.TryParse(lineRaw, out line))
                    continue;

                int col;
                if (colRaw == null || !Int32.TryParse(colRaw, out col))
                    col = 0;

                //add entry
                source.Add(instructionOffset, new SourcePosition(thisFile, line, col));
            }

            return source;
        }

        public void Store(IHasMetaTable subject)
        {
            Store(subject.Meta);
        }

        public void Store(MetaTable table)
        {
            if (table == null)
                throw new ArgumentNullException("table");

            if(Count == 0)
                return;

            //Check for existing source information
            SourceMapping finalMapping;
            if(table.ContainsKey(SourceMappingKey))
            {
                finalMapping = Load(table);
                //override with information in this mapping
                finalMapping.AddRange(this);
            }
            else
            {
                finalMapping = this;
            }

            //find most common file name
            var mcfTable = new Dictionary<string, int>();
            foreach (var pos in finalMapping._positionTable)
            {
                if(pos == null)
                    continue;

                int count;
                var file = pos.File;
                if (!mcfTable.TryGetValue(file, out count))
                    mcfTable.Add(file, 1);
                else
                    mcfTable[file]++;
            }

            var mostCommonFile = mcfTable.OrderByDescending(kvp => kvp.Value).First().Key;

            var rootEntry = new LinkedList<MetaEntry>();
            rootEntry.AddFirst(mostCommonFile);

            var entry = new List<MetaEntry>(4);
            var index = -1;
            foreach (var pos in finalMapping._positionTable)
            {
                index++;
                if(pos == null)
                    continue;

                entry.Clear();
                entry.Add(index.ToString());
                entry.Add(pos.Line.ToString());
                entry.Add(pos.Column.ToString());
                if (!pos.File.Equals(mostCommonFile, StringComparison.Ordinal))
                    entry.Add(pos.File);

                rootEntry.AddLast((MetaEntry) entry.ToArray());
            }

            table[SourceMappingKey] = (MetaEntry) rootEntry.ToArray();
        }

        #endregion
    }
}
