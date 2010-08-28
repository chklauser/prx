using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prx.Tests;
using Compiler = Prexonite.Compiler.Cil.Compiler;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class CilCompilerTests : VMTestsBase
    {

        [Test]
        public void SetCilHintTest()
        {
            Compile(@"
function main() {
    foreach(var x in var args)
        println(x);
}");

            var main = target.Functions["main"];
            var meta = main.Meta;

            var cilExt1 = new CilExtensionHint(new List<int> { 1, 5, 9 });
            var existingHints = _getCilHints(main, true);
            Assert.AreEqual(1, existingHints.Length);

            //Add, none existing
            Compiler.SetCilHint(main, cilExt1);
            var hints1 = _getCilHints(main, true);
            Assert.AreNotSame(existingHints, hints1);
            Assert.AreEqual(2, hints1.Length);
            Assert.IsTrue(hints1[1].IsList);
            var cilExt1P = CilExtensionHint.FromMetaEntry(hints1[1].List);
            Assert.IsTrue(
                cilExt1P.Offsets.All(offset => cilExt1.Offsets.Contains(offset)),
                "deserialized contains elements not in original");
            Assert.IsTrue(cilExt1.Offsets.All(offset => cilExt1P.Offsets.Contains(offset)),
                "original contains elements not in deserialized");

            //Add, one existing
            var cilExt2 = new CilExtensionHint(new List<int> { 2, 4, 8, 16 });
            Compiler.SetCilHint(main, cilExt2);
            var hints2 = _getCilHints(main, true);
            Assert.AreSame(hints1, hints2);
            Assert.AreEqual(2, hints2.Length);
            Assert.IsTrue(hints2[1].IsList);
            var cilExt2P = CilExtensionHint.FromMetaEntry(hints2[1].List);
            Assert.IsTrue(
                cilExt2P.Offsets.All(offset => cilExt2.Offsets.Contains(offset)),
                "deserialized contains elements not in original");
            Assert.IsTrue(cilExt2.Offsets.All(offset => cilExt2P.Offsets.Contains(offset)),
                "original contains elements not in deserialized");

            //Add, many existing
            var cilExts = new List<CilExtensionHint>
            {
                new CilExtensionHint(new List<int> {1, 6, 16, 66}),
                new CilExtensionHint(new List<int>{7,77,777}),
                new CilExtensionHint(new List<int>{9,88,777,6666}),
            };
            foreach (var cilExt in cilExts)
                Compiler.AddCilHint(main, cilExt);
            var hints3 = _getCilHints(main, true);
            Assert.AreNotSame(hints2, hints3);
            Assert.AreEqual(5, hints3.Length);
            var cilExt3 = new CilExtensionHint(new List<int> {44, 55, 66, 77, 88});
            Compiler.SetCilHint(main, cilExt3);
            var hints4 = _getCilHints(main, true);
            Assert.AreNotSame(hints3, hints4);
            Assert.AreEqual(2, hints4.Length);
            Assert.IsTrue(hints4[1].IsList);
            var cilExt3P = CilExtensionHint.FromMetaEntry(hints4[1].List);
            Assert.IsTrue(
                cilExt3P.Offsets.All(offset => cilExt3.Offsets.Contains(offset)),
                "deserialized contains elements not in original");
            Assert.IsTrue(cilExt3.Offsets.All(offset => cilExt3P.Offsets.Contains(offset)),
                "original contains elements not in deserialized");


            //Add, no cil hints key yet
            var emptyFunc = new PFunction(target);
            emptyFunc.Meta[PFunction.IdKey] = "empty";
            target.Functions.Add(emptyFunc);
            Compiler.SetCilHint(main, cilExt3);
            var hints5 = _getCilHints(main, true);
            Assert.AreEqual(2, hints5.Length);
            Assert.IsTrue(hints5[0].IsList);
            var cilExt4P = CilExtensionHint.FromMetaEntry(hints5[1].List);
            Assert.IsTrue(
                cilExt4P.Offsets.All(offset => cilExt3.Offsets.Contains(offset)),
                "deserialized contains elements not in original");
            Assert.IsTrue(cilExt3.Offsets.All(offset => cilExt4P.Offsets.Contains(offset)),
                "original contains elements not in deserialized");

        }

        private static MetaEntry[] _getCilHints(IHasMetaTable table, bool keyMustExist)
        {
            MetaEntry cilHintsEntry;
            if (table.Meta.TryGetValue(Loader.CilHintsKey, out cilHintsEntry))
            {
                Assert.IsTrue(cilHintsEntry.IsList, "CIL hints entry must be a list.");
                return cilHintsEntry.List;
            }
            else if (keyMustExist)
            {
                Assert.Fail("Meta table of {0} does not contain cil hints.", table);
                return null;
            }
            else
            {
                table.Meta[Loader.CilHintsKey] = (MetaEntry)new MetaEntry[0];
                return _getCilHints(table, true);
            }
        }

        [Test]
        public void UnbindCommandTest()
        {
            Compile(@"
function main()
{
    var result = [];
    var x = 1;
    ref y = ->x;
    result[] = x == 1;
    result[] = ->x == ->y;
    new var x;
    result[] = x == 1;
    result[] = not System::Object.ReferenceEquals(->x,  ->y);
    
    result[] = var x == new var x;
    result[] = x == 1;
    result[] = not System::Object.ReferenceEquals(->x,  ->y);

    //behave like ordinary command
    result[] = unbind(->x) is null;
    result[] = x == 1;
    result[] = not System::Object.ReferenceEquals(->x,  ->y);
    
    return result;
}
");
            Expect(Enumerable.Range(1,10).Select(_ => (PValue) true).ToList());
        }
    }
}
