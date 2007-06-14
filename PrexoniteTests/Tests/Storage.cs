using System;
using System.IO;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;

namespace Prx.Tests
{
    [TestFixture]
    public class Storage : Compiler
    {


        private const string storedShouldBeEqual =
            "Since the in-memory and the restored application are the same, they should" +
            " result in the same serialized form.";

        [Test]
        public void TestEmpty()
        {
            const string input1 = "";
            Loader ldr = new Loader(engine, target);
            Console.WriteLine("-- Compiling fixture");
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            string stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine(stored);

            Console.WriteLine("-- Compiling stored result");
            Loader reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            string restored = reldr.Options.TargetApplication.StoreInString();

            Assert.IsTrue(Engine.StringsAreEqual(stored, restored),
                          storedShouldBeEqual);
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            string stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine(stored);

            Loader reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            string restored = reldr.Options.TargetApplication.StoreInString();

            Assert.IsTrue(Engine.StringsAreEqual(stored, restored),
                          storedShouldBeEqual);
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
            Loader ldr = new Loader(engine, target);
            ldr.LoadFromString(input1);
            Assert.AreEqual(0, ldr.ErrorCount);

            string stored = ldr.Options.TargetApplication.StoreInString();
            Console.WriteLine("//== 1st Store");
            Console.WriteLine(stored);

            Loader reldr = new Loader(engine, new Application());
            reldr.LoadFromString(stored);
            string restored = reldr.Options.TargetApplication.StoreInString();
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

    }
}