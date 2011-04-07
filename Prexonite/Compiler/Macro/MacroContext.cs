using System;
using Prexonite.Compiler.Ast;
using Prexonite.Helper;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// 
    /// </summary>
    public class MacroContext
    {
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _globalSymbols;
        private readonly AstMacroInvocation _invocation;
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _localSymbols;
        private readonly CompilerTarget _target;
        private LoaderOptions _options;
        private readonly bool _isJustEffect;
        private readonly MacroContext _parentContext;

        public MacroContext(CompilerTarget target, AstMacroInvocation invocation, bool isJustEffect)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (invocation == null)
                throw new ArgumentNullException("invocation");
            _target = target;
            _isJustEffect = isJustEffect;
            _invocation = invocation;
            _localSymbols = new ReadOnlyDictionaryView<string, SymbolEntry>(_target.Symbols);
            _globalSymbols = new ReadOnlyDictionaryView<string, SymbolEntry>(_target.Loader.Symbols);
            _parentContext = null;
        }

        private MacroContext(MacroContext parentContext, AstMacroInvocation invocation, bool isJustEffect)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (invocation == null)
                throw new ArgumentNullException("invocation");
            _target = parentContext._target;
            _localSymbols = parentContext._localSymbols;
            _globalSymbols = parentContext._globalSymbols;
            _parentContext = parentContext;
        }

        #region Accessors 

        public AstMacroInvocation Invocation
        {
            get { return _invocation; }
        }

        public PCall Call
        {
            get { return Invocation.Call; }
        }

        public bool IsJustEffect
        {
            get { return _isJustEffect; }
        }

        public LoaderOptions LoaderOptions
        {
            get
            {
                if (_options == null)
                {
                    _options = new LoaderOptions(_target.Loader.ParentEngine, _target.Loader.ParentApplication);
                    _options.InheritFrom(_target.Loader.Options);
                }
                return _target.Loader.Options;
            }
        }

        public ILoopBlock CurrentLoopBlock
        {
            get { return _target.CurrentLoopBlock; }
        }

        public ReadOnlyDictionaryView<string, SymbolEntry> LocalSymbols
        {
            get { return _localSymbols; }
        }

        public ReadOnlyDictionaryView<string, SymbolEntry> GlobalSymbols
        {
            get { return _globalSymbols; }
        }

        public Application Application
        {
            get { return _target.Loader.ParentApplication; }
        }

        public PFunction Function
        {
            get { return _target.Function; }
        }

        #endregion

        #region Compiler interaction

        public string AllocateTemporaryVariable()
        {
            throw new NotImplementedException();
        }

        public void FreeTemporaryVariable(string temporaryVariable)
        {
            throw new NotImplementedException();
        }

        public void ReportMessage(ParseMessage message)
        {
            _target.Loader.ReportMessage(message);
        }

        public void RequireOuterVariable(string variable)
        {
            _target.RequireOuterVariable(variable);
        }

        #endregion
    }
}