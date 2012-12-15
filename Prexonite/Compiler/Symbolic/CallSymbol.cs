using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("call({Entity})")]
    public sealed class CallSymbol : Symbol
    {

        [NotNull]
        private readonly ISourcePosition _position;

        [NotNull]
        public override ISourcePosition Position
        {
            get { return _position; }
        }

        public override bool Equals(Symbol other)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        private CallSymbol()
        {
           
        }

        #region Overrides of Symbol

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}