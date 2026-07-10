using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Prexonite;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

public class MemberCallable : IObject
{
    public class CallExpectation
    {
        public required PCall ExpectedCall { get; init; }
        public required PValue[] ExpectedArguments { get; init; }
        public required PValue? ReturnValue { get; init; }
        public bool WasCalled { get; set; }
    }

    public required string Name { get; init; }

    public SymbolTable<CallExpectation> Expectations { [DebuggerStepThrough] get; } = new(8);

    #region Implementation of IObject

    public bool TryDynamicCall(
        StackContext sctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        Assert.IsTrue(
            Expectations.TryGetValue(id, out var expectation),
            $"A call to member {id} on object {Name} is not expected."
        );

        Assert.AreEqual(expectation!.ExpectedCall, call, "Call type (get/set)");
        Assert.AreEqual(
            expectation.ExpectedArguments.Length,
            args.Length,
            "Number of arguments do not match. Called with " + args.ToEnumerationString()
        );
        for (var i = 0; i < expectation.ExpectedArguments.Length; i++)
            Assert.AreEqual(
                expectation.ExpectedArguments[i],
                args[i],
                $"Arguments at position {i} don't match"
            );

        result = expectation.ReturnValue ?? PType.Null;
        expectation.WasCalled = true;
        return true;
    }

    #endregion

    public void Expect(
        string memberId,
        PValue[] args,
        PCall call = PCall.Get,
        PValue? returns = null
    )
    {
        Expectations.Add(
            memberId,
            new()
            {
                ExpectedArguments = args,
                ExpectedCall = call,
                ReturnValue = returns,
            }
        );
    }

    public void AssertCalledAll()
    {
        foreach (var expectation in Expectations)
            Assert.IsTrue(
                expectation.Value.WasCalled,
                $"The member {expectation.Key} was not called."
            );
    }
}
