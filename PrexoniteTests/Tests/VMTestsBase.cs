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

#define UseCil

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Compatibility;
using Prexonite.Types;
using Prx.Tests;
using Compiler = Prexonite.Compiler.Cil.Compiler;

namespace PrexoniteTests.Tests
{
    public class VMTestsBase
    {
        public const string StoreDebugImplementationKey = "store_debug_implementation";
        protected Engine engine;
        protected TestStackContext sctx;
        protected Application target;
        protected LoaderOptions options;
        public bool CompileToCil { get; set; }
        protected FunctionLinking StaticLinking { get; set; }

        public VMTestsBase()
        {
#if UseCil
            CompileToCil = true;
#else
            CompileToCil = false;
#endif
            StaticLinking = FunctionLinking.FullyStatic;
        }

        [SetUp]
        public void SetupCompilerEngine()
        {
            engine = new Engine();
            target = new Application("testApplication");
            sctx = new TestStackContext(engine, target);
            options = new LoaderOptions(engine, target);
        }

        [TearDown]
        public void TeardownCompilerEngine()
        {
            engine = null;
            sctx = null;
            target = null;
            options = null;
        }

        protected static string GenerateRandomString(int length)
        {
            return GenerateRandomString().Substring(0, length);
        }

        protected static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString("N");
        }

        protected void Compile(Loader ldr, string input)
        {
            _compile(ldr, input);
            Assert.AreEqual(0, ldr.ErrorCount, "Errors detected during compilation.");
        }

        protected Loader CompileInvalid(string input, params string[] keywords)
        {
            var ldr = new Loader(options);
            CompileInvalid(ldr, input, keywords);
            return ldr;
        }

        protected void CompileInvalid(Loader ldr, string input, params string[] keywords)
        {
            _compile(ldr, input);
            Assert.AreNotEqual(0, ldr.Errors.Count(m => m.Severity == MessageSeverity.Error),
                "Errors expected, but none were raised.");
            foreach (var keyword in keywords)
            {
                var word = keyword;
                Assert.IsTrue(ldr.Errors.Any(m => m.Text.Contains(word)),
                    "Expected keyword " + word + " in one of the error messages.");
            }
        }

        private void _compile(Loader ldr, string input)
        {
            try
            {
                ldr.LoadFromString(input);
                if (CompileToCil)
                    Compiler.Compile(ldr.ParentApplication, ldr.ParentEngine, StaticLinking);
            }
            finally
            {
                foreach (var s in ldr.Errors)
                    Console.WriteLine("ERROR: " + s);
                foreach (var s in ldr.Warnings)
                    Console.WriteLine("WARNING: " + s);
                foreach (var s in ldr.Infos)
                    Console.WriteLine("INFO: " + s);
                Console.WriteLine(ldr.StoreInString());
            }
        }

        protected Loader Compile(string input)
        {
            var ldr = new Loader(options);
            Compile(ldr, input);
            return ldr;
        }

        protected Loader Store(Loader ldr)
        {
            var sb = new StringBuilder();
            ldr.Store(sb);


            //Create a new engine
            SetupCompilerEngine();

            ldr = new Loader(options);
            try
            {
                ldr.LoadFromString(sb.ToString());
            }
            finally
            {
                foreach (var s in ldr.Errors)
                {
                    Console.Error.WriteLine(s);
                }
            }
            Assert.AreEqual(0, ldr.ErrorCount, "Errors detected while loading stored code.");
            Console.WriteLine(ldr.StoreInString());
            return ldr;
        }

        protected Loader CompileStore(Loader loader, string input)
        {
            Compile(loader, input);
            return Store(loader);
        }

        protected Loader CompileStore(string input)
        {
            return Store(Compile(input));
        }

        protected void Expect<T>(T expectedReturnValue, params PValue[] args)
        {
            ExpectReturnValue(target.Meta[Application.EntryKey], expectedReturnValue, args);
        }

        protected void ExpectNamed<T>(string functionId, T expectedReturnValue, params PValue[] args)
        {
            ExpectReturnValue(functionId, expectedReturnValue, args);
        }

        protected void ExpectReturnValue<T>(string functionId, T expectedReturnValue, PValue[] args)
        {
            if (!args.All(value => value != null))
                throw new ArgumentException(
                    "Arguments must not contain naked CLR null references. Use `PType.Null`.");

            var expected = engine.CreateNativePValue(expectedReturnValue);
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");

            Console.WriteLine("Expecting " + functionId + " to return " + expected);

            var func = target.Functions[functionId];
            if (func.Meta[StoreDebugImplementationKey].Switch)
                Compiler.StoreDebugImplementation(target, engine);

            PValue rv;
            try
            {
                rv = func.Run(engine, args);
            }
            catch (AccessViolationException)
            {
                Console.WriteLine(
                    "Detected AccessViolationException. Trying to store debug implementation (Repeats CIL compilation)");
                Compiler.StoreDebugImplementation(target, engine);
                throw;
            }
            catch (InvalidProgramException)
            {
                Console.WriteLine(
                    "Detected InvalidProgramException. Trying to store debug implementation (Repeats CIL compilation)");
                Compiler.StoreDebugImplementation(target, engine);
                throw;
            }

            AssertPValuesAreEqual(expected, rv);
        }

        public void AssertPValuesAreEqual(PValue expected, PValue rv)
        {
            Assert.AreEqual(
                expected.Type,
                rv.Type,
                string.Format(
                    "Return value is expected to be of type {0} and not {1}. Returned {2}.",
                    expected.Type,
                    rv.Type,
                    rv));
            if (expected.Type == PType.List)
            {
                var expectedL = (List<PValue>) expected.Value;
                var rvL = (List<PValue>) rv.Value;
                Assert.AreEqual(expectedL.Count, rvL.Count,
                    string.Format("Returned list differs in length. Elements returned {0}",
                        rvL.ToEnumerationString()));

                for (var i = 0; i < expectedL.Count; i++)
                {
                    var expectedLi = expectedL[i];
                    var rvLi = rvL[i];
                    AssertPValuesAreEqual(expectedLi, rvLi);
                }
            }
            else
            {
                Assert.AreEqual(
                    expected.Value,
                    rv.Value,
                    "Return value is expected to be " + expected + " and not " +
                        rv);
            }
        }

        protected void ExpectNull(params PValue[] args)
        {
            ExpectReturnValue<object>(target.Meta[Application.EntryKey], null, args);
        }

        protected void ExpectNull(string functionId, params PValue[] args)
        {
            ExpectReturnValue<object>(functionId, null, args);
        }

        protected PValue GetReturnValueNamed(string functionId, params PValue[] args)
        {
            return GetReturnValueNamedExplicit(functionId, args);
        }

        protected PValue GetReturnValueNamedExplicit(string functionId, PValue[] args)
        {
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");
            var fctx = target.Functions[functionId].CreateFunctionContext(engine, args);
            engine.Stack.AddLast(fctx);
            return engine.Process();
        }

        protected PValue GetReturnValue(params PValue[] args)
        {
            return GetReturnValueNamedExplicit(target.Meta[Application.EntryKey], args);
        }

        protected SymbolEntry LookupSymbolEntry(SymbolStore store, string symbolicId)
        {
            Prexonite.Compiler.Symbolic.Symbol symbol;
            Assert.IsTrue(store.TryGet(symbolicId, out symbol),
                          string.Format("Expected to find symbol {0} but there is no such entry.", symbolicId));
            CallSymbol callSymbol;
            Assert.IsTrue(symbol.TryGetCallSymbol(out callSymbol), string.Format("Expected symbol {0} to be an entity symbol. Actual: {1}.", symbolicId, symbol));
            return callSymbol.ToSymbolEntry();
        }

        protected void BoolTable4(Func<bool, bool, bool, bool, string> main, PValue pTrue,
            PValue pFalse)
        {
            Expect(main(false, false, false, false), pFalse, pFalse, pFalse, pFalse);
            Expect(main(false, false, false, true), pFalse, pFalse, pFalse, pTrue);
            Expect(main(false, false, true, false), pFalse, pFalse, pTrue, pFalse);
            Expect(main(false, false, true, true), pFalse, pFalse, pTrue, pTrue);
            Expect(main(false, true, false, false), pFalse, pTrue, pFalse, pFalse);
            Expect(main(false, true, false, true), pFalse, pTrue, pFalse, pTrue);
            Expect(main(false, true, true, false), pFalse, pTrue, pTrue, pFalse);
            Expect(main(false, true, true, true), pFalse, pTrue, pTrue, pTrue);

            Expect(main(true, false, false, false), pTrue, pFalse, pFalse, pFalse);
            Expect(main(true, false, false, true), pTrue, pFalse, pFalse, pTrue);
            Expect(main(true, false, true, false), pTrue, pFalse, pTrue, pFalse);
            Expect(main(true, false, true, true), pTrue, pFalse, pTrue, pTrue);
            Expect(main(true, true, false, false), pTrue, pTrue, pFalse, pFalse);
            Expect(main(true, true, false, true), pTrue, pTrue, pFalse, pTrue);
            Expect(main(true, true, true, false), pTrue, pTrue, pTrue, pFalse);
            Expect(main(true, true, true, true), pTrue, pTrue, pTrue, pTrue);
        }
    }
}