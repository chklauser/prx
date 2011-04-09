using System;
using System.Diagnostics;
using Prexonite.Helper;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// Provides information associated with a macro expansion session.
    /// </summary>
    public class MacroSession
    {
        private readonly CompilerTarget _target;
        private LoaderOptions _options;
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _globalSymbols;
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _localSymbols;

        /// <summary>
        /// Creates a new macro expansion session for the specified compiler target.
        /// </summary>
        /// <param name="target">The target to expand macros in.</param>
        public MacroSession(CompilerTarget target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            _target = target;
            _localSymbols = new ReadOnlyDictionaryView<string, SymbolEntry>(_target.Symbols);
            _globalSymbols = new ReadOnlyDictionaryView<string, SymbolEntry>(_target.Loader.Symbols);
        }

        /// <summary>
        /// Provides read-only access to the local symbol table.
        /// </summary>
        public ReadOnlyDictionaryView<string, SymbolEntry> LocalSymbols
        {
            get { return _localSymbols; }
        }

        /// <summary>
        /// Provides read-only access to the global symbol table.
        /// </summary>
        public ReadOnlyDictionaryView<string, SymbolEntry> GlobalSymbols
        {
            get { return _globalSymbols; }
        }

        /// <summary>
        /// The target the macro expansion session covers.
        /// </summary>
        public CompilerTarget Target
        {
            [DebuggerStepThrough]
            get { return _target; }
        }

        /// <summary>
        /// A copy of the loader options in effect during this macro expansion.
        /// </summary>
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

        /// <summary>
        /// Allocates a temporary variable for this macro expansion session.
        /// </summary>
        /// <returns>The (physical) id of a free temporary variable.</returns>
        /// <remarks>If a temporary variable is not freed during a macro expansion session, 
        /// it will no longer be considered a temporary variable and cannot be freed in 
        /// subsequent expansions</remarks>
        public string AllocateTemporaryVariable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Marks the temporary variable for freeing at the end of this expansion session.
        /// </summary>
        /// <param name="temporaryVariable">The temporary variable to be freed.</param>
        public void FreeTemporaryVariable(string temporaryVariable)
        {
            throw new NotImplementedException();
        }
    }
}