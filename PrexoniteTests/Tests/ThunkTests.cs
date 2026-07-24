using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Lazy;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

public abstract class ThunkTests : VMTestsBase
{
    [Test]
    public void SingularThunk()
    {
        Compile(
            """

            function _idT xT = xT.force;

            function main(n)
            {
                var t = thunk(->_idT,n);
                return t.force;
            }

            """
        );
        const int n = 77;
        Expect(n, n);
    }

    [Test]
    public void BasicThunk()
    {
        Compile(
            """

            function _addT xT yT = xT.force + yT.force;
            function _mulT xT yT = xT.force * yT.force;

            function main(x1,y1,x2,y2)
            {
                //x1*x2 + y1*y2
                var t = thunk(->_addT,thunk(->_mulT,x1,x2),thunk(->_mulT,y1,y2));
                return t.force;
            }

            """
        );
        const int x1 = 15;
        const int x2 = 17;
        const int y1 = 5;
        const int y2 = -8;
        const int dot = x1 * x2 + y1 * y2;
        Expect(dot, x1, y1, x2, y2);
    }

    [Test]
    public void NotExecuted()
    {
        Compile(
            """

            function _divT xT yT = xT.force / yT.force;
            function _throwT = throw "Invalid computation";
            function _consT hT tT = [hT,tT];
            function _headT xsT = xsT.force[0];

            function main(x1)
            {
                var t1 = thunk(->_throwT);
                var t2 = thunk(->_divT,4,0);
                var t3 = thunk(->_divT,2*x1,2);
                var t4 = thunk(->_consT,t3,thunk(->_consT,t2,null));
                var t5 = thunk(->_headT,t4); //x1
                return t5.force;
            }

            """
        );

        Expect(15, 15);
    }

    [Test]
    public void Repeat()
    {
        Compile(
            """

            function _consT hT tT = [hT,tT];
            function _headT xsT = xsT.force[0];
            function _tailT xsT = xsT.force[1];
            function _refT xT = xT.force.();
            function _addT x1 x2 = x1.force + x2.force;

            function main(x1)
            {
                function repeatT(x)
                {
                    var xsT;
                    xsT = thunk(->_consT,x,thunk(->_refT,->xsT));
                    return xsT;
                }

                var x1s = repeatT(x1);
                var y1 = thunk(->_headT,x1s);
                var y1s = thunk(->_tailT,x1s);
                var z1 = thunk(->_headT,y1s);
                var z1s = thunk(->_tailT,y1s);
                var a1 = thunk(->_headT,z1s);

                var result = thunk(->_addT,y1,thunk(->_addT,z1,a1));

                return result.force;
            }

            """
        );

        Expect(12, 4);
    }

    [Test]
    public void AsThunkPreservesThunksAndWrapsValues()
    {
        Compile(
            """

            function main(value)
            {
                var wrapped = asthunk(value);
                var pending = thunk(() => value);
                var existing = asthunk(pending);
                var before = pending.evaluated;
                var same = existing == pending;
                var result = existing.force;
                return [wrapped.evaluated,wrapped.force,before,same,result,pending.evaluated];
            }

            """
        );

        Expect(new List<PValue> { true, 42, false, true, 42, true }, 42);
    }

    [Test]
    public void ThunkMemoizesItsValue()
    {
        Compile(
            """

            function main()
            {
                var calls = 0;
                var value = thunk(() => { calls++; return 42; });
                return [value.evaluated,value.force,value.evaluated,value.force,calls];
            }

            """
        );

        Expect(new List<PValue> { false, 42, true, 42, 1 });
    }

    [Test]
    public void ThunkMemoizesExceptions()
    {
        var calls = 0;
        var expected = new InvalidOperationException("Expected failure.");
        var expression = new PValue(
            new ProvidedFunction(
                (_, _) =>
                {
                    calls++;
                    throw expected;
                }
            ),
            PType.Object[typeof(IIndirectCall)]
        );
        var thunk = Thunk.NewExpression(expression, []);

        Assert.That(
            Assert.Throws<InvalidOperationException>(() => thunk.Force(sctx)),
            Is.SameAs(expected)
        );
        Assert.That(
            Assert.Throws<InvalidOperationException>(() => thunk.Force(sctx)),
            Is.SameAs(expected)
        );
        Assert.That(calls, Is.EqualTo(1));
        Assert.That(thunk.IsEvaluated, Is.False);
    }

    [Test]
    public void RemovedKeywordsAreOrdinaryIdentifiers()
    {
        Compile(
            """

            function lazy(let) = let;

            function main(value)
            {
                var let = lazy(value);
                return let;
            }

            """
        );

        Expect(42, 42);
    }

    sealed class ProvidedFunction(ProvidedFunctionImpl function) : IIndirectCall
    {
        public PValue IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
        {
            return function(sctx, args);
        }
    }

    delegate PValue ProvidedFunctionImpl(StackContext sctx, ReadOnlySpan<PValue> args);
}
