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
using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Compatibility;

namespace Prx.Tests
{
    public class Compiler
    {
        #region Setup

        protected internal Engine engine;
        protected internal TestStackContext sctx;
        protected internal Application target;
        protected internal PFunction root;

        [SetUp]
        public void SetupCompilerEngine()
        {
            engine = new Engine();
            target = new Application("testApplication");
            sctx = new TestStackContext(engine, target);
        }

        [TearDown]
        public void TeardownCompilerEngine()
        {
            engine = null;
            sctx = null;
            target = null;
            root = null;
        }

        #endregion

        #region Helper

        protected SymbolEntry LookupSymbolEntry(SymbolStore store, string symbolicId)
        {
            Symbol symbol;
            Assert.IsTrue(store.TryGet(symbolicId, out symbol),
                          string.Format("Expected to find symbol {0} but there is no such entry.", symbolicId));
            EntitySymbol entitySymbol;
            Assert.IsTrue(symbol.TryGetEntitySymbol(out entitySymbol),string.Format("Expected symbol {0} to be an entity symbol. Actual: {1}.", symbolicId, symbol));
            return entitySymbol.ToSymbolEntry();
        }

        protected  List<Instruction> GetInstructions(string assemblerCode)
        {
            var app = new Application("getInstructions");
            var opt = new LoaderOptions(engine, app);
            opt.UseIndicesLocally = false;
            var ldr = new Loader(opt);
            ldr.LoadFromString("function MyAssemblerFunction does asm {" + assemblerCode + "\n}");
            if (ldr.ErrorCount != 0)
            {
                Console.WriteLine("--------------- Assembler Code --------------------");
                Console.WriteLine(assemblerCode);
                Console.WriteLine("---------------- End Asm Code ---------------------");
                foreach (var error in ldr.Errors)
                    Assert.Fail(string.Format("Error in the expected assembler code: {0}", error));
            }
            return app.Functions["MyAssemblerFunction"].Code;
        }

        protected internal Loader _compile(string input)
        {
            var opt = new LoaderOptions(engine, target) {UseIndicesLocally = false};
            var ldr = new Loader(opt);
            ldr.LoadFromString(input);
            foreach (var line in ldr.Errors)
                Console.Error.WriteLine(line);
            Assert.AreEqual(0, ldr.ErrorCount, "Test code did not compile without errors.");

            if (ldr.Warnings.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Warnings:");
                foreach (var warning in ldr.Warnings)
                    Console.WriteLine(warning);
                Console.WriteLine();
            }

            Console.WriteLine(target.StoreInString());
            return ldr;
        }

        protected internal void _expect(string assemblerCode)
        {
            _expect(target.Meta[Application.EntryKey], assemblerCode);
        }

        protected internal void _expect(string functionId, string assemblerCode)
        {
            var func = target.Functions[functionId];
            if (func == null)
                throw new ArgumentException(string.Format("No function with the id {0} exists",
                    functionId));
            var actual = func.Code;
            _expect(actual, assemblerCode);
        }

        protected internal void _expect(PFunction function, string assemblerCode)
        {
            _expect(function.Code, assemblerCode);
        }

        protected internal void _expect(List<Instruction> actual, string assemblerCode)
        {
            var expected = GetInstructions(assemblerCode);
            int i;
            for (i = 0; i < actual.Count; i++)
            {
                if (i == expected.Count)
                    Assert.AreEqual(
                        expected.Count,
                        actual.Count,
                        "Expected and actual instruction count missmatch detected at actual instruction " +
                            actual[i]);
                Assert.AreEqual(
                    expected[i],
                    actual[i],
                    String.Format(
                        "Instructions at address {0} do not match (e{1},a{2})",
                        i,
                        expected.Count,
                        actual.Count));
            }
            Assert.AreEqual(
                expected.Count,
                actual.Count,
                "Expected and actual instruction count missmatch" +
                    (i < expected.Count ? " detected at expected instruction " + expected[i] : ""));
        }

        protected internal static void _expectSharedVariables(
            PFunction func, params string[] shared)
        {
            _expectSharedVariables_(func, shared);
        }

        protected internal void _expectSharedVariables(string funcId, params string[] shared)
        {
            _expectSharedVariables_(target.Functions[funcId], shared);
        }

        protected internal static void _expectSharedVariables_(PFunction func, string[] shared)
        {
            MetaEntry entry;
            var hasShared = func.Meta.TryGetValue(PFunction.SharedNamesKey, out entry);
            if (shared.Length == 0 && hasShared)
                Assert.Fail("The function {0} is not expected to share variables.", func.Id);
            else if ((!hasShared) && shared.Length != 0)
                Assert.Fail("The function {0} is expected to share variables.", func.Id);
            else if ((!hasShared) && shared.Length == 0)
                return;

            var entries = entry.List;
            Assert.AreEqual(
                shared.Length,
                entries.Length,
                "The function {0} is expected to have a different number of shared variables.",
                func.Id);
            for (var i = 0; i < entries.Length; i++)
                Assert.IsTrue(
                    Engine.StringsAreEqual(shared[i], entries[i] == null ? "" : entries[i].Text),
                    "The function {0} is expected to require sharing of variable {1}.",
                    func.Id,
                    shared[i]);
        }

        #endregion
    }
}