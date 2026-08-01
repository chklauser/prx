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
}
