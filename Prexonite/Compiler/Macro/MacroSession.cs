using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Helper;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    /// Provides information associated with a macro expansion session.
    /// </summary>
    public class MacroSession : IDisposable
    {
        private readonly CompilerTarget _target;
        private LoaderOptions _options;
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _globalSymbols;
        private readonly ReadOnlyDictionaryView<string, SymbolEntry> _localSymbols;
        private readonly ReadOnlyCollectionView<string> _outerVariables;
        private readonly SymbolCollection _releaseList = new SymbolCollection();
        private readonly SymbolCollection _allocationList = new SymbolCollection();
        private readonly HashSet<AstMacroInvocation> _invocations = new HashSet<AstMacroInvocation>();

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
            _outerVariables = new ReadOnlyCollectionView<string>(_target.OuterVariables);
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

        public ReadOnlyCollectionView<string> OuterVariables
        {
            get { return _outerVariables; }
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
            var temp = _target.RequestTemporaryVariable();
            _allocationList.Add(temp);
            return temp;
        }

        /// <summary>
        /// Marks the temporary variable for freeing at the end of this expansion session.
        /// </summary>
        /// <param name="temporaryVariable">The temporary variable to be freed.</param>
        public void FreeTemporaryVariable(string temporaryVariable)
        {
            if(_releaseList.Contains(temporaryVariable))
                throw new PrexoniteException("Cannot release temporary variable " + temporaryVariable + " twice!");
            _releaseList.Add(temporaryVariable);
        }

        /// <summary>
        /// Releases managed resources bound by the macro expansion session, such as temporary variables.
        /// </summary>
        public void Dispose()
        {
            //Free all variables that were marked as free during the session.
            foreach (var temp in _releaseList)
            {
                _target.ReleaseTemporaryVariable(temp);
                _allocationList.Remove(temp);
            }

            //Remove those that weren't freed to persistent variables
            foreach (var temp in _allocationList)
            {
                _target.PromoteTemporaryVariable(temp);
            }
        }


        public void ExpandMacro(AstMacroInvocation invocation, bool justEffect)
        {
            var target = Target;
            var context = new MacroContext(this, invocation, justEffect);

            if (_invocations.Contains(invocation))
                throw new PrexoniteException(
                    "AstMacroInvocation.EmitCode is not reentrant. Use GetCopy() to operate on a copy of this macro invocation.");
            _invocations.Add(invocation);

            PFunction macroFunc;
            if (!target.Loader.Options.TargetApplication.Functions.TryGetValue(invocation.MacroId, out macroFunc))
                throw new PrexoniteException(
                    String.Format(
                        "The macro function {0} was called from function {1} but is not available at compile time.", invocation.MacroId,
                        target.Function.Id));

            string id;
            MetaEntry logicalIdEntry;
            if (macroFunc.Meta.TryGetValue(PFunction.LogicalIdKey, out logicalIdEntry))
                id = logicalIdEntry.Text;
            else
                id = invocation.MacroId;

            //check if this macro is a partial application (illegal)
            if (invocation.Arguments.Any(AstPartiallyApplicable.IsPlaceholder))
            {
                target.Loader.ReportSemanticError(invocation.Line, invocation.Column, "The macro " + id + " cannot be applied partially.");
                var ind =  new AstIndirectCall(invocation.File, invocation.Line, invocation.Column, new AstNull(invocation.File, invocation.Line, invocation.Column));
                if (justEffect)
                    ind.EmitEffectCode(target);
                else
                    ind.EmitCode(target);
                return;
            }

            var astRaw = _invokeMacroFunction(context, macroFunc, invocation.Arguments);

            //Optimize and then emit returned code.
            AstNode ast;
            if (astRaw == null || (ast = astRaw.Value as AstNode) == null)
            {
                //If a value was expected, we need to at least make up null, otherwise
                //  we risk stack corruption.
                if (!justEffect)
                    target.Emit(invocation, OpCode.ldc_null);
                return;
            }

            var expr = ast as IAstExpression;
            if (expr != null)
            {
                AstNode._OptimizeNode(target, ref expr);
                ast = (AstNode) expr;
            }

            var effect = ast as IAstEffect;
            if (effect != null && justEffect)
                effect.EmitEffectCode(target);
            else
                ast.EmitCode(target);
        }

        private PValue _invokeMacroFunction(MacroContext context, PFunction macroFunc, IEnumerable<IAstExpression> argumentNodes)
        {
            var env = new SymbolTable<PVariable>(1);
            env.Add(MacroAliases.ContextAlias,
                    CompilerTarget.CreateReadonlyVariable(Target.Loader.CreateNativePValue(context)));

            var sharedVariables =
                macroFunc.Meta[PFunction.SharedNamesKey].List.Select(entry => env[entry.Text]).
                    ToArray();
            var macro = new Closure(macroFunc, sharedVariables);

            //Execute macro (argument nodes of the invocation node are passed as arguments to the macro)
            var arguments = argumentNodes.Select(Target.Loader.CreateNativePValue).ToArray();
            var parentApplication = macroFunc.ParentApplication;
            try
            {
                parentApplication._SuppressInitialization = true;
                return macro.IndirectCall(Target.Loader, arguments);
            }
            finally
            {
                parentApplication._SuppressInitialization = false;
            }
        }
    }
}