using System.Collections.Immutable;
using System.Globalization;
using Prexonite.Commands;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

/// <summary>An expression that can branch without first materializing a boolean.</summary>
public interface IAstJumpCondition
{
    void EmitJumpCondition(CompilerTarget target, string trueLabel, string falseLabel);
}

/// <summary>A compiler-owned capture. Names are source-facing; physical ids are hygienic locals.</summary>
public sealed class PatternCapture
{
    internal PatternCapture(
        string name,
        string physicalId,
        string publicId,
        ISourcePosition position,
        bool isExplicit
    )
    {
        Name = name;
        PhysicalId = physicalId;
        PublicId = publicId;
        Position = position;
        IsExplicit = isExplicit;
    }

    public string Name { get; }
    public string PhysicalId { get; }
    public string PublicId { get; }
    public ISourcePosition Position { get; }
    public bool IsExplicit { get; internal set; }
    public int SourceReadCount { get; private set; }

    public Symbol CreateReadSymbol(ISourcePosition position, bool usePublicSlot = false) =>
        new PatternCaptureSymbol(
            this,
            Symbol.CreateDereference(
                Symbol.CreateReference(
                    EntityRef.Variable.Local.Create(usePublicSlot ? PublicId : PhysicalId),
                    position
                ),
                position
            ),
            position
        );

    internal void RecordSourceRead() => SourceReadCount++;
}

sealed class PatternCaptureSymbol : Symbol
{
    readonly PatternCapture _capture;
    readonly Symbol _inner;

    public PatternCaptureSymbol(PatternCapture capture, Symbol inner, ISourcePosition position)
    {
        _capture = capture;
        _inner = inner;
        Position = position;
    }

    public override ISourcePosition Position { get; }

    public override TResult HandleWith<TArg, TResult>(
        ISymbolHandler<TArg, TResult> handler,
        TArg argument
    )
    {
        _capture.RecordSourceRead();
        return _inner.HandleWith(handler, argument);
    }

    public override bool Equals(Symbol? other) =>
        other is PatternCaptureSymbol symbol && ReferenceEquals(_capture, symbol._capture);

    public override int GetHashCode() => _capture.GetHashCode();
}

public readonly record struct BindingOutcome(
    bool IsReachable,
    ImmutableHashSet<PatternCapture> Captures
)
{
    public static BindingOutcome Bottom => new(false, ImmutableHashSet<PatternCapture>.Empty);
    public static BindingOutcome Empty => new(true, ImmutableHashSet<PatternCapture>.Empty);

    public BindingOutcome Add(PatternCapture capture) =>
        IsReachable ? new(true, Captures.Add(capture)) : this;

    public static BindingOutcome Intersect(BindingOutcome left, BindingOutcome right)
    {
        if (!left.IsReachable)
            return right;
        if (!right.IsReachable)
            return left;
        return new(true, left.Captures.Intersect(right.Captures).ToImmutableHashSet());
    }

    public static BindingOutcome Sequence(BindingOutcome left, BindingOutcome right)
    {
        if (!left.IsReachable || !right.IsReachable)
            return Bottom;
        return new(true, left.Captures.Union(right.Captures).ToImmutableHashSet());
    }
}

public sealed record BindingFlow(BindingOutcome WhenTrue, BindingOutcome WhenFalse)
{
    public static BindingFlow Ordinary { get; } = new(BindingOutcome.Empty, BindingOutcome.Empty);
    public static BindingFlow Always { get; } = new(BindingOutcome.Empty, BindingOutcome.Bottom);

    public BindingFlow Not() => new(WhenFalse, WhenTrue);

    public static BindingFlow And(BindingFlow left, BindingFlow right) =>
        new(
            BindingOutcome.Sequence(left.WhenTrue, right.WhenTrue),
            BindingOutcome.Intersect(
                left.WhenFalse,
                BindingOutcome.Sequence(left.WhenTrue, right.WhenFalse)
            )
        );

    public static BindingFlow Or(BindingFlow left, BindingFlow right) =>
        new(
            BindingOutcome.Intersect(
                left.WhenTrue,
                BindingOutcome.Sequence(left.WhenFalse, right.WhenTrue)
            ),
            BindingOutcome.Sequence(left.WhenFalse, right.WhenFalse)
        );
}

/// <summary>Compiler-only pattern tree. Patterns branch directly and never exist at runtime.</summary>
public abstract class AstPattern : AstNode
{
    protected AstPattern(ISourcePosition position, BindingFlow flow)
        : base(position)
    {
        Flow = flow;
    }

    public BindingFlow Flow { get; }

    internal abstract void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    );

    internal virtual bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    ) => false;

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics) =>
        throw new NotSupportedException("Patterns are emitted only as part of a pattern test.");

    internal virtual IEnumerable<PatternCapture> GetCaptures() => [];

    internal static void EmitConsumerCopies(
        CompilerTarget target,
        ISourcePosition position,
        IEnumerable<PatternCapture> captures,
        BindingOutcome outcome
    )
    {
        foreach (var capture in captures.Distinct())
        {
            if (outcome.IsReachable && outcome.Captures.Contains(capture))
                target.EmitLoadLocal(position, capture.PhysicalId);
            else
                target.EmitNull(position);
            target.EmitStoreLocal(position, capture.PublicId);
        }
    }
}

public sealed class AstWildcardPattern : AstPattern
{
    public AstWildcardPattern(ISourcePosition position)
        : base(position, BindingFlow.Always) { }

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    ) => target.EmitJump(Position, trueLabel);

    internal override bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    )
    {
        subject.EmitEffectCode(target);
        target.EmitJump(Position, trueLabel);
        return true;
    }
}

public sealed class AstBindPattern : AstPattern
{
    public AstBindPattern(ISourcePosition position, PatternCapture capture)
        : base(position, new(new(true, ImmutableHashSet.Create(capture)), BindingOutcome.Bottom))
    {
        Capture = capture;
    }

    public PatternCapture Capture { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() => [Capture];

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        target.EmitLoadLocal(Position, subjectLocal);
        target.EmitStoreLocal(Position, Capture.PhysicalId);
        target.EmitJump(Position, trueLabel);
    }

    internal override bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    )
    {
        subject.EmitValueCode(target);
        target.EmitStoreLocal(Position, Capture.PhysicalId);
        target.EmitJump(Position, trueLabel);
        return true;
    }
}

public sealed class AstExpressionPattern : AstPattern, IAstHasExpressions
{
    public AstExpressionPattern(
        ISourcePosition position,
        AstExpr condition,
        bool usesSubjectLocal = true
    )
        : base(position, BindingFlow.Ordinary)
    {
        Condition = condition;
        UsesSubjectLocal = usesSubjectLocal;
    }

    public AstExpr Condition { get; }
    public bool UsesSubjectLocal { get; }
    public AstExpr[] Expressions => [Condition];

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    ) => AstLazyLogical.EmitJumpCondition(target, Condition, trueLabel, falseLabel);
}

/// <summary>
///     A relational pattern whose direct condition can embed the original subject expression.
/// </summary>
public sealed class AstRelationalPattern : AstPattern, IAstHasExpressions
{
    public AstRelationalPattern(
        ISourcePosition position,
        AstExpr condition,
        BinaryOperator @operator,
        AstExpr operand
    )
        : base(position, BindingFlow.Ordinary)
    {
        Condition = condition;
        Operator = @operator;
        Operand = operand;
    }

    public AstExpr Condition { get; }
    public BinaryOperator Operator { get; }
    public AstExpr Operand { get; }
    internal AstExpr? DirectCondition { get; set; }
    public AstExpr[] Expressions => [Condition];

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    ) => AstLazyLogical.EmitJumpCondition(target, Condition, trueLabel, falseLabel);

    internal override bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    )
    {
        if (DirectCondition == null)
            return false;
        AstLazyLogical.EmitJumpCondition(target, DirectCondition, trueLabel, falseLabel);
        return true;
    }
}

public sealed class AstPatternAnd : AstPattern
{
    public AstPatternAnd(ISourcePosition position, AstPattern left, AstPattern right)
        : base(position, BindingFlow.And(left.Flow, right.Flow))
    {
        Left = left;
        Right = right;
    }

    public AstPattern Left { get; }
    public AstPattern Right { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() =>
        Left.GetCaptures().Concat(Right.GetCaptures());

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        var next = $"pattern\\and\\{Guid.NewGuid():N}";
        Left.EmitBranch(target, subjectLocal, next, falseLabel);
        target.EmitLabel(Position, next);
        Right.EmitBranch(target, subjectLocal, trueLabel, falseLabel);
        target.FreeLabel(next);
    }

    internal override bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    )
    {
        if (
            Left is not AstBindPattern bind
            || Right is not AstExpressionPattern { UsesSubjectLocal: false } guard
        )
            return false;

        var next = $"pattern\\direct\\and\\{Guid.NewGuid():N}";
        bind.TryEmitDirectBranch(target, subject, next, falseLabel);
        target.EmitLabel(Position, next);
        guard.EmitBranch(target, "", trueLabel, falseLabel);
        target.FreeLabel(next);
        return true;
    }
}

public sealed class AstPatternOr : AstPattern
{
    public AstPatternOr(ISourcePosition position, AstPattern left, AstPattern right)
        : base(position, BindingFlow.Or(left.Flow, right.Flow))
    {
        Left = left;
        Right = right;
    }

    public AstPattern Left { get; }
    public AstPattern Right { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() =>
        Left.GetCaptures().Concat(Right.GetCaptures());

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        var next = $"pattern\\or\\{Guid.NewGuid():N}";
        Left.EmitBranch(target, subjectLocal, trueLabel, next);
        target.EmitLabel(Position, next);
        Right.EmitBranch(target, subjectLocal, trueLabel, falseLabel);
        target.FreeLabel(next);
    }
}

public sealed class AstPatternNot : AstPattern
{
    public AstPatternNot(ISourcePosition position, AstPattern operand)
        : base(position, operand.Flow.Not())
    {
        Operand = operand;
    }

    public AstPattern Operand { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() => Operand.GetCaptures();

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    ) => Operand.EmitBranch(target, subjectLocal, falseLabel, trueLabel);

    internal override bool TryEmitDirectBranch(
        CompilerTarget target,
        AstExpr subject,
        string trueLabel,
        string falseLabel
    ) => Operand.TryEmitDirectBranch(target, subject, falseLabel, trueLabel);
}

public sealed class AstPatternMacro : AstPattern
{
    readonly IReadOnlyDictionary<string, PatternCapture> _exported;
    readonly Func<CompilerTarget, PatternMacroExpansionContext, AstPattern> _expand;

    public AstPatternMacro(
        ISourcePosition position,
        string macroId,
        AstGetSet invocation,
        string subjectLocal,
        PatternMacroSignature signature,
        IReadOnlyDictionary<string, PatternCapture> exported,
        Func<CompilerTarget, PatternMacroExpansionContext, AstPattern> expand
    )
        : base(position, _flow(signature, exported))
    {
        MacroId = macroId;
        Invocation = invocation;
        SubjectLocal = subjectLocal;
        Signature = signature;
        _exported = exported;
        _expand = expand;
    }

    public string MacroId { get; }
    public AstGetSet Invocation { get; }
    public string SubjectLocal { get; }
    public PatternMacroSignature Signature { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() => _exported.Values;

    static BindingFlow _flow(
        PatternMacroSignature signature,
        IReadOnlyDictionary<string, PatternCapture> exported
    )
    {
        var whenTrue = signature
            .Captures.Where(x => x.WhenTrue)
            .Select(x => exported[x.Name])
            .ToImmutableHashSet();
        var whenFalse = signature
            .Captures.Where(x => x.WhenFalse)
            .Select(x => exported[x.Name])
            .ToImmutableHashSet();
        return new(new(true, whenTrue), new(true, whenFalse));
    }

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        try
        {
            var context = new PatternMacroExpansionContext(
                target,
                Invocation,
                SubjectLocal,
                _exported
            );
            var expanded = _expand(target, context);
            var expectedTrue = Flow.WhenTrue.Captures;
            var expectedFalse = Flow.WhenFalse.Captures;
            var exportedHandles = _exported.Values.ToHashSet();
            var actualTrue = expanded.Flow.WhenTrue.Captures.Intersect(exportedHandles).ToHashSet();
            var actualFalse = expanded
                .Flow.WhenFalse.Captures.Intersect(exportedHandles)
                .ToHashSet();
            var knownHandles = exportedHandles.Concat(context.PrivateCaptures).ToHashSet();
            var expansionIsValid = _validateExpansion(
                expanded,
                BindingOutcome.Empty,
                knownHandles,
                out var validationError
            );
            var escaped = expanded
                .Flow.WhenTrue.Captures.Union(expanded.Flow.WhenFalse.Captures)
                .Where(c => !exportedHandles.Contains(c) && !context.PrivateCaptures.Contains(c))
                .ToArray();
            if (
                !actualTrue.SetEquals(expectedTrue)
                || !actualFalse.SetEquals(expectedFalse)
                || escaped.Length > 0
                || !expansionIsValid
            )
            {
                target.Loader.ReportMessage(
                    Message.Error(
                        $"Pattern macro {MacroId} expansion does not match its frozen capture signature{(validationError == null ? "." : $": {validationError}")}",
                        Position,
                        MessageClasses.PatternMacroSignatureMismatch
                    )
                );
                target.EmitJump(Position, falseLabel);
                return;
            }

            expanded.EmitBranch(target, SubjectLocal, trueLabel, falseLabel);
        }
        catch (Exception exception)
        {
            target.Loader.ReportMessage(
                Message.Error(
                    $"Pattern macro {MacroId} failed during expansion: {exception.Message}",
                    Position,
                    MessageClasses.PatternMacroSignatureMismatch
                )
            );
            target.EmitJump(Position, falseLabel);
        }
    }

    static bool _validateExpansion(
        AstPattern pattern,
        BindingOutcome available,
        IReadOnlySet<PatternCapture> knownHandles,
        out string? error
    )
    {
        error = null;
        switch (pattern)
        {
            case AstBindPattern bind:
                if (available.IsReachable && available.Captures.Contains(bind.Capture))
                {
                    error = $"capture {bind.Capture.Name} is acquired twice on one route";
                    return false;
                }
                return true;
            case AstExpressionPattern expression:
                return _validateReads(expression.Condition, available, knownHandles, out error);
            case AstRelationalPattern relational:
                return _validateReads(relational.Condition, available, knownHandles, out error);
            case AstPatternAnd and:
                if (!_validateExpansion(and.Left, available, knownHandles, out error))
                    return false;
                var afterLeftTrue = BindingOutcome.Sequence(available, and.Left.Flow.WhenTrue);
                return !afterLeftTrue.IsReachable
                    || _validateExpansion(and.Right, afterLeftTrue, knownHandles, out error);
            case AstPatternOr or:
                if (!_validateExpansion(or.Left, available, knownHandles, out error))
                    return false;
                var afterLeftFalse = BindingOutcome.Sequence(available, or.Left.Flow.WhenFalse);
                return !afterLeftFalse.IsReachable
                    || _validateExpansion(or.Right, afterLeftFalse, knownHandles, out error);
            case AstPatternNot not:
                return _validateExpansion(not.Operand, available, knownHandles, out error);
            case AstExtractPattern extract:
                return _validateReads(extract.Extractor, available, knownHandles, out error)
                    && _validateExpansion(
                        extract.OutputPattern,
                        available,
                        knownHandles,
                        out error
                    );
            case AstListPattern list:
                return _validateExpansion(list.Body, available, knownHandles, out error);
            default:
                return true;
        }
    }

    static bool _validateReads(
        AstExpr expression,
        BindingOutcome available,
        IReadOnlySet<PatternCapture> knownHandles,
        out string? error
    )
    {
        error = null;
        foreach (var reference in _referencesIn(expression))
        {
            if (!reference.Entity.TryGetLocalVariable(out var local))
                continue;
            var capture = knownHandles.FirstOrDefault(c =>
                Engine.StringsAreEqual(c.PhysicalId, local.Id)
            );
            if (capture != null && !available.Captures.Contains(capture))
            {
                error = $"capture {capture.Name} is read before it is available";
                return false;
            }
        }
        return true;
    }

    static IEnumerable<AstReference> _referencesIn(AstExpr expression)
    {
        if (expression is AstReference reference)
            yield return reference;
        if (expression is not IAstHasExpressions parent)
            yield break;
        foreach (var child in parent.Expressions)
        foreach (var nested in _referencesIn(child))
            yield return nested;
    }
}

/// <summary>Runs an extractor once, optionally rejects null, then matches its result.</summary>
public sealed class AstExtractPattern : AstPattern, IAstHasExpressions
{
    public AstExtractPattern(
        ISourcePosition position,
        AstExpr extractor,
        string outputLocal,
        AstPattern outputPattern,
        bool rejectNull,
        bool extractorMayBeElided = false
    )
        : base(
            position,
            new(
                outputPattern.Flow.WhenTrue,
                BindingOutcome.Intersect(BindingOutcome.Empty, outputPattern.Flow.WhenFalse)
            )
        )
    {
        Extractor = extractor;
        OutputLocal = outputLocal;
        OutputPattern = outputPattern;
        RejectNull = rejectNull;
        ExtractorMayBeElided = extractorMayBeElided;
    }

    public AstExpr Extractor { get; }
    public string OutputLocal { get; }
    public AstPattern OutputPattern { get; }
    public bool RejectNull { get; }
    public bool ExtractorMayBeElided { get; }
    public AstExpr[] Expressions => [Extractor];
    internal int? LastExtractorCommandAddress { get; private set; }

    internal override IEnumerable<PatternCapture> GetCaptures() => OutputPattern.GetCaptures();

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        var extractorStart = target.Code.Count;
        if (ExtractorMayBeElided && OutputPattern is AstWildcardPattern)
        {
            target.EmitJump(Position, trueLabel);
            return;
        }

        if (
            !RejectNull
            && OutputPattern.TryEmitDirectBranch(target, Extractor, trueLabel, falseLabel)
        )
        {
            _recordExtractorCommand(target, extractorStart);
            return;
        }

        Extractor.EmitValueCode(target);
        _recordExtractorCommand(target, extractorStart);
        target.EmitStoreLocal(Position, OutputLocal);
        if (RejectNull)
        {
            target.EmitLoadLocal(Position, OutputLocal);
            target.Emit(Position, OpCode.check_null);
            target.EmitJumpIfTrue(Position, falseLabel);
        }
        OutputPattern.EmitBranch(target, OutputLocal, trueLabel, falseLabel);
    }

    void _recordExtractorCommand(CompilerTarget target, int start)
    {
        string? alias = null;
        if (Extractor.IsCommandCall(PatternOutputAt.Alias, out _))
            alias = PatternOutputAt.Alias;
        else if (Extractor.IsCommandCall(PatternListSlice.Alias, out _))
            alias = PatternListSlice.Alias;
        if (alias == null)
            return;

        for (var address = target.Code.Count - 1; address >= start; address--)
        {
            var instruction = target.Code[address];
            if (instruction.OpCode == OpCode.cmd && Engine.StringsAreEqual(instruction.Id, alias))
            {
                LastExtractorCommandAddress = address;
                return;
            }
        }
    }
}

/// <summary>
///     Retains the relationship between a list shape test and its element extractions for CIL.
/// </summary>
public sealed class AstListPattern : AstPattern
{
    public AstListPattern(
        ISourcePosition position,
        AstPattern body,
        IReadOnlyList<AstExtractPattern> extractions
    )
        : base(position, body.Flow)
    {
        Body = body;
        Extractions = extractions;
    }

    public AstPattern Body { get; }
    public IReadOnlyList<AstExtractPattern> Extractions { get; }

    internal override IEnumerable<PatternCapture> GetCaptures() => Body.GetCaptures();

    internal override void EmitBranch(
        CompilerTarget target,
        string subjectLocal,
        string trueLabel,
        string falseLabel
    )
    {
        var start = target.Code.Count;
        Body.EmitBranch(target, subjectLocal, trueLabel, falseLabel);

        int? shapeAddress = null;
        for (var address = start; address < target.Code.Count; address++)
        {
            var instruction = target.Code[address];
            if (
                instruction.OpCode == OpCode.cmd
                && Engine.StringsAreEqual(instruction.Id, PatternListShape.Alias)
            )
            {
                shapeAddress = address;
                break;
            }
        }
        if (shapeAddress == null)
            return;

        var extractionAddresses = Extractions
            .Select(extraction => extraction.LastExtractorCommandAddress)
            .Where(address => address.HasValue)
            .Select(address => address!.Value)
            .ToArray();
        if (extractionAddresses.Length == 0)
            return;
        var hint = new ListPatternHint(shapeAddress.Value, extractionAddresses);
        Cil.Compiler.AddCilHint(target, hint);
        _watchAddress(target, 0, shapeAddress.Value);
        for (var i = 0; i < extractionAddresses.Length; i++)
            _watchAddress(target, i + 1, extractionAddresses[i]);
    }

    static void _watchAddress(CompilerTarget target, int fieldIndex, int initialAddress)
    {
        var currentAddress = initialAddress;
        AddressChangeHook? hook = null;
        hook = new(
            initialAddress,
            newAddress =>
            {
                foreach (var hintEntry in target.Meta[Loader.CilHintsKey].List)
                {
                    var entry = hintEntry.List;
                    var metaIndex = fieldIndex + 1;
                    if (
                        entry.Length > metaIndex
                        && entry[0] == ListPatternHint.Key
                        && entry[metaIndex].Text
                            == currentAddress.ToString(CultureInfo.InvariantCulture)
                    )
                    {
                        entry[metaIndex] = newAddress.ToString(CultureInfo.InvariantCulture);
                        hook!.InstructionIndex = newAddress;
                        currentAddress = newAddress;
                        return;
                    }
                }
            }
        );
        target.AddressChangeHooks.Add(hook);
    }
}

public sealed class AstPatternTest : AstExpr, IAstJumpCondition, IAstHasExpressions
{
    public AstPatternTest(
        ISourcePosition position,
        AstExpr subject,
        string subjectLocal,
        AstPattern pattern,
        IReadOnlyCollection<PatternCapture> captures
    )
        : base(position)
    {
        Subject = subject;
        SubjectLocal = subjectLocal;
        Pattern = pattern;
        Captures = captures;
    }

    public AstExpr Subject { get; }
    public string SubjectLocal { get; }
    public AstPattern Pattern { get; }
    public IReadOnlyCollection<PatternCapture> Captures { get; }
    public BindingFlow Flow => Pattern.Flow;
    public AstExpr[] Expressions => [Subject];

    public void EmitJumpCondition(CompilerTarget target, string trueLabel, string falseLabel)
    {
        if (!Pattern.TryEmitDirectBranch(target, Subject, trueLabel, falseLabel))
        {
            Subject.EmitValueCode(target);
            target.EmitStoreLocal(Position, SubjectLocal);
            Pattern.EmitBranch(target, SubjectLocal, trueLabel, falseLabel);
        }
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var ns = $"pattern\\value\\{Guid.NewGuid():N}";
        var yes = $"true\\{ns}";
        var no = $"false\\{ns}";
        var end = $"end\\{ns}";
        EmitJumpCondition(target, yes, no);
        target.EmitLabel(Position, yes);
        if (stackSemantics == StackSemantics.Value)
            target.EmitConstant(Position, true);
        target.EmitJump(Position, end);
        target.EmitLabel(Position, no);
        if (stackSemantics == StackSemantics.Value)
            target.EmitConstant(Position, false);
        target.EmitLabel(Position, end);
        target.FreeLabel(yes);
        target.FreeLabel(no);
        target.FreeLabel(end);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }
}

public sealed class AstMatchArm : AstNode, IAstHasExpressions
{
    public AstMatchArm(ISourcePosition position, AstPattern pattern, AstExpr expression)
        : base(position)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Captures = pattern.GetCaptures().Distinct().ToArray();
    }

    public AstPattern Pattern { get; }
    public AstExpr Expression { get; internal set; }
    public IReadOnlyCollection<PatternCapture> Captures { get; }
    public AstExpr[] Expressions => [Expression];

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics) =>
        throw new NotSupportedException(
            "Match arms are emitted only as part of a match expression."
        );
}

public sealed class AstMatchExpression : AstExpr, IAstHasExpressions
{
    public AstMatchExpression(
        ISourcePosition position,
        AstExpr subject,
        string subjectLocal,
        string resultLocal,
        IEnumerable<AstMatchArm>? arms = null
    )
        : base(position)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        SubjectLocal = subjectLocal ?? throw new ArgumentNullException(nameof(subjectLocal));
        ResultLocal = resultLocal ?? throw new ArgumentNullException(nameof(resultLocal));
        if (arms != null)
            Arms.AddRange(arms);
    }

    public AstExpr Subject { get; internal set; }
    public string SubjectLocal { get; }

    public string ResultLocal { get; }
    public List<AstMatchArm> Arms { get; } = new();
    public AstExpr[] Expressions => [Subject, .. Arms.Select(arm => arm.Expression)];

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        var subject = Subject;
        _OptimizeNode(target, ref subject);
        Subject = subject;
        foreach (var arm in Arms)
        {
            var armExpression = arm.Expression;
            _OptimizeNode(target, ref armExpression);
            arm.Expression = armExpression;
        }
        expr = null;
        return false;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        Subject.EmitValueCode(target);
        target.EmitStoreLocal(Position, SubjectLocal);

        var id = Guid.NewGuid().ToString("N");
        var end = $"match\\end\\{id}";
        for (var i = 0; i < Arms.Count; i++)
        {
            var arm = Arms[i];
            var selected = $"match\\selected\\{i}\\{id}";
            var next = $"match\\next\\{i}\\{id}";

            arm.Pattern.EmitBranch(target, SubjectLocal, selected, next);

            target.EmitLabel(arm.Position, selected);
            AstPattern.EmitConsumerCopies(
                target,
                arm.Position,
                arm.Captures,
                arm.Pattern.Flow.WhenTrue
            );
            arm.Expression.EmitCode(target, stackSemantics);
            if (stackSemantics == StackSemantics.Value)
                target.EmitStoreLocal(arm.Position, ResultLocal);
            target.EmitJump(arm.Position, end);

            target.EmitLabel(arm.Position, next);
            target.FreeLabel(selected);
            target.FreeLabel(next);
        }

        if (stackSemantics == StackSemantics.Value)
        {
            target.EmitNull(Position);
            target.EmitStoreLocal(Position, ResultLocal);
        }
        target.EmitLabel(Position, end);
        if (stackSemantics == StackSemantics.Value)
            target.EmitLoadLocal(Position, ResultLocal);
        target.FreeLabel(end);
    }
}

public abstract class AstBindingBinary : AstExpr, IAstJumpCondition, IAstHasExpressions
{
    protected AstBindingBinary(
        ISourcePosition position,
        AstExpr left,
        AstExpr right,
        BindingFlow flow
    )
        : base(position)
    {
        Left = left;
        Right = right;
        Flow = flow;
    }

    public AstExpr Left { get; }
    public AstExpr Right { get; }
    public BindingFlow Flow { get; }
    public AstExpr[] Expressions => [Left, Right];

    public void EmitJumpCondition(CompilerTarget target, string trueLabel, string falseLabel) =>
        DoEmitJumpCondition(target, trueLabel, falseLabel);

    protected abstract void DoEmitJumpCondition(
        CompilerTarget target,
        string trueLabel,
        string falseLabel
    );

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        var ns = $"bbe\\value\\{Guid.NewGuid():N}";
        var yes = $"true\\{ns}";
        var no = $"false\\{ns}";
        var end = $"end\\{ns}";
        EmitJumpCondition(target, yes, no);
        target.EmitLabel(Position, yes);
        if (stackSemantics == StackSemantics.Value)
            target.EmitConstant(Position, true);
        target.EmitJump(Position, end);
        target.EmitLabel(Position, no);
        if (stackSemantics == StackSemantics.Value)
            target.EmitConstant(Position, false);
        target.EmitLabel(Position, end);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }
}

public sealed class AstBindingAnd : AstBindingBinary
{
    public AstBindingAnd(ISourcePosition position, AstExpr left, AstExpr right)
        : base(position, left, right, BindingFlow.And(BindingFlowFor(left), BindingFlowFor(right)))
    { }

    protected override void DoEmitJumpCondition(
        CompilerTarget target,
        string trueLabel,
        string falseLabel
    )
    {
        var next = $"bbe\\and\\{Guid.NewGuid():N}";
        AstLazyLogical.EmitJumpCondition(target, Left, next, falseLabel);
        target.EmitLabel(Position, next);
        AstLazyLogical.EmitJumpCondition(target, Right, trueLabel, falseLabel);
    }

    internal static BindingFlow BindingFlowFor(AstExpr expression) =>
        expression switch
        {
            AstPatternTest test => test.Flow,
            AstBindingBinary binary => binary.Flow,
            AstBindingNot not => not.Flow,
            _ => BindingFlow.Ordinary,
        };

    internal static IEnumerable<PatternCapture> CapturesFor(AstExpr expression) =>
        expression switch
        {
            AstPatternTest test => test.Captures,
            AstBindingBinary binary => CapturesFor(binary.Left).Concat(CapturesFor(binary.Right)),
            AstBindingNot not => CapturesFor(not.Operand),
            _ => [],
        };
}

public sealed class AstBindingOr : AstBindingBinary
{
    public AstBindingOr(ISourcePosition position, AstExpr left, AstExpr right)
        : base(
            position,
            left,
            right,
            BindingFlow.Or(AstBindingAnd.BindingFlowFor(left), AstBindingAnd.BindingFlowFor(right))
        ) { }

    protected override void DoEmitJumpCondition(
        CompilerTarget target,
        string trueLabel,
        string falseLabel
    )
    {
        var next = $"bbe\\or\\{Guid.NewGuid():N}";
        AstLazyLogical.EmitJumpCondition(target, Left, trueLabel, next);
        target.EmitLabel(Position, next);
        AstLazyLogical.EmitJumpCondition(target, Right, trueLabel, falseLabel);
    }
}

public sealed class AstBindingNot : AstExpr, IAstJumpCondition, IAstHasExpressions
{
    public AstBindingNot(ISourcePosition position, AstExpr operand)
        : base(position)
    {
        Operand = operand;
        Flow = AstBindingAnd.BindingFlowFor(operand).Not();
    }

    public AstExpr Operand { get; }
    public BindingFlow Flow { get; }
    public AstExpr[] Expressions => [Operand];

    public void EmitJumpCondition(CompilerTarget target, string trueLabel, string falseLabel) =>
        AstLazyLogical.EmitJumpCondition(target, Operand, falseLabel, trueLabel);

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (stackSemantics == StackSemantics.Effect)
        {
            Operand.EmitEffectCode(target);
            return;
        }
        var yes = $"bbe\\not\\true\\{Guid.NewGuid():N}";
        var no = $"bbe\\not\\false\\{Guid.NewGuid():N}";
        var end = $"bbe\\not\\end\\{Guid.NewGuid():N}";
        EmitJumpCondition(target, yes, no);
        target.EmitLabel(Position, yes);
        target.EmitConstant(Position, true);
        target.EmitJump(Position, end);
        target.EmitLabel(Position, no);
        target.EmitConstant(Position, false);
        target.EmitLabel(Position, end);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }
}
