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
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

[PublicAPI]
public interface ITarget
{
    [PublicAPI]
    Module Module
    {
        get;
    }

    [PublicAPI]
    IReadOnlyCollection<IResourceDescriptor> Resources
    {
        get;
    }

    [PublicAPI]
    SymbolStore Symbols
    {
        get;
    }

    [PublicAPI]
    ModuleName Name
    {
        get;
    }

    [PublicAPI]
    IReadOnlyCollection<Message> Messages { get; }

    [PublicAPI]
    [CanBeNull]
    Exception Exception { get; }

    bool IsSuccessful { get; }
}

public static class Target
{
    public static void ThrowIfFailed(this ITarget target, ITargetDescription description)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (description == null)
            throw new ArgumentNullException(nameof(description));
        if (target.Exception != null)
            throw target.Exception;
        else if (target.Messages.Any(m => m.Severity == MessageSeverity.Error))
            throw new BuildFailureException(description,
                "There {2} {0} {1} while translating " + description.Name + ".",
                target.Messages);
    }
}