using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class PartialApplication : Prx.Tests.VMTestsBase
    {

        #region Mock implementation of partial application

        public class PartialApplicationMock : PartialApplicationBase
        {
            public PartialApplicationMock(int[] mappings, PValue[] closedArguments, int theNonArgumentPrefox)
                : base(mappings, closedArguments, theNonArgumentPrefox)
            {
            }

            #region Overrides of PartialApplicationBase

            protected override PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments)
            {
                var temp = InvokeImpl;
                if (temp != null)
                {
                    return temp(sctx, nonArguments, arguments);
                }
                else
                {
                    //ignore in that case
                    return null;
                }
            }

            public Func<StackContext, PValue[], PValue[], PValue> InvokeImpl { get; set; }

            #endregion
        }


        public class RoundtripPartialApplicationCommandMock : PartialApplicationCommandBase<Object>
        {
            #region Overrides of PartialApplicationCommandBase

            protected override IIndirectCall CreatePartialApplication(StackContext sctx1, int[] mappings, PValue[] closedArguments, object parameter)
            {
                return new PartialApplicationImplMock { Mappings = mappings, ClosedArguments = closedArguments };
            }

            protected override Type GetPartialCallRepresentationType(object parameter)
            {
                return typeof (PartialApplicationImplMock);
            }

            #endregion
        }

        public class PartialApplicationImplMock : IIndirectCall
        {
            public PartialApplicationImplMock()
            {
            }

            public PartialApplicationImplMock(int[] mappings, PValue[] closedArguments)
            {
                Mappings = mappings;
                ClosedArguments = closedArguments;
            }


            public int[] Mappings { get; set; }
            public PValue[] ClosedArguments { get; set; }
            public Func<int[], PValue[], StackContext, PValue[], PValue> IndirectCallImpl { get; set; }

            #region Implementation of IIndirectCall

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                var indirectCallImpl = IndirectCallImpl;
                return indirectCallImpl == null ? PType.Null : indirectCallImpl(Mappings, ClosedArguments, sctx, args);
            }

            #endregion
        }

        #endregion

        [Test]
        public void ZeroArgumentsPassed()
        {
            const int nonArgc = 2;
            var closedArguments = new PValue[] { 1, 2 };
            var mappings = new[] { -1, 1, -1, 2, -2 };
            var pa = new PartialApplicationMock(mappings, closedArguments, nonArgc);
            Assert.AreSame(mappings, pa.Mappings);

            var callArgs = new PValue[] { };

            pa.InvokeImpl = (ctx, nonArgs, args) =>
            {
                Assert.AreSame(sctx, ctx, "Expected an unmodified stack context");
                Assert.IsNotNull(nonArgs);
                Assert.IsNotNull(args);
                Assert.AreEqual(nonArgc, nonArgs.Length, "unexpected number of non-arguments");
                Assert.AreEqual(mappings.Length - nonArgc, args.Length);
                for (var i = 0; i < args.Length; i++)
                    if (i == 1)
                        Assert.AreEqual(closedArguments[1], args[i], "Closed argument expected.");
                    else
                        Assert.IsNull(args[i].Value, string.Format("Effective argument at position {0} is not {{Null}}", i));

                Assert.AreSame(PType.Null.CreatePValue(), nonArgs[0], "Open argument #1 expected at non-arg position 0.");
                Assert.AreSame(closedArguments[0], nonArgs[1], "Closed argument #1 expected at non-arg position 1.");

                //check args
                Assert.AreSame(PType.Null.CreatePValue(), args[0], "Open argument #1 expected at position 0.");
                Assert.AreSame(closedArguments[1], args[1], "Closed argument #2 expected at position 1.");
                Assert.AreSame(PType.Null.CreatePValue(), args[2], "Open argument #2 expected at position 2.");

                return 77;
            };

            var result = pa.IndirectCall(sctx, callArgs);
            Assert.AreEqual(77, result.Value);
        }

        [Test]
        public void ExactArgumentsPassed()
        {
            const int nonArgc = 2;
            var closedArguments = new PValue[] { 1, 2 };
            var mappings = new[] { -1, 1, -1, 2, -2 };
            var pa = new PartialApplicationMock(mappings, closedArguments, nonArgc);
            Assert.AreSame(mappings, pa.Mappings);

            var callArgs = new PValue[] { "a", "b" };

            pa.InvokeImpl = (ctx, nonArgs, args) =>
            {
                Assert.AreSame(sctx, ctx, "Expected an unmodified stack context");
                Assert.IsNotNull(nonArgs);
                Assert.IsNotNull(args);
                Assert.AreEqual(nonArgc, nonArgs.Length, "unexpected number of non-arguments");
                Assert.AreEqual(mappings.Length - nonArgc, args.Length);

                //check non-args
                Assert.AreSame(callArgs[0], nonArgs[0], "Open argument #1 expected at non-arg position 0.");
                Assert.AreSame(closedArguments[0], nonArgs[1], "Closed argument #1 expected at non-arg position 1.");

                //check args
                Assert.AreSame(callArgs[0], args[0], "Open argument #1 expected at position 0.");
                Assert.AreSame(closedArguments[1], args[1], "Closed argument #2 expected at position 1.");
                Assert.AreSame(callArgs[1], args[2], "Open argument #2 expected at position 2.");

                return 77;
            };

            var result = pa.IndirectCall(sctx, callArgs);
            Assert.AreEqual(77, result.Value);
        }

        [Test]
        public void TooManyArgumentsPassed()
        {
            const int nonArgc = 2;
            var closedArguments = new PValue[] { 1, 2 };
            var mappings = new[] { -1, 1, -1, 2, -2 };
            var pa = new PartialApplicationMock(mappings, closedArguments, nonArgc);
            Assert.AreSame(mappings, pa.Mappings);

            var callArgs = new PValue[] { "a", "b", "c", "d", "e", "f", "g" };

            pa.InvokeImpl = (ctx, nonArgs, args) =>
            {
                Assert.AreSame(sctx, ctx, "Expected an unmodified stack context");
                Assert.IsNotNull(nonArgs);
                Assert.IsNotNull(args);
                Assert.AreEqual(nonArgc, nonArgs.Length, "unexpected number of non-arguments");
                Assert.AreEqual(mappings.Length - nonArgc + callArgs.Length - mappings.Where(x => x < 0).Distinct().Count(), args.Length, "unexpected number of effective arguments");

                //check non-args
                Assert.AreSame(callArgs[0], nonArgs[0], "Open argument #1 expected at non-arg position 0.");
                Assert.AreSame(closedArguments[0], nonArgs[1], "Closed argument #1 expected at non-arg position 1.");

                //check args
                Assert.AreSame(callArgs[0], args[0], "Open argument #1 expected at position 0.");
                Assert.AreSame(closedArguments[1], args[1], "Closed argument #2 expected at position 1.");
                Assert.AreSame(callArgs[1], args[2], "Open argument #2 expected at position 2.");

                //check excess args
                for (var i = 3; i < args.Length; i++)
                    Assert.AreSame(
                        callArgs[i - (3 - 2)], args[i], string.Format("Excess arguments don't match at position {0}", i));

                return 77;
            };

            var result = pa.IndirectCall(sctx, callArgs);
            Assert.AreEqual(77, result.Value);
        }

        [Test]
        public void NoPrefix()
        {
            const int nonArgc = 0;
            var closedArguments = new PValue[] { 1, 2 };
            var mappings = new[] { -1, 1, -1, 2, -2 };
            var pa = new PartialApplicationMock(mappings, closedArguments, nonArgc);
            Assert.AreSame(mappings, pa.Mappings);

            var callArgs = new PValue[] { "a", "b", "c", "d", "e", "f", "g" };

            pa.InvokeImpl = (ctx, nonArgs, args) =>
            {
                Assert.AreSame(sctx, ctx, "Expected an unmodified stack context");
                Assert.IsNotNull(nonArgs);
                Assert.IsNotNull(args);
                Assert.AreEqual(nonArgc, nonArgs.Length, "unexpected number of non-arguments");
                Assert.AreEqual(mappings.Length - nonArgc + callArgs.Length - mappings.Where(x => x < 0).Distinct().Count(), args.Length, "unexpected number of effective arguments");

                //check non-args

                //check args
                Assert.AreSame(callArgs[0], args[0], "Open argument #1 expected at non-arg position 0.");
                Assert.AreSame(closedArguments[0], args[1], "Closed argument #1 expected at non-arg position 1.");
                Assert.AreSame(callArgs[0], args[2], "Open argument #1 expected at position 2.");
                Assert.AreSame(closedArguments[1], args[3], "Closed argument #2 expected at position 3.");
                Assert.AreSame(callArgs[1], args[4], "Open argument #2 expected at position 4.");

                //check excess args
                for (var i = 5; i < args.Length; i++)
                    Assert.AreSame(
                        callArgs[i - (5 - 2)], args[i], string.Format("Excess arguments don't match at position {0}", i));

                return 77;
            };

            var result = pa.IndirectCall(sctx, callArgs);
            Assert.AreEqual(77, result.Value);
        }

        [Test]
        public void NoMappingExcessArgs()
        {
            const int nonArgc = 2;
            var closedArguments = new PValue[] { };
            var mappings = new int[] { };
            var pa = new PartialApplicationMock(mappings, closedArguments, nonArgc);
            Assert.AreSame(mappings, pa.Mappings);

            var callArgs = new PValue[] { "a", "b", "c", "d", "e", "f", "g" };

            pa.InvokeImpl = (ctx, nonArgs, args) =>
            {
                Assert.AreSame(sctx, ctx, "different stack context");
                Assert.IsNotNull(nonArgs);
                Assert.IsNotNull(args);
                Assert.AreEqual(nonArgc, nonArgs.Length, "number of non-arguments");
                Assert.AreEqual(
                    mappings.Length - nonArgc + callArgs.Length
                    - mappings.Where(x => x < 0).Distinct().Count(), args.Length,
                    "number of effective arguments");

                //check non-args
                Assert.AreSame(callArgs[0], nonArgs[0], "Open argument #1 expected at non-arg position 0.");
                Assert.AreSame(callArgs[1], nonArgs[1], "Open argument #2 expected at non-arg position 1.");

                //check args
                for (var i = 0; i < 5; i++ )
                    Assert.AreSame(callArgs[i+2], args[i], string.Format("Open argument #{0} expected at arg position {1}.", i+3,i));

                return 77;
            };

            var result = pa.IndirectCall(sctx, callArgs);
            Assert.AreEqual(77, result.Value);
        }

        [Test]
        public void PackRoundtrip32()
        {
            var mappings = new[]
            {
                1, -8, 2, -13, 3, -5, 4, 5
            };

            var packed = PartialApplicationCommandBase.PackMappings32(mappings);
            Assert.IsNotNull(packed);
            Assert.IsTrue(packed.Length <= mappings.Length, "Packed length must not be longer than mappings length");


            var packedPValues = packed.Select(i => (PValue) i);
            var closedArguments = new PValue[] {"a", "b", "c", "d", "e"};
            var argv = closedArguments.Append(packedPValues);
            var roundtripCommandMock = new RoundtripPartialApplicationCommandMock();
            var mockP = roundtripCommandMock.Run(sctx, argv.ToArray());

            Assert.IsNotNull(mockP.Value);
            Assert.IsAssignableFrom(typeof(PartialApplicationImplMock), mockP.Value);

            var mock = (PartialApplicationImplMock) mockP.Value;

            Assert.IsNotNull(mock.Mappings,"Mappings must no be null");
            Assert.AreEqual(mappings.Length, mock.Mappings.Length, "Mappings lengths don't match");

            //check mappings
            for (var i = 0; i < mappings.Length; i++)
            {
                var mappingE = mappings[i];
                var mappingA = mock.Mappings[i];

                Assert.AreEqual(mappingE, mappingA, string.Format("The mappings at index {0} are not equal.", i));
            }

            Assert.IsNotNull(mock.ClosedArguments, "Closed arguments must not be null");
            Assert.AreEqual(
                closedArguments.Length, mock.ClosedArguments.Length, "Closed arguments lengths don't match");
            //check closed arguments);
            for (var i = 0; i < closedArguments.Length; i++)
            {
                var caE = closedArguments[i];
                var caA = mock.ClosedArguments[i];

                Assert.AreSame(caE, caA, string.Format("The closed arguments at index {0} are not the same.", i));
            }
        }

        [Test]
        public void IndBasicExplicit()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = proc(?0,?1,?2);
    return pa.(x,y,z);
}
");

            Expect("a=1, b=2, c=3", 1,2,3);
        }

        [Test]
        public void IndBasicImplicit()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = proc(?,?,?);
    return pa.(x,y,z);
}
");

            Expect("a=1, b=2, c=3", 1, 2, 3);
        }

        [Test]
        public void BasicDefaultToNull()
        {
            Compile(@"
function main(x)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$(c is null)"";
    var pa = proc(?,?,?);
    return pa.(x);
}
");

            Expect("a=1, b=, c=" + (sctx.CreateNativePValue(true).CallToString(sctx)), 1);
        }
        
        [Test]
        public void AsLoadReferenceNotation()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = proc(?);
    return pa.(x,y,z);
}
");

            Expect("a=1, b=2, c=3", 1,2,3);
        }
        
        [Test]
        public void BasicExcess()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = proc(?2);
    return pa.(x,y,z);
}
");

            Expect("a=3, b=1, c=2", 1,2,3);
        }

        [Test]
        public void MissingMapped()
        {
            Compile(@"
function main(x)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = proc(?1,?2);
    return pa.(x);
}
");

            Expect("a=, b=, c=1", 1, 2, 3);
        }

        [Test]
        public void PartialCallOperatorSimple()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = ?.();
    return pa.(->proc, x, y);
}
");

            Expect("a=1, b=2, c=", 1, 2, 3);
        }

        [Test]
        public void PartialCallOperator()
        {
            Compile(@"
function main(x,y,z)
{
    function proc(a,b,c) = ""a=$a, b=$b, c=$c"";
    var pa = ?2.(?1,?0);
    return pa.(x, y, ->proc, z);
}
");

            Expect("a=2, b=1, c=3", 1, 2, 3);
        }


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
                Assert.IsTrue(_expectations.TryGetValue(id, out expectation), string.Format("A call to member {0} on object {1} is not expected.", id, Name));

                Assert.AreEqual(expectation.ExpectedCall, call,"Call type (get/set)");
                Assert.AreEqual(expectation.ExpectedArguments.Length, args.Length, "Number of arguments do not match. Called with " + args.ToEnumerationString());
                for(var i = 0; i < expectation.ExpectedArguments.Length; i++)
                    Assert.AreEqual(expectation.ExpectedArguments[i], args[i], string.Format("Arguments at position {0} don't match", i));

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
                        string.Format("The member {0} was not called.", expectation.Key));
            }
        }

        [Test]
        public void MemberSimpleGet()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = x.m(?,y);
    return pa.(z);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("m",new PValue[]{3,2}, call: PCall.Get, returns: 11);

            Expect(11, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void MemberSimpleGetIndex()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = x[y,?];
    return pa.(z);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("", new PValue[] { 2, 3 }, call: PCall.Get, returns: 11);

            Expect(11, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void MemberSubjectGet()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ?.m(z,?);
    return pa.(x,y);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("m", new PValue[] { 3, 2 }, call: PCall.Get, returns: 11);

            Expect(11, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }
        
        [Test]
        public void MemberOperatorGet()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ?.m;
    return pa.(x,z,y);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("m", new PValue[] { 3, 2 }, call: PCall.Get, returns: 11);

            Expect(11, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void MemberOperatorSet()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ?.m = ?;
    return pa.(x,z,y);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("m", new PValue[] { 3, 2 }, call: PCall.Set, returns: 11);

            Expect(2, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void MemberSetSimple()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = x.m(?) = ?;
    return pa.(z,y);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("m", new PValue[] { 3, 2 }, call: PCall.Set, returns: 11);

            Expect(2, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void MemberSetIndex()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = x[?] = ?;
    return pa.(z,y);
}
");

            var x = new MemberCallable {Name = "x"};
            x.Expect("", new PValue[] { 3, 2 }, call: PCall.Set, returns: 11);

            Expect(2, sctx.CreateNativePValue(x), 2, 3);
            x.AssertCalledAll();
        }

        [Test]
        public void Construct1()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = new List(?2,?1);
    return pa.(x,y,z);
}
");
            PValue x = 1;
            PValue y = 2;
            PValue z = 3;
            Expect(new List<PValue>{z,y,x}, x,y,z);
        }

        [Test]
        public void ConstructCustomFallback()
        {
            Compile(@"
function create_box(a,b,c) = [a, c, b];

function main(x,y,z)
{
    var pa = new box(?2);
    return pa.(x,y,z);
}
");


            PValue x = 1;
            PValue y = 2;
            PValue z = 3;
            Expect(new List<PValue> { z, y, x }, x, y, z);
        }

        [Test]
        public void ConstructDynamicType()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = new Object<(""System.$x"")>(y,?0,?0);
    return pa.(z);
}
");

            Expect(new DateTime(2010,10,10),"DateTime",2010,10);
        }

        [Test]
        public void TypeCast()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ?~Int;
    return pa.(x) + pa.(y,z);
}
");

            Expect(5,"2",3.0,"sixteen");
        }

        [Test]
        public void DynamicTypeCast()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ?~Object<(x)>;
    return pa.(y) is Object<(x)>;
}
");

            Expect(true, "Prexonite.StackContext",sctx.CreateNativePValue(sctx));
        }

        [Test]
        public void DynamicTypeCheck()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ? is Object<(x)>;
    return pa.(y~Object<(x)>);
}
");

            Expect(true, "Prexonite.StackContext", sctx.CreateNativePValue(sctx));
        }

        [Test]
        public void TypeCheck()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ? is String;
    function i(b) = if(b) ""T"" else ""_"";
    return i(pa.(x)) + i(pa.(y)) + i(pa.(z,x));
}
");
            Expect("T_T","I'm", 'a', "String");
        }

        [Test]
        public void NegativeTypeCheck()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ? is not String;
    function i(b) = if(b) ""T"" else ""_"";
    return i(pa.(x)) + i(pa.(y)) + i(pa.(z,x));
}
");
            Expect("_T_", "I'm", 'a', "String");
        }

        [Test]
        public void NullCheck()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ? is null;
    var pa2 = ? is not null;
    function i(b) = if(b) ""T"" else ""_"";
    return i(pa.(x)) + i(pa.(y)) + i(pa.(z,x)) + i(pa2.(x)) + i(pa2.(y)) + i(pa2.(z,x));
}
");

            Expect("_T_T_T","I'm",PType.Null,1);
        }

        [Test]
        public void StaticCall()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = System::Int32.Parse(?);
    var pa2 = System::Int32.MaxValue(?);
    return pa2.() - pa.(x);
}
");

            Expect(Int32.MaxValue - 255, "255");
        }

        [Test]
        public void DynamicStaticCall()
        {
            Compile(@"
function main(x,y,z)
{
    var pa = ~Object<(y)>.Parse(?);
    var pa2 = ~Object<(y)>.MaxValue(?);
    return pa2.() - pa.(x);
}
");

            Expect(Int32.MaxValue - 255, "255","System.Int32");
        }
    }
}
