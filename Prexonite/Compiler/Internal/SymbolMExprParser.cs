using System.Diagnostics;
using Prexonite.Compiler.Symbolic;
using Prexonite.Properties;

namespace Prexonite.Compiler.Internal;

public class SymbolMExprParser
{
    readonly ISymbolView<Symbol> _symbols;
    readonly ISymbolView<Symbol> _topLevelSymbols;
    readonly IMessageSink _messageSink;

    public SymbolMExprParser(
        ISymbolView<Symbol> symbols,
        IMessageSink messageSink,
        ISymbolView<Symbol>? topLevelSymbols = null
    )
    {
        _symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        _messageSink = messageSink ?? throw new ArgumentNullException(nameof(messageSink));
        _topLevelSymbols = topLevelSymbols ?? symbols;
    }

    public const string HerePositionHead = "here";

    public const string AbsoluteModifierHead = "absolute";

    bool _tryParseCrossReference(
        MExpr expr,
        ISymbolView<Symbol> symbols,
        [NotNullWhen(true)] out Symbol? symbol
    )
    {
        symbol = null;

        if (!expr.TryMatchHead(SymbolMExprSerializer.CrossReferenceHead, out List<MExpr>? elements))
            return false;

        var currentScope = symbols;
        for (var i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            if (!element.TryMatchStringAtom(out var symbolName))
                throw new ErrorMessageException(
                    Message.Error(
                        $"Symbolic reference must be consist only of symbol names. Found {element.GetType()} {element} instead.",
                        element.Position,
                        MessageClasses.SymbolNotResolved
                    )
                );
            if (!currentScope.TryGet(symbolName, out symbol))
                throw new ErrorMessageException(
                    Message.Error(
                        $"Cannot find symbol {symbolName} referred to by declaration {expr}.",
                        expr.Position,
                        MessageClasses.SymbolNotResolved
                    )
                );

            // If this is not the last element in the sequence, it must refer to a namespace symbol
            if (i < elements.Count - 1)
            {
                var errors = new List<Message>();
                var nsSym = NamespaceSymbol.UnwrapNamespaceSymbol(
                    symbol,
                    element.Position,
                    _messageSink,
                    errors
                );

                Message? abortMessage = null;
                foreach (var error in errors)
                {
                    if (abortMessage == null)
                        abortMessage = error;
                    else
                        _messageSink.ReportMessage(error);
                }

                if (abortMessage != null)
                    throw new ErrorMessageException(abortMessage);
                if (nsSym == null)
                    throw new PrexoniteException(
                        "UnwrapNamespaceSymbol returned null but no error message."
                    );

                currentScope = nsSym.Namespace;
            }
        }
        if (symbol == null)
            throw new ErrorMessageException(
                Message.Error(
                    Resources.SymbolMExprParser_EmptySymbolicReference,
                    expr.Position,
                    MessageClasses.SymbolNotResolved
                )
            );

        return true;
    }

    public Symbol Parse(MExpr expr)
    {
        Symbol? innerSymbol;
        if (expr.TryMatchHead(SymbolMExprSerializer.DereferenceHead, out MExpr? innerSymbolExpr))
        {
            innerSymbol = Parse(innerSymbolExpr);
            return Symbol.CreateDereference(innerSymbol, expr.Position);
        }
        else if (_tryParseCrossReference(expr, _symbols, out innerSymbol))
        {
            return innerSymbol;
        }
        else if (
            expr.TryMatchHead(AbsoluteModifierHead, out innerSymbolExpr)
            && _tryParseCrossReference(innerSymbolExpr, _topLevelSymbols, out innerSymbol)
        )
        {
            return innerSymbol;
        }
        else if (
            expr.TryMatchHead(SymbolMExprSerializer.ErrorHead, out List<MExpr>? elements)
            && elements.Count == 4
        )
        {
            return _parseMessage(MessageSeverity.Error, expr, elements);
        }
        else if (
            expr.TryMatchHead(SymbolMExprSerializer.WarningHead, out elements)
            && elements.Count == 4
        )
        {
            return _parseMessage(MessageSeverity.Warning, expr, elements);
        }
        else if (
            expr.TryMatchHead(SymbolMExprSerializer.InfoHead, out elements)
            && elements.Count == 4
        )
        {
            return _parseMessage(MessageSeverity.Info, expr, elements);
        }
        else if (expr.TryMatchHead(SymbolMExprSerializer.ExpandHead, out innerSymbolExpr))
        {
            innerSymbol = Parse(innerSymbolExpr);
            return Symbol.CreateExpand(innerSymbol, expr.Position);
        }
        else if (expr.TryMatchAtom(out var raw) && raw == null)
        {
            return Symbol.CreateNil(expr.Position);
        }
        else
        {
            // must be a reference
            return Symbol.CreateReference(EntityRefMExprParser.Parse(expr), expr.Position);
        }
    }

    Symbol _parseMessage(MessageSeverity severity, MExpr expr, List<MExpr> elements)
    {
        Debug.Assert(elements[0] != null);
        Debug.Assert(elements[1] != null);
        Debug.Assert(elements[2] != null);
        Debug.Assert(elements[3] != null);
        var position = _parsePosition(elements[0]);
        if (
            elements[1].TryMatchAtom(out var rawMessageClass)
            && elements[2].TryMatchStringAtom(out var messageText)
        )
        {
            var message = Message.Create(
                severity,
                messageText,
                position,
                rawMessageClass?.ToString()
            );
            return Symbol.CreateMessage(message, Parse(elements[3]), expr.Position);
        }
        else
        {
            throw new ErrorMessageException(
                Message.Error(
                    string.Format(Resources.Parser_Cannot_parse_message_symbol, expr),
                    expr.Position,
                    MessageClasses.CannotParseMExpr
                )
            );
        }
    }

    static ISourcePosition _parsePosition(MExpr expr)
    {
        if (
            expr.TryMatchHead(
                SymbolMExprSerializer.SourcePositionHead,
                out var fileExpr,
                out var lineExpr,
                out var columnExpr
            )
            && fileExpr.TryMatchStringAtom(out var file)
            && lineExpr.TryMatchIntAtom(out var line)
            && columnExpr.TryMatchIntAtom(out var column)
        )
        {
            return new SourcePosition(file, line, column);
        }
        else if (expr.TryMatchHead(HerePositionHead, out List<MExpr>? _))
        {
            return expr.Position;
        }
        else
        {
            throw new ErrorMessageException(
                Message.Error(
                    string.Format(Resources.Parser_Cannot_parse_source_position, expr),
                    expr.Position,
                    MessageClasses.CannotParseMExpr
                )
            );
        }
    }
}
