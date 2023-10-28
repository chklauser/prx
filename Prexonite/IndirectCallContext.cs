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
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite;

public class IndirectCallContext : StackContext
{
    readonly StackContext _originalStackContext;

    public IndirectCallContext(StackContext parent, IIndirectCall callable, PValue[] args)
        : this(
            parent, parent.ParentEngine, parent.ParentApplication, parent.ImportedNamespaces,
            callable, args)
    {
    }

    public IndirectCallContext(Engine parentEngine,
        Application parentApplication,
        IEnumerable<string> importedNamespaces,
        IIndirectCall callable,
        PValue[] args)
        : this(null, parentEngine, parentApplication, importedNamespaces, callable, args)
    {
    }

    public IndirectCallContext(StackContext originalSctx,
        Engine parentEngine,
        Application parentApplication,
        IEnumerable<string> importedNamespaces,
        IIndirectCall callable,
        PValue[] args)
    {
        if (importedNamespaces == null)
            throw new ArgumentNullException(nameof(importedNamespaces));

        ParentEngine = parentEngine ?? throw new ArgumentNullException(nameof(parentEngine));
        ParentApplication = parentApplication ?? throw new ArgumentNullException(nameof(parentApplication));
        ImportedNamespaces = importedNamespaces as SymbolCollection ??
            new SymbolCollection(importedNamespaces);
        Callable = callable ?? throw new ArgumentNullException(nameof(callable));
        Arguments = args ?? throw new ArgumentNullException(nameof(args));
        _originalStackContext = originalSctx;
    }

    public IIndirectCall Callable { get; }

    public PValue[] Arguments { get; }

    PValue _returnValue = PType.Null;

    /// <summary>
    ///     Represents the engine this context is part of.
    /// </summary>
    public override Engine ParentEngine { get; }

    /// <summary>
    ///     The parent application.
    /// </summary>
    public override Application ParentApplication { get; }

    public override SymbolCollection ImportedNamespaces { get; }

    /// <summary>
    ///     Indicates whether the context still has code/work to do.
    /// </summary>
    /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
    protected override bool PerformNextCycle(StackContext lastContext)
    {
        //Remove this context if possible (IndirectCallContext should be transparent)
        var sctx = _originalStackContext ?? this;

        _returnValue = Callable.IndirectCall(sctx, Arguments);
        return false;
    }

    /// <summary>
    ///     Tries to handle the supplied exception.
    /// </summary>
    /// <param name = "exc">The exception to be handled.</param>
    /// <returns>True if the exception has been handled, false otherwise.</returns>
    public override bool TryHandleException(Exception exc)
    {
        return false;
    }

    /// <summary>
    ///     Represents the return value of the context.
    ///     Just providing a value here does not mean that it gets consumed by the caller.
    ///     If the context does not provide a return value, this property should return null (not NullPType).
    /// </summary>
    public override PValue ReturnValue => _returnValue ?? PType.Null;
}