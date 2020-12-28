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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

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

            var cilExt1 = new CilExtensionHint(new List<int> {1, 5, 9});
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
            var cilExt2 = new CilExtensionHint(new List<int> {2, 4, 8, 16});
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
                    new(new List<int> {1, 6, 16, 66}),
                    new(new List<int> {7, 77, 777}),
                    new(new List<int> {9, 88, 777, 6666}),
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
            var emptyFunc = target.CreateFunction();
            emptyFunc.Meta[PFunction.IdKey] = "empty";
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
            if (table.Meta.TryGetValue(Loader.CilHintsKey, out var cilHintsEntry))
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
                table.Meta[Loader.CilHintsKey] = (MetaEntry) new MetaEntry[0];
                return _getCilHints(table, true);
            }
        }

        [Test]
        public void UnbindCommandTest()
        {
            Compile(
                @"
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
            _expectCil();
            Expect(Enumerable.Range(1, 10).Select(_ => (PValue) true).ToList());
        }


        [Test]
        public void JumpBreaksCilExtensions()
        {
            Compile(
                @"
function main(b)
{asm{
                ldloc b
                ldc.int 4
                cmd.2 (==)
                jump.f L_else 
label L_if      ldc.string ""IF""
                jump L_endif
label L_else    ldc.string ""ELSE""
label L_endif   ldc.string ""-branch""
                cmd.2 (+)
                ret
}}
");
            Assert.AreEqual(1, _getCilHints(target.Functions["main"], true).Length);
            _expectCil();
            Expect("IF-branch", 4);
            Expect("ELSE-branch", 3);
            Expect("ELSE-branch", 5);
        }

        [Test]
        public void TryCatchFinallyCompiles()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main()
{
    try {
        trace(""t"");
        throw ""e"";
    }catch(var exc){
        trace(""c"");
        trace(exc.Message);
    }finally{
        trace(""f"");
    }

    return t;
}
");
            _expectCil();
            Expect("tfce");
        }

        private void _expectCil(string functionId = "main")
        {
            var func = target.Functions[functionId];
            Assert.IsNotNull(func, "Function " + functionId + " must exist");
            Assert.IsFalse(func.Meta[PFunction.VolatileKey].Switch,
                functionId + " must not be volatile.");
        }

        [Test]
        public void TryFinallyCondCompiles()
        {
            Assert.Throws<PrexoniteRuntimeException>(() =>
            {
                Compile(
                    @"
var t = """";
function trace(x) = t+=x;

function main(x)
{
    try {
        trace(""t"");
        if(x)
            throw ""e"";
    }finally{
        trace(""f"");
    }

    return t;
}
");
                _expectCil();
                Expect("tf", true);
            });
        }

        [Test]
        public void TryCatchCondCompiles()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x)
{
    try {
        trace(""t"");
        if(x)
            throw ""e"";
    }catch(var exc){
        trace(""c"");
        trace(exc.Message);
    }

    return t;
}
");
            _expectCil();
            Expect("tce", true);
        }

        [Test]
        public void CatchInFinally1()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x)
{
    try {
        try {
            trace(""t"");
            if(x)
                throw ""e"";
        }catch(var exc){
            trace(""c"");
            trace(exc.Message);
        }
    } finally {
        trace(""f"");
    }

    return t;
}
");
            _expectCil();
            Expect("tcef", true);
        }

        [Test]
        public void CatchInFinally2()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x)
{
    try {
        try {
            trace(""t"");
            throw ""e"";
        }catch(var exc){
            if(x)
                trace(""x"");
        }
    } finally {
        trace(""f"");
    }

    return t;
}
");

            _expectCil();
            Expect("txf", true);
        }

        [Test]
        public void CatchInFinally3()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x)
{
    try {
        try {
            trace(""t"");
            throw ""e"";
        }catch(var exc){
            if(x)
                trace(""x"");
        }
    } catch(var exc){
        trace(""e"");
    } finally {
        trace(""f"");
    }

    return t;
}
");
            _expectCil();
            Expect("txf", true);
        }

        [Test]
        public void CatchInFinally4()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x)  [store_debug_implementation enabled;]
{
    for(var i = 1; i < 6; i++)
        try
        {
            try
            {
                throw i;
            }
            catch(var exc)
            {
                if(x)
                    throw exc;
            }
        }
        catch(var exc)
        {
            trace(""e"");
        }
        finally
        {
            trace(""f"");
        }
    return t;
}
");
            _expectCil();
            Expect("fefefefefe", true);
        }

        [Test]
        public void CatchInFinally5()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x,y)  [store_debug_implementation enabled;]
{
    try
    {
        try
        {
            throw ""i""; //this must be a throw; won't work with trace
        }
        catch(var exc)
        {
            if(not x) //needs to be false (it's a runtime error after all)
                throw exc;
        }
    } //must be a nested block
    finally
    {
        trace(""f"");
    }
    return t;
}
");
            _expectCil();
            Expect("f", true, true);
        }

        [Test]
        public void TryCatchFinallyCondCompiles()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x) //[store_debug_implementation enabled;]
{
    try {
        trace(""t"");
        if(x)
            throw ""e"";
    }catch(var exc){
        trace(""c"");
        trace(exc.Message);
    }finally{
        trace(""f"");
    }

    return t;
}
");
            _expectCil();
            Expect("tfce", true);
        }

        [Test]
        public void LabelOnFirstNeLabelOnTry()
        {
            Assert.Throws<PrexoniteRuntimeException>(() =>
            {
                Compile(
                    @"
var t = """";
function trace(x) = t+=x;

function main(x) //[store_debug_implementation enabled;]
{
    try {
        trace(""t"");
        goto L1;
        trace(""z"");
    } finally {
        trace(""f"");
    }

    try {
        trace(""g"");
        try {
L1:         trace(""b"");
        } finally {
            trace(""r"");
        }
    } finally {
        trace(""v"");
    }

    return t;
}
");
                _expectSehDeficiency();
                Expect("undefined", true);
            }, @"Unexpected leave instruction. This happens when jumping to an instruction in a try block from the outside.");
        }

        [Test]
        public void TryFinallyShadowingNoBridge()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

//This tets is expected not to compile to CIL
function main(x) //[store_debug_implementation enabled;]
{
    try {
        trace(""t"");
        goto L1;
        trace(""z"");
    } finally {
        trace(""f"");
    }

    try {
        trace(""g"");
L1:     try {
            trace(""b"");
        } finally {
            trace(""r"");
        }
    } finally {
        trace(""v"");
    }

    return t;
}
");
            _expectSehDeficiency();
            Expect("tbrv", true);
        }

        [Test]
        public void TryFinallyShadowingBridge()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x) //[store_debug_implementation enabled;]
{
    try {
        trace(""k"");
        try {
            trace(""t"");
            goto L1;
            trace(""z"");
        } finally {
            trace(""f"");
        }
    
        trace(""g"");
L1:     try {
            trace(""b"");
        } finally {
            trace(""r"");
        }
    } finally {
        trace(""v"");
    }

    return t;
}
");
            _expectCil();
            Expect("ktfbrv", true);
        }

        [Test]
        public void ReturnFromFinally()
        {
            Compile(
                @"
var t = """";
function trace(x) = t+=x;

function main(x) //[store_debug_implementation enabled;]
{
    try {
        trace(""k"");
    } finally {
        trace(""f"");
        return t;
        trace(""v"");
    }

    return t;
}
");
            _expectSehDeficiency();
            Expect("kf", true);
        }

        private void _expectSehDeficiency(string name = "main")
        {
            _expectSehDeficiency(target.Functions[name]);
        }

        private static void _expectSehDeficiency(PFunction function)
        {
            Assert.IsNotNull(function, "function not found");
            Assert.IsTrue(function.Meta[PFunction.VolatileKey].Switch,
                "Function is expected to be volatile.");
            Assert.IsTrue(function.Meta[PFunction.DeficiencyKey].Text.Contains("SEH"),
                "CIL deficiency is expected to be related to SEH.");
        }

        [Test]
        public void MinimalTryCatch()
        {
            Compile(
                @"
var t; 
function trace(x) = t+=x~String; 
function main(x) [store_debug_implementation enabled;]
{
    try {trace(1);}
    finally{trace(2);}
    return t;
}");

            Expect("12", true);
        }
    }
}