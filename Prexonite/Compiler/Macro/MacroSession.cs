using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Helper;
using Prexonite.Types;

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
        private readonly object _buildCommandToken;
        private readonly List<PValue> _transportStore = new List<PValue>();

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

            _buildCommandToken = target.Loader.RequestBuildCommands();
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
            Debug.Assert(!_allocationList.Contains(temp));
            Debug.Assert(!_releaseList.Contains(temp));
            _allocationList.Add(temp);
            return temp;
        }

        /// <summary>
        /// Marks the temporary variable for freeing at the end of this expansion session.
        /// </summary>
        /// <param name="temporaryVariable">The temporary variable to be freed.</param>
        public void FreeTemporaryVariable(string temporaryVariable)
        {
            if (_releaseList.Contains(temporaryVariable))
            {
                throw new PrexoniteException("Cannot release temporary variable " +
                                             temporaryVariable + " twice!");
            }
            _releaseList.Add(temporaryVariable);
        }

        /// <summary>
        /// Releases managed resources bound by the macro expansion session, such as temporary variables.
        /// </summary>
        public void Dispose()
        {
            //Free all variables that were marked as free during the session.
            if (_target != null)
            {
                try
                {
                    if (_releaseList != null)
                        foreach (var temp in _releaseList)
                        {
                            _target.FreeTemporaryVariable(temp);
                            _allocationList.Remove(temp);
                        }

                    //Remove those that weren't freed to persistent variables
                    if (_allocationList != null)
                        foreach (var temp in _allocationList)
                        {
                            _target.PromoteTemporaryVariable(temp);
                        }
                }
                finally
                {
                    var ldr = Target.Loader;
                    if (_buildCommandToken != null && ldr != null)
                        ldr.ReleaseBuildCommands(_buildCommandToken);
                }
            }
        }

        private interface IMacroExpander
        {
            void Initialize(CompilerTarget target, AstMacroInvocation invocation, bool justEffect);
            string HumanId { get; }
            void Expand(CompilerTarget target, MacroContext context);
            bool TryExpandPartially(CompilerTarget target, MacroContext context);
        }

        #region Command Expander

        private class MacroCommandExpander : IMacroExpander
        {
            private MacroCommand _macroCommand;

            public void Initialize(CompilerTarget target, AstMacroInvocation invocation, bool justEffect)
            {
                _macroCommand = null;

                if (target.Loader.MacroCommands.Contains(invocation.MacroId))
                    _macroCommand = target.Loader.MacroCommands[invocation.MacroId];
                else
                {
                    target.Loader.ReportMessage(new ParseMessage(ParseMessageSeverity.Error,
                                                                 String.Format("Cannot find macro command named `{0}`", invocation.MacroId),
                                                                 invocation));
                    HumanId = "cannot_find_macro_command";
                    return;
                }

                HumanId = _macroCommand.Id;
            }

            public string HumanId { get; private set; }

            public void Expand(CompilerTarget target, MacroContext context)
            {
                if(_macroCommand == null)
                    return;

                _macroCommand.Expand(context);
            }

            public bool TryExpandPartially(CompilerTarget target, MacroContext context)
            {
                var pac = _macroCommand as PartialMacroCommand;
                return pac != null && pac.ExpandPartialApplication(context);
            }
        }

        #endregion

        #region Function Expander

        private class MacroFunctionExpander : IMacroExpander
        {
            public void Initialize(CompilerTarget target, AstMacroInvocation invocation, bool justEffect)
            {
                _macroFunction = null;

                PFunction macroFunc;
                if (!target.Loader.Options.TargetApplication.Functions.TryGetValue(invocation.MacroId, out macroFunc))
                {
                    target.Loader.ReportMessage(
                        new ParseMessage(
                            ParseMessageSeverity.Error,
                            String.Format(
                                "The macro function {0} was called from function {1} but is not available at compile time.",
                                invocation.MacroId,
                                target.Function.Id), invocation));
                    HumanId = "could_not_resolve_macro_function";
                    return;
                }
                _macroFunction = macroFunc;

                MetaEntry logicalIdEntry;
                if (macroFunc.Meta.TryGetValue(PFunction.LogicalIdKey, out logicalIdEntry))
                    HumanId = logicalIdEntry.Text;
                else
                    HumanId = invocation.MacroId;
            }

            public string HumanId { get; private set; }
            private PFunction _macroFunction;

            public void Expand(CompilerTarget target, MacroContext context)
            {
                if (_macroFunction == null)
                    return;

                var astRaw = _invokeMacroFunction(target, context);

                //Optimize
                AstNode ast;
                if (astRaw != null)
                    ast = astRaw.Value as AstNode;
                else
                    ast = null;

                var expr = ast as IAstExpression;

                /*Merge with context expression block
                 *  cs = Statements from context
                 *  ce = Expression from context
                 *  fs = Statements from function return value
                 *  fe = Expression from function return value
                 * Rules
                 *  general:
                 *      {cs;ce;fs} = fe
                 *  no-fe:
                 *      {cs;tmp = ce;fs} = tmp
                 *  no-f:
                 *      {cs} = ce
                 *  no-c:
                 *      {fs} = fe
                 */
                var contextBlock = context.Block;
                var macroBlockExpr = ast as AstBlockExpression;
                var macroBlock = ast as AstBlock;

                // ReSharper disable JoinDeclarationAndInitializer
                IAstExpression ce, fe;
                IEnumerable<AstNode> fs;
                // ReSharper restore JoinDeclarationAndInitializer
                //determine ce
                ce = contextBlock.Expression;

                //determine fe
                if (macroBlockExpr != null)
                    fe = macroBlockExpr.Expression;
                else if (expr != null)
                {
                    fe = expr;

                    //cannot be statement at the same time, set ast to null.
                    ast = null; 
                }
                else
                    fe = null;

                //determine fs
                if (macroBlock != null)
                    fs = macroBlock.Count > 0 ? macroBlock.Statements : null;
                else if (ast != null)
                    fs = new[] {ast};
                else
                    fs = null;

                _implementMergeRules(context, ce, fs, fe);
            }

            public bool TryExpandPartially(CompilerTarget target, MacroContext context)
            {
                if(!_macroFunction.Meta[@"partial\macro"].Switch)
                    return false;

                var successRaw = _invokeMacroFunction(target, context);
                if(successRaw.Type != PType.Bool)
                {
                    context.ReportMessage(ParseMessageSeverity.Error,
                        "Partial macro must return a boolean value, indicating whether it can handle the partial application. Assuming it cannot.");
                    _setupDefaultExpression(context);
                    return false;
                }

                return (bool) successRaw.Value;
            }

            private void _implementMergeRules(MacroContext context, IAstExpression ce, IEnumerable<AstNode> fs, IAstExpression fe)
            {
                var contextBlock = context.Block;
                //cs  is already stored in contextBlock, 
                //  the rules position cs always at the beginning, thus no need to handle cs.

                //At this point
                //  {   ce   }  iff (ce ∧ fs ∧ fe)
                //  {tmp = ce}  iff (ce ∧ fs ∧ ¬fe)
                //  {        }  otherwise
                if (ce != null && fs != null)
                {
                    if (fe != null)
                    {
                        contextBlock.Add((AstNode) ce);
                    }
                    else
                    {
                        //Might at a later point become a warning
                        context.ReportMessage(ParseMessageSeverity.Info,
                                              String.Format(
                                                  "Macro {0} uses temporary variable to ensure that expression from `context.Block` is evaluated before statements from macro return value.",
                                                  HumanId),
                                              context.Invocation);

                        var tmpV = context.AllocateTemporaryVariable();

                        //Generate assignment to temporary variable
                        var assignTmpV = new AstGetSetSymbol(context.Invocation.File,
                                                             context.Invocation.Line,
                                                             context.Invocation.Column,
                                                             PCall.Set,
                                                             tmpV,
                                                             SymbolInterpretations.LocalObjectVariable);
                        assignTmpV.Arguments.Add(ce);
                        contextBlock.Add(assignTmpV);

                        //Generate lookup of computed value
                        ce = new AstGetSetSymbol(context.Invocation.File,
                                                 context.Invocation.Line,
                                                 context.Invocation.Column, PCall.Get,
                                                 tmpV,
                                                 SymbolInterpretations.LocalObjectVariable);
                    }
                }

                //At this point
                //  {fs}    iff (fs)
                //  {  }    otherwise

                if (fs != null)
                {
                    foreach (var stmt in fs)
                        contextBlock.Add(stmt);
                }

                //Finally determine expression
                //  = fe    iff (ce ∧ fe)
                //  = ce    iff (ce ∧ ¬fe ∧ ¬fs)
                //  = tmp   iff (ce ∧ ¬fe ∧ fs)
                //  = ⊥     otherwise
                if (fe != null)
                    contextBlock.Expression = fe;
                else if (ce != null)
                    contextBlock.Expression = ce; //if tmp is involved, it has replaced ce
                else
                    contextBlock.Expression = null; //macro session will cover this case
            }

            private PValue _invokeMacroFunction(CompilerTarget target, MacroContext context)
            {
                var macro = PrepareMacroImplementation(target.Loader, _macroFunction, context);

                //Execute macro (argument nodes of the invocation node are passed as arguments to the macro)
                var macroInvocation = context.Invocation;
                var arguments = macroInvocation.Arguments.Select(target.Loader.CreateNativePValue).ToArray();
                var parentApplication = _macroFunction.ParentApplication;
                PValue astRaw;
                try
                {
                    parentApplication._SuppressInitialization = true;
                    astRaw = macro.IndirectCall(target.Loader, arguments);
                }
                finally
                {
                    parentApplication._SuppressInitialization = false;
                }
                return astRaw;
            }
        }

        #endregion

        public AstNode ExpandMacro(AstMacroInvocation invocation, bool justEffect)
        {
            var target = Target;
            var context = new MacroContext(this, invocation, justEffect);

            //Delegate actual expansion to approriate expander
            var expander = _getExpander(invocation, target);

            if(expander != null)
            {
                expander.Initialize(target, invocation, justEffect);

                //Macro invocations need to be unique within a session
                if (_invocations.Contains(invocation))
                {
                    target.Loader.ReportMessage(
                        new ParseMessage(ParseMessageSeverity.Error,
                                         String.Format(
                                             "AstMacroInvocation.EmitCode is not reentrant. The invocation node for the macro {0} has been expanded already. Use GetCopy() to operate on a copy of this macro invocation.",
                                             expander.HumanId),
                                         invocation));
                    return CreateNeutralExpression(invocation);
                }
                _invocations.Add(invocation);

                //check if this macro is a partial application (illegal)
                if (invocation.Arguments.Any(AstPartiallyApplicable.IsPlaceholder))
                {
                    //Attempt to expand partial macro
                    try
                    {
                        if(!expander.TryExpandPartially(target, context))
                        {
                            target.Loader.ReportSemanticError(invocation.Line, invocation.Column, "The macro " + expander.HumanId + " cannot be applied partially.");
                            return CreateNeutralExpression(invocation);
                        }
                    }
                    catch (Exception e)
                    {
                        _setupDefaultExpression(context);
                        _reportException(context, expander, e);
                    }
                }
                else
                {
                    //Actual macro expansion takes place here
                    try
                    {
                        expander.Expand(target, context);
                    }
                    catch (Exception e)
                    {
                        _setupDefaultExpression(context);
                        _reportException(context, expander, e);
                    }
                }
            }

            //Sanitize output
            var ast = context.Block;

            //ensure that there is at least null being pushed onto the stack))
            if (!justEffect && ast.Expression == null && !context.SuppressDefaultExpression)
                ast.Expression = CreateNeutralExpression(invocation);

            var node = AstNode._GetOptimizedNode(Target, ast);

            return (AstNode) node;
        }

        private static void _reportException(MacroContext context, IMacroExpander expander, Exception e)
        {
            context.ReportMessage(ParseMessageSeverity.Error,
                String.Format(
                    "Exception during expansion of macro {0} in function {1}: {2}",
                    expander.HumanId, context.Function.LogicalId,
                    e.Message), context.Invocation);
#if DEBUG
            Console.WriteLine(e);
#endif
        }

        private static void _setupDefaultExpression(MacroContext context)
        {
            context.Block.Clear();
            context.Block.Expression = CreateNeutralExpression(context.Invocation);
            context.SuppressDefaultExpression = false;
        }

        public static AstGetSet CreateNeutralExpression(AstMacroInvocation invocation)
        {
            var nullLiteral = new AstNull(invocation.File, invocation.Line, invocation.Column);
            var call = new AstIndirectCall(invocation.File, invocation.Line, invocation.Column,
                                           invocation.Call, nullLiteral);
            if (invocation.Call == PCall.Set)
                call.Arguments.Add(new AstNull(invocation.File, invocation.Line, invocation.Column));

            return call;
        }

        private IMacroExpander _getExpander(AstMacroInvocation invocation, CompilerTarget target)
        {
            IMacroExpander expander = null;
            switch (invocation.Interpretation)
            {
                case SymbolInterpretations.Function:
                    expander = new MacroFunctionExpander();
                    break;
                case SymbolInterpretations.MacroCommand:
                    expander = new MacroCommandExpander();
                    break;
                default:
                    target.Loader.ReportMessage(
                        new ParseMessage(ParseMessageSeverity.Error,
                                         String.Format(
                                             "Cannot apply {0} as a macro at compile time.",
                                             Enum.GetName(
                                                 typeof (
                                                     SymbolInterpretations),
                                                 invocation.Interpretation)),
                                         invocation));
                    break;
            }
            return expander;
        }

        /// <summary>
        /// Provides macro environment to its implementing function. The resulting closure 
        /// implements the expansion of the macro.
        /// </summary>
        /// <param name="sctx">The stack context to use for wrapping the context.</param>
        /// <param name="func">The implementation of the macro.</param>
        /// <param name="context">The macro context for this expansion.</param>
        /// <returns>A closure that implements the expansion of this macro.</returns>
        public static Closure PrepareMacroImplementation(StackContext sctx, PFunction func, MacroContext context)
        {
            var contextVar =
                CompilerTarget.CreateReadonlyVariable(sctx.CreateNativePValue(context));

            var env = new SymbolTable<PVariable>(1);
            env.Add(MacroAliases.ContextAlias, contextVar);

            var sharedVariables =
                func.Meta[PFunction.SharedNamesKey].List.Select(entry => env[entry.Text]).
                    ToArray();
            return new Closure(func, sharedVariables);
        }

        /// <summary>
        /// Stores an object in the macro session. It can later be retrieved via <see cref="RetrieveFromTransport"/>.
        /// </summary>
        /// <param name="obj">The object to be stored.</param>
        /// <returns>The id with which to retrieve the object later.</returns>
        public int StoreForTransport(PValue obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            var transportId = _transportStore.Count;
            _transportStore.Add(obj);
            return transportId;
        }

        /// <summary>
        /// Returns an object previously stored via <see cref="StoreForTransport"/>.
        /// </summary>
        /// <param name="id">The id as returned by <see cref="StoreForTransport"/></param>
        /// <returns>The obejct stored before.</returns>
        public PValue RetrieveFromTransport(int id)
        {
            if (0 <= id && id < _transportStore.Count)
                return _transportStore[id];
            else
                throw new PrexoniteException(
                    string.Format("No object with id {0} in transport through this macro session.",
                        id));
        }
    }
}