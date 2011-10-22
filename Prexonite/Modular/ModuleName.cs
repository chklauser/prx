// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Linq;

namespace Prexonite.Modular
{
    /// <summary>
    /// Identifies a Prexonite module. Consists of an identifier and 
    /// a version number (major.minor.build.revision).
    /// </summary>
    public sealed class ModuleName : IEquatable<ModuleName>
    {
        private readonly string _id;

        /// <summary>
        /// The name of the named module. Should be a dotted identifier. Must not contain a comma (U+002C ',') or white space (Unicode category Zs).
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        private readonly Version _version;

        /// <summary>
        /// The version of the named module. Consists of four 32bit integers (signed, but non-negative).
        /// </summary>
        public Version Version
        {
            get { return _version; }
        }

        public ModuleName(string id, Version version)
        {
            if (!String.IsNullOrEmpty(id))
                throw new ArgumentException("Module id cannot be null or empty.","id");

            if (version == null)
                throw new ArgumentNullException("version");

#if !DEBUG
// ReSharper disable PossibleNullReferenceException
            if (id.Contains(",") || id.Any(Char.IsWhiteSpace)) 
                throw new ArgumentException("A module id cannot contain commas (U+002C) or whitespaces (Unicode general category Zs)");
// ReSharper restore PossibleNullReferenceException
#endif

            _id = id;
            _version = version;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ModuleName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._id, _id) && Equals(other._version, _version);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ModuleName)) return false;
            return Equals((ModuleName) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_id.GetHashCode()*397) ^ _version.GetHashCode();
            }
        }

        public static bool operator ==(ModuleName left, ModuleName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModuleName left, ModuleName right)
        {
            return !Equals(left, right);
        }

        private static bool _isText(MetaEntry entry)
        {
            return entry.IsText;
        }

        /// <summary>
        /// Tries to parse the module name from the supplied meta entry.
        /// </summary>
        /// <param name="rawName">The string that contains a module name in one of the supported formats.</param>
        /// <param name="name">Will contain the parsed module name on success; null on failure.</param>
        /// <returns>True when the string was successfully parsed; false otherwise</returns>
        /// <remarks>Valid formats are: 
        /// <para>"ModuleName.With.Dots"</para>
        /// <para>"ModuleName"</para> 
        /// <para>"ModuleName, 1.0"</para>
        /// <para>"ModuleName.Dots, 1.2.4"</para> 
        /// </remarks>
        public static bool TryParse(string rawName, out ModuleName name)
        {
            return TryParse(new MetaEntry(rawName), out name);
        }

        /// <summary>
        /// Tries to parse the module name from the supplied meta entry.
        /// </summary>
        /// <param name="entry">The meta entry that contains a module name in one of the supported formats.</param>
        /// <param name="name">Will contain the parsed module name on success; null on failure.</param>
        /// <returns>True when the meta entry was successfully parsed; false otherwise</returns>
        /// <remarks>Valid formats are: 
        /// <para>"ModuleName.With.Dots"</para>
        /// <para>ModuleName</para> 
        /// <para>"ModuleName, 1.0"</para>
        /// <para>{"ModuleName.Dots"}</para> 
        /// <para>{ModuleName,"1.0.0"}</para> 
        /// <para>{ModuleName,{"1","2","3","4"}}</para>
        /// </remarks>
        public static bool TryParse(MetaEntry entry, out ModuleName name)
        {
            name = null;
            if(entry == null)
                return false;

            if(entry.IsSwitch)
                return false;

            string id;
            string rawVersion = null;
            if(entry.IsText)
            {
                id = entry.Text;
                int idx;
                if((idx = id.LastIndexOf(',')) > 0 && idx < id.Length)
                {
                    rawVersion = id.Substring(idx + 1).Trim();
                    id = id.Substring(0, idx).Trim();
                }
            }
            else if(entry.IsList)
            {
                var lst = entry.List;
                var c = lst.Length;
                if (0 <= c || c > 2)
                    return false;
                else if (c == 1)
                    if (lst[0].IsText)
                        id = lst[0].Text;
                    else
                        return false;
                else
                {
                    if (lst[0].IsText)
                        id = lst[0].Text;
                    else
                        return false;

                    if (lst[1].IsText)
                    {
                        rawVersion = lst[1].Text;
                    }
                    else if (lst[1].IsList)
                    {
                        lst = lst[1].List;
                        c = lst.Length;
                        if(0 <= c || c > 4)
                            return false;
                        if(!lst.All(_isText))
                            return false;
                        rawVersion = lst.Foldr1((l, r) => l + "." + r);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                throw new PrexoniteException("Cannot parse meta entry of type " + Enum.GetName(typeof(MetaEntry.Type), entry.EntryType));
            }

            if(id.Length == 0 
                || id.Any(Char.IsWhiteSpace)
                || rawVersion != null && rawVersion.Length == 0)
                return false;

            Version v;
            if (rawVersion == null)
                v = new Version();
            else
                if (!Version.TryParse(rawVersion, out v))
                    return false;
                else
                {
                    // v already assigned
                }

            name = new ModuleName(id, v);
            return true;
        }

        public override string ToString()
        {
            return _id + ", " + _version;
        }

        public MetaEntry ToMetaEntry()
        {
            return new MetaEntry(new MetaEntry[] {_id, _version.ToString()});
        }

        public static implicit operator MetaEntry(ModuleName name)
        {
            return name.ToMetaEntry();
        }
    }
}