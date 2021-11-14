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
using Prexonite;
using Prexonite.Types;

namespace Prx.Tests;

public class TestStackContext : StackContext
{
    public TestStackContext(Engine engine, Application app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));
        ParentEngine = engine ?? throw new ArgumentNullException(nameof(engine));
        Implementation = app.CreateFunction();
    }

    public override Engine ParentEngine { get; }

    public PFunction Implementation { get; }

    public override PValue ReturnValue => PType.Null.CreatePValue();

    /// <summary>
    ///     The parent application.
    /// </summary>
    public override Application ParentApplication => Implementation.ParentApplication;

    public override SymbolCollection ImportedNamespaces => Implementation.ImportedNamespaces;

    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    /// <summary>
    ///     Indicates whether the context still has code/work to do.
    /// </summary>
    /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
    protected override bool PerformNextCycle(StackContext lastContext)
    {
        return false;
    }
}