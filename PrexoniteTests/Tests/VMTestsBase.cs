//comment the following line to temporarily disable CIL compilation tests.
#define UseCil

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;


namespace Prx.Tests
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
            Assert.AreNotEqual(0,ldr.Errors.Count(m => m.Severity == ParseMessageSeverity.Error), "Errors expected, but none were raised.");
            foreach (var keyword in keywords)
            {
                var word = keyword;
                Assert.IsTrue(ldr.Errors.Any(m => m.Message.Contains(word)),
                              "Expected keyword " + word + " in one of the error messages.");
            }
        }

        private void _compile(Loader ldr, string input)
        {
            try
            {
                ldr.LoadFromString(input);
                if(CompileToCil)
                    Prexonite.Compiler.Cil.Compiler.Compile(ldr.ParentApplication, ldr.ParentEngine,StaticLinking);
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
            Loader ldr = new Loader(options);
            Compile(ldr, input);
            return ldr;
        }

        protected Loader Store(Loader ldr)
        {
            StringBuilder sb = new StringBuilder();
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
            if(!args.All(value => value != null))
                throw new ArgumentException("Arguments must not contain naked CLR null references. Use `PType.Null`.");

            var expected = engine.CreateNativePValue(expectedReturnValue);
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");

            Console.WriteLine("Expecting " + functionId + " to return " + expected);

            var func = target.Functions[functionId];
            if (func.Meta[StoreDebugImplementationKey].Switch)
                Prexonite.Compiler.Cil.Compiler.StoreDebugImplementation(target, engine);

            PValue rv;
            try
            {
                rv = func.Run(engine, args);
            }
            catch(AccessViolationException)
            {
                Console.WriteLine("Detected AccessViolationException. Trying to store debug implementation (Repeats CIL compilation)");
                Prexonite.Compiler.Cil.Compiler.StoreDebugImplementation(target, engine);
                throw; 
            }
            catch(InvalidProgramException)
            {
                Console.WriteLine("Detected InvalidProgramException. Trying to store debug implementation (Repeats CIL compilation)");
                Prexonite.Compiler.Cil.Compiler.StoreDebugImplementation(target, engine);
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
                var rvL = (List<PValue>)rv.Value;
                Assert.AreEqual(expectedL.Count, rvL.Count, string.Format("Returned list differs in length. Elements returned {0}", rvL.ToEnumerationString()));

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
            FunctionContext fctx = target.Functions[functionId].CreateFunctionContext(engine, args);
            engine.Stack.AddLast(fctx);
            return engine.Process();
        }

        protected PValue GetReturnValue(params PValue[] args)
        {
            return GetReturnValueNamedExplicit(target.Meta[Application.EntryKey], args);
        }

        protected void BoolTable4(Func<bool, bool, bool, bool, string> main, PValue pTrue, PValue pFalse)
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