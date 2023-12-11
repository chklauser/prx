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

using System.Diagnostics;
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

    public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
        out PValue result)
    {
        Assert.IsTrue(Expectations.TryGetValue(id, out var expectation),
            $"A call to member {id} on object {Name} is not expected.");

        Assert.AreEqual(expectation!.ExpectedCall, call, "Call type (get/set)");
        Assert.AreEqual(expectation.ExpectedArguments.Length, args.Length,
            "Number of arguments do not match. Called with " + args.ToEnumerationString());
        for (var i = 0; i < expectation.ExpectedArguments.Length; i++)
            Assert.AreEqual(expectation.ExpectedArguments[i], args[i],
                $"Arguments at position {i} don't match");

        result = expectation.ReturnValue ?? PType.Null;
        expectation.WasCalled = true;
        return true;
    }

    #endregion

    public void Expect(string memberId, PValue[] args, PCall call = PCall.Get,
        PValue? returns = null)
    {
        Expectations.Add(
            memberId,
            new() {
                ExpectedArguments = args,
                ExpectedCall = call,
                ReturnValue = returns,
            });
    }

    public void AssertCalledAll()
    {
        foreach (var expectation in Expectations)
            Assert.IsTrue(
                expectation.Value.WasCalled,
                $"The member {expectation.Key} was not called.");
    }
}