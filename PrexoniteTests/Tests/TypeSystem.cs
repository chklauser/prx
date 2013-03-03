// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Threading;
using NUnit.Framework;
using Prexonite;
using Prexonite.Types;
using Prx.Tests;

namespace PrexoniteTests.Tests
{
    [TestFixture(Description = "General type system checks")]
    public class TypeSystem
    {
        private Engine engine;
        private StackContext sctx;

        [TestFixtureSetUp]
        public void SetupTypeSystemEngine()
        {
            engine = new Engine();
            sctx = new TestStackContext(engine, new Application());
        }

        [TestFixtureTearDown]
        public void TeardownTypeSystemEngine()
        {
            engine = null;
            sctx = null;
        }

        [Test(Description = "Does a number of set and get calls on a mock object")]
        public void TestFieldAccess()
        {
            //obj.Subject = 55;
            //obj.Count = obj.Subject;
            var test = new TestObject();
            var obj = engine.CreateNativePValue(test);
            Assert.AreEqual(obj.Type, PType.Object[typeof (TestObject)]);
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
            var res = obj.DynamicCall(
                sctx,
                new PValue[] {},
                PCall.Get,
                "Subject");
            Assert.AreEqual(test.Subject, res.Value);

            //Set obj.Count = res
            obj.DynamicCall(
                sctx,
                new[] {res},
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

        [Test(Description = "Test the implicit [basic] to [PValue] conversion operators")]
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
            var res =
                sctx.ConstructPType(
                    "Object", new[] {PType.Object.CreatePValue(typeof (Int32))});
            Assert.AreEqual(PType.Object[typeof (Int32)], res);
        }

        [Test(Description = "PType creation from type")]
        public void TestPTypeCreationUsingClrObjects_type()
        {
            var res =
                sctx.ConstructPType(
                    typeof (ObjectPType), new[] {PType.Object.CreatePValue(typeof (DateTime))});
            Assert.AreEqual(PType.Object[typeof (DateTime)], res);
        }

        [Test(Description = "PType creation from type as ClrPType")]
        public void TestPTypeCreationUsingClrObjects_clrtype()
        {
            var res =
                sctx.ConstructPType(
                    PType.Object[typeof (ObjectPType)],
                    new[] {PType.Object.CreatePValue(typeof (Thread))});
            Assert.AreEqual(PType.Object[typeof (Thread)], res);
        }

        [Test(Description = "Creation of a ClrPType from a fqTypeName")]
        public void TestTypeResolving()
        {
            var res = new ObjectPType(sctx, "System.Threading.Thread");
            Assert.AreEqual(PType.Object[typeof (Thread)], res);
        }

        [Test(Description = "PType creation from type simple expression")]
        public void TestPTypeCreationUsingExpression_simple()
        {
            var res = sctx.ConstructPType("Object(\"System.Threading.Thread\")");
            Assert.AreEqual(PType.Object[typeof (Thread)], res);
        }

        [Test]
        public void NativePValue()
        {
            var str = "Hello ";
            var nStr = engine.CreateNativePValue(str);
            Assert.IsInstanceOf(typeof (StringPType), nStr.Type);
            Assert.AreSame(str, nStr.Value);
        }

        [Test]
        public void TestStringEscape()
        {
            var sEscaped = @"This is a \n followed by a \ttab and an umlaut: \x" +
                ((int) 'ä').ToString("X") +
                    " escape sequence.";
            var sUnescaped = "This is a \n followed by a \ttab and an umlaut: ä escape sequence.";

            var escaped = PType.String.CreatePValue(sEscaped);
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