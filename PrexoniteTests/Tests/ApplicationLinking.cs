// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Core;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class ApplicationLinking
    {
        [Test]
        public void NoLocalButCount()
        {
            var s1 = SymbolStore.Create();
            var s = Symbol.CreateCall(EntityRef.Command.Create("print"), NoSourcePosition.Instance);
            s1.Declare("print",s);
            var s2 = SymbolStore.Create(s1);
            Assert.That(s2.IsEmpty,Is.False,"Expected s2.IsEmpty to be false.");
        }

       [Test]
       public void SimpleLinkUnlink()
       {
           var a1 = new Application();
           var a2 = new Application();

           Assert.That(a1.IsLinked,Is.False,"a1 should not be linked initially");
           Assert.That(a2.IsLinked, Is.False,"a2 should not be linked initially");

           Application.Link(a1,a2);

           Assert.That(a1.IsLinked, Is.True,"a1 should be linked");
           Assert.That(a2.IsLinked, Is.True,"a2 should be linked");

           a1.Unlink();

           Assert.That(a1.IsLinked,Is.False,"a1 should not be linked afterwards");
           Assert.That(a2.IsLinked, Is.False, "a2 should not be linked afterwards");
       }

        [Test]
        public void CompoundLinking()
        {
            var aps = 
                Enumerable.Range(1, 3)
                .Select(i => new Application("a" + i))
                .ToList();

            var bps = 
                Enumerable.Range(1, 4)
                .Select(i => new Application("b" + i))
                .ToList();

            var ps = aps.Append(bps).ToList();

            foreach (var a in ps)
                Assert.That(a.IsLinked, Is.False,
                    string.Format("Application {0} should not be linked initially.", a));

            //Link a's and b's, respectively
            foreach (var a in aps.Skip(1))
                Application.Link(a, aps[0]);
            foreach (var a in bps.Skip(1))
                Application.Link(a, bps[0]);

            Assert.That(aps[0].Compound,Is.Not.Contains(bps[0]));
            Assert.That(bps[0].Compound, Is.Not.Contains(aps[0]));

            Application.Link(aps[0],bps[1]);

            foreach (var a in ps)
                Assert.That(a.IsLinked, Is.True,
                    string.Format("Application {0} should be linked afterwards.", a));

            var c = aps[0].Compound;
            foreach (var a in ps)
            {
                Assert.That(a.Compound, Is.SameAs(c),
                    string.Format(
                        "Compound object of application {0} should be " +
                            "identical with that of application {1}.",
                        a, aps[0]));
            }

            Assert.That(aps[0].Compound.Count,Is.EqualTo(aps.Count + bps.Count));

            Assert.That(aps[0].Compound,Is.SubsetOf(ps), "Compound contains elements not in the original set.");
            Assert.That(ps, Is.SubsetOf(aps[0].Compound), "Not all elements of the original set are in the compound.");

            var p = aps[0];
            p.Unlink();
            Assert.That(p.IsLinked,Is.False,"p should now be unlinked.");
            Assert.That(aps[1].Compound.Count,Is.EqualTo(aps.Count + bps.Count - 1),"Compound is not the right size.");

            foreach (var a in ps.Where(x => x != p))
                Assert.That(a.IsLinked, Is.True,
                    string.Format("Application {0} should still be linked afterwards.", a));
            c = aps[1].Compound;
            foreach (var a in ps.Where(x => x != p))
            {
                Assert.That(a.Compound, Is.SameAs(c),
                    string.Format(
                        "Compound object of application {0} should be " +
                            "identical with that of application {1}.",
                        a, aps[1]));
            }
        }

        [Test]
        public void MergeLinking()
        {
            var cps =
                Enumerable.Range(1, 3)
                    .Select(i => new Application("common" + i))
                    .ToList();

            var aps = 
                Enumerable.Range(1, 3)
                .Select(i => new Application("a" + i))
                .Append(cps)
                .ToList();

            var bps = 
                Enumerable.Range(1, 4)
                .Select(i => new Application("b" + i))
                .Append(cps)
                .ToList();

            var ps = aps.Union(bps).ToList();

            foreach (var a in ps)
                Assert.That(a.IsLinked, Is.False,
                    string.Format("Application {0} should not be linked initially.", a));

            //Link a's and b's, respectively
            foreach (var a in aps.Skip(1))
                Application.Link(a, aps[0]); 
            foreach (var a in bps.Skip(1))
                Application.Link(a, bps[0]);

            Assert.That(aps[0].Compound, !Contains.Item(bps));
            Assert.That(bps[0].Compound, !Contains.Item(aps));

            Application.Link(aps[0],bps[1]);

            foreach (var a in ps)
                Assert.That(a.IsLinked, Is.True,
                    string.Format("Application {0} should be linked afterwards.", a.Module.Name));

            var c = aps[0].Compound;
            foreach (var a in ps)
            {
                Assert.That(a.Compound, Is.SameAs(c),
                    string.Format(
                        "Compound object of application {0} should be " +
                            "identical with that of application {1}.",
                        a, aps[0]));
            }

            Assert.That(aps[0].Compound.Count,Is.EqualTo(ps.Count));

            Assert.That(aps[0].Compound,Is.EquivalentTo(ps), "Compound is not equivalent to aps ∪ bps.");

            var p = cps[0];
            p.Unlink();
            Assert.That(p.IsLinked,Is.False,"p should now be unlinked.");
            Assert.That(aps[1].Compound.Count,Is.EqualTo(ps.Count - 1),"Compound is not the right size.");

            foreach (var a in ps.Where(x => x != p))
                Assert.That(a.IsLinked, Is.True,
                    string.Format("Application {0} should still be linked afterwards.", a));
            c = aps[1].Compound;
            foreach (var a in ps.Where(x => x != p))
            {
                Assert.That(a.Compound, Is.SameAs(c),
                    string.Format(
                        "Compound object of application {0} should be " +
                            "identical with that of application {1}.",
                        a, aps[1]));
            }
        }

        [Test]
        public void DryCrossModuleCall()
        {
            var m1 = Module.Create(new ModuleName("dragon", new Version(1, 2)));
            var m2 = Module.Create(new ModuleName("std", new Version(1, 3, 1)));

            var a1 = new Application(m1);
            var a2 = new Application(m2);

            var f1 = a1.CreateFunction(Application.DefaultEntryFunction);

            var f2 = a2.CreateFunction("sayHello");

            f1.Code.Add(new Instruction(OpCode.func, 0, f2.Id, m2.Name));
            f1.Code.Add(new Instruction(OpCode.ret_value));

            const string helloModules = "Hello Modules";
            f2.Code.Add(new Instruction(OpCode.ldc_string,helloModules));
            f2.Code.Add(new Instruction(OpCode.ret_value));

            Console.WriteLine("=========== Module {0} ==========", m1.Name);
            a1.Store(Console.Out);
            Console.WriteLine();
            Console.WriteLine("=========== Module {0} ==========", m2.Name);
            a2.Store(Console.Out);

            var eng = new Engine();

            try
            {
                a1.Run(eng);
                Assert.Fail("Should not succeed as applications are not linked.");
            }
            catch (PrexoniteRuntimeException e)
            {
                Console.WriteLine("EXPECTED EXCEPTION");
                Console.WriteLine(e.Message);
                Console.WriteLine("END OF EXPECTED EXCEPTION");
            }

            Application.Link(a1, a2);
            var r = a1.Run(eng);
            Assert.That(r.Value,Is.InstanceOf<string>());
            Assert.That(r.Value,Is.EqualTo(helloModules));
        }

        [Test]
        public void CreateModuleNameCommand()
        {
            var cmd = CreateModuleName.Instance;
            var eng = new Engine();
            var app = new Application("cmnc");
            var sctx = new NullContext(eng, app, Enumerable.Empty<string>());
            var rawMn = cmd.Run(sctx, new[] {sctx.CreateNativePValue(new MetaEntry(new MetaEntry[]{"sys","1.0"}))});
            Assert.That(rawMn.Value,Is.InstanceOf<ModuleName>());
            var mn = (ModuleName) rawMn.Value;
            Assert.That(mn.Id,Is.EqualTo("sys"));
            Assert.That(mn.Version,Is.EqualTo(new Version(1,0)));
        }
    }
}