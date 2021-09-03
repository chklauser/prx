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
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;
using Prexonite.Internal;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;

// ReSharper disable InconsistentNaming

namespace Prexonite.Compiler
{
    internal partial class Parser
    {
        [DebuggerStepThrough]
        internal Parser(IScanner scanner, Loader loader)
            : this(scanner)
        {
            Loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _createTableOfInstructions();
            Ast = new AstProxy(this);
            Create = new ParserAstFactory(this);
            _referenceTransformer = new ReferenceTransformer(this);
        }

        #region Proxy interface

        public Loader Loader { [DebuggerStepThrough] get; }

        /// <summary>
        /// Preflight mode causes the parser to abort at the 
        /// first non-meta construct, giving the user the opportunity 
        /// to inspect a file's "header" without fully compiling 
        /// that file.
        /// </summary>
        public bool PreflightModeEnabled => Loader.Options.PreflightModeEnabled;

        public Application TargetApplication
        {
            [DebuggerStepThrough]
            get => Loader.Options.TargetApplication;
        }

        public Module TargetModule
        {
            [DebuggerStepThrough]
            get => TargetApplication.Module;
        }

        public LoaderOptions Options
        {
            [DebuggerStepThrough]
            get => Loader.Options;
        }

        public Engine ParentEngine
        {
            [DebuggerStepThrough]
            get => Loader.Options.ParentEngine;
        }

        public SymbolStore Symbols
        {
            [DebuggerStepThrough]
            get => target?.CurrentBlock.Symbols ?? Loader.Symbols;
        }

        private DeclarationScopeBuilder _prepareDeclScope(QualifiedId relativeNsId, ISourcePosition idPosition)
        {
            if(relativeNsId.Count < 1)
                throw new ArgumentOutOfRangeException(nameof(relativeNsId),Resources.Parser_relativeNsId_empty);
            var outerScope = Loader.CurrentScope;
            QualifiedId prefix;
            SymbolStore declScopeStore;
            LocalNamespace surroundingNamespace;
            if (outerScope == null)
            {
                prefix = new QualifiedId(null);
                declScopeStore = Loader.TopLevelSymbols;
                surroundingNamespace = null;
            }
            else
            {
                prefix = outerScope.PathPrefix;
                declScopeStore = outerScope.Store;
                surroundingNamespace = outerScope._LocalNamespace;
            }

            var currentLookupScope = (ISymbolView<Symbol>) declScopeStore;
            var isOutermostNs = true;
            foreach (var nsId in relativeNsId)
            {
                var localNs = _tryGetLocalNamespace(currentLookupScope, nsId, idPosition);

                prefix += new QualifiedId(nsId);

                // Create namespace if necessary
                localNs ??= ((ModuleLevelView) Loader.TopLevelSymbols).CreateLocalNamespace(
                    new EmptySymbolView<Symbol>());

                // Make sure the namespace is exported from the current module and not just accessible via external declarations
                _declareNamespaceAsExported(surroundingNamespace, declScopeStore, nsId, isOutermostNs, idPosition, localNs, prefix);

                surroundingNamespace = localNs;
                currentLookupScope = localNs;
                if (isOutermostNs)
                    isOutermostNs = false;
            }

            // This should never happen, check is here to convey this fact to null-analysis
            if(surroundingNamespace == null)
                throw new PrexoniteException("Failed to create the innermost namespace of " + relativeNsId + ".");

            // Create the local scope of this namespace *declaration* (the scope inside the braces)
            // Not that this is almost completely independent of the namespace itself
            // It is just used as one possible source for exports
            var builder = SymbolStoreBuilder.Create(declScopeStore);
            return new DeclarationScopeBuilder(builder, prefix, surroundingNamespace);
        }

        class DeclarationScopeBuilder
        {
            public DeclarationScopeBuilder([NotNull]SymbolStoreBuilder localScopeBuilder, QualifiedId prefix, [NotNull]LocalNamespace ns)
            {
                LocalScopeBuilder = localScopeBuilder ?? throw new ArgumentNullException(nameof(localScopeBuilder));
                Prefix = prefix;
                Namespace = ns ?? throw new ArgumentNullException(nameof(ns));
            }

            [NotNull]
            public SymbolStoreBuilder LocalScopeBuilder { get; }

            public QualifiedId Prefix { get; }

            [NotNull]
            public LocalNamespace Namespace { get; }

            [NotNull]
            public DeclarationScope ToDeclarationScope()
            {
                return new(Namespace, Prefix, LocalScopeBuilder.ToSymbolStore());
            }
        }

        [CanBeNull]
        private LocalNamespace _tryGetLocalNamespace([NotNull] ISymbolView<Symbol> currentSurrounding, [NotNull] string superNsId, [NotNull] ISourcePosition idPosition)
        {
            LocalNamespace localNs = null;
            if (currentSurrounding.TryGet(superNsId, out var sym))
            {
                var fakeExpr = Create.ExprFor(idPosition, sym);
                if (!(fakeExpr is AstNamespaceUsage nsUsage))
                    Loader.ReportMessage(Message.Error(
                        string.Format(Resources.Parser_NamespaceExpected, superNsId, sym),
                        idPosition, MessageClasses.NamespaceExcepted));
                else
                {
                    // namespace already exists
                    localNs = nsUsage.Namespace as LocalNamespace;
                    if (localNs == null)
                    {
                        Loader.ReportMessage(Message.Error(
                            string.Format(Resources.Parser_CannotExtendMergedNamespace, superNsId),
                            idPosition, MessageClasses.CannotExtendMergedNamespace));
                    }
                }
            }
            return localNs;
        }

        /// <summary>
        /// Ensures the namespace is declared as an exported symbol (and not just available via external declarations)
        /// </summary>
        /// <param name="surroundingNamespace">Reference to the surrounding namespace, if any</param>
        /// <param name="outer">Reference to the top-level symbol store</param>
        /// <param name="superNsId">Name of the namespace to declare</param>
        /// <param name="isOutermostNs">True if the symbol should be added to the top-level scope; false if it should be declared in the surrounding namespace</param>
        /// <param name="idPosition">Position of the name that caused this declaration (position of the namespace name)</param>
        /// <param name="localNs">The namespace to declare</param>
        /// <param name="nextPrefix">Namespaces prefix to use for physical names in declared namespace. Or null if the namespace already has a prefix assigned.</param>
        private static void _declareNamespaceAsExported(
            LocalNamespace surroundingNamespace, 
            SymbolStore outer, 
            string superNsId, 
            bool isOutermostNs, 
            ISourcePosition idPosition, 
            LocalNamespace localNs, 
            QualifiedId nextPrefix)
        {
            var nsSym = Symbol.CreateNamespace(localNs, idPosition);
            Symbol existingSym;
            if (isOutermostNs)
            {
                // The outermost namespace (x in x.y.z) is declared as an ordinary symbol in the current scope
                if (!outer.IsDeclaredLocally(superNsId) ||
                    !(outer.TryGet(superNsId, out existingSym) && existingSym.Equals(nsSym)))
                {
                    outer.Declare(superNsId, nsSym);
                }
            }
            else if (surroundingNamespace == null)
                throw new PrexoniteException(
                    "Failed to create surrounding namespace (syntactic sugar for nested namespace)");
            else
            {
                // Inner namespaces (z and y in x.y.z) are exported from their respective super-namespaces
                if (!surroundingNamespace.TryGetExported(superNsId, out existingSym) || !existingSym.Equals(nsSym))
                {
                    surroundingNamespace.DeclareExports(
                        new KeyValuePair<string, Symbol>(superNsId, nsSym).Singleton());
                }
            }

            localNs.Prefix ??= nextPrefix.ToString().Replace('.', '\\');
        }

        [CanBeNull]
        private ISymbolView<Symbol> _resolveNamespace(ISymbolView<Symbol> scope, [NotNull] ISourcePosition qualifiedIdPosition, QualifiedId qualifiedId)
        {
            while (qualifiedId.Count > 0)
            {
                scope.TryGet(qualifiedId[0], out var sym);
                var expr = Create.ExprFor(qualifiedIdPosition, sym);
                if (expr is not AstNamespaceUsage nsUsage)
                {
                    Create.ReportMessage(
                        Message.Error(string.Format(Resources.Parser_NamespaceExpected, qualifiedId[0], sym == null ? "not defined" : sym.ToString()),
                            qualifiedIdPosition, MessageClasses.NamespaceExcepted));
                    return null;
                }
                else
                {
                    scope = nsUsage.Namespace;
                    qualifiedId = qualifiedId.WithPrefixDropped(1);
                }
            }
            return scope;
        }

        private void _updateNamespace(DeclarationScope scope, SymbolStoreBuilder builder)
        {
            scope._LocalNamespace.DeclareExports(builder.ToSymbolStore());
        }

        private SymbolOrigin _privateDeclarationOrigin(ISourcePosition position, DeclarationScope scope)
        {
            return new SymbolOrigin.NamespaceDeclarationScope(position, scope.PathPrefix);
        }

        private DeclarationScope _popDeclScope()
        {
            // The symbol for this namespace should already have been declared by the push operation
            return Loader.PopScope();
        }

        /// <summary>
        /// When forwarding exported symbols from the declaration scope, 
        /// we need to assemble a temporary symbol view that only contains
        /// the symbols declared locally.
        /// </summary>
        /// <param name="declStore">The store to extract the exported symbols from.</param>
        /// <returns>A static view (shallow copy) of the exported symbols of <paramref name="declStore"/>.</returns>
        private ISymbolView<Symbol> _indexExportedSymbols(SymbolStore declStore)
        {
            var index = SymbolStore.Create();
            foreach (var symbol in declStore.LocalDeclarations)
                index.Declare(symbol.Key, symbol.Value);
            return index;
        }

        private string _assignPhysicalFunctionSlot([CanBeNull] string primaryId)
        {
            return _assignPhysicalSlot(primaryId ?? Engine.GenerateName("f"));
        }

        private string _assignPhysicalSlot(string id)
        {
            var scope = Loader.CurrentScope;
            return scope == null ? id : scope._LocalNamespace.DerivePhysicalName(id);
        }

        private string _assignPhysicalGlobalVariableSlot([CanBeNull] string primaryId)
        {
            return _assignPhysicalSlot(primaryId ?? Engine.GenerateName("v"));
        }

        public Loader.FunctionTargetsIterator FunctionTargets
        {
            [DebuggerStepThrough]
            get => Loader.FunctionTargets;
        }

        public AstProxy Ast { get; }

        [DebuggerStepThrough]
        public class AstProxy
        {
            private readonly Parser outer;

            internal AstProxy(Parser outer)
            {
                this.outer = outer;
            }

            public AstBlock this[PFunction func] => outer.Loader.FunctionTargets[func].Ast;
        }

        public CompilerTarget target { [DebuggerStepThrough] get; private set; }

        protected int LocalState
        {
            get
            {
                bool flagLiteralsEnabled;
                if (target != null && target.Meta.TryGetValue(Shell.FlagLiteralsKey, out var flagSwitch))
                {
                    flagLiteralsEnabled = flagSwitch.Switch;
                }
                else if (TargetApplication.Meta.TryGetValue(Shell.FlagLiteralsKey, out flagSwitch))
                {
                    flagLiteralsEnabled = flagSwitch.Switch;
                }
                else
                {
                    flagLiteralsEnabled = Loader.Options.FlagLiteralsEnabled;
                }

                return flagLiteralsEnabled ? Lexer.LocalShell : Lexer.Local;
            }
        }

        public AstBlock CurrentBlock => target?.CurrentBlock;

        protected IAstFactory Create { get; }

        #endregion

        #region String cache

        [DebuggerStepThrough]
        internal string cache(string toCache)
        {
            return Loader.CacheString(toCache);
        }

        #endregion

        public void ViolentlyAbortParse()
        {
            scanner.Abort();
            la = new Token {kind = _EOF, pos = la.pos, line = la.pos, col = la.col, val = ""};
        }

        #region Helper

        #region General

        private static string _removeSingleQuotes(string s)
        {
            return s.Replace("'", "");
        }

        public static bool TryParseInteger(string s, out int i)
        {
            return int.TryParse(_removeSingleQuotes(s), IntegerStyle, CultureInfo.InvariantCulture,
                out i);
        }

        public static bool TryParseReal(string s, out double d)
        {
            return double.TryParse(_removeSingleQuotes(s), RealStyle, CultureInfo.InvariantCulture,
                out d);
        }

        public static bool TryParseVersion(string s, out Version version)
        {
            return System.Version.TryParse(s, out version);
        }

        public static NumberStyles RealStyle => NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

        public static NumberStyles IntegerStyle => NumberStyles.None;

        [DebuggerStepThrough, Obsolete("Use Loader.ReportMessage instead.")]
        public void SemErr(int line, int col, string message)
        {
            errors.SemErr(line, col, message);
        }

        [DebuggerStepThrough, Obsolete("Use Loader.ReportMessage instead.")]
        public void SemErr(Token tok, string s)
        {
            errors.SemErr(tok.line, tok.col, s);
        }

        private void _pushLexerState(int state)
        {
            if (!(scanner is Lexer lex))
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PushState(state);
        }

        private void _popLexerState()
        {
            if (!(scanner is Lexer lex))
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PopState();
            //Might be id or keyword
            if (la.kind > _BEGINKEYWORDS && la.kind < _ENDKEYWORDS || la.kind == _id)
                la.kind = lex.checkKeyword(la.val);
        }

        private void _inject(Token c)
        {
            if (!(scanner is Lexer lex))
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");

            if (c == null)
                throw new ArgumentNullException(nameof(c));

            lex._InjectToken(c);
        }

        private void _inject(int kind, string val)
        {
            if (val == null)
                throw new ArgumentNullException(nameof(val));
            var c = new Token
                {
                    kind = kind,
                    val = val
                };
            _inject(c);
        }

        private void _inject(int kind)
        {
            _inject(kind, string.Empty);
        }

        /// <summary>
        /// Defines a new global variable (or returns the existing declaration and instance)
        /// </summary>
        /// <param name="id">The physical name of the new global variable.</param>
        /// <param name="vari">The variable declaration for the new variable.</param>
        protected PVariable DefineGlobalVariable(string id, out VariableDeclaration vari)
        {
            if (TargetModule.Variables.TryGetVariable(id, out vari))
            {
                return TargetApplication.Variables[vari.Id];
            }
            else
            {
                vari = global::Prexonite.Modular.VariableDeclaration.Create(id);
                TargetModule.Variables.Add(vari);
                return TargetApplication.Variables[id] = new PVariable(vari);
            }
        }

        private void _compileAndExecuteBuildBlock(CompilerTarget buildBlockTarget)
        {
            if (errors.count > 0)
            {
                Loader.ReportMessage(Message.Error(Resources.Parser_ErrorsInBuildBlock,GetPosition(),MessageClasses.ErrorsInBuildBlock));
                return;
            }

            //Emit code for top-level build block
            try
            {
                buildBlockTarget.Ast.EmitCode(buildBlockTarget, true, StackSemantics.Effect);

                buildBlockTarget.Function.Meta["File"] = scanner.File;
                buildBlockTarget.FinishTarget();
                //Run the build block 
                var fctx = buildBlockTarget.Function.CreateFunctionContext(ParentEngine,
                    Array.Empty<PValue>(),
                    Array.Empty<PVariable>(), true);
                object token = null;
                try
                {
                    TargetApplication._SuppressInitialization = true;
                    token = Loader.RequestBuildCommands();
                    ParentEngine.Process(fctx);
                }
                finally
                {
                    if (token != null)
                        Loader.ReleaseBuildCommands(token);
                    TargetApplication._SuppressInitialization = false;
                }
            }
            catch (Exception e)
            {
                Loader.ReportMessage(Message.Error(string.Format(Resources.Parser_exception_in_build_block, e),GetPosition(),MessageClasses.ExceptionDuringCompilation));
            }
        }

        private bool _suppressPrimarySymbol(IHasMetaTable ihmt)
        {
            return ihmt.Meta[Loader.SuppressPrimarySymbol].Switch;
        }

        #endregion //General

        #region Prexonite Script

        #region Symbol management

        internal static AstGetSet _NullNode(ISourcePosition position)
        {
            var n = new AstNull(position.File, position.Line, position.Column);
            return new AstIndirectCall(position, PCall.Get, n);
        }

        private readonly Stack<object> _scopeStack = new();

        internal void _PushScope(AstScopedBlock block)
        {
            if (!ReferenceEquals(block.LexicalScope, CurrentBlock))
                throw new PrexoniteException("Cannot push scope of unrelated block.");
            _scopeStack.Push(block);
            target.BeginBlock(block);
        }

        internal void _PushScope(CompilerTarget ct)
        {
            if (!ReferenceEquals(ct.ParentTarget, target))
                throw new PrexoniteException("Cannot push scope of unrelated compiler target.");

            // SPECIAL CASE: Initialization code gets a separate environment every time 
            // a block of code is added to it.
            if (ct.Function.Id == Application.InitializationId)
            {
                ct.Ast._ReplaceSymbols(SymbolStore.Create(Symbols));
            }

            // Record scope
            _scopeStack.Push(ct);
            target = ct;
        }

        internal void _PopScope(AstScopedBlock block)
        {
            if (!ReferenceEquals(_scopeStack.Peek(), block))
                throw new PrexoniteException(
                    $"Tried to pop scope of block {block} but {_scopeStack.Peek()} was on top.");
            _scopeStack.Pop();
            target.EndBlock();
        }

        internal void _PopScope(CompilerTarget ct)
        {
            if (!ReferenceEquals(_scopeStack.Peek(), ct))
                throw new PrexoniteException(
                    $"Tried to pop scope of compiler target {ct} but {_scopeStack.Peek()} was on top.");
            target = ct.ParentTarget;
            _scopeStack.Pop();
        }

        #endregion

        [DebuggerStepThrough]
        public bool isLabel() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _lid || isId(c) && cla.kind == _colon;
        }

        /// <summary>
        /// Ensures the next two tokens represent <c>namespace import</c>. 
        /// </summary>
        /// <para>While <c>namespace</c> is a reserved keyword, <c>import</c> is only a contextual keyword.</para>
        /// <returns><c>true</c> if the next two tokens match; <c>false</c> otherwise</returns>
        protected bool isNamespaceImport() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _namespace && 
                   cla.kind == _id && cla.val.Equals("import", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool isAssignmentOperator() //LL2
        {
            /* Applies to:
             *  =
             *  +=
             *  -=
             *  *=
             *  /=
             *  ^=
             *  |=
             *  &=
             *  ??=
             *  ~=
             *  <|=
             *  |>=
             */
            scanner.ResetPeek();

            //current la = assign | plus | minus | times | div | pow | bitOr | bitAnd | coalesence | tilde

            switch (la.kind)
            {
                case _assign:
                    return true;
                case _plus:
                case _minus:
                case _times:
                case _div:
                case _pow:
                case _bitOr:
                case _bitAnd:
                case _coalescence:
                case _tilde:
                case _deltaleft:
                case _deltaright:
                    var c = scanner.Peek();
                    if (c.kind == _assign)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        public bool isSymbolDirective(string pattern)
        {
            scanner.ResetPeek();
            return la.kind == _id
                && scanner.Peek().kind == _lpar
                && la.val.ToUpperInvariant() == pattern;

        }

        private bool isFollowedByStatementBlock()
        {
            scanner.ResetPeek();
            return scanner.Peek().kind == _lbrace;
        }

        private bool isLambdaExpression() //LL(*)
        {
            scanner.ResetPeek();

            var current = la;
            if (!(current.kind == _lpar || isId(current)))
                return false;
            var next = scanner.Peek();

            var requirePar = false;
            if (current.kind == _lpar)
            {
                requirePar = true;
                current = next;
                next = scanner.Peek();

                //Check for lambda expression without arguments
                if (current.kind == _rpar && next.kind == _implementation)
                    return true;
            }

            if (isId(current))
            {
                //break if lookahead is not valid to save tokens
                if (
                    !(next.kind == _comma || next.kind == _implementation ||
                        next.kind == _rpar && requirePar))
                    return false;
                //Consume 1
                current = next;
                next = scanner.Peek();
            }
            else if ((current.kind == _var || current.kind == _ref) && isId(next))
            {
                //Consume 2
                current = scanner.Peek();
                //break if lookahead is not valid to save tokens
                if (
                    !(current.kind == _comma || current.kind == _implementation ||
                        current.kind == _rpar && requirePar))
                    return false;
                next = scanner.Peek();
            }
            else
            {
                return false;
            }

            while (current.kind == _comma && requirePar)
            {
                //Consume comma
                current = next;
                next = scanner.Peek();

                if (isId(current))
                {
                    //Consume 1
                    current = next;
                    next = scanner.Peek();
                }
                else if ((current.kind == _var || current.kind == _ref) && isId(next))
                {
                    //Consume 2
                    current = scanner.Peek();
                    next = scanner.Peek();
                }
                else
                {
                    return false;
                }
            }

            if (requirePar)
                if (current.kind == _rpar)
                {
                    current = next;
                    //cla = scanner.Peek();
                }
                else
                {
                    return false;
                }

            return current.kind == _implementation;
        }

        //LL(*)

        [DebuggerStepThrough]
        private bool isIndirectCall() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _dot && cla.kind == _lpar;
        }

        // used to distinguish asm{...} and asm(...) 
        private bool isAsmBlock() //LL(2)
        {
            var asm = la;
            if (asm.kind != _asm)
            {
                return false;
            }
            
            scanner.ResetPeek();
            return scanner.Peek().kind == _lbrace;
        }
        
        [DebuggerStepThrough]
        private bool isOuterVariable(string id) //context
        {
            return target._IsOuterVariable(id);
        }

        private string generateLocalId(string prefix = "")
        {
            return target.GenerateLocalId(prefix);
        }

        private Symbol _ensureDefinedLocal(string localAlias, string physicalId, bool isAutodereferenced, ISourcePosition declPos, bool isOverrideDecl)
        {
            var refSym =
                Symbol.CreateDereference(Symbol.CreateReference(EntityRef.Variable.Local.Create(physicalId), declPos),
                    declPos);
            var sym = isAutodereferenced
                ? Symbol.CreateDereference(refSym, declPos)
                : refSym;

            target.Symbols.Declare(localAlias, sym);
            if (!isOverrideDecl && !target.Function.Variables.Contains(physicalId) &&
                isOuterVariable(physicalId))
            {
                target.RequireOuterVariable(physicalId);
            }
            else if(!target.Function.Variables.Contains(physicalId))
            {
                target.Function.Variables.Add(physicalId);
            }
            return sym;
        }

        /// <summary>
        /// Resolve a symbol into an expression. Will emit error messages as a side-effect.
        /// Use with unqualified references. For qualified references, use <see cref="_useSymbolFromNamespace"/> instead.
        /// </summary>
        /// <param name="scope">The scope to resolve the symbol in. Usually just <see cref="Symbols"/>.</param>
        /// <param name="id">The ID to resolve.</param>
        /// <param name="position">The position to use for error messages.</param>
        /// <returns>The expression that this symbol resolves to.</returns>
        [NotNull]
        private AstGetSet _useSymbol([NotNull] ISymbolView<Symbol> scope, [NotNull] string id, [NotNull] ISourcePosition position)
        {
            var expr = scope.TryGet(id, out var sym)
                ? Create.ExprFor(position, sym)
                : new AstUnresolved(position, id);
            if (expr is AstGetSet complex)
            {
                // If we have a namespace usage at hand, record the id used to access it
                // (namespaces are otherwise anonymous, they have no physical name)
                // Note: similar code is located in the GetSetExtension parser production
                // for sub namespaces
                if (complex is AstNamespaceUsage {ReferencePath: null} nsu)
                {
                    nsu.ReferencePath = new QualifiedId(id);
                }
                return complex;
            }
            Loader.ReportMessage(Message.Error(Resources.Parser_SymbolicUsageAsLValue, position, MessageClasses.ParserInternal));
            complex = _NullNode(position);
            return complex;
        }
        
        /// <summary>
        /// Resolve a symbol into an expression. Will emit error messages as a side-effect.
        /// Use with qualified references. For unqualified references, use <see cref="_useSymbol"/> instead.
        /// </summary>
        /// <param name="ns">The namespace usage that serves as the scope for this resolution.</param>
        /// <param name="id">The ID to resolve within the base namespace.</param>
        /// <param name="position">The position to use for error messages.</param>
        /// <returns>The expression that this symbol resolves to.</returns>
        [NotNull]
        private AstGetSet _useSymbolFromNamespace([NotNull] AstNamespaceUsage ns, [NotNull] string id, [NotNull] ISourcePosition position)
        {
            var expr = _useSymbol(ns.Namespace, id, position);
            // write down qualified path
            if(expr is AstNamespaceUsage {ReferencePath: null} subNs)
            {
                subNs.ReferencePath = ns.ReferencePath + new QualifiedId(id);
            }

            return expr;
        }

        private Symbol _parseSymbol(MExpr expr)
        {
            try
            {
                var parser = new SymbolMExprParser(Symbols,Loader,Loader.TopLevelSymbols);
                return parser.Parse(expr);
            }
            catch (ErrorMessageException e)
            {
                Loader.ReportMessage(e.CompilerMessage);
                return Symbol.CreateNil(e.CompilerMessage.Position);
            }
        }

        [DebuggerStepThrough]
        private static bool isId(Token c)
        {
            if (isGlobalId(c))
                return true;
            switch (c.kind)
            {
                case _enabled:
                case _disabled:
                case _build:
                case _add:
                    return true;
                default:
                    return false;
            }
        }

        [DebuggerStepThrough]
        private static bool isGlobalId(Token c)
        {
            return c.kind is _id or _anyId;
        }

        private bool _isNotNewDecl()
        {
            if (la.kind != _new)
                return false;

            scanner.ResetPeek();
            var varTok = scanner.Peek();

            return varTok.kind != _var && varTok.kind != _ref;
        }

        private static IEnumerable<string> let_bindings(CompilerTarget ft)
        {
            var lets = new HashSet<string>(Engine.DefaultStringComparer);
            for (var ct = ft; ct != null; ct = ct.ParentTarget)
                lets.UnionWith(ct.Function.Meta[PFunction.LetKey].List.Select(e => e.Text));
            return lets;
        }

        private static void mark_as_let(PFunction f, string local)
        {
            f.Meta[PFunction.LetKey] = (MetaEntry)
                f.Meta[PFunction.LetKey].List
                    .Union(new[] { (MetaEntry)local })
                    .ToArray();
        }

        #region Assemble Invocation of Symbol

        private class ReferenceTransformer : SymbolHandler<int,Tuple<Symbol,bool>>
        {
            [NotNull]
            private readonly Parser _parser;

            public ReferenceTransformer([NotNull] Parser parser)
            {
                _parser = parser;
            }

            protected override Tuple<Symbol, bool> HandleWrappingSymbol(WrappingSymbol self, int argument)
            {
                if (argument == 0)
                    return Tuple.Create<Symbol,bool>(self,false);
                else
                {
                    var innerResult = self.InnerSymbol.HandleWith(this, argument);
                    return Tuple.Create<Symbol,bool>(self.With(innerResult.Item1),innerResult.Item2);
                }
            }

            protected override Tuple<Symbol, bool> HandleLeafSymbol(Symbol self, int argument)
            {
                if (argument > 0)
                {
                    throw new ErrorMessageException(
                        Message.Error(Resources.ReferenceTransformer_CannotCreateReferenceToValue,
                                      _parser.GetPosition(), MessageClasses.CannotCreateReference));
                }
                else
                {
                    return Tuple.Create(self,false);
                }
            }

            public override Tuple<Symbol, bool> HandleDereference(DereferenceSymbol self, int argument)
            {
                if (argument > 0)
                {
                    return self.InnerSymbol.HandleWith(this, argument - 1);
                }
                else
                {
                    return base.HandleDereference(self, argument);
                }
            }

            public override Tuple<Symbol, bool> HandleExpand(ExpandSymbol self, int argument)
            {
                if (argument > 1)
                {
                    throw new ErrorMessageException(
                        Message.Error(Resources.ReferenceTransformer_HandleExpand_CannotCreateReferenceToDefinitionOfMacroOrPartialApplication,
                                      _parser.GetPosition(), MessageClasses.CannotCreateReference));
                }
                else if(argument == 1)
                {
                    // Here, we don't actually remove the expansion, but rather switch the 
                    //  flag to true to indicate that the calling procedure should 
                    //  convert the resulting expansion node into a partial application.
                    return new Tuple<Symbol, bool>(base.HandleExpand(self,argument-1).Item1,true);
                }
                else
                {
                    return base.HandleExpand(self, argument);
                }
            }
        }

        [NotNull]
        private readonly ReferenceTransformer _referenceTransformer;

        private AstExpr _assembleReference(string id, int ptrCount)
        {
            Debug.Assert(id != null);
            Debug.Assert(ptrCount > 0);

            var position = GetPosition();
            if (!Symbols.TryGet(id, out var symbol))
            {
                Loader.ReportMessage(Message.Error(string.Format(Resources.Parser__assembleReference_SymbolNotDefined, id),position,MessageClasses.SymbolNotResolved));
                return Create.Null(position);
            }
            else
            {
                try
                {
                    var transformed = symbol.HandleWith(_referenceTransformer, ptrCount);
                    var invocation = Create.ExprFor(position, transformed.Item1);
                    if (transformed.Item2)
                    {
                        // If the reference transformer indicates that an Expand prefix was eliminated
                        //  during the transformation, we need to convert the invocation into a partial application
                        if (!(invocation is AstGetSet invocationCall))
                        {
                            Loader.ReportMessage(
                                Message.Error(Resources.Parser__assembleReference_MacroDefinitionNotLValue, position,
                                              MessageClasses.ParserInternal));
                            invocation = Create.Null(position);
                        }
                        else
                        {
                            invocationCall.Arguments.Add(Create.Placeholder(position));
                        }
                    }
                    return invocation;
                }
                catch (ErrorMessageException e)
                {
                    Loader.ReportMessage(e.CompilerMessage);
                    return Create.Null(position);
                }
            }
        }

        private static readonly SymbolShift _objectCreationShift = static id => Compiler.Loader.ObjectCreationPrefix + id;
        private static readonly SymbolShift _conversionShift =  static id => Compiler.Loader.ConversionPrefix + id;
        private static readonly SymbolShift _typeCheckShift =  static id => Compiler.Loader.TypeCheckPrefix + id;
        private static readonly SymbolShift _staticCallShift =  static id => Compiler.Loader.StaticCallPrefix + id;

        /// <summary>
        /// Given the partial application of a type check <c>(? is T)</c>, this method will construct the
        /// negated partial application <c>(? is not T)</c> by chaining the negation with <c>then</c>:
        /// <c>(? is T) then (not ?)</c>.
        /// </summary>
        /// <param name="position">The position of the overall expression.</param>
        /// <param name="check">The (non-inverted) type check.</param>
        /// <returns>The partial application of an inverted type check.</returns>
        [NotNull]
        private AstExpr _createPartialInvertedTypeCheck([NotNull] ISourcePosition position, [NotNull] AstExpr check)
        {
            // Special handling of "? is not Y" as that's not the same thing as "not (? is Y)"
            // when placeholders are involved.
            Debug.Assert(check.CheckForPlaceholders(), "check is expected to have placeholders");

            // Create "not ?"
            var notId = OperatorNames.Prexonite.GetName(UnaryOperator.LogicalNot);
            var notOp = CurrentBlock.Symbols.TryGet(notId, out var notSymbol)
                ? Create.ExprFor(position, notSymbol)
                : new AstUnresolved(position, notId);
            if (notOp is not AstGetSet notCall)
            {
                Loader.ReportMessage(
                    Message.Error(
                        Resources.AstFactoryBase_UnaryOperation_NotOperatorForTypecheckRequiresLValue,
                        position, MessageClasses.LValueExpected));
                notCall = _NullNode(position);
            }

            notCall.Arguments.Add(Create.Placeholder(position,0));

            // Assemble "(? is T) then (not ?)"
            var thenCmd = Create.Call(position, EntityRef.Command.Create(Engine.ThenAlias));
            thenCmd.Arguments.Add(check);
            thenCmd.Arguments.Add(notCall);

            return thenCmd;
        }

        private AstExpr _createUnknownExpr()
        {
            return _createUnknownGetSet();
        }

        private AstGetSet _createUnknownGetSet()
        {
            return new AstIndirectCall(this, new AstNull(this));
        }

        private void _appendRight(AstExpr lhs, AstGetSet rhs)
        {
            _appendRight(lhs.Singleton(), rhs);
        }

        private void _appendRight(IEnumerable<AstExpr> lhs, AstGetSet rhs)
        {
            rhs.Arguments.RightAppend(lhs);
            rhs.Arguments.ReleaseRightAppend();
            AstIndirectCall indirectCallNode;
            AstReference refNode;
            if ((indirectCallNode = rhs as AstIndirectCall) != null 
                && (refNode = indirectCallNode.Subject as AstReference) != null 
                && refNode.Entity.TryGetVariable(out _))
                rhs.Call = PCall.Set;
        }

        #endregion


        #endregion

        #region Assembler

        [DebuggerStepThrough]
        public void addInstruction(AstBlock block, Instruction ins)
        {
            block.Add(new AstAsmInstruction(this, ins));
        }

        public void addOpAlias(AstBlock block, string insBase, string detail)
        {
            var alias = getOpAlias(insBase, detail, out var argc);
            if (alias == null)
            {
                Loader.ReportMessage(
                    Message.Error(
                        string.Format(Resources.Parser_addOpAlias_Unknown, insBase, detail),
                        GetPosition(), MessageClasses.UnknownAssemblyOperator));
                block.Add(new AstAsmInstruction(this, new Instruction(OpCode.nop)));
                return;
            }

            block.Add(new AstAsmInstruction(this, Instruction.CreateCommandCall(argc, alias)));
        }

        [DebuggerStepThrough]
        public void addLabel(AstBlock block, string label)
        {
            block.Statements.Add(new AstExplicitLabel(this, label));
        }

        [DebuggerStepThrough]
        private bool isAsmInstruction(string insBase, string detail) //LL(4)
        {
            scanner.ResetPeek();
            var la1 = la.kind == _at ? scanner.Peek() : la;
            var la2 = scanner.Peek();
            var la3 = scanner.Peek();
            return checkAsmInstruction(la1, la2, la3, insBase, detail);
        }

        private static bool checkAsmInstruction(
            Token la1, Token la2, Token la3, string insBase, string detail)
        {
            return
                la1.kind != _string && Engine.StringsAreEqual(la1.val, insBase) &&
                    (detail == null
                        ? la2.kind != _dot || la3.kind == _integer
                        : la2.kind == _dot && la3.kind != _string &&
                          Engine.StringsAreEqual(la3.val, detail)
                        );
        }

        [DebuggerStepThrough]
        private bool isInIntegerGroup()
        {
            return peekIsOneOf(asmIntegerGroup);
        }

        [DebuggerStepThrough]
        private bool isInJumpGroup()
        {
            return peekIsOneOf(asmJumpGroup);
        }

        private bool isInOpAliasGroup()
        {
            return peekIsOneOf(asmOpAliasGroup);
        }

        [DebuggerStepThrough]
        private bool isInNullGroup()
        {
            return peekIsOneOf(asmNullGroup);
        }

        [DebuggerStepThrough]
        private bool isInIdGroup()
        {
            return peekIsOneOf(asmIdGroup);
        }

        [DebuggerStepThrough]
        private bool isInIdArgGroup()
        {
            return peekIsOneOf(asmIdArgGroup);
        }

        [DebuggerStepThrough]
        private bool isInArgGroup()
        {
            return peekIsOneOf(asmArgGroup);
        }

        [DebuggerStepThrough]
        private bool isInQualidArgGroup()
        {
            return peekIsOneOf(asmQualidArgGroup);
        }

        //[NoDebug()]
        private bool peekIsOneOf(string[,] table)
        {
            scanner.ResetPeek();
            var la1 = la.kind == _at ? scanner.Peek() : la;
            var la2 = scanner.Peek();
            var la3 = scanner.Peek();
            for (var i = table.GetUpperBound(0); i >= 0; i--)
                if (checkAsmInstruction(la1, la2, la3, table[i, 0], table[i, 1]))
                    return true;
            return false;
        }

        private readonly SymbolTable<OpCode> _instructionNameTable = new(60);

        private readonly SymbolTable<Tuple<string, int>> _opAliasTable =
            new(32);

        [DebuggerStepThrough]
        private void _createTableOfInstructions()
        {
            var tab = _instructionNameTable;
            //Add original names
            foreach (var code in Enum.GetNames(typeof(OpCode)))
                tab.Add(code.Replace('_', '.'), (OpCode)Enum.Parse(typeof(OpCode), code));

            //Add instruction aliases -- NOTE: You'll also have to add them to the respective groups
            tab.Add("new", OpCode.newobj);
            tab.Add("check", OpCode.check_const);
            tab.Add("cast", OpCode.cast_const);
            tab.Add("ret", OpCode.ret_value);
            tab.Add("ret.val", OpCode.ret_value);
            tab.Add("yield", OpCode.ret_continue);
            tab.Add("exit", OpCode.ret_exit);
            tab.Add("break", OpCode.ret_break);
            tab.Add("continue", OpCode.ret_continue);
            tab.Add("jump.true", OpCode.jump_t);
            tab.Add("jump.false", OpCode.jump_f);
            tab.Add("inc", OpCode.incloc);
            tab.Add("inci", OpCode.incloci);
            tab.Add("dec", OpCode.decloc);
            tab.Add("deci", OpCode.decloci);
            tab.Add("inda", OpCode.indarg);
            tab.Add("cor", OpCode.newcor);
            tab.Add("exception", OpCode.exc);
            tab.Add("ldnull", OpCode.ldc_null);

            //Add operator aliases
            var ops = _opAliasTable;
            ops.Add("neg", Tuple.Create(UnaryNegation.DefaultAlias, 1));
            ops.Add("not", Tuple.Create(LogicalNot.DefaultAlias, 1));
            ops.Add("add", Tuple.Create(Addition.DefaultAlias, 2));
            ops.Add("sub", Tuple.Create(Subtraction.DefaultAlias, 2));
            ops.Add("mul", Tuple.Create(Multiplication.DefaultAlias, 2));
            ops.Add("div", Tuple.Create(Division.DefaultAlias, 2));
            ops.Add("mod", Tuple.Create(Modulus.DefaultAlias, 2));
            ops.Add("pow", Tuple.Create(Power.DefaultAlias, 2));
            ops.Add("ceq", Tuple.Create(Equality.DefaultAlias, 2));
            ops.Add("cne", Tuple.Create(Inequality.DefaultAlias, 2));
            ops.Add("clt", Tuple.Create(LessThan.DefaultAlias, 2));
            ops.Add("cle", Tuple.Create(LessThanOrEqual.DefaultAlias, 2));
            ops.Add("cgt", Tuple.Create(GreaterThan.DefaultAlias, 2));
            ops.Add("cge", Tuple.Create(GreaterThanOrEqual.DefaultAlias, 2));
            ops.Add("or", Tuple.Create(BitwiseOr.DefaultAlias, 2));
            ops.Add("and", Tuple.Create(BitwiseAnd.DefaultAlias, 2));
            ops.Add("xor", Tuple.Create(ExclusiveOr.DefaultAlias, 2));
        }

        //[DebuggerStepThrough]
        private string getOpAlias(string insBase, string detail, out int argc)
        {
            var combined = insBase + (detail == null ? "" : "." + detail);
            var entry = _opAliasTable.GetDefault(combined, null);
            if (entry == null)
            {
                argc = -1;
                return null;
            }
            else
            {
                argc = entry.Item2;
                return entry.Item1;
            }
        }

        //[DebuggerStepThrough]
        private OpCode getOpCode(string insBase, string detail)
        {
            var combined = insBase + (detail == null ? "" : "." + detail);
            return _instructionNameTable.GetDefault(combined, OpCode.invalid);
        }

        #region Instruction tables

        private readonly string[,] asmIntegerGroup =
            {
                {"ldc", "int"},
                {"pop", null},
                {"dup", null},
                {"ldloci", null},
                {"stloci", null},
                {"incloci", null},
                {"inci", null},
                {"deci", null},
                {"ldr", "loci"}
            };

        private readonly string[,] asmJumpGroup =
            {
                {"jump", null},
                {"jump", "t"},
                {"jump", "f"},
                {"jump", "true"},
                {"jump", "false"},
                {"leave", null}
            };

        private readonly string[,] asmNullGroup =
            {
                {"ldc", "null"},
                {"ldnull", null},
                {"check", "arg"},
                {"check", "null"},
                {"cast", "arg"},
                {"ldr", "eng"},
                {"ldr", "app"},
                {"ret", null},
                {"ret", "set"},
                {"ret", "value"},
                {"ret", "val"},
                {"ret", "break"},
                {"ret", "continue"},
                {"ret", "exit"},
                {"break", null},
                {"continue", null},
                {"yield", null},
                {"exit", null},
                {"try", null},
                {"throw", null},
                {"exc", null},
                {"exception", null}
            };

        private readonly string[,] asmOpAliasGroup =
            {
                {"neg", null},
                {"not", null},
                {"add", null},
                {"sub", null},
                {"mul", null},
                {"div", null},
                {"mod", null},
                {"pow", null},
                {"ceq", null},
                {"cne", null},
                {"clt", null},
                {"cle", null},
                {"cgt", null},
                {"cge", null},
                {"or", null},
                {"and", null},
                {"xor", null}
            };

        private readonly string[,] asmIdGroup =
            {
                {"inc", null},
                {"incloc", null},
                {"incglob", null},
                {"dec", null},
                {"decloc", null},
                {"decglob", null},
                {"ldc", "string"},
                {"ldr", "func"},
                {"ldr", "cmd"},
                {"ldr", "glob"},
                {"ldr", "loc"},
                {"ldr", "type"},
                {"ldloc", null},
                {"stloc", null},
                {"ldglob", null},
                {"stglob", null},
                {"check", "const"},
                {"check", null},
                {"cast", "const"},
                {"cast", null},
                {"newclo", null}
            };

        private readonly string[,] asmIdArgGroup =
            {
                {"newtype", null},
                {"get", null},
                {"set", null},
                {"func", null},
                {"cmd", null},
                {"indloc", null},
                {"indglob", null}
            };

        private readonly string[,] asmArgGroup =
            {
                {"indarg", null},
                {"inda", null},
                {"newcor", null},
                {"cor", null},
                {"tail", null}
            };

        private readonly string[,] asmQualidArgGroup =
            {
                {"sget", null},
                {"sset", null},
                {"newobj", null},
                {"new", null}
            };

        #endregion

        #endregion

        #endregion

    }
}

// ReSharper restore InconsistentNaming