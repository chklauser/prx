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
using System.Diagnostics;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class ExpandSymbol : WrappingSymbol
{
    public override string ToString()
    {
        return $"expand {InnerSymbol}";
    }

    [NotNull]
    internal static ExpandSymbol _Create([NotNull] Symbol inner, [CanBeNull] ISourcePosition position)
    {
        return new(position ?? inner.Position, inner);
    }

    ExpandSymbol([NotNull] ISourcePosition position, [NotNull] Symbol inner) : base(position, inner)
    {
    }

    protected override int HashCodeXorFactor => 588697;

    public override WrappingSymbol With(Symbol newInnerSymbol, ISourcePosition newPosition = null)
    {
        return new ExpandSymbol(newPosition ??  Position,newInnerSymbol);
    }

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleExpand(this, argument);
    }

    public override bool TryGetExpandSymbol(out ExpandSymbol expandSymbol)
    {
        expandSymbol = this;
        return true;
    }

    public override bool Equals(Symbol other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return other is ExpandSymbol otherExpand && Equals(otherExpand);
    }
}