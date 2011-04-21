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
using System.Diagnostics;
using System.Linq;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    [DebuggerNonUserCode]
    public class ArgumentsProxy : IList<IAstExpression>, IObject
    {
        private List<IAstExpression> _arguments;
        private int _rightAppendPosition = -1;
        private readonly List<IAstExpression> _rightAppends = new List<IAstExpression>();

        internal ArgumentsProxy(List<IAstExpression> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            _arguments = arguments;
        }

        internal void ResetProxy(List<IAstExpression> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            _arguments = arguments;
        }

        public int RightAppendPosition
        {
            get { return _rightAppendPosition; }
        }

        public void RightAppend(IAstExpression item)
        {
            _rightAppends.Add(item);
        }

        public void RightAppend(IEnumerable<IAstExpression> item)
        {
            _rightAppends.AddRange(item);
        }

        public void RememberRightAppendPosition()
        {
            _rightAppendPosition = _arguments.Count;
        }

        public void ReleaseRightAppend()
        {
            _arguments.InsertRange(
                (_rightAppendPosition < 0) ? _arguments.Count : _rightAppendPosition,
                _rightAppends);
            _rightAppends.Clear();
        }

        #region IList<IAstExpression> Members

        ///<summary>
        ///Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///The index of item if found in the list; otherwise, -1.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        public int IndexOf(IAstExpression item)
        {
            return _arguments.IndexOf(item);
        }

        ///<summary>
        ///Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        ///</summary>
        ///
        ///<param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        ///<param name="index">The zero-based index at which item should be inserted.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void Insert(int index, IAstExpression item)
        {
            _arguments.Insert(index, item);
        }

        ///<summary>
        ///Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        ///</summary>
        ///
        ///<param name="index">The zero-based index of the item to remove.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void RemoveAt(int index)
        {
            _arguments.RemoveAt(index);
        }

        ///<summary>
        ///Gets or sets the element at the specified index.
        ///</summary>
        ///
        ///<returns>
        ///The element at the specified index.
        ///</returns>
        ///
        ///<param name="index">The zero-based index of the element to get or set.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        ///<exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public IAstExpression this[int index]
        {
            get { return _arguments[index]; }
            set { _arguments[index] = value; }
        }

        #endregion

        #region ICollection<IAstExpression> Members

        ///<summary>
        ///Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add(IAstExpression item)
        {
            _arguments.Add(item);
        }

        /// <summary>
        /// Adds a number of items to the list of arguments.
        /// </summary>
        /// <param name="items">A collection of arguments.</param>
        public void AddRange(IEnumerable<IAstExpression> items)
        {
            _arguments.AddRange(items);
        }

        ///<summary>
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            _arguments.Clear();
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        ///</summary>
        ///
        ///<returns>
        ///true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        public bool Contains(IAstExpression item)
        {
            return _arguments.Contains(item);
        }

        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        ///</summary>
        ///
        ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        ///<param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ///<exception cref="T:System.ArgumentNullException">array is null.</exception>
        ///<exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(IAstExpression[] array, int arrayIndex)
        {
            _arguments.CopyTo(array, arrayIndex);
        }

        ///<summary>
        ///Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        ///<param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(IAstExpression item)
        {
            return _arguments.Remove(item);
        }

        ///<summary>
        ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        public int Count
        {
            get { return _arguments.Count; }
        }

        ///<summary>
        ///Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
        ///</returns>
        ///
        public bool IsReadOnly
        {
            get { return ((ICollection<IAstExpression>) _arguments).IsReadOnly; }
        }

        #endregion

        #region IEnumerable<IAstExpression> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        IEnumerator<IAstExpression> IEnumerable<IAstExpression>.GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        #endregion

        public IAstExpression[] ToArray()
        {
            return _arguments.ToArray();
        }

        public void CopyFrom(ArgumentsProxy proxy)
        {
            _arguments.Clear();
            _arguments.AddRange(proxy._arguments);

            _rightAppendPosition = proxy._rightAppendPosition;

            _rightAppends.Clear();
            _rightAppends.AddRange(proxy._rightAppends);
        }

        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            switch (id.ToUpperInvariant())
            {
                case "ADDRANGE":
                    if (args.Length > 0)
                    {
                        var arg0 = args[0];
                        var exprs = arg0.Value as IEnumerable<IAstExpression>;
                        IEnumerable<PValue> xs;
                        if (exprs != null)
                        {
                            AddRange(exprs);
                            result = PType.Null;
                            return true;
                        }
                        else if ((xs = arg0.Value as IEnumerable<PValue>) != null)
                        {
                            AddRange(
                                from x in xs
                                select (IAstExpression)
                                       x.ConvertTo(sctx, typeof (IAstExpression), true).Value
                                );
                            result = PType.Null;
                            return true;
                        }
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                default:
                    result = null;
                    return false;
            }

            result = null;
            return false;
        }

        #endregion
    }
}