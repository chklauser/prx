#define UseCil

using System;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;



namespace Prx.Tests
{
    public class VMTestsBase
    {
        protected Engine engine;
        protected TestStackContext sctx;
        protected Application target;
        protected LoaderOptions options;
        public bool CompileToCil { get; set; }

        public VMTestsBase()
        {
            CompileToCil = true;
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
            try
            {
                ldr.LoadFromString(input);
#if UseCil
                if(CompileToCil)
                    Prexonite.Compiler.Cil.Compiler.Compile(ldr.ParentApplication, ldr.ParentEngine);
#endif                
            }
            finally
            {
                foreach (string s in ldr.Errors)
                {
                    Console.WriteLine(s);
                }
            }
            Assert.AreEqual(0, ldr.ErrorCount, "Errors detected during compilation.");
            Console.WriteLine(ldr.StoreInString());
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
                foreach (string s in ldr.Errors)
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
            PValue expected = engine.CreateNativePValue(expectedReturnValue);
            if (!target.Functions.Contains(functionId))
                throw new PrexoniteException("Function " + functionId + " cannot be found.");

            var rv = target.Functions[functionId].Run(engine, args);
            
            Assert.AreEqual(
                expected.Type,
                rv.Type,
                string.Format(
                    "Return type is expected to be of type {0} and not {1}. Returned {2}.",
                    expected.Type,
                    rv.Type,
                    rv));
            Assert.AreEqual(
                expected.Value,
                rv.Value,
                "Return value is expected to be " + expected + " and not " +
                rv);
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
    }
}