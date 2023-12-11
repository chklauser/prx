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
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic;

[DebuggerDisplay("{ToString()}")]
public sealed class ReferenceSymbol : Symbol, IEquatable<ReferenceSymbol>
{
    public override string ToString()
    {
        return $"->{Entity}";
    }

    [DebuggerStepThrough]
    ReferenceSymbol(ISourcePosition position, EntityRef entity)
    {
        Entity = entity;
        Position = position;
    }

    internal static ReferenceSymbol _Create(EntityRef entity, ISourcePosition position)
    {
        return new(position, entity);
    }

    public EntityRef Entity { get; }

    public override ISourcePosition Position { get; }

    #region Overrides of Symbol

    public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
    {
        return handler.HandleReference(this, argument);
    }

    public override bool TryGetReferenceSymbol([NotNullWhen(true)] out ReferenceSymbol? referenceSymbol)
    {
        referenceSymbol = this;
        return true;
    }

    #endregion

    public bool Equals(ReferenceSymbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Entity, other.Entity);
    }

    public override bool Equals(Symbol? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is ReferenceSymbol && Equals((ReferenceSymbol)obj);
    }

    public override int GetHashCode()
    {
        return Entity != null ? Entity.GetHashCode() : 0;
    }
}