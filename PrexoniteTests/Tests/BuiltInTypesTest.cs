using NUnit.Framework;

namespace PrexoniteTests.Tests;

public abstract class BuiltInTypeTests : VMTestsBase
{
    [Test]
    public void HashStaticMethodCreate()
    {
        Compile(
            """

            function main(x,y) {
                var h = ~Hash.Create("x": x, "y": y);
                return "x=$(h["x"]), y=$(h["y"]), c=$(h.Count)";
            }

            """
        );
        Expect("x=5, y=7, c=2", 5, 7);
    }

    [Test]
    public void HashStaticMethodCreateFromArgs()
    {
        Compile(
            """

            function main(x,y) {
                var h = ~Hash.CreateFromArgs("x", x, "y", y);
                return "x=$(h["x"]), y=$(h["y"]), c=$(h.Count)";
            }

            """
        );
        Expect("x=3, y=8, c=2", 3, 8);
    }

    [Test]
    public void HashStaticMethodCreateFromArgsIgnoresExcessArg()
    {
        Compile(
            """

            function main(x,y) {
                var h = ~Hash.CreateFromArgs("x", x, "y", y, "z");
                return "x=$(h["x"]), y=$(h["y"]), c=$(h.Count)";
            }

            """
        );
        Expect("x=6, y=9, c=2", 6, 9);
    }
}
