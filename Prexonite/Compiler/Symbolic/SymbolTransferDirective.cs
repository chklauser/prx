using System;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class SymbolTransferDirective : IEquatable<SymbolTransferDirective>
    {
        private SymbolTransferDirective([NotNull] ISourcePosition position)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        [NotNull]
        public ISourcePosition Position { get; }

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
            return new(position);
        }

        public static Rename CreateRename([NotNull] ISourcePosition position, [NotNull] string originalName, [NotNull] string newName)
        {
            return new(position,originalName,newName);
        }

        public static Drop CreateDrop([NotNull] ISourcePosition position, [NotNull] string name)
        {
            return new(position, name);
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
                return other is Wildcard;
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
            public Rename([NotNull] ISourcePosition position, [NotNull] string originalName, [NotNull] string newName) : base(position)
            {
                OriginalName = originalName ?? throw new ArgumentNullException(nameof(originalName));
                NewName = newName ?? throw new ArgumentNullException(nameof(newName));
            }

            [NotNull]
            public string OriginalName { get; }

            [NotNull]
            public string NewName { get; }

            public override T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop)
            {
                return onRename(this);
            }

            public override bool Equals(SymbolTransferDirective other)
            {
                return other is Rename otherRename && OriginalName.Equals(otherRename.OriginalName) && NewName.Equals(otherRename.NewName);
            }

            public override string ToString()
            {
                return $"{OriginalName} as {NewName}";
            }

            public override int GetHashCode()
            {
                return unchecked(OriginalName.GetHashCode() ^ (NewName.GetHashCode()*331));
            }
        }

        public sealed class Drop : SymbolTransferDirective
        {
            public Drop([NotNull] ISourcePosition position, [NotNull] string name) : base(position)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }

            [NotNull]
            public string Name { get; }

            public override T Match<T>(Func<T> onWildcard, Func<Rename, T> onRename, Func<Drop, T> onDrop)
            {
                return onDrop(this);
            }

            public override bool Equals(SymbolTransferDirective other)
            {
                return other is Drop otherDrop && Name.Equals(otherDrop.Name);
            }

            public override string ToString()
            {
                return $"not {Name}";
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ 331;
            }
        }
    }
}