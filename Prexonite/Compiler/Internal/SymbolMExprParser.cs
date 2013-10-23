// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
    public static class SymbolMExprParser
    {
        private const string HerePositionHead = "here";

        [NotNull]
        public static Symbol Parse([NotNull] ISymbolView<Symbol> symbols, [NotNull] MExpr expr, [NotNull] IMessageSink messageSink)
        {
            MExpr innerSymbolExpr;
            Symbol innerSymbol;
            string symbolName;
            List<MExpr> elements;
            object raw;
            if (expr.TryMatchHead(SymbolMExprSerializer.DereferenceHead, out innerSymbolExpr))
            {
                innerSymbol = Parse(symbols, innerSymbolExpr, messageSink);
                return Symbol.CreateDereference(innerSymbol, expr.Position);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.CrossReferenceHead, out elements))
            {
                var currentScope = symbols;
                innerSymbol = null;
                for (var i = 0; i < elements.Count; i++)
                {
                    var element = elements[i];
                    if(!element.TryMatchStringAtom(out symbolName))
                        throw new ErrorMessageException(Message.Error(
                            string.Format("Symbolic reference must be consist only of symbol names. Found {0} instead.", element),
                            element.Position,MessageClasses.SymbolNotResolved));
                    if(!currentScope.TryGet(symbolName, out innerSymbol))
                        throw new ErrorMessageException(
                           Message.Error(
                               String.Format("Cannot find symbol {0} referred to by delcaration {1}.", symbolName, expr),
                               expr.Position, MessageClasses.SymbolNotResolved));

                    // If this is not the last element in the sequence, it must refer to a namespace symbol
                    if (i < elements.Count - 1)
                    {
                        var errors = new List<Message>();
                        var nsSym = NamespaceSymbol.UnwrapNamespaceSymbol(innerSymbol, element.Position, messageSink, errors);

                        Message abortMessage = null; 
                        foreach (var error in errors)
                        {
                            if (abortMessage == null)
                                abortMessage = error;
                            else
                                messageSink.ReportMessage(error);
                        }
                            
                        if(abortMessage != null)
                            throw new ErrorMessageException(abortMessage);

                        // Impossible. Condition required to convey that fact to null-analysis
                        if(nsSym == null)
                            throw new PrexoniteException("Namespace symbol was expected to exist. Internal error (\"impossible condition\").");
                        currentScope = nsSym.Namespace;
                    }
                }
                if(innerSymbol == null)
                    throw new ErrorMessageException(Message.Error(
                        Resources.SymbolMExprParser_EmptySymbolicReference,expr.Position,
                        MessageClasses.SymbolNotResolved));
                return innerSymbol;
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.ErrorHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Error, symbols, expr, elements,messageSink);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.WarningHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Warning, symbols, expr, elements,messageSink);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.InfoHead, out elements) && elements.Count == 4)
            {
                return _parseMessage(MessageSeverity.Info, symbols, expr, elements,messageSink);
            }
            else if (expr.TryMatchHead(SymbolMExprSerializer.ExpandHead, out innerSymbolExpr))
            {
                innerSymbol = Parse(symbols, innerSymbolExpr,messageSink);
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
        private static Symbol _parseMessage(MessageSeverity severity, [NotNull] ISymbolView<Symbol> symbols,
                                            [NotNull] MExpr expr, [NotNull] List<MExpr> elements, [NotNull] IMessageSink messageSink)
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
                return Symbol.CreateMessage(message, Parse(symbols, elements[3],messageSink), expr.Position);
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
                    Message.Error(String.Format(Resources.Parser_Cannot_parse_source_position_, expr), expr.Position,
                                  MessageClasses.CannotParseMExpr));
            }
        }
    }
}