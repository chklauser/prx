// Prexonite – ParserV2 AST – Type expressions

using System.Collections.Immutable;

namespace Prexonite.Compiler.ParserV2.Ast;

// ─────────────────────────────────────────────────────────────────────────────
//  Type argument (for generic type annotations like List<Int>)
// ─────────────────────────────────────────────────────────────────────────────

public abstract record TypeArg(SourceSpan Span) : Node(Span);

/// <summary>A literal constant used as a type argument: <c>List&lt;5&gt;</c>.</summary>
public sealed record TypeArgLiteral(SourceSpan Span, object Value) : TypeArg(Span);

/// <summary>A parenthesised expression used as a type argument: <c>List&lt;(n)&gt;</c>.</summary>
public sealed record TypeArgExpr(SourceSpan Span, Expr Expression) : TypeArg(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Type expressions
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A Prexonite built-in type like <c>Int</c>, <c>String</c>, <c>List&lt;Int&gt;</c>.
/// The optional <c>~</c> prefix (explicit cast marker) is tracked separately.
/// </summary>
public sealed record PrxTypeExpr(
    SourceSpan Span,
    string Name,
    ImmutableArray<TypeArg> TypeArgs) : TypeExpr(Span);

/// <summary>
/// A CLR type reference like <c>::Console</c>, <c>System::Console</c>,
/// <c>Prexonite::Types::PValueKeyValuePair</c>.
/// <paramref name="Parts"/> is the dotted CLR type name, e.g. <c>["System", "Console"]</c>.
/// A leading <c>::</c> means the first element is an empty string (<c>["", "Console"]</c>).
/// </summary>
public sealed record ClrTypeExpr(
    SourceSpan Span,
    ImmutableArray<string> Parts) : TypeExpr(Span)
{
    public string FullName => string.Join(".", Parts.Where(p => p.Length > 0));
}
