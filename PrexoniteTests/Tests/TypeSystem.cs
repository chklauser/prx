using System;
using System.Threading;
using NUnit.Framework;
using Prexonite;
using Prexonite.Types;
using Prx.Tests;

namespace PrexoniteTests.Tests
{
    [TestFixture(Description="General type system checks")]
    public class TypeSystem
    {
        private Engine engine;
        private StackContext sctx;

        [TestFixtureSetUp()]
        public void SetupTypeSystemEngine()
        {
            engine = new Engine();
            sctx = new TestStackContext(engine, new Application());
        }

        [TestFixtureTearDown()]
        public void TeardownTypeSystemEngine()
        {
            engine = null;
            sctx = null;
        }

        [Test(Description="Does a number of set and get calls on a mock object")]
        public void TestFieldAccess()
        {
            //obj.Subject = 55;
            //obj.Count = obj.Subject;
            TestObject test = new TestObject();
            PValue obj = engine.CreateNativePValue(test);
            Assert.AreEqual(obj.Type, PType.Object[typeof(TestObject)]);
            Assert.AreSame(test, obj.Value);

            //Set obj.Subject = 55
            obj.DynamicCall(
                sctx,
                new PValue[] {55},
                PCall.Set,
                "Subject");
            Assert.AreSame(test, obj.Value);
            Assert.AreEqual(test.Subject, ((TestObject) obj.Value).Subject);

            //Get res = obj.Subject
            PValue res = obj.DynamicCall(
                sctx,
                new PValue[] {},
                PCall.Get,
                "Subject");
            Assert.AreEqual(test.Subject, res.Value);

            //Set obj.Count = res
            obj.DynamicCall(
                sctx,
                new PValue[] {res},
                PCall.Set,
                "Count");
            Assert.AreEqual(55, test.Count);

            //Get res = obj.Count
            res = obj.DynamicCall(
                sctx,
                new PValue[] {},
                PCall.Get,
                "Count");
            Assert.AreEqual(55, test.Count);
            Assert.AreEqual(55, (int) res.Value);
        }

        [Test(Description="Test the implicit [basic] to [PValue] conversion operators")]
        public void TestImplicitPValueConversion()
        {
            PValue obj;

            obj = 55;
            Assert.AreSame(PType.Int, obj.Type);

            obj = 5.5;
            Assert.AreSame(PType.Real, obj.Type);

            obj = true;
            Assert.AreSame(PType.Bool, obj.Type);

            obj = "Hello World";
            Assert.AreSame(PType.String, obj.Type);
        }

        [Test(Description = "PType creation from type name.")]
        public void TestPTypeCreationUsingClrObjects_name()
        {
            PType res =
                sctx.ConstructPType(
                    "Object", new PValue[] {PType.Object.CreatePValue(typeof(Int32))});
            Assert.AreEqual(PType.Object[typeof(Int32)], res);
        }

        [Test(Description = "PType creation from type")]
        public void TestPTypeCreationUsingClrObjects_type()
        {
            PType res =
                sctx.ConstructPType(
                    typeof(ObjectPType), new PValue[] {PType.Object.CreatePValue(typeof(DateTime))});
            Assert.AreEqual(PType.Object[typeof(DateTime)], res);
        }

        [Test(Description = "PType creation from type as ClrPType")]
        public void TestPTypeCreationUsingClrObjects_clrtype()
        {
            PType res =
                sctx.ConstructPType(
                    PType.Object[typeof(ObjectPType)],
                    new PValue[] {PType.Object.CreatePValue(typeof(Thread))});
            Assert.AreEqual(PType.Object[typeof(Thread)], res);
        }

        [Test(Description="Creation of a ClrPType from a fqTypeName")]
        public void TestTypeResolving()
        {
            ObjectPType res = new ObjectPType(sctx, "System.Threading.Thread");
            Assert.AreEqual(PType.Object[typeof(Thread)], res);
        }

        [Test(Description="PType creation from type simple expression")]
        public void TestPTypeCreationUsingExpression_simple()
        {
            PType res = sctx.ConstructPType("Object(\"System.Threading.Thread\")");
            Assert.AreEqual(PType.Object[typeof(Thread)], res);
        }

        [Test]
        public void NativePValue()
        {
            string str = "Hello ";
            PValue nStr = engine.CreateNativePValue(str);
            Assert.IsInstanceOfType(typeof(StringPType), nStr.Type);
            Assert.AreSame(str, nStr.Value);
        }

        [Test()]
        public void TestStringEscape()
        {
            string sEscaped = @"This is a \n followed by a \ttab and an umlaut: \x" +
                              ((int) 'ä').ToString("X") +
                              " escape sequence.";
            string sUnescaped = "This is a \n followed by a \ttab and an umlaut: ä escape sequence.";

            PValue escaped = PType.String.CreatePValue(sEscaped);
            PValue unescaped;
            Assert.IsTrue(
                escaped.TryDynamicCall(sctx, new PValue[] {}, PCall.Get, "unescape", out unescaped));
            Assert.AreEqual(sUnescaped, unescaped.Value as string);
            Assert.IsTrue(
                unescaped.TryDynamicCall(sctx, new PValue[] {}, PCall.Get, "escape", out escaped));
            Assert.AreEqual(sEscaped, escaped.Value as string);
        }
    }
}