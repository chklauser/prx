using System;
using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using Prexonite.Helper;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// Provides macros with access to Prexonite and compiler internals.
    /// </summary>
    public class MacroContext
    {
        #region Representation
        
        private readonly AstMacroInvocation _invocation;
        
        private readonly CompilerTarget _target;
        private readonly bool _isJustEffect;
        private readonly MacroSession _session;

        #endregion

        /// <summary>
        /// Creates a new macro context in the specified macro session.
        /// </summary>
        /// <param name="session">The macro expansion session.</param>
        /// <param name="invocation">The node that is being expanded.</param>
        /// <param name="isJustEffect">Whether the nodes return value will be discarded by the surrounding program.</param>
        public MacroContext(MacroSession session, AstMacroInvocation invocation, bool isJustEffect)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (invocation == null)
                throw new ArgumentNullException("invocation");
            _target = session.Target;
            _isJustEffect = isJustEffect;
            _invocation = invocation;
            _session = session;
        }

        #region Accessors 

        /// <summary>
        /// The node that is being expanded.
        /// </summary>
        public AstMacroInvocation Invocation
        {
            get { return _invocation; }
        }

        /// <summary>
        /// Determines whether the macro is used as a get or as a set call.
        /// </summary>
        public PCall Call
        {
            get { return Invocation.Call; }
        }

        /// <summary>
        /// Indicates whether the macros return value should produce a return value.
        /// </summary>
        public bool IsJustEffect
        {
            get { return _isJustEffect; }
        }

        /// <summary>
        /// Provides access to a copy of the loader options currently in effect.
        /// </summary>
        public LoaderOptions LoaderOptions
        {
            get { return _session.LoaderOptions; }
        }

        /// <summary>
        /// Returns the loop block (reacting to break, continue) that is currently active. Can be null.
        /// </summary>
        public ILoopBlock CurrentLoopBlock
        {
            get { return _session.CurrentLoopBlock; }
        }

        /// <summary>
        /// Provides read-only access to the local symbol table.
        /// </summary>
        public ReadOnlyDictionaryView<string, SymbolEntry> LocalSymbols
        {
            get { return _session.LocalSymbols; }
        }

        /// <summary>
        /// Provides read-only access to the global symbol table.
        /// </summary>
        public ReadOnlyDictionaryView<string, SymbolEntry> GlobalSymbols
        {
            get { return _session.GlobalSymbols; }
        }

        public ReadOnlyCollectionView<string> OuterVariables
        {
            get { return _session.OuterVariables; }
        }

        /// <summary>
        /// A reference to the application being compiled.
        /// </summary>
        public Application Application
        {
            get { return _target.Loader.ParentApplication; }
        }

        /// <summary>
        /// A reference to the function being compiled.
        /// </summary>
        public PFunction Function
        {
            get { return _target.Function; }
        }

        /// <summary>
        /// Returns the functions direct parent (outer) function.
        /// </summary>
        public IEnumerable<PFunction> GetParentFunctions()
        {
            var target = _target.ParentTarget;
            while (target != null)
            {
                yield return target.Function;
                target = target.ParentTarget;
            }
        }

        #endregion

        #region Compiler interaction

        /// <summary>
        /// Allocates a temporary variable for this macro expansion session.
        /// </summary>
        /// <returns>The (physical) id of a free temporary variable.</returns>
        /// <remarks>If a temporary variable is not freed during a macro expansion session, 
        /// it will no longer be considered a temporary variable and cannot be freed in 
        /// subsequent expansions</remarks>
        public string AllocateTemporaryVariable()
        {
            return _session.AllocateTemporaryVariable();
        }

        /// <summary>
        /// Marks the temporary variable for freeing at the end of this expansion session.
        /// </summary>
        /// <param name="temporaryVariable">The temporary variable to be freed.</param>
        public void FreeTemporaryVariable(string temporaryVariable)
        {
            _session.FreeTemporaryVariable(temporaryVariable);
        }

        /// <summary>
        /// Reports a compiler message (error, warning, info).
        /// </summary>
        /// <param name="severity">The message severity (error, warning, info)</param>
        /// <param name="message">The actual message (human-readable)</param>
        /// <param name="position">The location in the code associated with the message.</param>
        /// <remarks>Issuing an error message does not automatically abort execution of the macro.</remarks>
        public void ReportMessage(ParseMessageSeverity severity, string message, ISourcePosition position = null)
        {
            position = position ?? _invocation;
            _target.Loader.ReportMessage(new ParseMessage(severity,message,position));
        }

        /// <summary>
        /// Requires that a variable from an outer function scope is shared with the function currently compiled.
        /// </summary>
        /// <param name="variable">The physical name of the variable to request.</param>
        public void RequireOuterVariable(string variable)
        {
            _target.RequireOuterVariable(variable);
        }

        #endregion
    }
}