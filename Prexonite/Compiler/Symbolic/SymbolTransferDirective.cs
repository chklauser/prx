using System;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolTransferDirective : IEquatable<SymbolTransferDirective>
    {

        [NotNull]
        private readonly ISourcePosition _position;

        private SymbolTransferDirective([NotNull] ISourcePosition position)
        {
            if (position == null)
                throw new ArgumentNullException("position");
            
            _position = position;
        }

        [NotNull]
        public ISourcePosition Position
        {
            get { return _position; }
        }

        public abstract T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop);
        public abstract bool Equals(SymbolTransferDirective other);
        public abstract override string ToString();
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (other.GetType() != GetType())
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals((SymbolTransferDirective) other);
        }
        public abstract override int GetHashCode();

        public static Func<SymbolTransferDirective, T> Matching<T>(Func<T> onWildcard, Func<Rename, T> onRename,
            Func<Drop, T> onDrop)
        {
            return d => d.Match(onWildcard, onRename, onDrop);
        }

        public static Action<SymbolTransferDirective> Matching(Action onWildcard, Action<Rename> onRename,
            Action<Drop> onDrop)
        {
            return d => d.Match(onWildcard, onRename, onDrop);
        }

        public void Match(Action onWildcard, Action<Rename> onRename, Action<Drop> onDrop)
        {
            Match<object>(
                () =>
                    {
                        onWildcard();
                        return null;
                    }, r =>
                        {
                            onRename(r);
                            return null;
                        }, d =>
                            {
                                onDrop(d);
                                return null;
                            });
        }

        public static Wildcard CreateWildcard([NotNull] ISourcePosition position)
        {
            return new Wildcard(position);
        }

        public static Rename CreateRename([NotNull] ISourcePosition position, [NotNull] string originalName, [NotNull] string newName)
        {
            return new Rename(position,originalName,newName);
        }

        public static Drop CreateDrop([NotNull] ISourcePosition position, [NotNull] string name)
        {
            return new Drop(position, name);
        }

        public sealed class Wildcard : SymbolTransferDirective
        {
            public Wildcard([NotNull] ISourcePosition position) : base(position)
            {
            }

            public override T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop)
            {
                return onWildcard();
            }

            public override bool Equals(SymbolTransferDirective other)
            {
                var otherWildcard = other as Wildcard;
                return otherWildcard != null;
            }

            public override string ToString()
            {
                return "*";
            }

            public override int GetHashCode()
            {
                return 71;
            }
        }

        public sealed class Rename : SymbolTransferDirective
        {
            [NotNull]
            private readonly string _originalName;

            [NotNull]
            private readonly string _newName;

            public Rename([NotNull] ISourcePosition position, [NotNull] string originalName, [NotNull] string newName) : base(position)
            {
                if (newName == null)
                    throw new ArgumentNullException("newName");
                if (originalName == null)
                    throw new ArgumentNullException("originalName");
                
                _originalName = originalName;
                _newName = newName;
            }

            [NotNull]
            public string OriginalName
            {
                get { return _originalName; }
            }

            [NotNull]
            public string NewName
            {
                get { return _newName; }
            }

            public override T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop)
            {
                return onRename(this);
            }

            public override bool Equals(SymbolTransferDirective other)
            {
                var otherRename = other as Rename;
                return otherRename != null &&
                    (OriginalName.Equals(otherRename.OriginalName) && NewName.Equals(otherRename.NewName));
            }

            public override string ToString()
            {
                return string.Format("{0} as {1}", OriginalName, NewName);
            }

            public override int GetHashCode()
            {
                return unchecked(OriginalName.GetHashCode() ^ (NewName.GetHashCode()*331));
            }
        }

        public sealed class Drop : SymbolTransferDirective
        {
            [NotNull]
            private readonly string _name;

            public Drop([NotNull] ISourcePosition position, [NotNull] string name) : base(position)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                
                _name = name;
            }

            public string Name
            {
                get { return _name; }
            }

            public override T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop)
            {
                return onDrop(this);
            }

            public override bool Equals(SymbolTransferDirective other)
            {
                var otherDrop = other as Drop;
                return otherDrop != null && _name.Equals(otherDrop.Name);
            }

            public override string ToString()
            {
                return string.Format("not {0}", Name);
            }

            public override int GetHashCode()
            {
                return _name.GetHashCode() ^ 331;
            }
        }
    }
}