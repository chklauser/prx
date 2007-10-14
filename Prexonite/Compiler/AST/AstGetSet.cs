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
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstGetSet : AstNode,
                                      IAstEffect,
                                      IAstHasExpressions
    {
        //public readonly List<IAstExpression> Arguments = new List<IAstExpression>();
        //public readonly List<IAstExpression> RightAppendArguments = new List<IAstExpression>();

        private List<IAstExpression> _arguments = new List<IAstExpression>();
        private readonly ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            [DebuggerNonUserCode()]
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public virtual IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        [DebuggerNonUserCode()]
        public class ArgumentsProxy : IList<IAstExpression>
        {
            private List<IAstExpression> _arguments;

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

            private List<IAstExpression> rightAppends = new List<IAstExpression>();

            public int RightAppendPosition
            {
                get { return _rightAppendPosition; }
            }

            private int _rightAppendPosition = -1;

            public void RightAppend(IAstExpression item)
            {
                rightAppends.Add(item);
            }

            public void RightAppend(IEnumerable<IAstExpression> item)
            {
                rightAppends.AddRange(item);
            }

            public void RemeberRightAppendPosition()
            {
                _rightAppendPosition = _arguments.Count;
            }

            public void ReleaseRightAppend()
            {
                _arguments.InsertRange(
                    (_rightAppendPosition < 0) ? _arguments.Count : _rightAppendPosition,
                    rightAppends);
                rightAppends.Clear();
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
        }

        public PCall Call;
        public BinaryOperator SetModifier;

        protected AstGetSet(string file, int line, int column, PCall call)
            : base(file, line, column)
        {
            Call = call;
            _proxy = new ArgumentsProxy(_arguments);
        }

        internal AstGetSet(Parser p, PCall call)
            : this(p.scanner.File, p.t.line, p.t.col, call)
        {
        }

        #region IAstExpression Members

        public virtual bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + _arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }

            return false;
        }

        public void EmitArguments(CompilerTarget target)
        {
            foreach (IAstExpression expr in Arguments)
                expr.EmitCode(target);
        }

        public void EmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        public virtual void EmitCode(CompilerTarget target, bool justEffect)
        {
            switch (Call)
            {
                case PCall.Get:
                    EmitArguments(target);
                    EmitGetCode(target, justEffect);
                    break;
                case PCall.Set:
                    if (SetModifier == BinaryOperator.Coalescence)
                    {
                        AstGetSet assignment = GetCopy();
                        assignment.SetModifier = BinaryOperator.None;

                        AstGetSet getVariation = GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation.Arguments.RemoveAt(getVariation._arguments.Count - 1);

                        AstTypecheck check =
                            new AstTypecheck(
                                File,
                                Line,
                                Column,
                                getVariation,
                                new AstConstantTypeExpression(File, Line, Column, "Null"));

                        AstCondition cond = new AstCondition(File, Line, Column, check);
                        cond.IfBlock.Add(assignment);

                        cond.EmitCode(target);
                    }
                    else if (SetModifier != BinaryOperator.None)
                    {
                        //Without more detailed information, a Set call with a set modifier has to be expressed using 
                        //  conventional set call and binary operator nodes.
                        //Note that code generator for this original node is completely bypassed.
                        AstGetSet assignment = GetCopy();
                        assignment.SetModifier = BinaryOperator.None;
                        AstGetSet getVariation = GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation._arguments.RemoveAt(getVariation._arguments.Count - 1);
                        assignment._arguments[assignment._arguments.Count - 1] =
                            new AstBinaryOperator(
                                File,
                                Line,
                                Column,
                                getVariation,
                                SetModifier,
                                _arguments[_arguments.Count - 1]);
                        assignment.EmitCode(target);
                    }
                    else
                    {
                        EmitArguments(target);
                        EmitSetCode(target);
                    }
                    break;
            }
        }

        public override sealed void EmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        public abstract void EmitGetCode(CompilerTarget target, bool justEffect);
        public abstract void EmitSetCode(CompilerTarget target);

        public void EmitGetCode(CompilerTarget target)
        {
            EmitGetCode(target, false);
        }

        void IAstExpression.EmitCode(CompilerTarget target)
        {
            PCall ocall = Call;
            Call = PCall.Get;
            try
            {
                EmitCode(target, false);
            }
            finally
            {
                Call = ocall;
            }
        }

        #endregion

        public abstract AstGetSet GetCopy();

        public override string ToString()
        {
            string typeName;
            return String.Format(
                "{0}{2}: {1}",
                Enum.GetName(typeof(PCall), Call).ToLowerInvariant(),
                (typeName = GetType().Name).StartsWith("AstGetSet")
                    ? typeName.Substring(9)
                    : typeName,
                SetModifier != BinaryOperator.None
                    ? "(" + Enum.GetName(typeof(BinaryOperator), SetModifier) + ")"
                    : "");
        }

        public string ArgumentsToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("(");
            foreach (IAstExpression expr in Arguments)
                if (expr != null)
                    buffer.Append(expr + ", ");
                else
                    buffer.Append("{null}, ");
            return buffer + ")";
        }

        protected virtual void CopyBaseMembers(AstGetSet target)
        {
            target._arguments.AddRange(_arguments);
        }
    }
}