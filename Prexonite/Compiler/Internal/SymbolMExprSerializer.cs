using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;

namespace Prexonite.Compiler.Internal
{
    public class SymbolMExprSerializer : SymbolHandler<IDictionary<Symbol, string>,MExpr>
    {
        #region Singleton

        [NotNull] private static readonly SymbolMExprSerializer _instance = new SymbolMExprSerializer();

        public static SymbolMExprSerializer Instance
        {
            get { return _instance; }
        }

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
        private MExpr _lookForExistingSymbol(ISourcePosition position, IDictionary<Symbol, string> existingSymbols, Symbol symbol)
        {
            String symbolName;
            if (existingSymbols.TryGetValue(symbol, out symbolName))
            {
                return new MExpr.MList(position, CrossReferenceHead, symbolName);
            }
            else
            {
                return null;
            }
        }

        public override MExpr HandleReference(ReferenceSymbol self, IDictionary<Symbol,String> existingSymbols)
        {
            return _lookForExistingSymbol(self.Position,existingSymbols,self) ?? self.Entity.Match(EntityRefMExprSerializer.Instance, self.Position);
        }

        public override MExpr HandleNil(NilSymbol self, IDictionary<Symbol,String> existingSymbols)
        {
            return _lookForExistingSymbol(self.Position,existingSymbols, self) ?? new MExpr.MAtom(self.Position, null);
        }

        public override MExpr HandleExpand(ExpandSymbol self, IDictionary<Symbol,String> existingSymbols)
        {
            return _lookForExistingSymbol(self.Position, existingSymbols, self) ?? new MExpr.MList(self.Position, ExpandHead, self.InnerSymbol.HandleWith(this, existingSymbols));
        }

        public override MExpr HandleDereference(DereferenceSymbol self, IDictionary<Symbol,String> existingSymbols)
        {
            return _lookForExistingSymbol(self.Position, existingSymbols, self) ?? new MExpr.MList(self.Position, DereferenceHead, self.InnerSymbol.HandleWith(this, existingSymbols));
        }

        public override MExpr HandleMessage(MessageSymbol self, IDictionary<Symbol,String> existingSymbols)
        {
            var existing = _lookForExistingSymbol(self.Position, existingSymbols, self);
            if (existing != null)
                return existing;

            string head;
            switch (self.Message.Severity)
            {
                case MessageSeverity.Error:
                    head = ErrorHead;
                    break;
                case MessageSeverity.Warning:
                    head = WarningHead;
                    break;
                case MessageSeverity.Info:
                    head = InfoHead;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown message severity " + Enum.GetName(typeof(MessageSeverity),self.Message.Severity));
            }

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
}