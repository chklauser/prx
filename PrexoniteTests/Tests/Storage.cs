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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prx.Tests;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class Storage : Compiler
    {
        private const string _storedShouldBeEqual =
            "Since the in-memory and the restored application are the same, they should" +
                " result in the same serialized form.";

        [Test]
        public void TestEmpty()
        {
            const string input1 = "";
            var ldr = new Loader(engine, target);
            Console.WriteLine("-- Compiling fixture");
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            var stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine(stored);

            Console.WriteLine("-- Compiling stored result");
            var reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            var restored = reldr.Options.TargetApplication.StoreInString();

            Assert.That(stored,
                Is.EqualTo(stored).Using((IEqualityComparer<string>) Engine.DefaultStringComparer),
                _storedShouldBeEqual);
        }

        [Test]
        public void TestMeta()
        {
            const string input1 =
                @"
Name fixture\storage\meta;
Description ""Contains dummy meta information for a unit test"";
Author SealedSun;

Import 
{
    System,
    System::Text
};

Is AsFastAsPossible;

Add System::Xml To Imports;
";
            var ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            var stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine(stored);

            var reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            var restored = reldr.Options.TargetApplication.StoreInString();

            Assert.That(stored,
                Is.EqualTo(stored).Using((IEqualityComparer<string>)Engine.DefaultStringComparer),
                _storedShouldBeEqual);
        }

        [Test]
        public void TestDeclarations()
        {
            const string input1 =
                @"
var obj0;
var obj1;
ref func0 
[
    Description ""Contains a constant value"";
    is constant;
    UsedBy func1;    
];

ref func2;

function func1 does asm {}

ref func0 [ Add func2 to UsedBy; ];

function func2(arg0, arg1, arg2)
[
    Uses { func0, func1 };
]
{//line# 20
    goto L1;
    L0:;
    func0();
    goto LE;
    L1:;
    func1();
    goto L0;
    LE:;
}
";
            var ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            var stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine("//== 1st Store");
            Console.WriteLine(stored);

            var reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            var restored = reldr.Options.TargetApplication.StoreInString();
            Console.WriteLine("//== 2nd Store");
            Console.WriteLine(restored);

            //The two of them are not supposed to be equal because of a debug feature
            /*Assert.IsTrue(Engine.StringsAreEqual(stored, restored),
                storedShouldBeEqual); //*/
        }

#if Compressed

        [Test]
        public void CompressNoException()
        {
            Loader ldr = _compile(@"

coroutine mapf(ref f, xs) does
    foreach(var x in xs)
        yield a => f(a,x);


");
            using (MemoryStream mstr = new MemoryStream())
            {
                ldr.StoreCompressed(mstr);
                Assert.Greater(mstr.Length, 10f);
            }
        }
#endif

        [Test]
        public void MetaEntryIntegerNotString()
        {
            var ldr = new Loader(engine, target);
            const string numberToStore = "1234567890";
            ldr.LoadFromString(@"
meta_entry " + numberToStore + @";
");
            foreach (var error in ldr.Errors)
                Console.WriteLine("ERROR: {0}", error);
            Assert.AreEqual(0, ldr.ErrorCount, "no errors expected");
            var stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine("//== 1st Store");
            Console.WriteLine(stored);

            var reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            var restored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine("//== 2nd Store");
            Console.WriteLine(restored);

            Assert.AreEqual(stored, restored);
            Assert.AreEqual(numberToStore, ldr.Options.TargetApplication.Meta["meta_entry"].Text);
            Assert.AreEqual(numberToStore, reldr.Options.TargetApplication.Meta["meta_entry"].Text);
        }
    }
}