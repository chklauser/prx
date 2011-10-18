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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Prexonite.Compiler.Cil;

namespace PrexoniteTests.Tests.Configurations
{
    public abstract class UnitTestConfiguration
    {
        public class InMemory : UnitTestConfiguration
        {
        }

        public class FromStored : UnitTestConfiguration
        {
            public override void PrepareTestCompilation(ScriptedUnitTestContainer runner)
            {
                base.PrepareTestCompilation(runner);
                using (var buffer = new MemoryStream(512*1024))
                {
                    //we don't need to wrap reader/writer in using because 
                    // we can dispose of the buffer directly
                    var writer = new StreamWriter(buffer, Encoding.UTF8);
                    var reader = new StreamReader(buffer, Encoding.UTF8);

                    runner.Loader.Store(writer);
                    writer.Flush();
                    runner.SetUpLoader();
                    //throws away old engine,loader,application; creates new one
                    buffer.Seek(0, SeekOrigin.Begin);
                    runner.Loader.LoadFromReader(reader);
                }
            }
        }

        protected UnitTestConfiguration()
        {
            Linking = FunctionLinking.FullyStatic;
            CompileToCil = false;
        }

        public FunctionLinking Linking { get; set; }
        public bool CompileToCil { get; set; }

        public virtual void SetupTestFile(ScriptedUnitTestContainer runner,
            IEnumerable<string> filesUnderTest)
        {
            foreach (var fut in filesUnderTest)
                runner.RequireFile(fut);
        }

        public virtual void PrepareExecution(ScriptedUnitTestContainer runner)
        {
            if (CompileToCil)
                Compiler.Compile(runner.Application, runner.Engine, Linking);
        }

        public virtual void PrepareTestCompilation(ScriptedUnitTestContainer runner)
        {
            // do nothing
        }
    }
}