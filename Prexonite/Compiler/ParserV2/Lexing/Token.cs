// Prexonite – ParserV2

namespace Prexonite.Compiler.ParserV2.Lexing;

/// <summary>A single lexical token with its kind, raw text, and source location.</summary>
/// <param name="Kind">What kind of token this is.</param>
/// <param name="Text">
///   The resolved text of the token.
///   For string literals, this is the decoded string value (escapes processed).
///   For identifiers, keywords and operators, this is the raw source text (lower-cased for keywords).
/// </param>
/// <param name="Span">Source location of this token.</param>
public readonly record struct Token(TokenKind Kind, string Text, SourceSpan Span)
{
    public static Token Synthetic(TokenKind kind, string text = "") =>
        new(kind, text, SourceSpan.Unknown);

    public bool IsKeyword =>
        Kind is >= TokenKind.KwVar and <= TokenKind.KwExport;

    public bool IsIdentifierLike =>
        Kind is TokenKind.Identifier or TokenKind.LabelId or TokenKind.NsId
        || IsKeyword;

    public override string ToString() => $"{Kind}({Text}) @ {Span}";
}
