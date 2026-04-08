// Prexonite – ParserV2 AST – Expressions

using System.Collections.Immutable;

namespace Prexonite.Compiler.ParserV2.Ast;

// ─────────────────────────────────────────────────────────────────────────────
//  Literals
// ─────────────────────────────────────────────────────────────────────────────

public sealed record IntLit(SourceSpan Span, int Value) : Expr(Span);
public sealed record RealLit(SourceSpan Span, double Value) : Expr(Span);
public sealed record BoolLit(SourceSpan Span, bool Value) : Expr(Span);
public sealed record NullLit(SourceSpan Span) : Expr(Span);

/// <summary>A string literal with no interpolation (raw decoded value).</summary>
public sealed record StringLit(SourceSpan Span, string Value) : Expr(Span);

/// <summary>
/// An interpolated string like <c>"hello $name"</c> or <c>"$(x + 1) items"</c>.
/// Segments alternate between text and embedded expressions/identifiers.
/// </summary>
public sealed record InterpolatedString(
    SourceSpan Span,
    ImmutableArray<StringSegment> Segments) : Expr(Span);

public abstract record StringSegment(SourceSpan Span) : Node(Span);
public sealed record TextSegment(SourceSpan Span, string Text) : StringSegment(Span);
/// <summary>Short <c>$name</c> interpolation (no parentheses).</summary>
public sealed record IdSegment(SourceSpan Span, string Name) : StringSegment(Span);
/// <summary>Full <c>$(expr)</c> interpolation.</summary>
public sealed record ExprSegment(SourceSpan Span, Expr Expression) : StringSegment(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  References and placeholders
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A bare identifier. May contain <c>\</c> and <c>'</c>.</summary>
public sealed record NameExpr(SourceSpan Span, string Name) : Expr(Span);

/// <summary>
/// A pointer/reference expression: <c>-&gt;name</c>, <c>-&gt;-&gt;name</c>.
/// <paramref name="PointerCount"/> is the number of <c>-&gt;</c> tokens.
/// </summary>
public sealed record RefExpr(SourceSpan Span, string Name, int PointerCount) : Expr(Span);

/// <summary>Partial-application placeholder: <c>?</c> or <c>?1</c>, <c>?2</c>…</summary>
public sealed record PlaceholderExpr(SourceSpan Span, int? Index) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Local variable declaration (usable as LValue)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A local variable declaration expression: <c>var x</c>, <c>ref ref x</c>,
/// <c>static var x</c>, <c>new var x</c>, etc.
/// Appears both as statements and as LValues in assignments/foreach/destructuring.
/// </summary>
public sealed record LocalVarDecl(
    SourceSpan Span,
    bool IsNew,      // `new` prefix before the decl
    bool IsStatic,
    int RefCount,    // how many `ref` keywords (1 = first ref)
    bool HasVar,     // `var` keyword was present
    string Name) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Collection literals
// ─────────────────────────────────────────────────────────────────────────────

public sealed record ListLit(SourceSpan Span, ImmutableArray<Expr> Elements) : Expr(Span);

/// <summary>Hash/dictionary literal: <c>{ k1:v1, k2:v2 }</c> or <c>{ expr, expr }</c>.</summary>
public sealed record HashLit(SourceSpan Span, ImmutableArray<Expr> Elements) : Expr(Span);

/// <summary>Key-value pair expression: <c>a : b</c>. Right-recursive.</summary>
public sealed record KeyValueExpr(SourceSpan Span, Expr Key, Expr Value) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Operators
// ─────────────────────────────────────────────────────────────────────────────

public enum BinaryOp
{
    Add, Sub, Mul, Div, Mod, Pow,
    Eq, Ne, Lt, Le, Gt, Ge,
    LogicalAnd, LogicalOr,
    BitwiseAnd, BitwiseOr, Xor,
    DeltaLeft, DeltaRight,
    Then,      // `then` keyword
}

public enum UnaryOp
{
    Negate,        // -
    LogicalNot,    // not
    PreIncrement,  // ++x
    PreDecrement,  // --x
    PostIncrement, // x++
    PostDecrement, // x--
    PreDeltaLeft,  // <|x
    PreDeltaRight, // |>x
    PostDeltaLeft, // x<|
    PostDeltaRight,// x|>
    Splice,        // *x  (argument splice)
}

public enum AssignOp
{
    Assign,
    Add, Sub, Mul, Div,
    BitwiseAnd, BitwiseOr,
    Coalesce,
    DeltaLeft, DeltaRight,
    Cast,  // ~=
}

public sealed record BinaryExpr(SourceSpan Span, BinaryOp Op, Expr Left, Expr Right) : Expr(Span);
public sealed record UnaryExpr(SourceSpan Span, UnaryOp Op, Expr Operand) : Expr(Span);

/// <summary>
/// Null-coalescing chain: <c>a ?? b ?? c</c> stored as a flat multi-operand node.
/// </summary>
public sealed record CoalesceExpr(SourceSpan Span, ImmutableArray<Expr> Operands) : Expr(Span);

/// <summary>Assignment: <c>lhs = rhs</c>, <c>lhs += rhs</c>, etc.</summary>
public sealed record AssignExpr(SourceSpan Span, Expr Target, Expr Value, AssignOp Op) : Expr(Span);

/// <summary>Cast-assignment: <c>x ~= Type</c>.</summary>
public sealed record CastAssignExpr(SourceSpan Span, Expr Target, TypeExpr Type) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Member access, calls, indexing
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Explicit call: <c>f(args)</c> where the callee is any expression.
/// The argument list also carries any <c>&lt;&lt;</c> prepend-args.
/// </summary>
public sealed record CallExpr(SourceSpan Span, Expr Callee, CallArgs Args) : Expr(Span);

/// <summary>Member access without an explicit call: <c>obj.member</c>.</summary>
public sealed record MemberAccessExpr(SourceSpan Span, Expr Subject, string Member) : Expr(Span);

/// <summary>Member call with explicit <c>()</c>: <c>obj.member(args)</c>.</summary>
public sealed record MemberCallExpr(
    SourceSpan Span,
    Expr Subject,
    string Member,
    CallArgs Args) : Expr(Span);

/// <summary>Indirect call: <c>obj.(args)</c>.</summary>
public sealed record IndirectCallExpr(SourceSpan Span, Expr Subject, CallArgs Args) : Expr(Span);

/// <summary>Index access: <c>obj[i]</c>, <c>obj[i, j]</c>.</summary>
public sealed record IndexExpr(SourceSpan Span, Expr Subject, ImmutableArray<Expr> Indices) : Expr(Span);

/// <summary>
/// Append-right operator: <c>left &gt;&gt; right</c>.
/// The right side is typically a call expression.
/// </summary>
public sealed record AppendRightExpr(SourceSpan Span, Expr Left, Expr Right) : Expr(Span);

/// <summary>
/// Append-left operator: <c>expr &lt;&lt; (a, b)</c>.
/// <paramref name="PrependArgs"/> are the arguments to prepend.
/// </summary>
public sealed record AppendLeftExpr(
    SourceSpan Span,
    Expr Callee,
    ImmutableArray<Expr> PrependArgs) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Type operations
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Type cast: <c>expr~Type</c>.</summary>
public sealed record TypeCastExpr(SourceSpan Span, Expr Subject, TypeExpr Type) : Expr(Span);

/// <summary>Type check: <c>expr is Type</c> or <c>expr is not Type</c>.</summary>
public sealed record TypeCheckExpr(
    SourceSpan Span,
    Expr Subject,
    TypeExpr Type,
    bool IsNegated) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Control-flow expressions
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Conditional expression: <c>if (cond) expr else expr</c> or <c>unless …</c>.</summary>
public sealed record ConditionalExpr(
    SourceSpan Span,
    bool IsNegated,
    Expr Condition,
    Expr Then,
    Expr Else) : Expr(Span);

public sealed record ThrowExpr(SourceSpan Span, Expr Value) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Object creation
// ─────────────────────────────────────────────────────────────────────────────

public sealed record NewExpr(SourceSpan Span, TypeExpr Type, CallArgs Args) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Lambdas and lazy / coroutine
// ─────────────────────────────────────────────────────────────────────────────

public abstract record LambdaBody(SourceSpan Span) : Node(Span);
public sealed record LambdaBlockBody(SourceSpan Span, Block Statements) : LambdaBody(Span);
public sealed record LambdaExprBody(SourceSpan Span, Expr Expression) : LambdaBody(Span);

/// <summary>Lambda expression: <c>x =&gt; expr</c> or <c>(x, y) =&gt; { … }</c>.</summary>
public sealed record LambdaExpr(
    SourceSpan Span,
    ImmutableArray<FormalParam> Params,
    LambdaBody Body) : Expr(Span);

/// <summary>Lazy thunk: <c>lazy expr</c> or <c>lazy { … }</c>.</summary>
public sealed record LazyExpr(SourceSpan Span, LambdaBody Body) : Expr(Span);

/// <summary>Coroutine creation: <c>coroutine expr</c> or <c>coroutine expr for (args)</c>.</summary>
public sealed record CoroutineExpr(
    SourceSpan Span,
    Expr Callee,
    ImmutableArray<Expr> Args) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Inline assembler (expression context)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Inline asm block used as an expression: <c>asm(…)</c>.</summary>
public sealed record AsmExpr(SourceSpan Span, ImmutableArray<AsmInstr> Instructions) : Expr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Static call
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Static member call: <c>~Type.Member(args)</c> or <c>::ClrType.Member(args)</c>.
/// </summary>
public sealed record StaticCallExpr(
    SourceSpan Span,
    TypeExpr Type,
    string Member,
    CallArgs Args) : Expr(Span);
