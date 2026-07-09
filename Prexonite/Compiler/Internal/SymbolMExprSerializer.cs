

using Prexonite.Compiler.Symbolic;

namespace Prexonite.Compiler.Internal;

public class SymbolMExprSerializer : SymbolHandler<IDictionary<Symbol, QualifiedId>,MExpr>
{
    #region Singleton

    public static SymbolMExprSerializer Instance { get; } = new();

    #endregion

    public const string DereferenceHead = "ref";
    public const string ExpandHead = "expand";
    public const string WarningHead = "warn";
    public const string ErrorHead = "error";
    public const string InfoHead = "info";
    public const string SourcePositionHead = "pos";
    public const string CrossReferenceHead = "sym";

    public static MExpr SerializePosition(ISourcePosition exprPosition, ISourcePosition sourcePosition)
    {
        return new MExpr.MList(exprPosition, SourcePositionHead,
        [
            new MExpr.MAtom(exprPosition, sourcePosition.File),
            new MExpr.MAtom(exprPosition, sourcePosition.Line), 
            new MExpr.MAtom(exprPosition, sourcePosition.Column),
        ]);
    }

    MExpr? _lookForExistingSymbol(ISourcePosition position, IDictionary<Symbol, QualifiedId> existingSymbols, Symbol symbol)
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
                Enum.GetName(typeof(MessageSeverity), self.Message.Severity)),
        };

        return new MExpr.MList(self.Position, head,
        [
            SerializePosition(self.Position, self.Message.Position),
                new MExpr.MAtom(self.Position, self.Message.MessageClass),
                new MExpr.MAtom(self.Position, self.Message.Text),
                self.InnerSymbol.HandleWith(this,existingSymbols),
        ]);
    }
            
}