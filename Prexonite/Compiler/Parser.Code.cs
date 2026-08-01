using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Prexonite.Commands;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Internal;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;
using Prexonite.Internal;
using Prexonite.Modular;
using Prexonite.Properties;

// ReSharper disable InconsistentNaming

namespace Prexonite.Compiler;

partial class Parser
{
    sealed class PatternBindingRoot
    {
        readonly Dictionary<string, PatternCapture> _captures = new(
            StringComparer.OrdinalIgnoreCase
        );

        public IReadOnlyCollection<PatternCapture> Captures => _captures.Values;

        public PatternCapture GetOrCreate(
            Parser parser,
            string name,
            ISourcePosition position,
            bool isExplicit
        )
        {
            if (_captures.TryGetValue(name, out var existing))
            {
                if (isExplicit)
                    existing.IsExplicit = true;
                return existing;
            }

            parser.ensureHasTarget();
            var physicalId = parser.generateLocalId("pattern\\capture\\" + name);
            var publicId = parser.generateLocalId("pattern\\public\\" + name);
            parser.target.Function.Variables.Add(physicalId);
            parser.target.Function.Variables.Add(publicId);
            var capture = new PatternCapture(name, physicalId, publicId, position, isExplicit);
            _captures.Add(name, capture);
            parser.target.PatternCaptures.Add(capture);
            return capture;
        }
    }

    PatternBindingRoot? _bindingRoot;
    int _bindingRootDepth;
    readonly Stack<(PatternBindingRoot? Root, int Depth)> _suspendedBindingRoots = new();
    readonly Stack<SymbolStore> _captureOverlays = new();
    readonly Stack<BindingOutcome> _captureEnvironments = new();
    readonly Stack<string> _patternSubjectLocals = new();
    bool _moduleHeaderOpen = true;

    void _freezePatternMatchingMode()
    {
        _moduleHeaderOpen = false;
        Loader.FreezePatternMatchingMode();
    }

    bool _observeMetaAssignment(IHasMetaTable metaTable, string key, MetaEntry entry)
    {
        if (
            !ReferenceEquals(metaTable, TargetApplication)
            || !Engine.StringsAreEqual(key, Loader.PatternMatchingKey)
        )
            return true;
        if (!_moduleHeaderOpen)
        {
            Loader.ReportMessage(
                Message.Error(
                    $"{Loader.PatternMatchingKey} must appear in the module header before the first non-meta construct.",
                    GetPosition(),
                    MessageClasses.PatternMatchingModeLate
                )
            );
            return false;
        }
        return Loader.TrySetPatternMatchingMode(entry.Switch, GetPosition());
    }

    void _enterBindingRoot()
    {
        if (_bindingRootDepth++ == 0)
            _bindingRoot = new PatternBindingRoot();
    }

    void _leaveBindingRoot()
    {
        if (--_bindingRootDepth == 0)
            _bindingRoot = null;
    }

    void _enterIsolatedBindingRoot()
    {
        _suspendedBindingRoots.Push((_bindingRoot, _bindingRootDepth));
        _bindingRoot = new PatternBindingRoot();
        _bindingRootDepth = 1;
    }

    void _leaveIsolatedBindingRoot()
    {
        if (_bindingRootDepth != 1 || _suspendedBindingRoots.Count == 0)
            throw new PrexoniteException("Unbalanced isolated pattern binding root.");
        (_bindingRoot, _bindingRootDepth) = _suspendedBindingRoots.Pop();
    }

    BindingFlow _bindingFlowOf(AstExpr expression) => AstBindingAnd.BindingFlowFor(expression);

    void _pushCaptureEnvironment(BindingOutcome outcome, bool usePublicSlots = false)
    {
        if (_captureEnvironments.Count > 0)
            outcome = BindingOutcome.Sequence(_captureEnvironments.Peek(), outcome);
        // Keep only the capture declarations here. Symbols composes these overlays with the
        // current compiler scope dynamically, so entering a nested function or lambda while
        // parsing a consumer branch does not hide that nested scope's parameters and locals.
        var overlay = SymbolStore.Create();
        if (_bindingRoot != null)
        {
            foreach (var capture in _bindingRoot.Captures)
            {
                if (outcome.IsReachable && outcome.Captures.Contains(capture))
                {
                    overlay.Declare(
                        capture.Name,
                        capture.CreateReadSymbol(GetPosition(), usePublicSlots)
                    );
                }
                else
                {
                    var message = Message.Error(
                        $"Pattern binding {capture.Name} is not available on this control-flow route.",
                        GetPosition(),
                        MessageClasses.PatternBindingUnavailable
                    );
                    overlay.Declare(
                        capture.Name,
                        Symbol.CreateMessage(message, Symbol.CreateNil(GetPosition()))
                    );
                }
            }
        }
        _captureOverlays.Push(overlay);
        _captureEnvironments.Push(outcome);
    }

    void _popCaptureEnvironment()
    {
        _captureEnvironments.Pop();
        _captureOverlays.Pop();
    }

    AstPattern _createBindingPattern(string name, ISourcePosition position, bool isExplicit)
    {
        ensureHasTarget();
        _bindingRoot ??= new PatternBindingRoot();
        var capture = _bindingRoot.GetOrCreate(this, name, position, isExplicit);
        if (
            _captureEnvironments.Count > 0
            && _captureEnvironments.Peek().IsReachable
            && _captureEnvironments.Peek().Captures.Contains(capture)
        )
        {
            Loader.ReportMessage(
                Message.Error(
                    $"Pattern binding {name} is acquired more than once on the same route. Use == {name} for a value pattern.",
                    position,
                    MessageClasses.PatternBindingDuplicate
                )
            );
        }
        return new AstBindPattern(position, capture);
    }

    string _createPatternTemporary(string prefix)
    {
        ensureHasTarget();
        var id = generateLocalId("pattern\\" + prefix);
        target.Function.Variables.Add(id);
        return id;
    }

    AstExpr _patternLocalReference(ISourcePosition position, string id)
    {
        return Create.ExprFor(
            position,
            Symbol.CreateDereference(
                Symbol.CreateReference(EntityRef.Variable.Local.Create(id), position),
                position
            )
        );
    }

    AstExpr _currentPatternSubject(ISourcePosition position) =>
        _patternLocalReference(position, _patternSubjectLocals.Peek());

    bool _isPatternAcquisitionSlot(AstExpr? expression, out PatternCapture? capture)
    {
        capture = null;
        var reference = expression switch
        {
            AstIndirectCall { Subject: AstReference indirectReference } => indirectReference,
            AstReference directReference => directReference,
            _ => null,
        };
        if (
            _bindingRoot == null
            || reference == null
            || !reference.Entity.TryGetLocalVariable(out var local)
        )
            return false;
        capture = _bindingRoot.Captures.FirstOrDefault(c =>
            Engine.StringsAreEqual(c.PhysicalId, local.Id)
        );
        return capture != null;
    }

    void _rejectPatternCaptureMutation(AstExpr? expression, ISourcePosition position)
    {
        if (!_isPatternAcquisitionSlot(expression, out var capture))
            return;
        Loader.ReportMessage(
            Message.Error(
                $"Pattern binding {capture!.Name} is read-only while matching is still in progress.",
                position,
                MessageClasses.PatternBindingReadOnly
            )
        );
    }

    AstExpr _patternHelper(ISourcePosition position, string id, params AstExpr[] arguments) =>
        Create.Call(position, EntityRef.Command.Create(id), PCall.Get, arguments);

    AstPatternTest _createPatternTest(
        ISourcePosition position,
        AstExpr subject,
        string subjectLocal,
        AstPattern pattern
    )
    {
        _bindingRoot ??= new PatternBindingRoot();
        if (_prepareDirectPattern(pattern, subject))
            target!.Function.Variables.Remove(subjectLocal);
        return new(
            position,
            subject,
            subjectLocal,
            pattern,
            pattern.GetCaptures().Distinct().ToArray()
        );
    }

    bool _prepareDirectPattern(AstPattern pattern, AstExpr subject)
    {
        switch (pattern)
        {
            case AstWildcardPattern:
            case AstBindPattern:
                return true;
            case AstRelationalPattern relational:
                relational.DirectCondition = Create.BinaryOperation(
                    relational.Position,
                    subject,
                    relational.Operator,
                    relational.Operand
                );
                return true;
            case AstPatternNot not:
                return _prepareDirectPattern(not.Operand, subject);
            case AstPatternAnd
            {
                Left: AstBindPattern,
                Right: AstExpressionPattern { UsesSubjectLocal: false }
            }:
                return true;
            default:
                return false;
        }
    }

    bool _isWildcardPattern() => la.kind == _id && la.val == "_";

    bool _isContextualWhen() => la.kind == _id && Engine.StringsAreEqual(la.val, "when");

    bool _isMatchExpression()
    {
        if (la.kind != _id || !Engine.StringsAreEqual(la.val, "match"))
            return false;

        scanner.ResetPeek();
        var next = scanner.Peek();
        if (next.kind != _lpar)
            return false;

        var depth = 1;
        while (depth > 0)
        {
            next = scanner.Peek();
            if (next.kind == _EOF)
                return false;
            if (next.kind == _lpar)
                depth++;
            else if (next.kind == _rpar)
                depth--;
        }
        return scanner.Peek().kind == _lbrace;
    }

    AstMatchExpression _createMatchExpression(ISourcePosition position, AstExpr subject)
    {
        var subjectLocal = _createPatternTemporary("match\\subject");
        var resultLocal = _createPatternTemporary("match\\result");
        return new AstMatchExpression(position, subject, subjectLocal, resultLocal);
    }

    void _beginMatchArm(AstMatchExpression expression)
    {
        _enterIsolatedBindingRoot();
        _patternSubjectLocals.Push(expression.SubjectLocal);
    }

    void _beginMatchArmExpression(AstPattern pattern)
    {
        _patternSubjectLocals.Pop();
        _pushCaptureEnvironment(pattern.Flow.WhenTrue, true);
    }

    void _finishMatchArm(
        AstMatchExpression expression,
        ISourcePosition position,
        AstPattern pattern,
        AstExpr value
    )
    {
        _popCaptureEnvironment();
        expression.Arms.Add(new AstMatchArm(position, pattern, value));
        _leaveIsolatedBindingRoot();
    }

    bool _isShiftedPatternName()
    {
        if (la.kind != _id)
            return false;
        return Symbols.TryGet(Loader.TypeCheckPrefix + la.val, out _);
    }

    bool _isViewPattern()
    {
        if (la.kind != _id)
            return false;
        scanner.ResetPeek();
        var next = scanner.Peek();
        while (next.kind == _dot)
        {
            if (scanner.Peek().kind != _id)
                return false;
            next = scanner.Peek();
        }
        return next.kind == _lpar;
    }

    AstPattern _combinePatternAnd(ISourcePosition position, AstPattern? left, AstPattern right) =>
        left == null ? right : new AstPatternAnd(position, left, right);

    AstExtractPattern _extractPattern(
        ISourcePosition position,
        AstExpr extractor,
        string outputLocal,
        AstPattern outputPattern,
        bool rejectNull
    )
    {
        if (!rejectNull && _prepareDirectPattern(outputPattern, extractor))
            target!.Function.Variables.Remove(outputLocal);
        var extractorMayBeElided =
            !rejectNull
            && outputPattern is AstWildcardPattern
            && (
                extractor.IsCommandCall(PatternOutputAt.Alias, out _)
                || extractor.IsCommandCall(PatternListSlice.Alias, out _)
            );
        return new AstExtractPattern(
            position,
            extractor,
            outputLocal,
            outputPattern,
            rejectNull,
            extractorMayBeElided
        );
    }

    AstPattern _relationalPattern(ISourcePosition position, BinaryOperator op, AstExpr operand)
    {
        var condition = Create.BinaryOperation(
            position,
            _currentPatternSubject(position),
            op,
            operand
        );
        return new AstRelationalPattern(position, condition, op, operand);
    }

    AstPattern _predicateEscapePattern(ISourcePosition position, AstExpr callable)
    {
        var invocation = Create.IndirectCall(position, callable);
        invocation.Arguments.Add(_currentPatternSubject(position));
        return new AstExpressionPattern(position, invocation);
    }

    AstGetSet _resolveShiftedPatternCall(ISourcePosition position, string prefix, string id)
    {
        var shifted = prefix + id;
        if (
            Symbols.TryGet(shifted, out var symbol)
            && Create.ExprFor(position, symbol) is AstGetSet call
        )
            return call;

        Loader.ReportMessage(
            Message.Error(
                $"Cannot find {shifted} in scope.",
                position,
                MessageClasses.ShiftedSymbolRequired
            )
        );
        return _NullNode(position);
    }

    AstGetSet _resolveShiftedPatternCall(
        ISourcePosition position,
        string prefix,
        IReadOnlyList<string> path
    )
    {
        if (path.Count == 1)
            return _resolveShiftedPatternCall(position, prefix, path[0]);

        AstNamespaceUsage? current = null;
        if (
            Symbols.TryGet(path[0], out var root)
            && Create.ExprFor(position, root) is AstNamespaceUsage rootNamespace
        )
            current = rootNamespace;

        for (var i = 1; current != null && i < path.Count - 1; i++)
        {
            current =
                current.Namespace.TryGet(path[i], out _)
                && _useSymbolFromNamespace(current, path[i], position) is AstNamespaceUsage child
                    ? child
                    : null;
        }

        var shifted = prefix + path[^1];
        if (
            current != null
            && current.Namespace.TryGet(shifted, out _)
            && _useSymbolFromNamespace(current, shifted, position) is AstGetSet call
        )
            return call;

        Loader.ReportMessage(
            Message.Error(
                $"Cannot find {string.Join('.', path.Take(path.Count - 1).Append(shifted))} in scope.",
                position,
                MessageClasses.ShiftedSymbolRequired
            )
        );
        return _NullNode(position);
    }

    AstPattern _propertyMemberPattern(
        ISourcePosition position,
        AstExpr subject,
        string memberName,
        string valueLocal,
        AstPattern valuePattern
    )
    {
        var probeLocal = _createPatternTemporary("member\\probe");
        var probe = _patternHelper(
            position,
            PatternMemberProbe.Alias,
            subject,
            Create.Constant(position, memberName)
        );
        var present = _patternHelper(
            position,
            PatternMemberPresent.Alias,
            _patternLocalReference(position, probeLocal)
        );
        var body = new AstPatternAnd(
            position,
            new AstExpressionPattern(position, present),
            _extractPattern(
                position,
                _patternLocalReference(position, probeLocal),
                valueLocal,
                valuePattern,
                false
            )
        );
        return _extractPattern(position, probe, probeLocal, body, false);
    }

    bool _tryCreatePatternMacro(
        AstGetSet invocation,
        ISourcePosition position,
        [NotNullWhen(true)] out AstPattern? pattern
    )
    {
        pattern = null;
        if (invocation is not AstExpand expansion)
            return false;

        string macroId;
        PatternMacroSignature signature;
        Func<CompilerTarget, PatternMacroExpansionContext, AstPattern> expand;
        if (
            expansion.Entity.TryGetMacroCommand(out var macroReference)
            && macroReference.TryGetEntity(Loader, out var commandEntity)
            && commandEntity.Value is PatternMacroCommand command
        )
        {
            macroId = command.Id;
            try
            {
                signature = command.InvokeDescribePattern(
                    new PatternMacroDescribeContext(invocation)
                );
            }
            catch (Exception exception)
            {
                Loader.ReportMessage(
                    Message.Error(
                        $"Pattern macro {macroId} failed while describing its captures: {exception.Message}",
                        position,
                        MessageClasses.PatternMacroSignatureMismatch
                    )
                );
                pattern = new AstExpressionPattern(position, Create.Constant(position, false));
                return true;
            }
            expand = (_, context) => command.InvokeExpandPattern(context);
        }
        else if (
            expansion.Entity.TryGetFunction(out var functionReference)
            && functionReference.TryGetEntity(Loader, out var functionEntity)
            && functionEntity.Value is PFunction function
            && function.Meta[ScriptPatternMacroRuntime.PatternMacroKey].Switch
        )
        {
            macroId = function.LogicalId;
            try
            {
                ensureHasTarget();
                signature = ScriptPatternMacroRuntime.Describe(target, function, invocation);
            }
            catch (Exception exception)
            {
                Loader.ReportMessage(
                    Message.Error(
                        $"Script pattern macro {macroId} failed while describing its captures: {exception.Message}",
                        position,
                        MessageClasses.PatternMacroSignatureMismatch
                    )
                );
                pattern = new AstExpressionPattern(position, Create.Constant(position, false));
                return true;
            }
            expand = (compilerTarget, context) =>
                ScriptPatternMacroRuntime.Expand(compilerTarget, function, invocation, context);
        }
        else
        {
            return false;
        }

        try
        {
            _bindingRoot ??= new PatternBindingRoot();
            var exported = new Dictionary<string, PatternCapture>(StringComparer.OrdinalIgnoreCase);
            foreach (var description in signature.Captures)
            {
                var capture = _bindingRoot.GetOrCreate(this, description.Name, position, true);
                if (
                    _captureEnvironments.Count > 0
                    && _captureEnvironments.Peek().IsReachable
                    && _captureEnvironments.Peek().Captures.Contains(capture)
                )
                {
                    Loader.ReportMessage(
                        Message.Error(
                            $"Pattern macro {macroId} reacquires {description.Name} on a route where it is already available.",
                            position,
                            MessageClasses.PatternBindingDuplicate
                        )
                    );
                }
                exported.Add(description.Name, capture);
            }
            pattern = new AstPatternMacro(
                position,
                macroId,
                invocation,
                _patternSubjectLocals.Peek(),
                signature,
                exported,
                expand
            );
            return true;
        }
        catch (Exception exception)
        {
            Loader.ReportMessage(
                Message.Error(
                    $"Pattern macro {macroId} failed while freezing its captures: {exception.Message}",
                    position,
                    MessageClasses.PatternMacroSignatureMismatch
                )
            );
            pattern = new AstExpressionPattern(position, Create.Constant(position, false));
            return true;
        }
    }

    IReadOnlyCollection<PatternCapture> _patternCapturesOf(AstExpr expression)
    {
        var captures = new HashSet<PatternCapture>();
        void collect(AstExpr node)
        {
            switch (node)
            {
                case AstPatternTest test:
                    captures.UnionWith(test.Captures);
                    break;
                case AstBindingBinary binary:
                    collect(binary.Left);
                    collect(binary.Right);
                    break;
                case AstBindingNot not:
                    collect(not.Operand);
                    break;
            }
        }
        collect(expression);
        return captures;
    }

    void _declarePatternCaptures(
        SymbolStore symbols,
        IReadOnlyCollection<PatternCapture> captures,
        BindingOutcome valid,
        bool errorsForInvalid
    )
    {
        foreach (var capture in captures)
        {
            if (valid.IsReachable && valid.Captures.Contains(capture))
            {
                symbols.Declare(capture.Name, capture.CreateReadSymbol(GetPosition(), true));
            }
            else if (errorsForInvalid)
            {
                var message = Message.Error(
                    $"Pattern binding {capture.Name} is not available on this control-flow route.",
                    GetPosition(),
                    MessageClasses.PatternBindingUnavailable
                );
                symbols.Declare(
                    capture.Name,
                    Symbol.CreateMessage(message, Symbol.CreateNil(GetPosition()))
                );
            }
        }
    }

    void _promotePatternCaptures(SymbolStore symbols, IReadOnlyCollection<PatternCapture> captures)
    {
        foreach (var capture in captures)
            symbols.Declare(capture.Name, capture.CreateReadSymbol(GetPosition(), true));
    }

    void _reportUnusedPatternBindings(CompilerTarget compilerTarget)
    {
        const string warningKey = "warn_unused_pattern_bindings";
        var enabled = compilerTarget.Function.Meta.ContainsKey(warningKey)
            ? compilerTarget.Function.Meta[warningKey].Switch
            : TargetApplication.Meta[warningKey].Switch;
        if (!enabled)
            return;

        foreach (
            var capture in compilerTarget
                .PatternCaptures.Where(c => !c.IsExplicit && c.SourceReadCount == 0)
                .Distinct()
        )
        {
            Loader.ReportMessage(
                Message.Warning(
                    $"Implicit pattern binding {capture.Name} is never read. Use var {capture.Name} to document an intentional binding or _ to discard it.",
                    capture.Position,
                    MessageClasses.UnusedPatternBinding
                )
            );
        }
    }

    [DebuggerStepThrough]
    internal Parser(IScanner scanner, Loader loader)
        : this(scanner)
    {
        Loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _createTableOfInstructions();
        Ast = new(this);
        Create = new ParserAstFactory(this);
        _referenceTransformer = new(this);
    }

    #region Proxy interface

    public Loader Loader { get; } = null!; // initialized in the only publicly available constructor

    /// <summary>
    /// Preflight mode causes the parser to abort at the
    /// first non-meta construct, giving the user the opportunity
    /// to inspect a file's "header" without fully compiling
    /// that file.
    /// </summary>
    public bool PreflightModeEnabled => Loader.Options.PreflightModeEnabled;

    public bool PatternMatchingEnabled =>
        target?.PatternMatchingEnabled ?? Loader.PatternMatchingEnabled;

    public Application TargetApplication
    {
        [DebuggerStepThrough]
        get => Loader.ParentApplication;
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
        get => Loader.ParentEngine;
    }

    public SymbolStore Symbols
    {
        [DebuggerStepThrough]
        get
        {
            var symbols = target?.CurrentBlock.Symbols ?? Loader.Symbols;
            foreach (var overlay in _captureOverlays.Reverse())
            {
                overlay.ExternalScope = symbols;
                symbols = overlay;
            }
            return symbols;
        }
    }

    DeclarationScopeBuilder _prepareDeclScope(QualifiedId relativeNsId, ISourcePosition idPosition)
    {
        if (relativeNsId.Count < 1)
            throw new ArgumentOutOfRangeException(
                nameof(relativeNsId),
                Resources.Parser_relativeNsId_empty
            );
        var outerScope = Loader.CurrentScope;
        QualifiedId prefix;
        SymbolStore declScopeStore;
        LocalNamespace? surroundingNamespace;
        if (outerScope == null)
        {
            prefix = new(null);
            declScopeStore = Loader.TopLevelSymbols;
            surroundingNamespace = null;
        }
        else
        {
            prefix = outerScope.PathPrefix;
            declScopeStore = outerScope.Store;
            surroundingNamespace = outerScope._LocalNamespace;
        }

        var currentLookupScope = (ISymbolView<Symbol>)declScopeStore;
        var isOutermostNs = true;
        foreach (var nsId in relativeNsId)
        {
            var localNs = _tryGetLocalNamespace(currentLookupScope, nsId, idPosition);

            prefix += new QualifiedId(nsId);

            // Create namespace if necessary
            localNs ??= ((ModuleLevelView)Loader.TopLevelSymbols).CreateLocalNamespace(
                new EmptySymbolView<Symbol>()
            );

            // Make sure the namespace is exported from the current module and not just accessible via external declarations
            _declareNamespaceAsExported(
                surroundingNamespace,
                declScopeStore,
                nsId,
                isOutermostNs,
                idPosition,
                localNs,
                prefix
            );

            surroundingNamespace = localNs;
            currentLookupScope = localNs;
            if (isOutermostNs)
                isOutermostNs = false;
        }

        // This should never happen, check is here to convey this fact to null-analysis
        if (surroundingNamespace == null)
            throw new PrexoniteException(
                "Failed to create the innermost namespace of " + relativeNsId + "."
            );

        // Create the local scope of this namespace *declaration* (the scope inside the braces)
        // Not that this is almost completely independent of the namespace itself
        // It is just used as one possible source for exports
        var builder = SymbolStoreBuilder.Create(declScopeStore);
        return new(builder, prefix, surroundingNamespace);
    }

    class DeclarationScopeBuilder
    {
        public DeclarationScopeBuilder(
            SymbolStoreBuilder localScopeBuilder,
            QualifiedId prefix,
            LocalNamespace ns
        )
        {
            LocalScopeBuilder =
                localScopeBuilder ?? throw new ArgumentNullException(nameof(localScopeBuilder));
            Prefix = prefix;
            Namespace = ns ?? throw new ArgumentNullException(nameof(ns));
        }

        public SymbolStoreBuilder LocalScopeBuilder { get; }

        public QualifiedId Prefix { get; }

        public LocalNamespace Namespace { get; }

        public DeclarationScope ToDeclarationScope()
        {
            return new(Namespace, Prefix, LocalScopeBuilder.ToSymbolStore());
        }
    }

    LocalNamespace? _tryGetLocalNamespace(
        ISymbolView<Symbol> currentSurrounding,
        string superNsId,
        ISourcePosition idPosition
    )
    {
        LocalNamespace? localNs = null;
        if (currentSurrounding.TryGet(superNsId, out var sym))
        {
            var fakeExpr = Create.ExprFor(idPosition, sym);
            if (fakeExpr is not AstNamespaceUsage nsUsage)
                Loader.ReportMessage(
                    Message.Error(
                        string.Format(Resources.Parser_NamespaceExpected, superNsId, sym),
                        idPosition,
                        MessageClasses.NamespaceExcepted
                    )
                );
            else
            {
                // namespace already exists
                localNs = nsUsage.Namespace as LocalNamespace;
                if (localNs == null)
                {
                    Loader.ReportMessage(
                        Message.Error(
                            string.Format(Resources.Parser_CannotExtendMergedNamespace, superNsId),
                            idPosition,
                            MessageClasses.CannotExtendMergedNamespace
                        )
                    );
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
    static void _declareNamespaceAsExported(
        LocalNamespace? surroundingNamespace,
        SymbolStore outer,
        string superNsId,
        bool isOutermostNs,
        ISourcePosition idPosition,
        LocalNamespace localNs,
        QualifiedId nextPrefix
    )
    {
        var nsSym = Symbol.CreateNamespace(localNs, idPosition);
        Symbol? existingSym;
        if (isOutermostNs)
        {
            // The outermost namespace (x in x.y.z) is declared as an ordinary symbol in the current scope
            if (
                !outer.IsDeclaredLocally(superNsId)
                || !(outer.TryGet(superNsId, out existingSym) && existingSym.Equals(nsSym))
            )
            {
                outer.Declare(superNsId, nsSym);
            }
        }
        else if (surroundingNamespace == null)
            throw new PrexoniteException(
                "Failed to create surrounding namespace (syntactic sugar for nested namespace)"
            );
        else
        {
            // Inner namespaces (z and y in x.y.z) are exported from their respective super-namespaces
            if (
                !surroundingNamespace.TryGetExported(superNsId, out existingSym)
                || !existingSym.Equals(nsSym)
            )
            {
                surroundingNamespace.DeclareExports(
                    new KeyValuePair<string, Symbol>(superNsId, nsSym).Singleton()
                );
            }
        }

        localNs.Prefix ??= nextPrefix.ToString().Replace('.', '\\');
    }

    ISymbolView<Symbol>? _resolveNamespace(
        ISymbolView<Symbol> scope,
        ISourcePosition qualifiedIdPosition,
        QualifiedId qualifiedId
    )
    {
        while (qualifiedId.Count > 0)
        {
            scope.TryGet(qualifiedId[0], out var sym);
            var expr = Create.ExprFor(qualifiedIdPosition, sym);
            if (expr is not AstNamespaceUsage nsUsage)
            {
                Create.ReportMessage(
                    Message.Error(
                        string.Format(
                            Resources.Parser_NamespaceExpected,
                            qualifiedId[0],
                            sym == null ? "not defined" : sym.ToString()
                        ),
                        qualifiedIdPosition,
                        MessageClasses.NamespaceExcepted
                    )
                );
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

    void _updateNamespace(DeclarationScope scope, SymbolStoreBuilder builder)
    {
        scope._LocalNamespace.DeclareExports(builder.ToSymbolStore());
    }

    SymbolOrigin _privateDeclarationOrigin(ISourcePosition position, DeclarationScope scope)
    {
        return new SymbolOrigin.NamespaceDeclarationScope(position, scope.PathPrefix);
    }

    DeclarationScope _popDeclScope()
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
    ISymbolView<Symbol> _indexExportedSymbols(SymbolStore declStore)
    {
        var index = SymbolStore.Create();
        foreach (var symbol in declStore.LocalDeclarations)
            index.Declare(symbol.Key, symbol.Value);
        return index;
    }

    string _assignPhysicalFunctionSlot(string? primaryId)
    {
        return _assignPhysicalSlot(primaryId ?? Engine.GenerateName("f"));
    }

    string _assignPhysicalSlot(string id)
    {
        var scope = Loader.CurrentScope;
        return scope == null ? id : scope._LocalNamespace.DerivePhysicalName(id);
    }

    string _assignPhysicalGlobalVariableSlot(string? primaryId)
    {
        return _assignPhysicalSlot(primaryId ?? Engine.GenerateName("v"));
    }

    public Loader.FunctionTargetsIterator FunctionTargets
    {
        [DebuggerStepThrough]
        get => Loader.FunctionTargets;
    }

    public AstProxy Ast { get; } = null!; // initialized in the only publicly available constructor

    [DebuggerStepThrough]
    public class AstProxy
    {
        readonly Parser outer;

        internal AstProxy(Parser outer)
        {
            this.outer = outer;
        }

        public AstBlock this[PFunction func]
        {
            get
            {
                var target = outer.Loader.FunctionTargets[func];
                if (target == null)
                {
                    throw new PrexoniteException(
                        "Internal error: tried to access AST of function that is not part of this compilation unit: "
                            + func
                    );
                }
                return target.Ast;
            }
        }
    }

    public CompilerTarget? target { [DebuggerStepThrough] get; private set; }

    protected int LocalState
    {
        get
        {
            bool flagLiteralsEnabled;
            if (
                target != null
                && target.Meta.TryGetValue(Shell.FlagLiteralsKey, out var flagSwitch)
            )
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

    public AstBlock? CurrentBlock => target?.CurrentBlock;

    protected IAstFactory Create { get; } = null!; // initialized in the only publicly available constructor
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
        la = new()
        {
            kind = _EOF,
            pos = la.pos,
            line = la.pos,
            col = la.col,
            val = "",
        };
    }

    #region Helper

    #region General

    static string _removeSingleQuotes(string s)
    {
        return s.Replace("'", "");
    }

    public static bool TryParseInteger(string s, out int i)
    {
        return int.TryParse(
            _removeSingleQuotes(s),
            IntegerStyle,
            CultureInfo.InvariantCulture,
            out i
        );
    }

    public static bool TryParseReal(string s, out double d)
    {
        return double.TryParse(
            _removeSingleQuotes(s),
            RealStyle,
            CultureInfo.InvariantCulture,
            out d
        );
    }

    public static bool TryParseVersion(string? s, [NotNullWhen(true)] out Version? version)
    {
        return System.Version.TryParse(s, out version);
    }

    public static NumberStyles RealStyle =>
        NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

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

    void _pushLexerState(int state)
    {
        if (scanner is not Lexer lex)
            throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
        lex.PushState(state);
    }

    void _popLexerState()
    {
        if (scanner is not Lexer lex)
            throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
        lex.PopState();
        //Might be id or keyword
        if (la.kind is > _BEGINKEYWORDS and < _ENDKEYWORDS or _id)
            la.kind = lex.checkKeyword(la.val);
    }

    void _inject(Token c)
    {
        if (scanner is not Lexer lex)
            throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");

        if (c == null)
            throw new ArgumentNullException(nameof(c));

        lex._InjectToken(c);
    }

    void _inject(int kind, string val)
    {
        if (val == null)
            throw new ArgumentNullException(nameof(val));
        var c = new Token { kind = kind, val = val };
        _inject(c);
    }

    void _inject(int kind)
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
        if (TargetModule.Variables.TryGetVariable(id, out var x))
        {
            vari = x;
            return TargetApplication.Variables[vari.Id]
                ?? throw new PrexoniteException(
                    "Internal error: declared variable is not reflected in application."
                );
        }
        else
        {
            vari = global::Prexonite.Modular.VariableDeclaration.Create(id);
            TargetModule.Variables.Add(vari);
            return TargetApplication.Variables[id] = new(vari);
        }
    }

    void _compileAndExecuteBuildBlock(CompilerTarget buildBlockTarget)
    {
        if (errors.count > 0)
        {
            Loader.ReportMessage(
                Message.Error(
                    Resources.Parser_ErrorsInBuildBlock,
                    GetPosition(),
                    MessageClasses.ErrorsInBuildBlock
                )
            );
            return;
        }

        //Emit code for top-level build block
        try
        {
            buildBlockTarget.Ast.EmitCode(buildBlockTarget, true, StackSemantics.Effect);

            buildBlockTarget.Function.Meta[nameof(File)] = scanner.File;
            buildBlockTarget.FinishTarget();
            //Run the build block
            var fctx = buildBlockTarget.Function.CreateFunctionContext(ParentEngine, [], [], true);
            object? token = null;
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
            Loader.ReportMessage(
                Message.Error(
                    string.Format(Resources.Parser_exception_in_build_block, e),
                    GetPosition(),
                    MessageClasses.ExceptionDuringCompilation
                )
            );
        }
    }

    bool _suppressPrimarySymbol(IHasMetaTable ihmt)
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

    readonly Stack<object> _scopeStack = new();

    [MemberNotNull(nameof(target), nameof(CurrentBlock))]
    void ensureHasTarget([CallerMemberName] string? caller = null)
    {
        if (target == null || CurrentBlock == null)
        {
            throw new PrexoniteException(
                $"Internal error: {caller} does not have access to a compiler target."
            );
        }
    }

    [MemberNotNull(nameof(target), nameof(CurrentBlock))]
    internal void _PushScope(AstScopedBlock block)
    {
        if (!ReferenceEquals(block.LexicalScope, CurrentBlock))
            throw new PrexoniteException("Cannot push scope of unrelated block.");
        ensureHasTarget();
        _scopeStack.Push(block);
        target.BeginBlock(block);
        ensureHasTarget();
    }

    [MemberNotNull(nameof(target), nameof(CurrentBlock))]
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
        ensureHasTarget();
    }

    internal void _PopScope(AstScopedBlock block)
    {
        if (!ReferenceEquals(_scopeStack.Peek(), block))
            throw new PrexoniteException(
                $"Tried to pop scope of block {block} but {_scopeStack.Peek()} was on top."
            );
        ensureHasTarget();
        _scopeStack.Pop();
        target.EndBlock();
    }

    internal void _PopScope(CompilerTarget ct)
    {
        if (!ReferenceEquals(_scopeStack.Peek(), ct))
            throw new PrexoniteException(
                $"Tried to pop scope of compiler target {ct} but {_scopeStack.Peek()} was on top."
            );
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

        return c.kind == _namespace
            && cla.kind == _id
            && cla.val.Equals("import", StringComparison.InvariantCultureIgnoreCase);
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

    bool isFollowedByStatementBlock()
    {
        scanner.ResetPeek();
        return scanner.Peek().kind == _lbrace;
    }

    bool isLambdaExpression() //LL(*)
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
                !(
                    next.kind == _comma
                    || next.kind == _implementation
                    || next.kind == _rpar && requirePar
                )
            )
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
                !(
                    current.kind == _comma
                    || current.kind == _implementation
                    || current.kind == _rpar && requirePar
                )
            )
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
    bool isIndirectCall() //LL(2)
    {
        scanner.ResetPeek();
        var c = la;
        var cla = scanner.Peek();

        return c.kind == _dot && cla.kind == _lpar;
    }

    // used to distinguish asm{...} and asm(...)
    bool isAsmBlock() //LL(2)
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
    [MemberNotNull(nameof(target))]
    bool isOuterVariable(string id) //context
    {
        ensureHasTarget();
        return target._IsOuterVariable(id);
    }

    string generateLocalId(string prefix = "")
    {
        ensureHasTarget();
        return target.GenerateLocalId(prefix);
    }

    Symbol _ensureDefinedLocal(
        string localAlias,
        string physicalId,
        bool isAutodereferenced,
        ISourcePosition declPos,
        bool isOverrideDecl
    )
    {
        var refSym = Symbol.CreateDereference(
            Symbol.CreateReference(EntityRef.Variable.Local.Create(physicalId), declPos),
            declPos
        );
        var sym = isAutodereferenced ? Symbol.CreateDereference(refSym, declPos) : refSym;

        ensureHasTarget();
        target.Symbols.Declare(localAlias, sym);
        if (
            !isOverrideDecl
            && !target.Function.Variables.Contains(physicalId)
            && isOuterVariable(physicalId)
        )
        {
            target.RequireOuterVariable(physicalId);
        }
        else if (!target.Function.Variables.Contains(physicalId))
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
    AstGetSet _useSymbol(ISymbolView<Symbol> scope, string id, ISourcePosition position)
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
            if (complex is AstNamespaceUsage { ReferencePath: null } nsu)
            {
                nsu.ReferencePath = new QualifiedId(id);
            }
            return complex;
        }
        Loader.ReportMessage(
            Message.Error(
                Resources.Parser_SymbolicUsageAsLValue,
                position,
                MessageClasses.ParserInternal
            )
        );
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
    AstGetSet _useSymbolFromNamespace(AstNamespaceUsage ns, string id, ISourcePosition position)
    {
        var expr = _useSymbol(ns.Namespace, id, position);
        // write down qualified path
        if (expr is AstNamespaceUsage { ReferencePath: null } subNs)
        {
            subNs.ReferencePath = ns.ReferencePath + new QualifiedId(id);
        }

        return expr;
    }

    Symbol _parseSymbol(MExpr expr)
    {
        try
        {
            var parser = new SymbolMExprParser(Symbols, Loader, Loader.TopLevelSymbols);
            return parser.Parse(expr);
        }
        catch (ErrorMessageException e)
        {
            Loader.ReportMessage(e.CompilerMessage);
            return Symbol.CreateNil(e.CompilerMessage.Position);
        }
    }

    [DebuggerStepThrough]
    static bool isId(Token c)
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
    static bool isGlobalId(Token c)
    {
        return c.kind is _id or _anyId;
    }

    bool _isNotNewDecl()
    {
        if (la.kind != _new)
            return false;

        scanner.ResetPeek();
        var varTok = scanner.Peek();

        return varTok.kind != _var && varTok.kind != _ref;
    }

    #region Assemble Invocation of Symbol

    class ReferenceTransformer : SymbolHandler<int, Tuple<Symbol, bool>>
    {
        readonly Parser _parser;

        public ReferenceTransformer(Parser parser)
        {
            _parser = parser;
        }

        protected override Tuple<Symbol, bool> HandleWrappingSymbol(
            WrappingSymbol self,
            int argument
        )
        {
            if (argument == 0)
                return Tuple.Create<Symbol, bool>(self, false);
            else
            {
                var innerResult = self.InnerSymbol.HandleWith(this, argument);
                return Tuple.Create<Symbol, bool>(self.With(innerResult.Item1), innerResult.Item2);
            }
        }

        protected override Tuple<Symbol, bool> HandleLeafSymbol(Symbol self, int argument)
        {
            if (argument > 0)
            {
                throw new ErrorMessageException(
                    Message.Error(
                        Resources.ReferenceTransformer_CannotCreateReferenceToValue,
                        _parser.GetPosition(),
                        MessageClasses.CannotCreateReference
                    )
                );
            }
            else
            {
                return Tuple.Create(self, false);
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
                    Message.Error(
                        Resources.ReferenceTransformer_HandleExpand_CannotCreateReferenceToDefinitionOfMacroOrPartialApplication,
                        _parser.GetPosition(),
                        MessageClasses.CannotCreateReference
                    )
                );
            }
            else if (argument == 1)
            {
                // Here, we don't actually remove the expansion, but rather switch the
                //  flag to true to indicate that the calling procedure should
                //  convert the resulting expansion node into a partial application.
                return new(base.HandleExpand(self, argument - 1).Item1, true);
            }
            else
            {
                return base.HandleExpand(self, argument);
            }
        }
    }

    readonly ReferenceTransformer _referenceTransformer = null!; // initialized in the only publicly available constructor

    AstExpr _assembleReference(string id, int ptrCount)
    {
        Debug.Assert(id != null);
        Debug.Assert(ptrCount > 0);

        var position = GetPosition();
        if (!Symbols.TryGet(id, out var symbol))
        {
            Loader.ReportMessage(
                Message.Error(
                    string.Format(Resources.Parser__assembleReference_SymbolNotDefined, id),
                    position,
                    MessageClasses.SymbolNotResolved
                )
            );
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
                    if (invocation is not AstGetSet invocationCall)
                    {
                        Loader.ReportMessage(
                            Message.Error(
                                Resources.Parser__assembleReference_MacroDefinitionNotLValue,
                                position,
                                MessageClasses.ParserInternal
                            )
                        );
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

    static readonly SymbolShift _objectCreationShift = static id =>
        Loader.ObjectCreationPrefix + id;
    static readonly SymbolShift _conversionShift = static id => Loader.ConversionPrefix + id;
    static readonly SymbolShift _typeCheckShift = static id => Loader.TypeCheckPrefix + id;
    static readonly SymbolShift _staticCallShift = static id => Loader.StaticCallPrefix + id;

    /// <summary>
    /// Given the partial application of a type check <c>(? is T)</c>, this method will construct the
    /// negated partial application <c>(? is not T)</c> by chaining the negation with <c>then</c>:
    /// <c>(? is T) then (not ?)</c>.
    /// </summary>
    /// <param name="position">The position of the overall expression.</param>
    /// <param name="check">The (non-inverted) type check.</param>
    /// <returns>The partial application of an inverted type check.</returns>
    AstExpr _createPartialInvertedTypeCheck(ISourcePosition position, AstExpr check)
    {
        // Special handling of "? is not Y" as that's not the same thing as "not (? is Y)"
        // when placeholders are involved.
        Debug.Assert(check.CheckForPlaceholders(), "check is expected to have placeholders");
        ensureHasTarget();

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
                    position,
                    MessageClasses.LValueExpected
                )
            );
            notCall = _NullNode(position);
        }

        notCall.Arguments.Add(Create.Placeholder(position, 0));

        // Assemble "(? is T) then (not ?)"
        var thenCmd = Create.Call(position, EntityRef.Command.Create(Engine.ThenAlias));
        thenCmd.Arguments.Add(check);
        thenCmd.Arguments.Add(notCall);

        return thenCmd;
    }

    AstExpr _createUnknownExpr()
    {
        return _createUnknownGetSet();
    }

    AstGetSet _createUnknownGetSet()
    {
        var pos = GetPosition();
        return Create.IndirectCall(pos, Create.Null(pos));
    }

    void _appendRight(AstExpr lhs, AstGetSet rhs)
    {
        _appendRight(lhs.Singleton(), rhs);
    }

    void _appendRight(IEnumerable<AstExpr> lhs, AstGetSet rhs)
    {
        rhs.Arguments.RightAppend(lhs);
        rhs.Arguments.ReleaseRightAppend();
        AstIndirectCall? indirectCallNode;
        AstReference? refNode;
        if (
            (indirectCallNode = rhs as AstIndirectCall) != null
            && (refNode = indirectCallNode.Subject as AstReference) != null
            && refNode.Entity.TryGetVariable(out _)
        )
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

    public void addOpAlias(AstBlock block, string insBase, string? detail)
    {
        var alias = getOpAlias(insBase, detail, out var argc);
        if (alias == null)
        {
            Loader.ReportMessage(
                Message.Error(
                    string.Format(Resources.Parser_addOpAlias_Unknown, insBase, detail),
                    GetPosition(),
                    MessageClasses.UnknownAssemblyOperator
                )
            );
            block.Add(new AstAsmInstruction(this, new(OpCode.nop)));
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
    bool isAsmInstruction(string insBase, string? detail) //LL(4)
    {
        scanner.ResetPeek();
        var la1 = la.kind == _at ? scanner.Peek() : la;
        var la2 = scanner.Peek();
        var la3 = scanner.Peek();
        return checkAsmInstruction(la1, la2, la3, insBase, detail);
    }

    static bool checkAsmInstruction(Token la1, Token la2, Token la3, string insBase, string? detail)
    {
        return la1.kind != _string
            && Engine.StringsAreEqual(la1.val, insBase)
            && (
                detail == null
                    ? la2.kind != _dot || la3.kind == _integer
                    : la2.kind == _dot
                        && la3.kind != _string
                        && Engine.StringsAreEqual(la3.val, detail)
            );
    }

    [DebuggerStepThrough]
    bool isInIntegerGroup()
    {
        return peekIsOneOf(asmIntegerGroup);
    }

    [DebuggerStepThrough]
    bool isInJumpGroup()
    {
        return peekIsOneOf(asmJumpGroup);
    }

    bool isInOpAliasGroup()
    {
        return peekIsOneOf(asmOpAliasGroup);
    }

    [DebuggerStepThrough]
    bool isInNullGroup()
    {
        return peekIsOneOf(asmNullGroup);
    }

    [DebuggerStepThrough]
    bool isInIdGroup()
    {
        return peekIsOneOf(asmIdGroup);
    }

    [DebuggerStepThrough]
    bool isInIdArgGroup()
    {
        return peekIsOneOf(asmIdArgGroup);
    }

    [DebuggerStepThrough]
    bool isInArgGroup()
    {
        return peekIsOneOf(asmArgGroup);
    }

    [DebuggerStepThrough]
    bool isInQualidArgGroup()
    {
        return peekIsOneOf(asmQualidArgGroup);
    }

    //[NoDebug()]
    bool peekIsOneOf(string?[,] table)
    {
        scanner.ResetPeek();
        var la1 = la.kind == _at ? scanner.Peek() : la;
        var la2 = scanner.Peek();
        var la3 = scanner.Peek();
        for (var i = table.GetUpperBound(0); i >= 0; i--)
            if (checkAsmInstruction(la1, la2, la3, table[i, 0]!, table[i, 1]))
                return true;
        return false;
    }

    readonly SymbolTable<OpCode> _instructionNameTable = new(60);

    readonly SymbolTable<Tuple<string, int>> _opAliasTable = new(32);

    [DebuggerStepThrough]
    void _createTableOfInstructions()
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

    string? getOpAlias(string insBase, string? detail, out int argc)
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
    OpCode getOpCode(string insBase, string? detail)
    {
        var combined = insBase + (detail == null ? "" : "." + detail);
        return _instructionNameTable.GetDefault(combined, OpCode.invalid);
    }

    #region Instruction tables

    readonly string?[,] asmIntegerGroup =
    {
        { "ldc", "int" },
        { "pop", null },
        { "dup", null },
        { "ldloci", null },
        { "stloci", null },
        { "incloci", null },
        { "inci", null },
        { "deci", null },
        { "ldr", "loci" },
    };

    readonly string?[,] asmJumpGroup =
    {
        { "jump", null },
        { "jump", nameof(t) },
        { "jump", "f" },
        { "jump", "true" },
        { "jump", "false" },
        { "leave", null },
    };

    readonly string?[,] asmNullGroup =
    {
        { "ldc", "null" },
        { "ldnull", null },
        { "check", "arg" },
        { "check", "null" },
        { "cast", "arg" },
        { "ldr", "eng" },
        { "ldr", "app" },
        { "ret", null },
        { "ret", nameof(set) },
        { "ret", "value" },
        { "ret", "val" },
        { "ret", "break" },
        { "ret", "continue" },
        { "ret", "exit" },
        { "break", null },
        { "continue", null },
        { "exit", null },
        { "try", null },
        { "throw", null },
        { "exc", null },
        { "exception", null },
    };

    readonly string?[,] asmOpAliasGroup =
    {
        { "neg", null },
        { "not", null },
        { "add", null },
        { "sub", null },
        { "mul", null },
        { "div", null },
        { "mod", null },
        { "pow", null },
        { "ceq", null },
        { "cne", null },
        { "clt", null },
        { "cle", null },
        { "cgt", null },
        { "cge", null },
        { "or", null },
        { "and", null },
        { "xor", null },
    };

    readonly string?[,] asmIdGroup =
    {
        { "inc", null },
        { "incloc", null },
        { "incglob", null },
        { "dec", null },
        { "decloc", null },
        { "decglob", null },
        { "ldc", "string" },
        { "ldr", "func" },
        { "ldr", "cmd" },
        { "ldr", "glob" },
        { "ldr", "loc" },
        { "ldr", "type" },
        { "ldloc", null },
        { "stloc", null },
        { "ldglob", null },
        { "stglob", null },
        { "check", "const" },
        { "check", null },
        { "cast", "const" },
        { "cast", null },
        { "newclo", null },
    };

    readonly string?[,] asmIdArgGroup =
    {
        { "newtype", null },
        { "get", null },
        { "set", null },
        { "func", null },
        { "cmd", null },
        { "indloc", null },
        { "indglob", null },
    };

    readonly string?[,] asmArgGroup =
    {
        { "indarg", null },
        { "inda", null },
    };

    readonly string?[,] asmQualidArgGroup =
    {
        { "sget", null },
        { "sset", null },
        { "newobj", null },
        { "new", null },
    };

    #endregion

    #endregion

    #endregion
}

// ReSharper restore InconsistentNaming
