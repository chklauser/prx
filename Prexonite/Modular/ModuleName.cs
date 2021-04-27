#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Modular
{
    /// <summary>
    /// Identifies a Prexonite module. Consists of an identifier and 
    /// a version number (major.minor.build.revision).
    /// </summary>
    [DebuggerDisplay("{Id}/{Version}")]
    public sealed class ModuleName : IEquatable<ModuleName>
    {
        /// <summary>
        /// The name of the named module. Should be a dotted identifier. Must not contain a comma (U+002C ',') or white space (Unicode category Zs).
        /// </summary>
        public string Id { get; }

        [PublicAPI]
        public static readonly PType PType = PType.Object[typeof(ModuleName)];
        private static readonly Version ZeroVersion = new();

        /// <summary>
        /// The version of the named module. Consists of four 32bit integers (signed, but non-negative).
        /// </summary>
        public Version Version { get; }

        public ModuleName(string id, Version version)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException(Resources.ModuleName_Module_id_cannot_be_null_or_empty_,nameof(id));

            if (version == null)
                throw new ArgumentNullException(nameof(version));

#if !DEBUG
// ReSharper disable PossibleNullReferenceException
            if (id.Contains("/") || id.Any(char.IsWhiteSpace)) 
                throw new ArgumentException("A module id cannot contain commas (U+002C), slashes (U+002F) or whitespace (Unicode general category Zs)");
// ReSharper restore PossibleNullReferenceException
#endif

            Id = id;
            Version = version;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ModuleName? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Engine.StringsAreEqual(other.Id, Id) && Equals(other.Version, Version);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object? obj)
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
                return (Id.GetHashCode()*397) ^ Version.GetHashCode();
            }
        }

        public static bool operator ==(ModuleName? left, ModuleName? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModuleName? left, ModuleName? right)
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
        /// <para>"Name.With.Dots"</para>
        /// <para>"Name"</para> 
        /// <para>"Name, 1.0"</para>
        /// <para>"Name.Dots, 1.2.4"</para> 
        /// </remarks>
        public static bool TryParse(string rawName, [NotNullWhen(true)] out ModuleName? name)
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
        /// <para>"Name.With.Dots"</para>
        /// <para>Name</para> 
        /// <para>"Name, 1.0"</para>
        /// <para>{"Name.Dots"}</para> 
        /// <para>{Name,"1.0.0"}</para> 
        /// <para>{Name,{"1","2","3","4"}}</para>
        /// </remarks>
        public static bool TryParse(MetaEntry entry, [NotNullWhen(true)] out ModuleName? name)
        {
            name = null;
            if(entry == null)
                return false;

            if(entry.IsSwitch)
                return false;

            string id;
            string? rawVersion = null;
            if(entry.IsText)
            {
                id = entry.Text;
                int idx;
                if((idx = id.LastIndexOf('/')) > 0 && idx < id.Length)
                {
                    rawVersion = id[(idx + 1)..].Trim();
                    id = id[..idx].Trim();
                }
            }
            else if(entry.IsList)
            {
                var lst = entry.List;
                var c = lst.Length;
                switch (c)
                {
                    case <= 0 or > 2:
                        return false;
                    case 1 when lst[0].IsText:
                        id = lst[0].Text;
                        break;
                    case 1:
                        return false;
                    default:
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
                            var c2 = lst.Length;
                            if(c2 is <= 0 or > 4)
                                return false;
                            if(!lst.All(_isText))
                                return false;
                            rawVersion = lst.Foldr1((l, r) => l + "." + r);
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    }
                }
            }
            else
            {
                throw new PrexoniteException("Cannot parse meta entry of type " + Enum.GetName(typeof(MetaEntry.Type), entry.EntryType));
            }

            if(id.Length == 0 
                || id.Any(char.IsWhiteSpace)
                || rawVersion is {Length: 0})
                return false;

            Version? v;
            if (rawVersion == null)
                v = ZeroVersion;
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
            if (Version == ZeroVersion)
            {
                return Id;
            }
            return Id + "/" + Version;
        }

        public MetaEntry ToMetaEntry()
        {
            return new(new MetaEntry[] {Id, Version.ToString()});
        }

        public static implicit operator MetaEntry(ModuleName name)
        {
            return name.ToMetaEntry();
        }
    }
}