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

using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Prexonite.Compiler.Ast;

[DebuggerNonUserCode]
public class ArgumentsProxy : IList<AstExpr>, IObject
{
    List<AstExpr> _arguments;
    readonly List<AstExpr> _rightAppends = new();

    internal ArgumentsProxy(List<AstExpr> arguments)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    internal void ResetProxy(List<AstExpr> arguments)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }

    public int RightAppendPosition { get; private set; } = -1;

    public void RightAppend(AstExpr item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        _rightAppends.Add(item);
    }

    public void RightAppend(IEnumerable<AstExpr> item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        _rightAppends.AddRange(item);
    }

    public void RememberRightAppendPosition()
    {
        RightAppendPosition = _arguments.Count;
    }

    public void ReleaseRightAppend()
    {
        _arguments.InsertRange(_getEffectiveRightAppendPosition(), _rightAppends);
        _rightAppends.Clear();
    }

    int _getEffectiveRightAppendPosition()
    {
        return RightAppendPosition < 0 ? _arguments.Count : RightAppendPosition;
    }

    #region IList<AstExpr> Members

    ///<summary>
    ///    Determines the index of a specific item in the <see cref = "T:System.Collections.Generic.IList`1"></see>.
    ///</summary>
    ///<returns>
    ///    The index of item if found in the list; otherwise, -1.
    ///</returns>
    ///<param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.IList`1"></see>.</param>
    public int IndexOf(AstExpr? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        return _arguments.IndexOf(item);
    }

    ///<summary>
    ///    Inserts an item to the <see cref = "T:System.Collections.Generic.IList`1"></see> at the specified index.
    ///</summary>
    ///<param name = "item">The object to insert into the <see cref = "T:System.Collections.Generic.IList`1"></see>.</param>
    ///<param name = "index">The zero-based index at which item should be inserted.</param>
    ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
    ///<exception cref = "T:System.ArgumentOutOfRangeException">index is not a valid index in the <see
    ///     cref = "T:System.Collections.Generic.IList`1"></see>.</exception>
    public void Insert(int index, AstExpr? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        _arguments.Insert(index, item);
    }

    ///<summary>
    ///    Removes the <see cref = "T:System.Collections.Generic.IList`1"></see> item at the specified index.
    ///</summary>
    ///<param name = "index">The zero-based index of the item to remove.</param>
    ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
    ///<exception cref = "T:System.ArgumentOutOfRangeException">index is not a valid index in the <see
    ///     cref = "T:System.Collections.Generic.IList`1"></see>.</exception>
    public void RemoveAt(int index)
    {
        _arguments.RemoveAt(index);
    }

    ///<summary>
    ///    Gets or sets the element at the specified index.
    ///</summary>
    ///<returns>
    ///    The element at the specified index.
    ///</returns>
    ///<param name = "index">The zero-based index of the element to get or set.</param>
    ///<exception cref = "T:System.ArgumentOutOfRangeException">index is not a valid index in the <see
    ///     cref = "T:System.Collections.Generic.IList`1"></see>.</exception>
    ///<exception cref = "T:System.NotSupportedException">The property is set and the <see
    ///     cref = "T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
    public AstExpr this[int index]
    {
        get
        {
            var value = _arguments[index];
            Debug.Assert(value != null, "Arguments of Ast nodes cannot be null",
                "Found null entry at index {0}", index);
            return value;
        }
        set => _arguments[index] = value ?? throw new ArgumentNullException(nameof(value));
    }

    #endregion

    #region ICollection<AstExpr> Members

    ///<summary>
    ///    Adds an item to the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</summary>
    ///<param name = "item">The object to add to the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
    ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
    public void Add(AstExpr? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        _arguments.Add(item);
    }

    /// <summary>
    ///     Adds a number of items to the list of arguments.
    /// </summary>
    /// <param name = "items">A collection of arguments.</param>
    public void AddRange(IEnumerable<AstExpr> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
            
        _arguments.AddRange(items);
    }

    ///<summary>
    ///    Removes all items from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</summary>
    ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
    public void Clear()
    {
        _arguments.Clear();
    }

    ///<summary>
    ///    Determines whether the <see cref = "T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
    ///</summary>
    ///<returns>
    ///    true if item is found in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
    ///</returns>
    ///<param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
    public bool Contains(AstExpr? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        return _arguments.Contains(item);
    }

    ///<summary>
    ///    Copies the elements of the <see cref = "T:System.Collections.Generic.ICollection`1"></see> to an <see
    ///     cref = "T:System.Array"></see>, starting at a particular <see cref = "T:System.Array"></see> index.
    ///</summary>
    ///<param name = "array">The one-dimensional <see cref = "T:System.Array"></see> that is the destination of the elements copied from <see
    ///     cref = "T:System.Collections.Generic.ICollection`1"></see>. The <see cref = "T:System.Array"></see> must have zero-based indexing.</param>
    ///<param name = "arrayIndex">The zero-based index in array at which copying begins.</param>
    ///<exception cref = "T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
    ///<exception cref = "T:System.ArgumentNullException">array is null.</exception>
    ///<exception cref = "T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see
    ///     cref = "T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
    public void CopyTo(AstExpr[] array, int arrayIndex)
    {
        _arguments.CopyTo(array, arrayIndex);
    }

    ///<summary>
    ///    Removes the first occurrence of a specific object from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</summary>
    ///<returns>
    ///    true if item was successfully removed from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see
    ///     cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</returns>
    ///<param name = "item">The object to remove from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
    ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
    public bool Remove(AstExpr? item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
            
        return _arguments.Remove(item);
    }

    ///<summary>
    ///    Gets the number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</summary>
    ///<returns>
    ///    The number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
    ///</returns>
    public int Count => _arguments.Count;

    ///<summary>
    ///    Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.
    ///</summary>
    ///<returns>
    ///    true if the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
    ///</returns>
    public bool IsReadOnly => ((ICollection<AstExpr>) _arguments).IsReadOnly;

    #endregion

    #region IEnumerable<AstExpr> Members

    ///<summary>
    ///    Returns an enumerator that iterates through the collection.
    ///</summary>
    ///<returns>
    ///    A <see cref = "T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
    ///</returns>
    ///<filterpriority>1</filterpriority>
    IEnumerator<AstExpr> IEnumerable<AstExpr>.GetEnumerator()
    {
        return _arguments.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    ///<summary>
    ///    Returns an enumerator that iterates through a collection.
    ///</summary>
    ///<returns>
    ///    An <see cref = "T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    public IEnumerator GetEnumerator()
    {
        return _arguments.GetEnumerator();
    }

    #endregion

    public AstExpr[] ToArray()
    {
        return _arguments.ToArray();
    }

    public void CopyFrom(ArgumentsProxy proxy)
    {
        _arguments.Clear();
        _arguments.AddRange(proxy._arguments);

        RightAppendPosition = proxy.RightAppendPosition;

        _rightAppends.Clear();
        _rightAppends.AddRange(proxy._rightAppends);
    }

    #region Implementation of IObject

    public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
        [NotNullWhen(true)] out PValue? result)
    {
        switch (id.ToUpperInvariant())
        {
            case "ADDRANGE":
                if (args.Length > 0)
                {
                    var arg0 = args[0];
                    IEnumerable<PValue>? xs;
                    if (arg0.Value is IEnumerable<AstExpr> exprs)
                    {
                        AddRange(exprs);
                        result = PType.Null;
                        return true;
                    }
                    else if ((xs = arg0.Value as IEnumerable<PValue>) != null)
                    {
                        AddRange(xs.Select(x => (AstExpr)x.ConvertTo(sctx, typeof(AstExpr), true).Value!));
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

    #region ToString

    public override string ToString()
    {
        var sb = new StringBuilder();
        var rap = _getEffectiveRightAppendPosition();
        int i;
        for (i = 0; i < _arguments.Count; i++)
        {
            if (i == rap)
                _writeRightAppends(sb);
            else
                _writeArgument(sb, _arguments[i]);
        }

        if (i == rap)
            _writeRightAppends(sb);

        return sb.ToString(0, Math.Max(0, sb.Length - 2));
    }

    void _writeRightAppends(StringBuilder sb)
    {
        foreach (var rightExpr in _rightAppends)
            _writeArgument(sb, rightExpr);
    }

    static void _writeArgument(StringBuilder sb, AstExpr expr)
    {
        sb.AppendFormat("{0}, ", expr);
    }

    #endregion
}