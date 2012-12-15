using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Properties;

namespace Prexonite.Compiler.Internal
{
    public static class SymbolMExprParser
    {
        [NotNull]
        public static Symbol Parse([NotNull] ISymbolView<Symbol> symbols, [NotNull] MExpr expr)
        {
            MExpr innerSymbolExpr;
            Symbol innerSymbol;
            string symbolName;
            List<MExpr> elements;
            object raw;
            if (expr.TryMatchHead(SymbolMExprSerializer.DereferenceHead, out innerSymbolExpr))
            {
                innerSymbol = Parse(symbols, innerSymbolExpr);
                return Symbol.CreateDereference(innerSymbol, expr.Position);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.CrossReferenceHead, out innerSymbolExpr)
                     && innerSymbolExpr.TryMatchStringAtom(out symbolName))
            {
                if (symbols.TryGet(symbolName, out innerSymbol))
                {
                    return innerSymbol;
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(
                            String.Format("Cannot find symbol {0} referred to by delcaration {1}.", symbolName, expr),
                            expr.Position, MessageClasses.SymbolNotResolved));
                }
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.ErrorHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Error, symbols, expr, elements);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.WarningHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Warning, symbols, expr, elements);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.InfoHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Info, symbols, expr, elements);
            }
            else if(expr.TryMatchHead(SymbolMExprSerializer.ExpandHead,out innerSymbolExpr))
            {
                innerSymbol = Parse(symbols, innerSymbolExpr);
                return Symbol.CreateExpand(innerSymbol, expr.Position);
            }
            else if (expr.TryMatchAtom(out raw) && raw == null)
            {
                return Symbol.CreateNil(expr.Position);
            }
            else
            {
                // must be a reference 
                return Symbol.CreateReference(EntityRefMExprParser.Parse(expr),expr.Position);
            }
        }

        [NotNull]
        private static Symbol _parseMessage(MessageSeverity severity, [NotNull] ISymbolView<Symbol> symbols,
                                            [NotNull] MExpr expr, [NotNull] List<MExpr> elements)
        {
            Debug.Assert(elements[0] != null);
            Debug.Assert(elements[1] != null);
            Debug.Assert(elements[2] != null);
            Debug.Assert(elements[3] != null);
            var position = _parsePosition(elements[0]);
            object rawMessageClass;
            string messageText;
            if (elements[1].TryMatchAtom(out rawMessageClass) && elements[2].TryMatchStringAtom(out messageText))
            {
                return
                    Symbol.CreateMessage(
                        Message.Create(severity, messageText, position,
                                       (rawMessageClass == null ? null : rawMessageClass.ToString())),
                        Parse(symbols, elements[3]), expr.Position);
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error(String.Format(Resources.Parser_Cannot_parse_message_symbol, expr),
                                  expr.Position, MessageClasses.CannotParseMExpr));
            }
        }

        [NotNull]
        private static ISourcePosition _parsePosition([NotNull] MExpr expr)
        {
            MExpr fileExpr;
            MExpr lineExpr;
            MExpr columnExpr;
            string file;
            int line;
            int column;
            if (expr.TryMatchHead(SymbolMExprSerializer.SourcePositionHead, out fileExpr, out lineExpr,
                                  out columnExpr)
                && fileExpr.TryMatchStringAtom(out file)
                && lineExpr.TryMatchIntAtom(out line)
                && columnExpr.TryMatchIntAtom(out column))
            {
                return new SourcePosition(file,line,column);
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error(String.Format(Resources.Parser_Cannot_parse_source_position_, expr), expr.Position,
                                  MessageClasses.CannotParseMExpr));
            }
        }
    }
}