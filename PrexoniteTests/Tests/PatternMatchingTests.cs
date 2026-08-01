using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

[TestFixture]
public sealed class PatternMatchingTests : VMTestsBase
{
    public PatternMatchingTests()
    {
        CompileToCil = false;
    }

    [Test]
    public void BasicPatternsAndBooleanTransportAreObservable()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(x)
            {
                if (x is > 0 and < 10 && x is var captured)
                    return captured;
                return null;
            }
            """
        );

        Expect(5, 5);
        Expect(value => Assert.That(value.IsNull, Is.True), 12);
    }

    [Test]
    public void LaterPatternTestsPreserveEarlierConjunctCaptures()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(left, right)
            {
                if (left is var captured && right is == captured)
                    return captured;
                return null;
            }
            """
        );

        Expect(7, 7, 7);
        Expect(value => Assert.That(value.IsNull, Is.True), 7, 8);
    }

    [Test]
    public void FailedAlternativeCapturesAreNormalizedBeforePromotion()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value)
            {
                if (value is ([stale, > 10] or [_, _]))
                {
                }
                return stale;
            }
            """
        );

        Expect(
            value => Assert.That(value.IsNull, Is.True),
            engine.CreateNativePValue(new List<PValue> { 5, 0 })
        );
    }

    [Test]
    public void ConsumerCopiesAreOrdinaryMutableLocals()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(x)
            {
                if (x is var captured)
                    captured = captured + 1;
                return captured;
            }
            """
        );

        Expect(4, 3);
    }

    [Test]
    public void WhileAndConditionalConsumersReceiveCaptureCopies()
    {
        Compile(
            """
            pattern_matching enabled;

            function choose(x) = if (x is var value) value + 1 else 0;

            function main(x)
            {
                var result = null;
                while (x is var current && current > 0)
                {
                    result = current;
                    x = -1;
                }
                return [result, choose(result)];
            }
            """
        );

        Expect(new List<PValue> { 3, 4 }, 3);
    }

    [Test]
    public void InProgressCapturesAreReadOnly()
    {
        var loader = CompileInvalid(
            """
            pattern_matching enabled;

            function main(x) = x is var captured when (captured = 1);
            """,
            "read-only while matching"
        );

        Assert.That(
            loader.Errors.Any(e => e.MessageClass == MessageClasses.PatternBindingReadOnly),
            Is.True
        );
    }

    [Test]
    public void InProgressCapturesCannotBePassedByReference()
    {
        var loader = CompileInvalid(
            """
            pattern_matching enabled;

            function main(x) = x is var captured when ->captured;
            """,
            "read-only while matching"
        );

        Assert.That(
            loader.Errors.Any(e => e.MessageClass == MessageClasses.PatternBindingReadOnly),
            Is.True
        );
    }

    [Test]
    public void BuiltInPredicateKeepsLegacyTypeCheckMeaning()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(x) = x is String;
            """
        );

        Expect(true, "text");
        Expect(false, 42);
    }

    [Test]
    public void GuardAndAliasCanReadEarlierCapture()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(x)
            {
                if (x is (> 0 as positive) when positive < 10)
                    return positive;
                return null;
            }
            """
        );

        Expect(7, 7);
        Expect(value => Assert.That(value.IsNull, Is.True), 17);
    }

    [Test]
    public void ViewOutputsAreArityTolerant()
    {
        Compile(
            """
            pattern_matching enabled;

            function view_pair(value) = value;

            function main(value)
            {
                if (value is pair(left, right))
                    return [left, right];
                return null;
            }
            """
        );

        Expect(
            new List<PValue> { 1, PType.Null },
            engine.CreateNativePValue(new List<PValue> { 1 })
        );
    }

    [Test]
    public void QualifiedViewsShiftOnlyTheFinalName()
    {
        Compile(
            """
            pattern_matching enabled;

            namespace helpers
            {
                function view_pair(value) = value;
            }

            function main(value)
            {
                if (value is helpers.pair(first))
                    return first;
                return null;
            }
            """
        );

        Expect(11, engine.CreateNativePValue(new List<PValue> { 11 }));
    }

    [Test]
    public void ExactAndRestListPatternsWork()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value)
            {
                if (value is [head, *tail])
                    return [head, tail];
                return null;
            }
            """
        );

        var input = engine.CreateNativePValue(new List<PValue> { 1, 2, 3 });
        Expect(
            value =>
            {
                var result = (List<PValue>)value.Value!;
                Assert.That(result[0].Value, Is.EqualTo(1));
                Assert.That(
                    ((List<PValue>)result[1].Value!).Select(x => x.Value),
                    Is.EqualTo(new[] { 2, 3 })
                );
            },
            input
        );
    }

    [Test]
    public void ExclusiveAlternativesCanAcquireTheSameLogicalName()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value)
            {
                if (value is ([item] or [item, _]))
                    return item;
                return null;
            }
            """
        );

        Expect(1, engine.CreateNativePValue(new List<PValue> { 1 }));
        Expect(2, engine.CreateNativePValue(new List<PValue> { 2, 3 }));
    }

    [Test]
    public void SameRouteDuplicateAcquisitionIsRejected()
    {
        var loader = CompileInvalid(
            """
            pattern_matching enabled;
            function main(value) = value is [same, same];
            """,
            "acquired more than once"
        );

        Assert.That(
            loader.Errors.Any(e => e.MessageClass == MessageClasses.PatternBindingDuplicate),
            Is.True
        );
    }

    [Test]
    public void PropertyPatternsProbeMembersAndNegativeGuardExports()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value)
            {
                if (value is not {Policy: {} as policy})
                    return "missing";
                return policy.Name;
            }
            """
        );

        Expect("active", engine.CreateNativePValue(new Envelope(new Policy("active"))));
        Expect("missing", engine.CreateNativePValue(new object()));
    }

    [Test]
    public void PropertyPatternsDistinguishHandledNullAndPreserveGetterEffects()
    {
        Compile(
            """
            pattern_matching enabled;

            function capture(value)
            {
                if (value is {Policy: var policy})
                    return [true, policy];
                return [false, null];
            }

            function reject_late(value) = value is {Policy: > 0, Absent: _};
            """
        );

        ExpectNamed(
            "capture",
            new List<PValue> { true, PType.Null },
            engine.CreateNativePValue(new NullableEnvelope(null))
        );

        var effects = new SideEffectEnvelope();
        ExpectNamed("reject_late", false, engine.CreateNativePValue(effects));
        Assert.That(effects.ReadCount, Is.EqualTo(1));
    }

    [Test]
    public void DisabledModeRetainsWordAndSymbolBooleanSynonyms()
    {
        Compile(
            """
            function main(a, b) = (a and b) == (a && b) && (a or b) == (a || b);
            """
        );

        Expect(true, true, false);
    }

    [Test]
    public void DisabledModeRetainsStatementAndSeparator()
    {
        Compile(
            """
            function main
            {
                return; and return;
            }
            """
        );

        Expect(value => Assert.That(value.IsNull, Is.True));
    }

    [Test]
    public void EnabledModeRejectsStatementAndSeparator()
    {
        CompileInvalid(
            """
            pattern_matching enabled;
            function main
            {
                return; and return;
            }
            """,
            "expected"
        );
    }

    [Test]
    public void FunctionOverridesInheritRecursively()
    {
        Compile(
            """
            function modern(x) [pattern_matching enabled]
            {
                function nested(y) = y is [value];
                return nested(x);
            }

            function legacy(x) [pattern_matching disabled] = x is String;
            """
        );

        ExpectNamed("modern", true, engine.CreateNativePValue(new List<PValue> { 1 }));
        ExpectNamed("legacy", true, "text");
    }

    [Test]
    public void FrozenModuleModeRejectsLateAndConflictingChanges()
    {
        var late = CompileInvalid(
            """
            function main = true;
            pattern_matching enabled;
            """,
            "module header"
        );
        Assert.That(late.PatternMatchingEnabled, Is.False);

        SetupCompilerEngine();
        CompileToCil = false;
        var loader = new Loader(options);
        loader.LoadFromString("pattern_matching enabled; function first = true;");
        loader.LoadFromString("pattern_matching disabled;");
        Assert.That(
            loader.Errors.Any(e => e.MessageClass == MessageClasses.PatternMatchingModeConflict),
            Is.True
        );
        Assert.That(loader.PatternMatchingEnabled, Is.True);
    }

    [Test]
    public void InteractiveStyleIncrementalCompilationRetainsMatchingMode()
    {
        var loader = new Loader(options);
        loader.LoadFromString("pattern_matching enabled; var prompt = null;");
        loader.LoadFromString("function repl_eval(value) = value is [item] && item > 0;");

        Assert.That(loader.Errors, Is.Empty);
        Assert.That(loader.PatternMatchingEnabled, Is.True);
        ExpectNamed("repl_eval", true, engine.CreateNativePValue(new List<PValue> { 3 }));
    }

    [Test]
    public void OptionalUnusedImplicitBindingWarningDoesNotApplyToExplicitVar()
    {
        var loader = Compile(
            """
            pattern_matching enabled;
            warn_unused_pattern_bindings enabled;

            function main(x)
            {
                if (x is implicit_name)
                    return true;
                if (x is var documented_name)
                    return true;
                return false;
            }
            """
        );

        Assert.That(
            loader.Warnings.Count(w => w.MessageClass == MessageClasses.UnusedPatternBinding),
            Is.EqualTo(1)
        );
    }

    [Test]
    public void CommandPatternMacroFreezesSignatureBeforeExpansion()
    {
        var loader = new Loader(options);
        var macro = new CapturePatternMacro();
        loader.MacroCommands.Add(macro);
        loader.TopLevelSymbols.Declare(
            macro.Id,
            Symbol.CreateExpand(
                Symbol.CreateReference(
                    EntityRef.MacroCommand.Create(macro.Id),
                    NoSourcePosition.Instance
                )
            )
        );

        Compile(
            loader,
            """
            pattern_matching enabled;

            function main(x)
            {
                if (x is capture && captured > 0)
                    return captured;
                return null;
            }
            """
        );

        Expect(3, 3);
    }

    [Test]
    public void PatternMacroMayUseItsOwnPrivateCaptureInternally()
    {
        var loader = new Loader(options);
        _registerPatternMacro(loader, new PrivateCapturePatternMacro());

        Compile(
            loader,
            """
            pattern_matching enabled;
            function main(x) = x is private_capture;
            """
        );

        Expect(true, 8);
    }

    [Test]
    public void PatternMacroUseBeforeBindingIsRejectedAfterExpansion()
    {
        var loader = new Loader(options);
        _registerPatternMacro(loader, new InvalidCaptureOrderPatternMacro());

        CompileInvalid(
            loader,
            """
            pattern_matching enabled;
            function main(x) = x is invalid_capture_order;
            """,
            "read before it is available"
        );

        Assert.That(
            loader.Errors.Any(e =>
                e.MessageClass == MessageClasses.PatternMacroSignatureMismatch
                && e.Text.Contains("read before it is available", StringComparison.Ordinal)
            ),
            Is.True
        );
    }

    [Test]
    public void ScriptPatternMacroUsesDedicatedDescribeAndExpandPhases()
    {
        Compile(
            """
            pattern_matching enabled;

            macro is_script_capture [pattern\macro]
            {
                if (context.IsDescribe)
                {
                    context.Export("captured", true, false);
                    return null;
                }
                return context.Factory.PatternBind(
                    context.Invocation.Position,
                    context.ExportedCapture("captured"));
            }

            function main(x)
            {
                if (x is script_capture && captured > 0)
                    return captured;
                return null;
            }
            """
        );

        Expect(4, 4);
    }

    [Test]
    public void MatchSelectsTheFirstSuccessfulArmAndScopesItsCaptures()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value) = match (value)
            {
                [a, b] when a > 0 -> "$a:$b";
                String as text    -> text;
                _                 -> "fallback";
            };
            """
        );

        Expect("2:3", engine.CreateNativePValue(new List<PValue> { 2, 3 }));
        Expect("hello", "hello");
        Expect("fallback", -1);
    }

    [Test]
    public void MatchEvaluatesItsSubjectOnceAndDefaultsToNull()
    {
        Compile(
            """
            pattern_matching enabled;

            function once(x)
            {
                var result = match (x++)
                {
                    3 -> "three";
                    _ -> "other";
                };
                return [result, x];
            }

            function no_match(value) = match (value) { > 10 -> "large" };
            """
        );

        ExpectNamed("once", new List<PValue> { "three", 4 }, 3);
        ExpectReturnValue(
            "no_match",
            value => Assert.That(value.IsNull, Is.True),
            new PValue[] { 5 }
        );
    }

    [Test]
    public void MatchGuardDelimiterDoesNotCompeteWithLambdaArrows()
    {
        Compile(
            """
            pattern_matching enabled;

            function guarded(value, null_allowed) = match (value)
            {
                null when null_allowed -> true;
                _ when (x => x + 1) != null -> false;
                _ -> null;
            };

            function rhs_lambda(value)
            {
                var increment = match (value) { _ -> x => x + 1 };
                return increment.(4);
            }
            """
        );

        ExpectNamed("guarded", true, PType.Null, true);
        ExpectNamed("guarded", false, 12, false);
        ExpectNamed("rhs_lambda", 5, "anything");
    }

    [Test]
    public void MatchIsContextualAndComposesAsAPrimaryExpression()
    {
        Compile(
            """
            pattern_matching enabled;

            function match(x) = x + 1;
            function ordinary(value) = match(value);
            function nested(value) = 10 + match (value) { 2 -> 3; _ -> 0 };
            function lambda_subject = match (x => x + 1) { _ -> true };
            """
        );

        ExpectNamed("ordinary", 5, 4);
        ExpectNamed("nested", 13, 2);
        ExpectNamed("lambda_subject", true);
    }

    [Test]
    public void MatchArmsHaveIndependentBindingsAndStrictEffects()
    {
        Compile(
            """
            pattern_matching enabled;

            var probes = 0;
            function view_counted(value)
            {
                probes++;
                return null;
            }

            function main(value)
            {
                probes = 0;
                var selected = match (value)
                {
                    counted(item) -> item;
                    [item]        -> item;
                    _             -> null;
                };
                return [selected, probes];
            }
            """
        );

        Expect(new List<PValue> { 7, 1 }, engine.CreateNativePValue(new List<PValue> { 7 }));
    }

    [Test]
    public void MatchArmBindingsDoNotLeak()
    {
        CompileInvalid(
            """
            pattern_matching enabled;

            function main(value)
            {
                var selected = match (value)
                {
                    [item] -> item;
                    _      -> item;
                };
                return item;
            }
            """,
            "has not been resolved"
        );
    }

    public sealed record Policy(string Name);

    public sealed record Envelope(Policy Policy);

    public sealed record NullableEnvelope(Policy? Policy);

    public sealed class SideEffectEnvelope
    {
        public int ReadCount { get; private set; }

        public int Policy
        {
            get
            {
                ReadCount++;
                return 3;
            }
        }
    }

    static void _registerPatternMacro(Loader loader, PatternMacroCommand macro)
    {
        loader.MacroCommands.Add(macro);
        loader.TopLevelSymbols.Declare(
            macro.Id,
            Symbol.CreateExpand(
                Symbol.CreateReference(
                    EntityRef.MacroCommand.Create(macro.Id),
                    NoSourcePosition.Instance
                )
            )
        );
    }

    static AstExpr _readCapture(PatternCapture capture, ISourcePosition position) =>
        new AstIndirectCall(
            position,
            PCall.Get,
            new AstReference(position, EntityRef.Variable.Local.Create(capture.PhysicalId))
        );

    sealed class CapturePatternMacro : PatternMacroCommand
    {
        public CapturePatternMacro()
            : base("is_capture") { }

        protected override PatternMacroSignature DescribePattern(
            PatternMacroDescribeContext context
        ) => new([new PatternMacroCaptureDescription("captured", true, false)]);

        protected override AstPattern ExpandPattern(PatternMacroExpansionContext context) =>
            context.Factory.PatternBind(
                context.Invocation.Position,
                context.ExportedCapture("captured")
            );
    }

    sealed class PrivateCapturePatternMacro : PatternMacroCommand
    {
        public PrivateCapturePatternMacro()
            : base("is_private_capture") { }

        protected override PatternMacroSignature DescribePattern(
            PatternMacroDescribeContext context
        ) => PatternMacroSignature.Empty;

        protected override AstPattern ExpandPattern(PatternMacroExpansionContext context)
        {
            var position = context.Invocation.Position;
            var capture = context.AllocatePrivateCapture("internal", position);
            var equality = context.Factory.BinaryOperation(
                position,
                _readCapture(capture, position),
                BinaryOperator.Equality,
                new AstIndirectCall(
                    position,
                    PCall.Get,
                    new AstReference(
                        position,
                        EntityRef.Variable.Local.Create(context.SubjectLocal)
                    )
                )
            );
            return context.Factory.PatternAnd(
                position,
                context.Factory.PatternBind(position, capture),
                context.Factory.PatternExpression(position, equality)
            );
        }
    }

    sealed class InvalidCaptureOrderPatternMacro : PatternMacroCommand
    {
        public InvalidCaptureOrderPatternMacro()
            : base("is_invalid_capture_order") { }

        protected override PatternMacroSignature DescribePattern(
            PatternMacroDescribeContext context
        ) => new([new PatternMacroCaptureDescription("late", true, false)]);

        protected override AstPattern ExpandPattern(PatternMacroExpansionContext context)
        {
            var position = context.Invocation.Position;
            var capture = context.ExportedCapture("late");
            var read = _readCapture(capture, position);
            var condition = context.Factory.BinaryOperation(
                position,
                read,
                BinaryOperator.Equality,
                read
            );
            return context.Factory.PatternAnd(
                position,
                context.Factory.PatternExpression(position, condition),
                context.Factory.PatternBind(position, capture)
            );
        }
    }
}
