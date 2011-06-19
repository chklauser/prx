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

        private readonly SymbolTable<CallExpectation> _expectations = new SymbolTable<CallExpectation>(8);

        public SymbolTable<CallExpectation> Expectations
        {
            [DebuggerStepThrough]
            get { return _expectations; }
        }

        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            CallExpectation expectation;
            Assert.IsTrue(_expectations.TryGetValue(id, out expectation), String.Format("A call to member {0} on object {1} is not expected.", id, Name));

            Assert.AreEqual(expectation.ExpectedCall, call,"Call type (get/set)");
            Assert.AreEqual(expectation.ExpectedArguments.Length, args.Length, "Number of arguments do not match. Called with " + args.ToEnumerationString());
            for(var i = 0; i < expectation.ExpectedArguments.Length; i++)
                Assert.AreEqual(expectation.ExpectedArguments[i], args[i], String.Format("Arguments at position {0} don't match", i));

            result = expectation.ReturnValue ?? PType.Null;
            expectation.WasCalled = true;
            return true;
        }

        #endregion

        public void Expect(string memberId, PValue[] args, PCall call = PCall.Get, PValue returns = null)
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