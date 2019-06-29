
// Prexonite
//
// Copyright (c) 2016, Christian Klauser
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
using System.Linq;
using System.Net;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands.Math;
using Prexonite.Types;

namespace PrexoniteTests.Tests
{
    public class ShellExtensions : VMTestsBase
    {
        public override void SetupCompilerEngine()
        {
            base.SetupCompilerEngine();

            // Enable the perhaps most controversal of shell extensions
            options.FlagLiteralsEnabled = true;
        }

        [Test]
        public void DeltaOperatorLiterals()
        {
            Compile(@"
function (<|) {}
function (<|.) {}
function (.<|) {}
function (|>) {}
function (|>.) {}
function (.|>) {}
");

            _assertFunctionExists(OperatorNames.Prexonite.BinaryDeltaLeft);
            _assertFunctionExists(OperatorNames.Prexonite.UnaryDeltaLeftPre);
            _assertFunctionExists(OperatorNames.Prexonite.UnaryDeltaLeftPost);
            _assertFunctionExists(OperatorNames.Prexonite.BinaryDeltaRight);
            _assertFunctionExists(OperatorNames.Prexonite.UnaryDeltaRightPre);
            _assertFunctionExists(OperatorNames.Prexonite.UnaryDeltaRightPost);
        }

        private void _assertFunctionExists(string name)
        {
            var f = target.Functions[name];
            Assert.That(f, Is.Not.Null, $"Function {name} should exist");
        }

        [Test]
        public void ResolveBinaryDeltaLeft()
        {
            Compile(@"
function (<|)(l,r) = l + "" <| "" + r;

function main() {
    return 1 <| 2;
}
");
            
            Expect("1 <| 2");
        }
        

        [Test]
        public void ResolveBinaryDeltaRight()
        {
            Compile(@"
function (|>)(l,r) = ""$l |> $r"";

function main() {
    return 1 |> 2;
}
");
            
            Expect("1 |> 2");
        }
        

        [Test]
        public void ResolveUnaryDeltaLeftPre()
        {
            Compile(@"
function (<|.)(x) = ""<| $x"";

function main() {
    return <| 1;
}
");
            
            Expect("<| 1");
        }
        

        [Test]
        public void ResolveUnaryDeltaLeftPost()
        {
            Compile(@"
function (.<|)(x) = ""$x <|"";

function main() {
    return 1 <|;
}
");
            
            Expect("1 <|");
        }
        

        [Test]
        public void ResolveUnaryDeltaRightPre()
        {
            Compile(@"
function (|>.)(x) = ""|> $x"";

function main() {
    return |> 1;
}
");
            
            Expect("|> 1");
        }
        

        [Test]
        public void ResolveUnaryDeltaRightPost()
        {
            Compile(@"
function (.|>)(x) = ""$x |>"";

function main() {
    return 1 |>;
}
");
            
            Expect("1 |>");
        }

        [Test]
        public void SingleCharFlagLiteral()
        {
            Compile(@"
function main() {
    return -q;
}
");
            Expect("-q");
        }


        [Test]
        public void MultiCharFlagLiteral()
        {
            Compile(@"
function main() {
    return -qip;
}
");
            Expect("-qip");
        }


        [Test]
        public void LongFlagLiteral()
        {
            Compile(@"
function main() {
    return --query;
}
");
            Expect("--query");
        }

        [Test]
        public void LongFlagLiteralSingleChar()
        {
            Compile(@"
function main() {
    return --q;
}
");
            Expect("--q");
        }

        [Test]
        public void LongOptionLiteral()
        {
            Compile(@"
function with_string() {
    return --query-format=""aBc"";
}

function with_expr() {
    return --query-format=(1 + 2);
}

function with_var() {
    var fmt = ""%{Version}"";
    return --query-format=fmt;
}
");
            ExpectNamed("with_string", "--query-format=aBc");
            ExpectNamed("with_expr", "--query-format=3");
            ExpectNamed("with_var", "--query-format=%{Version}");
        }

        [Test]
        public void PreIncrementInParens()
        {
            Compile(@"
function main(x) {
    var y = --(x);
    var z = -- x;
    return x + y + z;
}
");

            Expect(14 + 13 + 13, 15);
        }

        [Test]
        public void UnaryMinusInParens()
        {
            Compile(@"
function main(x) {
    var y = -(x);
    var z = - x;
    return x + y - z;
}
");
            const int x = 15;
            Expect(x + (-x) - (-x), x);
        }

        [Test]
        public void FlagLiteralsDisabledInherit()
        {
            Compile(@"
FlagLiterals Disabled;
function main(x){
    return -x + --x;
}
");
            Expect((-15) + 14, 15);
        }

        [Test]
        public void FlagLiteralsDisabled()
        {
            Compile(@"
function main(x)[FlagLiterals Disabled]{
    return -x + --x;
}
");
            Expect((-15) + 14, 15);
        }


        [Test]
        public void FlagLiteralsEnabled()
        {
            options.FlagLiteralsEnabled = false;
            Compile(@"
function main()[FlagLiterals Enabled]{
    return -x + --x;
}
");
            Expect("-x--x");
        }

        [Test]
        public void FlagLiteralsEnabledInherit()
        {
            options.FlagLiteralsEnabled = false;
            Compile(@"
FlagLiterals Enabled;
function main(){
    return -x + --x;
}
");
            Expect("-x--x");
        }

        [Test]
        public void FlagLiteralsDisabledGlobally()
        {
            Compile(@"
FlagLiterals Disabled;
var x = 15;
var z = -x + --x;
function main(){
    return z;
}
");
            Expect((-15) + 14);
        }


        [Test]
        public void FlagLiteralsEnabledGlobally()
        {
            options.FlagLiteralsEnabled = false;
            Compile(@"
FlagLiterals Enabled;
var z = -x + --x;
function main(){
    return z;
}
");
            Expect("-x--x");
        }

        [Test]
        public void NullSupportsForeach()
        {
            Compile(@"
function main(a){
    var z = a;
    foreach(var x in null){
        z += x;
    }
    return z;
}
");
            Expect(16, 16);
        }

        [Test]
        public void SpliceFunctionCall()
        {
            Compile(@"
function f() {
    return call(string_concat(?), ["":""], var args >> map(x => ""<$x>"")); 
}

function main(a, xs, b, ys, c){
    return ->f.(a, *xs, b, *ys, c);
}

function main2(xs) {
    return f(*xs);
}

function main3(xs, ys){
    return f(*xs, *ys);
}

function main4(a, b, c, xs) {
    return f(a, b, c, *xs);
}

function main5(xs, a, b, c) {
    return f(*xs, a, b, c);
}
");
            
            Expect(":<a><x1><x2><x3><b><y1><y2><y3><c>", 
                "a", _list("x1", "x2", "x3"), "b", _list("y1", "y2", "y3"), "c");
            Expect(":<a><x1><x2><x3><b><c>", 
                "a", _list("x1", "x2", "x3"), "b", _list(), "c");
            Expect(":<a><x1><b><c>", 
                "a", _list("x1"), "b", _list(), "c");
            
            ExpectNamed("main2", ":<x1><x2><x3>", _list("x1", "x2", "x3"));
            ExpectNamed("main2", ":<x1>", _list("x1"));
            ExpectNamed("main2", ":", _list());
            
            ExpectNamed("main3", ":<x1><x2><x3><y1><y2><y3>", 
                _list("x1", "x2", "x3"), _list("y1", "y2", "y3"));
            ExpectNamed("main3", ":<x1><x2><x3>", 
                _list("x1", "x2", "x3"), _list());
            ExpectNamed("main3", ":<x1>", 
                _list("x1"), _list());
            
            ExpectNamed("main4", ":<a><b><c><x1><x2><x3>", "a", "b", "c", _list("x1", "x2", "x3"));
            ExpectNamed("main4", ":<a><b><c><x1>", "a", "b", "c", _list("x1"));
            ExpectNamed("main4", ":<a><b><c>", "a", "b", "c", _list());
            ExpectNamed("main5", ":<x1><x2><x3><a><b><c>", _list("x1", "x2", "x3"), "a", "b", "c");
            ExpectNamed("main5", ":<x1><a><b><c>", _list("x1"), "a", "b", "c");
            ExpectNamed("main5", ":<a><b><c>", _list(), "a", "b", "c");
            ExpectNamed("main4", ":<a><b><c>", "a", "b", "c", PType.Null);
            ExpectNamed("main5", ":<a><b><c>", PType.Null, "a", "b", "c");
            ExpectNamed("main3", ":<x1>", 
                _list("x1"), PType.Null);
            ExpectNamed("main2", ":", PType.Null);
        }

        /// <summary>
        /// Verify that <code>$@</code> is equivalent to 'var args'
        /// </summary>
        [Test]
        public void VarArgsSigil()
        {
            Compile(@"
function main(){
    return call(string_concat(?), ["":""], $@ >> map(x => ""<$x>"")); 
}
");
            Expect(":<a><b><c>", "a", "b", "c");
            Expect(":");
        }

        [Test]
        public void VarArgsSigilSplice()
        {
            Compile(@"
function main(){
    return string_concat("":"", *$@);
}
");
            Expect(":abc", "a", "b", "c");
        }

        [Test]
        public void FlowSplice()
        {
            Compile(@"
function main(){
    var xs = var args;
    return *xs >> foldl(->(+), "":"");
}
");
            Expect(":abc", "a", "b", "c");
        }

        private PValue _list(params object[] elements)
        {
            var lst = new List<PValue>(elements.Length);
            lst.AddRange(elements.Select(element => engine.CreateNativePValue(element)));
            return engine.CreateNativePValue(lst);
        }

        [Test]
        public void PartialFunctionAtomSplice()
        {
            Compile(@"
function f = var args >> map(x => ""<$x>"") >> foldl(->(+), "":"");

function main(){
    var xs = var args;
    var f1 = f(?, *xs, ?, 0, ?);
    return f1.(10,20,30);
}
");

            Expect(":<10><1><2><3><20><0><30>",1,2,3);
        }
    }
    
}