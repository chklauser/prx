// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro
{
    /// <summary>
    ///     Provides information associated with a macro expansion session.
    /// </summary>
    public class MacroSession : IDisposable
    {
        [CanBeNull]
        private LoaderOptions _options;

        [NotNull]
        private readonly SymbolCollection _releaseList = new();
        [NotNull]
        private readonly SymbolCollection _allocationList = new();

        [NotNull]
        private readonly HashSet<AstGetSet> _invocations =
            new();

        [NotNull]
        private readonly object _buildCommandToken;

        [NotNull]
        private readonly List<PValue> _transportStore = new();

        /// <summary>
        ///     Creates a new macro expansion session for the specified compiler target.
        /// </summary>
        /// <param name = "target">The target to expand macros in.</param>
        public MacroSession([NotNull] CompilerTarget target)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Factory = Target.Factory;
            
            GlobalSymbols = SymbolStore.Create(Target.Loader.Symbols);
            OuterVariables = new ReadOnlyCollectionView<string>(Target.OuterVariables);

            _buildCommandToken = target.Loader.RequestBuildCommands();
        }

        /// <summary>
        ///     Provides read-only access to the global symbol table.
        /// </summary>
        [NotNull]
        public SymbolStore GlobalSymbols { get; }

        /// <summary>
        ///     The target that this macro expansion session covers.
        /// </summary>
        [NotNull]
        public CompilerTarget Target { [DebuggerStepThrough] get; }

        [NotNull]
        public ReadOnlyCollectionView<string> OuterVariables { get; }

        /// <summary>
        ///     A copy of the loader options in effect during this macro expansion.
        /// </summary>
        public LoaderOptions LoaderOptions
        {
            get
            {
                if (_options == null)
                {
                    _options = new LoaderOptions(Target.Loader.ParentEngine,
                        Target.Loader.ParentApplication);
                    _options.InheritFrom(Target.Loader.Options);
                }
                return Target.Loader.Options;
            }
        }

        public ILoopBlock CurrentLoopBlock => Target.CurrentLoopBlock;

        public AstBlock CurrentBlock => Target.CurrentBlock;

        [NotNull]
        public IAstFactory Factory { get; }

        /// <summary>
        ///     Allocates a temporary variable for this macro expansion session.
        /// </summary>
        /// <returns>The (physical) id of a free temporary variable.</returns>
        /// <remarks>
        ///     If a temporary variable is not freed during a macro expansion session, 
        ///     it will no longer be considered a temporary variable and cannot be freed in 
        ///     subsequent expansions
        /// </remarks>
        public string AllocateTemporaryVariable()
        {
            var temp = Target.RequestTemporaryVariable();
            Debug.Assert(!_allocationList.Contains(temp));
            Debug.Assert(!_releaseList.Contains(temp));
            _allocationList.Add(temp);
            return temp;
        }

        /// <summary>
        ///     Marks the temporary variable for freeing at the end of this expansion session.
        /// </summary>
        /// <param name = "temporaryVariable">The temporary variable to be freed.</param>
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
        ///     Releases managed resources bound by the macro expansion session, such as temporary variables.
        /// </summary>
        public void Dispose()
        {
            //Free all variables that were marked as free during the session.
            try
            {
                foreach (var temp in _releaseList)
                {
                    Target.FreeTemporaryVariable(temp);
                    _allocationList.Remove(temp);
                }

                //Remove those that weren't freed to persistent variables
                foreach (var temp in _allocationList)
                {
                    Target.PromoteTemporaryVariable(temp);
                }
            }
            finally
            {
                var ldr = Target.Loader;
                ldr?.ReleaseBuildCommands(_buildCommandToken);
            }
        }

        private interface IMacroExpander
        {
            void Initialize(CompilerTarget target, AstGetSet macroNode, bool justEffect);
            string HumanId { get; }
            void Expand(CompilerTarget target, MacroContext context);
            bool TryExpandPartially(CompilerTarget target, MacroContext context);
        }

        #region Command Expander

        private abstract class MacroCommandExpanderBase : IMacroExpander
        {
            protected MacroCommand MacroCommand;
            public string HumanId { get; protected set; }

            public abstract void Initialize(CompilerTarget target, AstGetSet macroNode,
                                            bool justEffect);

            public void Expand(CompilerTarget target, MacroContext context)
            {
                MacroCommand?.Expand(context);
            }

            public bool TryExpandPartially(CompilerTarget target, MacroContext context)
            {
                return MacroCommand is PartialMacroCommand pac && pac.ExpandPartialApplication(context);
            }
        }

        private class MacroCommandExpander : MacroCommandExpanderBase
        {
            public override void Initialize(CompilerTarget target, AstGetSet macroNode, bool justEffect)
            {
                var expansion = (AstExpand) macroNode;

                MacroCommand = null;

                if(!expansion.Entity.TryGetMacroCommand(out var mcmdRef))
                    throw new InvalidOperationException(string.Format(Resources.MacroCommandExpander_MacroCommandExpected, expansion.Entity));
                MacroCommand mcmd;
                if (mcmdRef.TryGetEntity(target.Loader, out var value) && (mcmd = value.Value as MacroCommand) != null)
                {
                    HumanId = mcmdRef.Id;
                    MacroCommand = mcmd;
                }
                else
                {
                    target.Loader.ReportMessage(Message.Create(MessageSeverity.Error,
                                                               string.Format(
                                                                   Resources.MacroCommandExpander_CannotFindMacro,
                                                                   mcmdRef.Id),
                                                               macroNode.Position, MessageClasses.NoSuchMacroCommand));
                    HumanId = "cannot_find_macro_command";
                }
            }
        }

        #endregion

        #region Function Expander

        private abstract class MacroFunctionExpanderBase : IMacroExpander
        {
            protected PFunction MacroFunction;

            [PublicAPI]
            public const string PartialMacroKey = @"partial\macro";

            public string HumanId { get; protected set; }

            public abstract void Initialize(CompilerTarget target, AstGetSet macroNode,
                                            bool justEffect);

            public void Expand(CompilerTarget target, MacroContext context)
            {
                if (MacroFunction == null)
                    return;

                var astRaw = _invokeMacroFunction(target, context);

                //Optimize
                AstNode ast;
                if (astRaw != null)
                    ast = astRaw.Value as AstNode;
                else
                    ast = null;

                var expr = ast as AstExpr;

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
                var macroBlock = ast as AstBlock;

                // ReSharper disable JoinDeclarationAndInitializer
                AstExpr ce, fe;
                IEnumerable<AstNode> fs;
                // ReSharper restore JoinDeclarationAndInitializer
                //determine ce
                ce = contextBlock.Expression;

                //determine fe
                if (macroBlock != null)
                    fe = macroBlock.Expression;
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
                if (!MacroFunction.Meta[PartialMacroKey].Switch)
                    return false;

                var successRaw = _invokeMacroFunction(target, context);
                if (successRaw.Type != PType.Bool)
                {
                    context.ReportMessage(Message.Create(MessageSeverity.Error,
                                                         Resources.MacroFunctionExpander_PartialMacroMustIndicateSuccessWithBoolean,
                                                         context.Invocation.Position,
                                                         MessageClasses.PartialMacroMustReturnBoolean));
                    _setupDefaultExpression(context);
                    return false;
                }

                return (bool) successRaw.Value;
            }

            private void _implementMergeRules(MacroContext context, AstExpr ce,
                                              IEnumerable<AstNode> fs, AstExpr fe)
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
                        contextBlock.Add(ce);
                    }
                    else
                    {
                        //Might at a later point become a warning
                        var invocationPosition = context.Invocation.Position;
                        context.ReportMessage(Message.Create(MessageSeverity.Info,
                                                             string.Format(
                                                                 Resources.MacroFunctionExpander__UsedTemporaryVariable,
                                                                 HumanId),
                                                             invocationPosition, MessageClasses.BlockMergingUsesVariable));

                        var tmpV = context.AllocateTemporaryVariable();

                        //Generate assignment to temporary variable
                        var tmpVRef = context.Factory.Reference(invocationPosition, EntityRef.Variable.Local.Create(tmpV));
                        var assignTmpV = context.Factory.IndirectCall(invocationPosition,tmpVRef,PCall.Set);
                        assignTmpV.Arguments.Add(ce);
                        contextBlock.Add(assignTmpV);

                        //Generate lookup of computed value
                        ce = context.Factory.IndirectCall(invocationPosition,tmpVRef);
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
                var macro = PrepareMacroImplementation(target.Loader, MacroFunction, context);

                //Execute macro (argument nodes of the invocation node are passed as arguments to the macro)
                var macroInvocation = context.Invocation;
                var arguments =
                    macroInvocation.Arguments.Select(target.Loader.CreateNativePValue).ToArray();
                var parentApplication = MacroFunction.ParentApplication;
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

        private class MacroFunctionExpander : MacroFunctionExpanderBase
        {
            public override void Initialize(CompilerTarget target, AstGetSet macroNode, bool justEffect)
            {
                var expansion = (AstExpand)macroNode;

                MacroFunction = null;

                if (!expansion.Entity.TryGetFunction(out var functionRef))
                    throw new InvalidOperationException(string.Format(Resources.MacroFunctionExpander_ExpectedFunctionReference, expansion.Entity));
                PFunction func;
                if (functionRef.TryGetEntity(target.Loader, out var value) && (func = value.Value as PFunction) != null)
                {
                    HumanId = functionRef.Id;
                    MacroFunction = func;
                }
                else
                {
                    target.Loader.ReportMessage(
                        Message.Create(
                            MessageSeverity.Error,
                            string.Format(
                                Resources.MacroFunctionExpander_MacroFunctionNotAvailable,
                                functionRef,
                                target.Function.Id, target.Loader.ParentApplication.Module.Name),
                            macroNode.Position, MessageClasses.NoSuchMacroFunction));
                    HumanId = "could_not_resolve_macro_function";
                }
            }
        }

        #endregion

        public AstNode ExpandMacro(AstGetSet invocation, bool justEffect)
        {
            var target = Target;
            var context = new MacroContext(this, invocation, justEffect);

            //Delegate actual expansion to appropriate expander
            var expander = _getExpander(invocation, target);

            if (expander != null)
            {
                expander.Initialize(target, invocation, justEffect);

                //Macro invocations need to be unique within a session
                if (_invocations.Contains(invocation))
                {
                    target.Loader.ReportMessage(
                        Message.Create(MessageSeverity.Error,
                               string.Format(
                                   Resources.MacroSession_MacroNotReentrant,
                                   expander.HumanId),
                               invocation.Position, MessageClasses.MacroNotReentrant));
                    return CreateNeutralExpression(invocation);
                }
                _invocations.Add(invocation);

                T lockDownLexicalScope<T>(Func<T> action)
                {
                    var cub = target.CurrentBlock;
                    var r = action();
                    if(!ReferenceEquals(cub,target.CurrentBlock))
                        throw new PrexoniteException("Macro must restore previous lexical scope.");
                    return r;
                }
                void lockDownLexicalScopeA(Action action)
                {
                    lockDownLexicalScope<object>(() =>
                    {
                        action();
                        return null;
                    });
                }

                //check if this macro is a partial application
                if (invocation.CheckNodeApplicationState().HasPlaceholders)
                {
                    //Attempt to expand partial macro
                    try
                    {
                        if (!lockDownLexicalScope(() => expander.TryExpandPartially(target, context)))
                        {
                            target.Loader.ReportMessage(
                                Message.Create(
                                    MessageSeverity.Error,
                                    string.Format(
                                        Resources.MacroSession_MacroCannotBeAppliedPartially,
                                        expander.HumanId), invocation.Position,
                                    MessageClasses.PartialApplicationNotSupported));
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
                        lockDownLexicalScopeA(() => expander.Expand(target, context));
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

            return node;
        }

        private static void _reportException(MacroContext context, IMacroExpander expander,
            Exception e)
        {
            context.ReportMessage(Message.Create(MessageSeverity.Error,
                string.Format(
                    Resources.MacroSession_ExceptionDuringExpansionOfMacro,
                    expander.HumanId, context.Function.LogicalId,
                    e.Message), context.Invocation.Position, MessageClasses.ExceptionDuringCompilation));
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

        public static AstGetSet CreateNeutralExpression(AstGetSet invocation)
        {
            var nullLiteral = new AstNull(invocation.File, invocation.Line, invocation.Column);
            var call = new AstIndirectCall(invocation.File, invocation.Line, invocation.Column,
                invocation.Call, nullLiteral);
            if (invocation.Call == PCall.Set)
                call.Arguments.Add(new AstNull(invocation.File, invocation.Line, invocation.Column));

            return call;
        }

        private IMacroExpander _getExpander(AstGetSet macroNode, CompilerTarget target)
        {
            IMacroExpander expander = null;
            if (macroNode is AstExpand expansion)
            {
                if (expansion.Entity.TryGetMacroCommand(out _))
                    expander = new MacroCommandExpander();
                else if (expansion.Entity.TryGetFunction(out _))
                    expander = new MacroFunctionExpander();
                else
                    _reportMacroNodeNotMacro(target, expansion.Entity.GetType().Name, macroNode);
            }
            else
            {
                _reportMacroNodeNotMacro(target, macroNode.GetType().Name, macroNode);
            }
            return expander;
        }

        private static void _reportMacroNodeNotMacro(CompilerTarget target, string implName, AstGetSet invocation)
        {
            target.Loader.ReportMessage(
                Message.Create(MessageSeverity.Error,
                               string.Format(
                                   Resources.MacroSession_NotAMacro,
                                   implName),
                               invocation.Position, MessageClasses.NotAMacro));
        }

        /// <summary>
        ///     Provides macro environment to its implementing function. The resulting closure 
        ///     implements the expansion of the macro.
        /// </summary>
        /// <param name = "sctx">The stack context to use for wrapping the context.</param>
        /// <param name = "func">The implementation of the macro.</param>
        /// <param name = "context">The macro context for this expansion.</param>
        /// <returns>A closure that implements the expansion of this macro.</returns>
        public static Closure PrepareMacroImplementation(StackContext sctx, PFunction func,
            MacroContext context)
        {
            var contextVar =
                CompilerTarget.CreateReadonlyVariable(sctx.CreateNativePValue(context));

            var env = new SymbolTable<PVariable>(1) {{MacroAliases.ContextAlias, contextVar}};

            var sharedVariables =
                func.Meta[PFunction.SharedNamesKey].List.Select(entry => env[entry.Text]).
                    ToArray();
            return new Closure(func, sharedVariables);
        }

        /// <summary>
        ///     Stores an object in the macro session. It can later be retrieved via <see cref = "RetrieveFromTransport" />.
        /// </summary>
        /// <param name = "obj">The object to be stored.</param>
        /// <returns>The id with which to retrieve the object later.</returns>
        public int StoreForTransport(PValue obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            var transportId = _transportStore.Count;
            _transportStore.Add(obj);
            return transportId;
        }

        /// <summary>
        ///     Returns an object previously stored via <see cref = "StoreForTransport" />.
        /// </summary>
        /// <param name = "id">The id as returned by <see cref = "StoreForTransport" /></param>
        /// <returns>The object stored before.</returns>
        public PValue RetrieveFromTransport(int id)
        {
            if (0 <= id && id < _transportStore.Count)
                return _transportStore[id];
            else
                throw new PrexoniteException(
                    $"No object with id {id} in transport through this macro session.");
        }
    }
}