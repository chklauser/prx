using System;

namespace Prexonite.Modular
{
    /// <summary>
    /// A (potentially) inexact pattern (most likely a range) of module names.
    /// </summary>
    public abstract class ModuleNamePattern
    {
        /// <summary>
        /// Initializes a ModuleNamePattern.
        /// </summary>
        /// <remarks>This constructor is internal because Prexonite requires
        /// control over all ModuleNamePattern case classes.</remarks>
        internal ModuleNamePattern()
        {
        }

        public static ModuleNamePattern Exact(ModuleName name)
        {
            return new ExactPattern(name);
        }

        public abstract string Id { get; }

        public static ModuleNamePattern Prefix(ModuleName prototype, int components)
        {
            if (prototype == null)
                throw new ArgumentNullException("prototype");
            if (components < 0 || 4 < components)
                throw new ArgumentOutOfRangeException("components",
                    "Version numbers prefixes must be "
                    +
                    "at least 0 and at most 4 components in length. Supplied number of components: "
                    + components);

            if(components == 4)
                return new ExactPattern(prototype);
            else
                return new PrefixPattern(prototype, components);
        }

        public static ModuleNamePattern Prefix(string name, params int[] prefix)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (prefix == null)
                throw new ArgumentNullException("prefix");

            
                ModuleName prototype;
            switch (prefix.Length)
            {
                case 0:
                    prototype = new ModuleName(name,
                        new Version(0,0));
                    break;
                case 1:
                    prototype = new ModuleName(name,
                        new Version(prefix[0], 0));
                    break;
                case 2:
                    prototype = new ModuleName(name,
                        new Version(prefix[0], prefix[1]));
                    break;
                case 3:
                    prototype = new ModuleName(name,
                        new Version(prefix[0], prefix[1], prefix[2]));
                    break;
                case 4:
                    prototype = new ModuleName(name,
                        new Version(prefix[0], prefix[1], prefix[2], prefix[3]));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("prefix",
                    "Invalid number of version number components for module name prefix pattern. " + 
                    "Expected 0-4 components, actual "
                    + prefix.Length + " components.");
            }

            return Prefix(prototype, prefix.Length);
        }

        /// <summary>
        /// Indicates whether this pattern matches a subset of the names matched by the other pattern.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>True if this pattern matches the same set of names as <paramref name="other"/> or a subset thereof; false otherwise.</returns>
        public abstract bool IsSubsetOf(ModuleNamePattern other);

        public bool IsSupersetOf(ModuleNamePattern other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            return other.IsSubsetOf(this);
        }

        public virtual bool IsEquivalentTo(ModuleNamePattern other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            return IsSubsetOf(other) && other.IsSubsetOf(this);
        }

        /// <summary>
        /// Indicates whether the supplied module name satisfies this pattern.
        /// </summary>
        /// <param name="name">The module name to check against.</param>
        /// <returns>True if the supplied module name satisfies this pattern; false otherwise.</returns>
        public abstract bool SatisfiedBy(ModuleName name);

        internal abstract bool _IsSupersetOfPrefix(PrefixPattern other);

        public static ModuleNamePattern Conjunction(ModuleNamePattern left, ModuleNamePattern right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            if (left.IsSubsetOf(right))
                return left;
            else if (right.IsSubsetOf(left))
                return right;
            else
                throw new NotSupportedException("");
        }


    }

    internal class ExactPattern : ModuleNamePattern
    {
        private readonly ModuleName _name;

        public override string Id
        {
            get { return _name.Id; }
        }

        internal ExactPattern(ModuleName name)
        {
            _name = name;
        }

        public override bool SatisfiedBy(ModuleName name)
        {
            return name == _name;
        }

        internal override bool _IsSupersetOfPrefix(PrefixPattern other)
        {
            return other.Components == 4 && SatisfiedBy(other.Prototype);
        }

        public override bool IsSubsetOf(ModuleNamePattern other)
        {
            return other.SatisfiedBy(_name);
        }
    }

    internal class PrefixPattern : ModuleNamePattern
    {
        private readonly ModuleName _prototype;
        private readonly int _components;


        public override string Id
        {
            get { return Prototype.Id; }
        }

        public PrefixPattern(ModuleName prototype, int components)
        {
            _prototype = prototype;
            _components = components;
        }

        public int Components
        {
            get { return _components; }
        }

        public ModuleName Prototype
        {
            get { return _prototype; }
        }

        public override bool IsSubsetOf(ModuleNamePattern other)
        {
            return other._IsSupersetOfPrefix(this);
        }

        public override bool SatisfiedBy(ModuleName name)
        {
            if(!Engine.StringsAreEqual(name.Id,Prototype.Id))
                return false;
            var otherVersion = name.Version;
            var thisVersion = Prototype.Version;
            switch (Components)
            {
                case 0:
                    return true;
                case 1:
                    if (otherVersion.Major != thisVersion.Major)
                        return false;
                    else
                        goto case 0;
                case 2:
                    if (otherVersion.Minor != thisVersion.Minor)
                        return false;
                    else
                        goto case 1;
                case 3:
                    if (thisVersion.Build != otherVersion.Build)
                        return false;
                    else
                        goto case 2;
                case 4:
                    if (thisVersion.Revision != otherVersion.Revision)
                        return false;
                    else
                        goto case 3;
                default:
                    throw new InvalidOperationException(
                        "Number of components of a perfix module name pattern should be in 1-4.");
            }
        }

        internal override bool _IsSupersetOfPrefix(PrefixPattern other)
        {
            return other.Components >= Components && SatisfiedBy(other.Prototype);
        }
    }
}