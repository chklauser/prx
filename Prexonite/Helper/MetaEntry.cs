#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite;

//[DebuggerNonUserCode]
public sealed class MetaEntry
{
    #region Fields

    /// <summary>
    ///     The possible kinds of meta entries
    /// </summary>
    public enum Type
    {
        Text = 0,
        List = 2,
        Switch = 3
    }

    /// <summary>
    ///     Holds a list in case the meta entry is a list entry
    /// </summary>
    readonly MetaEntry[]? _list;

    /// <summary>
    ///     Holds a switch in case the meta entry is a switch entry
    /// </summary>
    readonly bool _switch;

    /// <summary>
    ///     Holds text in case the meta entry is a text entry
    /// </summary>
    readonly string? _text;

    static readonly MetaEntry[] EmptyList = Array.Empty<MetaEntry>();

    #endregion

    #region MetaEntry -> Value

    public string Text
    {
        [DebuggerNonUserCode]
        get
        {
            switch (EntryType)
            {
                case Type.Text:
                    return _text ?? "";
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
            return EntryType switch
            {
                Type.List => _list ?? EmptyList,
                Type.Switch => new MetaEntry[] {_switch},
                Type.Text => string.IsNullOrEmpty(_text) ? EmptyList : new MetaEntry[] {_text ?? ""},
                _ => throw new PrexoniteException("Unknown type in meta entry")
            };
        }
    }

    public bool Switch
    {
        [DebuggerNonUserCode]
        get
        {
            return EntryType switch
            {
                Type.Text => bool.TryParse(_text, out var sw) && sw,
                Type.Switch => _switch,
                Type.List => List.Length > 0,
                _ => throw new PrexoniteException("Unknown type in meta entry")
            };
        }
    }

    #endregion

    #region Construction

    [DebuggerNonUserCode]
    public MetaEntry(string text)
    {
        EntryType = Type.Text;
        _list = null;
        _switch = false;
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    [DebuggerNonUserCode]
    public MetaEntry(MetaEntry[] list)
    {
        //Check sanity
        if (list == null)
            throw new ArgumentNullException(nameof(list));
        if (list.Any(entry => entry == null))
        {
            throw new ArgumentException(
                "A MetaEntry list must not contain null references.", nameof(list));
        }
        EntryType = Type.List;
        _text = null;
        _switch = false;
        _list = list;
    }

    [PublicAPI]
    [DebuggerNonUserCode]
    public MetaEntry(bool @switch)
    {
        EntryType = Type.Switch;
        _text = null;
        _list = null;
        _switch = @switch;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Indicates the kind of meta entry
    /// </summary>
    public Type EntryType { get; }

    public bool IsText => EntryType == Type.Text;

    public bool IsList => EntryType == Type.List;

    public bool IsSwitch => EntryType == Type.Switch;

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
        return new(item);
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
        switch (item.EntryType)
        {
            case Type.Text:
                return PType.String.CreatePValue(item._text);
            case Type.Switch:
                return PType.Bool.CreatePValue(item._switch);
            case Type.List:
                List<PValue> lst;
                if (item._list != null)
                {
                    lst = new List<PValue>(item._list.Length);
                    foreach (var entry in item._list)
                        lst.Add(entry);
                }
                else
                {
                    lst = new List<PValue>(0);
                }
                return PType.List.CreatePValue(lst);
            default:
                throw new PrexoniteException(
                    "Meta entry type " + item.EntryType + " is not supported.");
        }
    }

    public static bool operator ==(MetaEntry? a, MetaEntry? b)
    {
        if (ReferenceEquals(a,null) && ReferenceEquals(b,null))
            return true;
        else if (ReferenceEquals(a,null) || ReferenceEquals(b,null))
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

    public static bool operator !=(MetaEntry? a, MetaEntry? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;

        var entry = obj as MetaEntry;
        return entry != null && this == entry;
    }

    public override int GetHashCode()
    {
        return EntryType switch
        {
            Type.List => _list?.GetHashCode() ?? 0,
            Type.Switch => _switch.GetHashCode(),
            Type.Text => _text?.GetHashCode() ?? 0,
            _ => -1
        } ^ (EntryType.GetHashCode() + 23);
    }

    #endregion

    #region Modification

    [PublicAPI]
    public MetaEntry AddToList(params MetaEntry[] newEntries)
    {
        var list = _asList();
        var newList = new MetaEntry[list.Length + newEntries.Length];
        Array.Copy(list, newList, list.Length);
        Array.Copy(newEntries, 0, newList, list.Length, newEntries.Length);
        return (MetaEntry) newList;
    }

    MetaEntry[] _asList()
    {
        //Change type to list
        return EntryType switch
        {
            Type.Switch => new MetaEntry[] {_switch},
            Type.Text => string.IsNullOrEmpty(_text) ? EmptyList : new MetaEntry[] {_text},
            Type.List => _list ?? EmptyList,
            _ => throw new PrexoniteException("Invalid meta entry.")
        };
    }

    [PublicAPI]
    public MetaEntry RemoveFromList(int index)
    {
        return RemoveFromList(index, 1);
    }

    [PublicAPI]
    public MetaEntry RemoveFromList(int index, int length)
    {
        MetaEntry[] list = _asList();
        if (index + length > list.Length - 1 || index < 0 || length < 0)
            throw new ArgumentOutOfRangeException(
                nameof(index),
                $"The supplied index and length {index} are out of the range of 0..{list.Length - 1}.");
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

    [PublicAPI]
    public static MetaEntry[] CreateArray(StackContext sctx, List<PValue> elements)
    {
        var proto = new List<MetaEntry>(elements.Count);
        foreach (var pv in elements)
        {
            if (pv.TryConvertTo(sctx, typeof (MetaEntry), out var pvEntry))
                proto.Add((MetaEntry) pvEntry.Value);
            else switch (pv.Type)
            {
                case ListPType _:
                    proto.Add((MetaEntry) CreateArray(sctx, (List<PValue>) pv.Value));
                    break;
                case BoolPType _:
                    proto.Add((bool) pv.Value);
                    break;
                default:
                    proto.Add(pv.CallToString(sctx));
                    break;
            }
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
        switch (EntryType)
        {
            case Type.List when _list == null:
                buffer.Append("{}");
                break;
            case Type.List:
                buffer.Append('{');
                foreach (var entry in _list)
                {
                    if (entry == null)
                        continue;
                    entry.ToString(buffer);
                    buffer.Append(',');
                }
                if (_list.Length > 0)
                    buffer.Remove(buffer.Length - 1, 1);
                buffer.Append('}');
                break;
            case Type.Switch:
                buffer.Append(_switch.ToString(CultureInfo.InvariantCulture));
                break;
            case Type.Text when _text == null:
                buffer.Append("\"\"");
                break;
            case Type.Text:
                //Special case: allow integer numbers
                if (_text.Length <= LengthOfInt32MaxValue && _looksLikeNumberOrVersion(_text))
                {
                    if (long.TryParse(_text, out var num))
                    {
                        var format = NumberFormatInfo.InvariantInfo;
                        var numStr = num.ToString(format);
                        Debug.Assert(_looksLikeNumberOrVersion(numStr));
                        buffer.Append(numStr);
                        break;
                    }
                    else if (Version.TryParse(_text, out var version))
                    {
                        buffer.Append(version);
                        break;
                    }
                }

                if (_text.Contains('.'))
                {
                    var first = true;
                    foreach (var part in _text.Split('.'))
                    {
                        if (!first)
                        {
                            buffer.Append('.');
                        }
                        first = false;

                        var idOrLiteral = StringPType.ToIdOrLiteral(part);
                        if (idOrLiteral.StartsWith("\""))
                        {
                            buffer.Append('$');
                        }

                        buffer.Append(idOrLiteral);
                    }
                }
                else
                {
                    buffer.Append(StringPType.ToIdOrLiteral(_text));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.MetaEntry_EntryTypeUnknownToString, EntryType));
        }
    }

    const int LengthOfInt32MaxValue = 10 + 1; //sign allowed

    static bool _looksLikeNumberOrVersion(string text)
    {
        var end = Math.Min(text.Length, LengthOfInt32MaxValue);
        var remainingDotsAllowed = 4;
        for (var i = 0; i < end; i++)
        {
            if (remainingDotsAllowed > 0 && text[i] == '.')
            {
                remainingDotsAllowed -= 1;
                continue;
            }
            if (!char.IsDigit(text[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Returns the default meta entry.
    /// </summary>
    /// <returns>The default meta entry.</returns>
    public static MetaEntry CreateDefaultEntry()
    {
        return new("");
    }
}