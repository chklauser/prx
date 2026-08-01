using NUnit.Framework;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

[TestFixture]
public sealed class PatternMatchingOptimizationTests : VMTestsBase
{
    [Test]
    public void SimplePatternsAvoidRedundantSubjectAndCaptureInitializationInstructions()
    {
        Compile(
            """
            pattern_matching enabled;

            function literal(value) = value is true;
            function binding(value) = value is var captured;
            function listed(value) = value is [head, _, _];
            function main = null;
            """
        );

        var literal = target.Functions["literal"]!;
        var binding = target.Functions["binding"]!;
        var listed = target.Functions["listed"]!;

        Assert.Multiple(() =>
        {
            Assert.That(literal.Variables.Any(id => id.Contains("pattern\\subject")), Is.False);
            Assert.That(
                literal.Code,
                Has.None.Matches<Instruction>(instruction =>
                    instruction.Id?.Contains("pattern\\subject") == true
                )
            );
            Assert.That(binding.Variables.Any(id => id.Contains("pattern\\subject")), Is.False);
            Assert.That(
                binding.Code.Any(instruction => instruction.OpCode == OpCode.ldc_null),
                Is.False
            );
            Assert.That(
                listed.Code.Count(instruction =>
                    Engine.StringsAreEqual(instruction.Id, PatternOutputAt.Alias)
                ),
                Is.EqualTo(1)
            );
        });
    }

    [Test]
    public void UnavailableCapturePromotionStaysNullAcrossLoopIterations()
    {
        Compile(
            """
            pattern_matching enabled;

            function main
            {
                var values = [[7, 20], [8, 0]];
                var first = null;
                var second = null;
                var i = 0;
                while (i < 2)
                {
                    if (values[i] is ([captured, > 10] or [_, _]))
                    {
                    }
                    if (i == 0)
                        first = captured;
                    else
                        second = captured;
                    i++;
                }
                return [first, second];
            }
            """
        );

        Expect(new List<PValue> { PType.Null, PType.Null });
    }

    [Test]
    public void MatchArmValuesWorkInFullyStaticCil()
    {
        Compile(
            """
            pattern_matching enabled;

            function select(value) = match (value)
            {
                true  -> 11;
                false -> 22;
            };

            function main = [select(true), select(false), select(null)];
            """
        );

        Expect(new List<PValue> { 11, 22, PType.Null });
    }

    [Test]
    public void NestedAndRestListPatternsWorkWithFullyStaticCilHints()
    {
        Compile(
            """
            pattern_matching enabled;

            function main(value)
            {
                if (value is [[head, *tail], last])
                    return [head, tail, last];
                return null;
            }
            """
        );

        Expect(
            new List<PValue> { 1, PType.List.CreatePValue(new List<PValue> { 2, 3 }), 4 },
            engine.CreateNativePValue(
                new List<PValue> { PType.List.CreatePValue(new List<PValue> { 1, 2, 3 }), 4 }
            )
        );
        Expect(value => Assert.That(value.IsNull, Is.True), 17);
    }

    [Test]
    public void PropertyProbeSentinelPreservesNullMissingAndGetterEffectsInFullyStaticCil()
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

            function main(value) = capture(value);
            """
        );

        Expect(
            new List<PValue> { true, PType.Null },
            engine.CreateNativePValue(new OptimizationNullableEnvelope(null))
        );
        Expect(new List<PValue> { false, PType.Null }, engine.CreateNativePValue(new object()));

        var effects = new OptimizationSideEffectEnvelope();
        ExpectNamed("reject_late", false, engine.CreateNativePValue(effects));
        Assert.That(effects.ReadCount, Is.EqualTo(1));
    }

    [Test]
    public void CombinedViewPropertyAndListMatchWorksInFullyStaticCil()
    {
        Compile(
            """
            pattern_matching enabled;

            function view_record(value) = [value.Policy, value.Values];

            function list_shape(value, iterations)
            {
                var hits = 0;
                var i = 0;
                while (i < iterations)
                {
                    if (value is [_, _, _])
                        hits++;
                    i++;
                }
                return hits;
            }

            function list_pattern(value, iterations)
            {
                var sum = 0;
                var i = 0;
                while (i < iterations)
                {
                    if (value is [head, _, _])
                        sum += head;
                    i++;
                }
                return sum;
            }

            function property_pattern(value, iterations)
            {
                var hits = 0;
                var i = 0;
                while (i < iterations)
                {
                    if (value is {Policy: _})
                        hits++;
                    i++;
                }
                return hits;
            }

            function view_pattern(value, iterations)
            {
                var hits = 0;
                var i = 0;
                while (i < iterations)
                {
                    if (value is record(_, _))
                        hits++;
                    i++;
                }
                return hits;
            }

            function complex(value) = match (value)
            {
                record({Active: true, Name: name}, [head, _, _])
                    when name != null && head > 0 -> head;
                {Fallback: fallback} -> fallback;
                _ -> 0;
            };

            function main(value) = [
                list_shape(value.Values, 2),
                list_pattern(value.Values, 2),
                property_pattern(value, 2),
                view_pattern(value, 2),
                complex(value)
            ];
            """
        );

        var values = new List<PValue> { 3, 5, 8 };
        var envelope = engine.CreateNativePValue(
            new OptimizationEnvelope(new OptimizationPolicy(true, "active"), values, 11)
        );
        var iterations = PType.Int.CreatePValue(2);
        var list = PType.List.CreatePValue(values);

        ExpectNamed("list_shape", 2, list, iterations);
        ExpectNamed("list_pattern", 6, list, iterations);
        ExpectNamed("property_pattern", 2, envelope, iterations);
        ExpectNamed("view_pattern", 2, envelope, iterations);
        ExpectNamed("complex", 3, envelope);
        Expect(new List<PValue> { 2, 6, 2, 2, 3 }, envelope);
    }

    public sealed record OptimizationNullableEnvelope(object? Policy);

    public sealed record OptimizationPolicy(bool Active, string Name);

    public sealed record OptimizationEnvelope(
        OptimizationPolicy Policy,
        List<PValue> Values,
        int Fallback
    );

    public sealed class OptimizationSideEffectEnvelope
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
}
