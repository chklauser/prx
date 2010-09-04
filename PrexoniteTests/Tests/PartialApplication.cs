using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Types;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class PartialApplication : Prx.Tests.VMTestsBase
    {

        #region Mock implementation of partial application

        public class PartialApplicationMock : PartialApplicationBase
        {
            public PartialApplicationMock(sbyte[] mappings, PValue[] closedArguments, int theNonArgumentPrefox)
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


        public class RoundtripPartialApplicationCommandMock : PartialApplicationCommandBase
        {
            #region Overrides of PartialApplicationCommandBase

            protected override IIndirectCall CreatePartialApplication(sbyte[] mappings, PValue[] closedArguments)
            {
                return new PartialApplicationImplMock { Mappings = mappings, ClosedArguments = closedArguments };
            }

            #endregion
        }

        public class PartialApplicationImplMock : IIndirectCall
        {

            public sbyte[] Mappings { get; set; }
            public PValue[] ClosedArguments { get; set; }
            public Func<sbyte[], PValue[], StackContext, PValue[], PValue> IndirectCallImpl { get; set; }

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
            var mappings = new sbyte[] { -1, 1, -1, 2, -2 };
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
            var mappings = new sbyte[] { -1, 1, -1, 2, -2 };
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
            var mappings = new sbyte[] { -1, 1, -1, 2, -2 };
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
            var mappings = new sbyte[] { -1, 1, -1, 2, -2 };
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
            var mappings = new sbyte[] { };
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
            var mappings = new sbyte[]
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

    }
}
