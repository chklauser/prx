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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Build;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace PrexoniteTests.Tests;

public abstract class BuiltInTypeTests : VMTestsBase
{
    [Test]
    public void HashStaticMethodCreate()
    {
        Compile(@"
function main(x,y) {
    var h = ~Hash.Create(""x"": x, ""y"": y);
    return ""x=$(h[""x""]), y=$(h[""y""]), c=$(h.Count)"";
}
");
        Expect("x=5, y=7, c=2", 5, 7);
    }

    [Test]
    public void HashStaticMethodCreateFromArgs()
    {
        Compile(@"
function main(x,y) {
    var h = ~Hash.CreateFromArgs(""x"", x, ""y"", y);
    return ""x=$(h[""x""]), y=$(h[""y""]), c=$(h.Count)"";
}
");
        Expect("x=3, y=8, c=2", 3, 8);
    }

    [Test]
    public void HashStaticMethodCreateFromArgsIgnoresExcessArg()
    {
        Compile(@"
function main(x,y) {
    var h = ~Hash.CreateFromArgs(""x"", x, ""y"", y, ""z"");
    return ""x=$(h[""x""]), y=$(h[""y""]), c=$(h.Count)"";
}
");
        Expect("x=6, y=9, c=2", 6,9);
    }
}