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
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Properties;

namespace Prexonite.Compiler.Internal
{
    public class SymbolMExprParser
    {
        [NotNull] private readonly ISymbolView<Symbol> _symbols;
        [NotNull] private readonly ISymbolView<Symbol> _topLevelSymbols;
        [NotNull] private readonly IMessageSink _messageSink;

        public SymbolMExprParser([NotNull] ISymbolView<Symbol> symbols,[NotNull] IMessageSink messageSink, [NotNull]ISymbolView<Symbol> topLevelSymbols = null)
        {
            if (symbols == null)
                throw new ArgumentNullException("symbols");
            if (messageSink == null)
                throw new ArgumentNullException("messageSink");
            
            _symbols = symbols;
            _messageSink = messageSink;
            _topLevelSymbols = topLevelSymbols ?? symbols;
        }

        public const string HerePositionHead = "here";

        public const string AbsoluteModifierHead = "absolute";

        private bool _tryParseCrossReference(MExpr expr, [NotNull] ISymbolView<Symbol> symbols, out Symbol symbol)
        {
            symbol = null;

            List<MExpr> elements;
            if (!expr.TryMatchHead(SymbolMExprSerializer.CrossReferenceHead, out elements))
                return false;

            var currentScope = symbols;
            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                string symbolName;
                if (!element.TryMatchStringAtom(out symbolName))
                    throw new ErrorMessageException(Message.Error(
                        string.Format("Symbolic reference must be consist only of symbol names. Found {0} {1} instead.", (element != null ? element.GetType().ToString() : ""), element),
                        element.Position, MessageClasses.SymbolNotResolved));
                if (!currentScope.TryGet(symbolName, out symbol))
                    throw new ErrorMessageException(
                       Message.Error(
                           String.Format("Cannot find symbol {0} referred to by delcaration {1}.", symbolName, expr),
                           expr.Position, MessageClasses.SymbolNotResolved));

                // If this is not the last element in the sequence, it must refer to a namespace symbol
                if (i < elements.Count - 1)
                {
                    var errors = new List<Message>();
                    var nsSym = NamespaceSymbol.UnwrapNamespaceSymbol(symbol, element.Position, _messageSink, errors);

                    Message abortMessage = null;
                    foreach (var error in errors)
                    {
                        if (abortMessage == null)
                            abortMessage = error;
                        else
                            _messageSink.ReportMessage(error);
                    }

                    if (abortMessage != null)
                        throw new ErrorMessageException(abortMessage);

                    // Impossible. Condition required to convey that fact to null-analysis
                    if (nsSym == null)
                        throw new PrexoniteException("Namespace symbol was expected to exist. Internal error (\"impossible condition\").");
                    currentScope = nsSym.Namespace;
                }
            }
            if (symbol == null)
                throw new ErrorMessageException(Message.Error(
                    Resources.SymbolMExprParser_EmptySymbolicReference, expr.Position,
                    MessageClasses.SymbolNotResolved));

            return true;
        }

        [NotNull]
        public Symbol Parse( [NotNull] MExpr expr)
        {
            MExpr innerSymbolExpr;
            Symbol innerSymbol;
            List<MExpr> elements;
            object raw;
            if (expr.TryMatchHead(SymbolMExprSerializer.DereferenceHead, out innerSymbolExpr))
            {
                innerSymbol = Parse(innerSymbolExpr);
                return Symbol.CreateDereference(innerSymbol, expr.Position);
            }
            else if (_tryParseCrossReference(expr, _symbols, out innerSymbol))
            {
                return innerSymbol;
            }
            else if (expr.TryMatchHead(AbsoluteModifierHead, out innerSymbolExpr) && _tryParseCrossReference(innerSymbolExpr,_topLevelSymbols,out innerSymbol))
            {
                return innerSymbol;
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.ErrorHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Error, expr, elements);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.WarningHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Warning, expr, elements);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.InfoHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Info, expr, elements);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.ExpandHead, out innerSymbolExpr))
            {
                innerSymbol = Parse(innerSymbolExpr);
                return Symbol.CreateExpand(innerSymbol, expr.Position);
            }
            else if (expr.TryMatchAtom(out raw) && raw == null)
            {
                return Symbol.CreateNil(expr.Position);
            }
            else
            {
                // must be a reference 
                return Symbol.CreateReference(EntityRefMExprParser.Parse(expr), expr.Position);
            }
        }

        [NotNull]
        private Symbol _parseMessage(MessageSeverity severity,
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
                var message = Message.Create(severity, messageText, position,
                    (rawMessageClass == null ? null : rawMessageClass.ToString()));
                return Symbol.CreateMessage(message, Parse(elements[3]), expr.Position);
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
            List<MExpr> hereArgs;
            if (expr.TryMatchHead(SymbolMExprSerializer.SourcePositionHead, out fileExpr, out lineExpr,
                                  out columnExpr)
                && fileExpr.TryMatchStringAtom(out file)
                && lineExpr.TryMatchIntAtom(out line)
                && columnExpr.TryMatchIntAtom(out column))
            {
                return new SourcePosition(file, line, column);
            }
            else if(expr.TryMatchHead(HerePositionHead, out hereArgs))
            {
                return expr.Position;
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error(String.Format(Resources.Parser_Cannot_parse_source_position, expr), expr.Position,
                                  MessageClasses.CannotParseMExpr));
            }
        }
    }
}