// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    public struct QualifiedId : IReadOnlyList<string>, IEquatable<QualifiedId>, IComparable<QualifiedId>
    {
        [CanBeNull]
        private readonly string[] _elements;

        public QualifiedId(params string[] elements) : this()
        {
            _elements = elements;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _elements == null
                ? Enumerable.Empty<string>().GetEnumerator()
                : ((IEnumerable<string>)_elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _elements == null ? 0 : _elements.Length; }
        }

        public string this[int index]
        {
            get
            {
                if (_elements == null)
                    throw new IndexOutOfRangeException("Index into qualified id is out of range. (Empty qualified id)");
                return _elements[index];
            }
        }

        public bool Equals(QualifiedId other)
        {
            var thisZero = _elements == null || _elements.Length == 0;
            var otherZero = other._elements == null || other._elements.Length == 0;
            if (thisZero && otherZero)
            {
                return true;
            }
            else if (thisZero || otherZero || _elements.Length != other._elements.Length)
            {
                // Treat uninitialized and zero length paths the same.
                return false;
            }
            else
            {
                // Compare paths in revrese, becaus they are much more likely to
                // differ at the end than at the beginning
                for (var i = _elements.Length - 1; i >= 0; i++)
                {
                    if (!Engine.StringsAreEqual(_elements[i],other._elements[i]))
                        return false;
                }

                // Did not detect a difference
                return true;
            }
        }

        public int CompareTo(QualifiedId other)
        {
            var thisLen = Count;
            var otherLen = other.Count;
            if (thisLen == 0 && otherLen == 0)
                return 0;
            else if (thisLen == 0)
                return -1;
            else if (otherLen == 0)
                return 1;
            else
            {
                Debug.Assert(_elements != null && other._elements != null);
                // must compare element by element
                for (var i = 0; i < thisLen && i < otherLen; i++)
                {
// ReSharper disable PossibleNullReferenceException
                    var r = StringComparer.OrdinalIgnoreCase.Compare(_elements[i], other._elements[i]);
// ReSharper restore PossibleNullReferenceException
                    if (r != 0)
                        return r;
                }

                // The qualified ids share a common prefix, compare their lengths instead
                return thisLen.CompareTo(otherLen);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is QualifiedId && Equals((QualifiedId)obj);
        }

        public override int GetHashCode()
        {
            return (_elements != null ? _elements.GetHashCode() : 0);
        }

        public static bool operator ==(QualifiedId left, QualifiedId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(QualifiedId left, QualifiedId right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(QualifiedId left, QualifiedId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(QualifiedId left, QualifiedId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(QualifiedId left, QualifiedId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(QualifiedId left, QualifiedId right)
        {
            return left.CompareTo(right) <= 0;
        }
    }
}