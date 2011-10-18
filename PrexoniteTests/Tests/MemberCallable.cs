// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Diagnostics;
using NUnit.Framework;
using Prexonite;
using Prexonite.Types;

namespace PrexoniteTests.Tests
{
    public class MemberCallable : IObject
    {
        public class CallExpectation
        {
            public PCall ExpectedCall { get; set; }
            public PValue[] ExpectedArguments { get; set; }
            public PValue ReturnValue { get; set; }
            public bool WasCalled { get; set; }
        }

        public string Name { get; set; }

        private readonly SymbolTable<CallExpectation> _expectations =
            new SymbolTable<CallExpectation>(8);

        public SymbolTable<CallExpectation> Expectations
        {
            [DebuggerStepThrough]
            get { return _expectations; }
        }

        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
        {
            CallExpectation expectation;
            Assert.IsTrue(_expectations.TryGetValue(id, out expectation),
                String.Format("A call to member {0} on object {1} is not expected.", id, Name));

            Assert.AreEqual(expectation.ExpectedCall, call, "Call type (get/set)");
            Assert.AreEqual(expectation.ExpectedArguments.Length, args.Length,
                "Number of arguments do not match. Called with " + args.ToEnumerationString());
            for (var i = 0; i < expectation.ExpectedArguments.Length; i++)
                Assert.AreEqual(expectation.ExpectedArguments[i], args[i],
                    String.Format("Arguments at position {0} don't match", i));

            result = expectation.ReturnValue ?? PType.Null;
            expectation.WasCalled = true;
            return true;
        }

        #endregion

        public void Expect(string memberId, PValue[] args, PCall call = PCall.Get,
            PValue returns = null)
        {
            _expectations.Add(
                memberId,
                new CallExpectation
                    {
                        ExpectedArguments = args,
                        ExpectedCall = call,
                        ReturnValue = returns
                    });
        }

        public void AssertCalledAll()
        {
            foreach (var expectation in _expectations)
                Assert.IsTrue(
                    expectation.Value.WasCalled,
                    String.Format("The member {0} was not called.", expectation.Key));
        }
    }
}