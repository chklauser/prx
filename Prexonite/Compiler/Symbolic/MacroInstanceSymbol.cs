using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    public sealed class MacroInstanceSymbol : Symbol
    {
        private readonly EntityRef.IMacro _macroReference;
        private readonly ReadOnlyDictionaryView<string,AstExpr> _arguments;

        internal MacroInstanceSymbol(EntityRef.IMacro macroReference, IEnumerable<KeyValuePair<string, AstExpr>> arguments)
        {
            _macroReference = macroReference;
            var d = new Dictionary<string, AstExpr>();
            foreach (var entry in arguments)
                d[entry.Key] = entry.Value;
            _arguments = new ReadOnlyDictionaryView<string, AstExpr>(d);
        }

        public static MacroInstanceSymbol Create<T>(T macroEntity, IEnumerable<KeyValuePair<string, AstExpr>> arguments) where T : EntityRef, EntityRef.IMacro
        {
            return new MacroInstanceSymbol(macroEntity, arguments);
        }

        public EntityRef.IMacro MacroReference
        {
            get { return _macroReference; }
        }

        public ReadOnlyDictionaryView<string, AstExpr> Arguments
        {
            get { return _arguments; }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleMacroInstance(this, argument);
        }

        public override bool TryGetMacroInstanceSymbol(out MacroInstanceSymbol macroInstanceSymbol)
        {
            macroInstanceSymbol = this;
            return true;
        }
    }
}