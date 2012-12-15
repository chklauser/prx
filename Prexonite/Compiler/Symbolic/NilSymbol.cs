using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("Nil")]
    public sealed class NilSymbol : Symbol, IEquatable<NilSymbol>
    {
        #region Overrides of Symbol

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleNil(this,argument);
        }

        public override bool TryGetNilSymbol(out NilSymbol nilSymbol)
        {
            nilSymbol = this;
            return true;
        }

        public override bool Equals(Symbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is NilSymbol && Equals((NilSymbol)other);
        }

        [NotNull]
        private readonly ISourcePosition _position;

        public override ISourcePosition Position
        {
            get { return _position; }
        }

        #endregion

        private NilSymbol([NotNull] ISourcePosition position)
        {
            _position = position;
        }

        [NotNull]
        internal static NilSymbol _Create([NotNull] ISourcePosition position)
        {
            return new NilSymbol(position);
        }

        public override string ToString()
        {
            return "Nil";
        }

        public bool Equals(NilSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return true;
        }

        private const int NilSymbolHashCode = 384950146;

        public override int GetHashCode()
        {
            return NilSymbolHashCode;
        }
    }
}