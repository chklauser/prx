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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Prexonite.Types;

namespace Prexonite
{
    //[DebuggerNonUserCode]
    public sealed class MetaEntry
    {
        #region Fields

        /// <summary>
        ///     The possible kinds of meta entries
        /// </summary>
        public enum Type
        {
            Invalid = 0,
            Text = 1,
            List = 2,
            Switch = 3
        }

        /// <summary>
        ///     Holds a list in case the meta entry is a list entry
        /// </summary>
        private readonly MetaEntry[] _list;

        /// <summary>
        ///     Indicates the kind of meta entry
        /// </summary>
        private readonly Type _mtype;

        /// <summary>
        ///     Holds a switch in case the meta entry is a switch entry
        /// </summary>
        private readonly bool _switch;

        /// <summary>
        ///     Holds text in case the meta entry is a text entry
        /// </summary>
        private readonly string _text;

        #endregion

        #region MetaEntry -> Value

        public string Text
        {
            [DebuggerNonUserCode]
            get
            {
                switch (_mtype)
                {
                    case Type.Text:
                        return _text;
                    case Type.Switch:
                        return _switch.ToString();
                    case Type.List:
                        var buffer = new StringBuilder();
                        buffer.Append("{");
                        foreach (string entry in List)
                        {
                            buffer.Append(entry);
                            buffer.Append(", ");
                        }
                        //Cut off last ", "
                        if (buffer.Length >= 2)
                            buffer.Remove(buffer.Length - 2, 2);
                        buffer.Append("}");
                        return buffer.ToString();
                    default:
                        throw new PrexoniteException("Unknown type in meta entry");
                }
            }
        }

        public MetaEntry[] List
        {
            [DebuggerNonUserCode]
            get
            {
                switch (_mtype)
                {
                    case Type.List:
                        return _list;
                    case Type.Switch:
                        return new MetaEntry[] {_switch};
                    case Type.Text:
                        return new MetaEntry[] {_text};
                    default:
                        throw new PrexoniteException("Unknown type in meta entry");
                }
            }
        }

        public bool Switch
        {
            [DebuggerNonUserCode]
            get
            {
                switch (_mtype)
                {
                    case Type.Text:
                        bool sw;
                        return bool.TryParse(_text, out sw) ? sw : false;
                    case Type.Switch:
                        return _switch;
                    case Type.List:
                        return List.Length > 0;
                    default:
                        throw new PrexoniteException("Unknown type in meta entry");
                }
            }
        }

        #endregion

        #region Construction

        [DebuggerNonUserCode]
        public MetaEntry(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            _mtype = Type.Text;
            _list = null;
            _switch = false;
            _text = text;
        }

        [DebuggerNonUserCode]
        public MetaEntry(MetaEntry[] list)
        {
            //Check sanity
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            foreach (var entry in list)
            {
                if (entry == null)
                    throw new ArgumentException(
                        "A MetaEntry list must not contain null references.", nameof(list));
            }
            _mtype = Type.List;
            _text = null;
            _switch = false;
            _list = list;
        }

        [DebuggerNonUserCode]
        public MetaEntry(bool @switch)
        {
            _mtype = Type.Switch;
            _text = null;
            _list = null;
            _switch = @switch;
        }

        #endregion

        #region Properties

        public Type EntryType
        {
            [DebuggerNonUserCode]
            get { return _mtype; }
        }

        public bool IsText
        {
            [DebuggerNonUserCode]
            get { return _mtype == Type.Text; }
        }

        public bool IsList
        {
            [DebuggerNonUserCode]
            get { return _mtype == Type.List; }
        }

        public bool IsSwitch
        {
            [DebuggerNonUserCode]
            get { return _mtype == Type.Switch; }
        }

        #endregion

        #region Operators and Conversions

        [DebuggerNonUserCode]
        public static implicit operator string(MetaEntry item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be implicitly converted to a meta entry.");
            return item.Text;
        }

        [DebuggerNonUserCode]
        public static implicit operator bool(MetaEntry item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be implicitly converted to a meta entry.");
            return item.Switch;
        }

        [DebuggerNonUserCode]
        public static explicit operator MetaEntry[](MetaEntry item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be explicitly converted to a meta entry.");
            return item.List;
        }

        [DebuggerNonUserCode]
        public static implicit operator MetaEntry(bool item)
        {
            return new MetaEntry(item);
        }

        [DebuggerNonUserCode]
        public static implicit operator MetaEntry(string item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be implicitly converted to a meta entry.");
            return new MetaEntry(item);
        }

        [DebuggerNonUserCode]
        public static explicit operator MetaEntry(MetaEntry[] item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be explicitly converted to a meta entry.");
            return new MetaEntry(item);
        }

        public static implicit operator PValue(MetaEntry item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item),
                    "A null reference cannot be implicitly converted to a meta entry.");
            switch (item._mtype)
            {
                case Type.Text:
                    return PType.String.CreatePValue(item._text);
                case Type.Switch:
                    return PType.Bool.CreatePValue(item._switch);
                case Type.List:
                    var lst = new List<PValue>(item._list.Length);
                    foreach (var entry in item._list)
                        lst.Add(entry);
                    return PType.List.CreatePValue(lst);
                default:
                    throw new PrexoniteException(
                        "Meta entry type " + item.EntryType + " is not supported.");
            }
        }

        public static bool operator ==(MetaEntry a, MetaEntry b)
        {
            if ((object) a == null && (object) b == null)
                return true;
            else if ((object) a == null || (object) b == null)
                return false;
            else
            {
                /*       T  S  L
                 *   -----------
                 *   T | T  T  T
                 *   S | T  S  L
                 *   L | T  L  L
                 */

                if (a.IsText || b.IsText)
                {
                    return Engine.StringsAreEqual(a.Text, b.Text);
                }
                else if (b.IsList || b.IsList)
                {
                    MetaEntry[] ar = a.List,
                                br = b.List;
                    if (ar.Length != br.Length)
                        return false;

                    int i;
                    for (i = 0; i < ar.Length; i++)
                        if (ar[i] != br[i])
                            break;

                    return i == ar.Length;
                }
                else //S S
                    return a._switch == b.Switch;
            }
        }

        public static bool operator !=(MetaEntry a, MetaEntry b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var entry = obj as MetaEntry;
            if (entry == null)
                return false;
            else
                return this == entry;
        }

        public override int GetHashCode()
        {
            switch (_mtype)
            {
                case Type.List:
                    return _list.GetHashCode();
                case Type.Switch:
                    return _switch.GetHashCode();
                case Type.Text:
                    return _text.GetHashCode();
                default:
                    return -1;
            }
        }

        #endregion

        #region Modification

        public MetaEntry AddToList(params MetaEntry[] newEntries)
        {
            var list = _asList();
            var newList = new MetaEntry[list.Length + newEntries.Length];
            Array.Copy(list, newList, list.Length);
            Array.Copy(newEntries, 0, newList, list.Length, newEntries.Length);
            return (MetaEntry) newList;
        }

        private MetaEntry[] _asList()
        {
            MetaEntry[] list;
            //Change type to list
            switch (_mtype)
            {
                case Type.Switch:
                    list = new MetaEntry[] {_switch};
                    break;
                case Type.Text:
                    list = new MetaEntry[] {_text};
                    break;
                case Type.List:
                    list = _list;
                    break;
                case Type.Invalid:
                default:
                    throw new PrexoniteException("Invalid meta entry.");
            }
            return list;
        }

        public MetaEntry RemoveFromList(int index)
        {
            return RemoveFromList(index, 1);
        }

        public MetaEntry RemoveFromList(int index, int length)
        {
            MetaEntry[] list = _asList();
            if (index + length > list.Length - 1 || index < 0 || length < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    string.Format("The supplied index and length {0} are out of the range of 0..{1}.", index, (list.Length - 1)));
            var newList = new MetaEntry[list.Length - 1];
            //Copy the elements before the ones to remove
            if (index > 0)
                Array.Copy(list, newList, index);
            //Copy the elements after the ones to remove
            if (index + length < list.Length - 1)
                Array.Copy(list, index + length, newList, index, list.Length - (index + length));
            return (MetaEntry) newList;
        }

        #endregion

        public static MetaEntry[] CreateArray(StackContext sctx, List<PValue> elements)
        {
            var proto = new List<MetaEntry>(elements.Count);
            foreach (var pv in elements)
            {
                PValue pventry;
                if (pv.TryConvertTo(sctx, typeof (MetaEntry), out pventry))
                    proto.Add((MetaEntry) pventry.Value);
                else if (pv.Type is ListPType)
                    proto.Add((MetaEntry) CreateArray(sctx, (List<PValue>) pv.Value));
                else if (pv.Type is BoolPType)
                    proto.Add((bool) pv.Value);
                else
                    proto.Add(pv.CallToString(sctx));
            }
            return proto.ToArray();
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            ToString(buffer);
            return buffer.ToString();
        }

        public void ToString(StringBuilder buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            switch (_mtype)
            {
                case Type.List:
                    buffer.Append("{");
                    foreach (var entry in _list)
                    {
                        if (entry == null)
                            continue;
                        entry.ToString(buffer);
                        buffer.Append(",");
                    }
                    if (_list.Length > 0)
                        buffer.Remove(buffer.Length - 1, 1);
                    buffer.Append("}");
                    break;
                case Type.Switch:
                    buffer.Append(_switch.ToString());
                    break;
                case Type.Text:
                    //Special case: allow integer numbers
                    long num;
                    if (_text.Length <= LengthOfInt32MaxValue && _looksLikeNumber(_text) &&
                        Int64.TryParse(_text, out num))
                    {
                        var format = NumberFormatInfo.InvariantInfo;
                        var numStr = num.ToString(format);
                        Debug.Assert(_looksLikeNumber(numStr));
                        buffer.Append(numStr);
                    }
                    else
                    {
                        buffer.Append(StringPType.ToIdOrLiteral(_text));
                    }
                    break;
            }
        }

        private const int LengthOfInt32MaxValue = 10 + 1; //sign allowed

        private static bool _looksLikeNumber(string text)
        {
            var end = Math.Min(text.Length, LengthOfInt32MaxValue);
            for (var i = 0; i < end; i++)
                if (!Char.IsDigit(text[i]))
                    return false;
            return true;
        }

        /// <summary>
        ///     Returns the default meta entry.
        /// </summary>
        /// <returns>The default meta entry.</returns>
        public static MetaEntry CreateDefaultEntry()
        {
            return new MetaEntry("");
        }
    }
}