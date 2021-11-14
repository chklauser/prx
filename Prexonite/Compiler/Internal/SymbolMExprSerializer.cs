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

namespace Prexonite.Compiler.Internal;

public class SymbolMExprSerializer : SymbolHandler<IDictionary<Symbol, QualifiedId>,MExpr>
{
    #region Singleton

    [NotNull]
    public static SymbolMExprSerializer Instance { get; } = new();

    #endregion

    public const string DereferenceHead = "ref";
    public const string ExpandHead = "expand";
    public const string WarningHead = "warn";
    public const string ErrorHead = "error";
    public const string InfoHead = "info";
    public const string SourcePositionHead = "pos";
    public const string CrossReferenceHead = "sym";

    [NotNull]
    public static MExpr SerializePosition([NotNull] ISourcePosition exprPosition, [NotNull] ISourcePosition sourcePosition)
    {
        return new MExpr.MList(exprPosition, SourcePositionHead, new MExpr[]
        {
            new MExpr.MAtom(exprPosition, sourcePosition.File),
            new MExpr.MAtom(exprPosition, sourcePosition.Line), 
            new MExpr.MAtom(exprPosition, sourcePosition.Column)
        });
    }

    [CanBeNull]
    private MExpr _lookForExistingSymbol(ISourcePosition position, IDictionary<Symbol, QualifiedId> existingSymbols, Symbol symbol)
    {
        if (existingSymbols.TryGetValue(symbol, out var symbolName))
        {
            return new MExpr.MList(position, SymbolMExprParser.AbsoluteModifierHead, new MExpr.MList(position, CrossReferenceHead, 
                symbolName.Select(part => new MExpr.MAtom(position,part))));
        }
        else
        {
            return null;
        }
    }

    public override MExpr HandleNamespace(NamespaceSymbol self, IDictionary<Symbol, QualifiedId> argument)
    {
        return _lookForExistingSymbol(self.Position, argument, self) ?? base.HandleNamespace(self, argument);
    }

    public override MExpr HandleReference(ReferenceSymbol self, IDictionary<Symbol, QualifiedId> existingSymbols)
    {
        return _lookForExistingSymbol(self.Position,existingSymbols,self) ?? self.Entity.Match(EntityRefMExprSerializer.Instance, self.Position);
    }

    public override MExpr HandleNil(NilSymbol self, IDictionary<Symbol, QualifiedId> existingSymbols)
    {
        return _lookForExistingSymbol(self.Position,existingSymbols, self) ?? new MExpr.MAtom(self.Position, null);
    }

    public override MExpr HandleExpand(ExpandSymbol self, IDictionary<Symbol, QualifiedId> existingSymbols)
    {
        return _lookForExistingSymbol(self.Position, existingSymbols, self) ?? new MExpr.MList(self.Position, ExpandHead, self.InnerSymbol.HandleWith(this, existingSymbols));
    }

    public override MExpr HandleDereference(DereferenceSymbol self, IDictionary<Symbol, QualifiedId> existingSymbols)
    {
        return _lookForExistingSymbol(self.Position, existingSymbols, self) ?? new MExpr.MList(self.Position, DereferenceHead, self.InnerSymbol.HandleWith(this, existingSymbols));
    }

    public override MExpr HandleMessage(MessageSymbol self, IDictionary<Symbol, QualifiedId> existingSymbols)
    {
        var existing = _lookForExistingSymbol(self.Position, existingSymbols, self);
        if (existing != null)
            return existing;

        string head = self.Message.Severity switch
        {
            MessageSeverity.Error => ErrorHead,
            MessageSeverity.Warning => WarningHead,
            MessageSeverity.Info => InfoHead,
            _ => throw new ArgumentOutOfRangeException("Unknown message severity " +
                Enum.GetName(typeof(MessageSeverity), self.Message.Severity))
        };

        return new MExpr.MList(self.Position, head,
            new[]
            {
                SerializePosition(self.Position, self.Message.Position),
                new MExpr.MAtom(self.Position, self.Message.MessageClass),
                new MExpr.MAtom(self.Position, self.Message.Text),
                self.InnerSymbol.HandleWith(this,existingSymbols)
            });
    }
            
}