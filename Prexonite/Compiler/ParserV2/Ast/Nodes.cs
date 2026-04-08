// Prexonite – ParserV2 AST
// Base node types, shared helpers and common small records.

using System.Collections.Immutable;

namespace Prexonite.Compiler.ParserV2.Ast;

// ─────────────────────────────────────────────────────────────────────────────
//  Root
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Root of the AST hierarchy. Every node carries a source span.</summary>
public abstract record Node(SourceSpan Span);

/// <summary>Any expression that produces a value (or acts as an LValue).</summary>
public abstract record Expr(SourceSpan Span) : Node(Span);

/// <summary>Any statement (side effect, control flow, declaration inside a function).</summary>
public abstract record Stmt(SourceSpan Span) : Node(Span);

/// <summary>A top-level or namespace-level declaration.</summary>
public abstract record Decl(SourceSpan Span) : Node(Span);

/// <summary>A type expression (used in casts, type checks, object creation).</summary>
public abstract record TypeExpr(SourceSpan Span) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Shared small building blocks
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>An ordered list of statements (function body, if-branch, etc.).</summary>
public sealed record Block(SourceSpan Span, ImmutableArray<Stmt> Statements) : Node(Span)
{
    public static Block Empty(SourceSpan span) => new(span, []);
}

/// <summary>Argument list for a call expression.</summary>
/// <param name="HasExplicitParens">Whether the source contained an explicit <c>()</c> pair.</param>
/// <param name="Args">Positional arguments inside <c>()</c>.</param>
/// <param name="PrependArgs">Arguments added via <c>&lt;&lt;</c> operator (inserted before >>-position).</param>
public sealed record CallArgs(
    bool HasExplicitParens,
    ImmutableArray<Expr> Args,
    ImmutableArray<Expr> PrependArgs)
{
    public static CallArgs NoCall => new(false, [], []);
    public static CallArgs Empty => new(true, [], []);
    public static CallArgs Of(ImmutableArray<Expr> args) => new(true, args, []);
}

/// <summary>A formal parameter of a function or lambda.</summary>
public sealed record FormalParam(SourceSpan Span, bool IsRef, string Name) : Node(Span);

/// <summary>A qualified namespace name like <c>prx.cli.io</c>.</summary>
public sealed record QualifiedName(SourceSpan Span, ImmutableArray<string> Parts) : Node(Span)
{
    public override string ToString() => string.Join("::", Parts);
}

// ─────────────────────────────────────────────────────────────────────────────
//  Meta / annotation nodes
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A single meta-table annotation key/value entry.</summary>
public abstract record MetaEntry(SourceSpan Span) : Node(Span);

/// <summary><c>is key</c> or <c>is not key</c> or <c>not key</c>.</summary>
public sealed record MetaBoolEntry(SourceSpan Span, string Key, bool Value) : MetaEntry(Span);

/// <summary><c>key</c> (true) / <c>key enabled</c> / <c>key disabled</c>.</summary>
public sealed record MetaSwitchEntry(SourceSpan Span, string Key, bool Value) : MetaEntry(Span);

/// <summary><c>key expr</c> where expr is an MExpr literal.</summary>
public sealed record MetaValueEntry(SourceSpan Span, string Key, MExprNode Value) : MetaEntry(Span);

/// <summary><c>add expr to key</c></summary>
public sealed record MetaAddEntry(SourceSpan Span, string Key, MExprNode Addition) : MetaEntry(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  MExpr nodes (used in declare(...) and meta expressions)
//  Mirrors Prexonite.Compiler.Internal.MExpr
// ─────────────────────────────────────────────────────────────────────────────

public abstract record MExprNode(SourceSpan Span) : Node(Span);

/// <summary>An MExpr atom: string, int, bool, Version, or null.</summary>
public sealed record MExprAtom(SourceSpan Span, object? Value) : MExprNode(Span)
{
    public override string ToString() => Value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => Value.ToString() ?? "null"
    };
}

/// <summary>An MExpr list: <c>head(arg1, arg2, ...)</c> or <c>head arg</c>.</summary>
public sealed record MExprList(SourceSpan Span, string Head, ImmutableArray<MExprNode> Args) : MExprNode(Span)
{
    public override string ToString() => Args.Length switch
    {
        0 => Head,
        1 => $"{Head} {Args[0]}",
        _ => $"{Head}({string.Join(",", Args)})"
    };
}

// ─────────────────────────────────────────────────────────────────────────────
//  Namespace transfer directives (used in namespace declarations and imports)
// ─────────────────────────────────────────────────────────────────────────────

public abstract record NsTransferDirective(SourceSpan Span) : Node(Span);

/// <summary>A wildcard import: <c>*</c>.</summary>
public sealed record NsWildcardDirective(SourceSpan Span) : NsTransferDirective(Span);

/// <summary>An identifier import/rename: <c>name</c> or <c>externalName =&gt; internalName</c>.</summary>
public sealed record NsRenameDirective(SourceSpan Span, string ExternalName, string InternalName)
    : NsTransferDirective(Span);

/// <summary>A drop directive: <c>not name</c>.</summary>
public sealed record NsDropDirective(SourceSpan Span, string Name) : NsTransferDirective(Span);

/// <summary>
/// One source-spec in a transfer list: <c>ns.path(*)</c> or <c>ns.path(a, b =&gt; c)</c>.
/// </summary>
public sealed record NsTransferSpec(
    SourceSpan Span,
    QualifiedName Source,
    bool SourceHasWildcard,
    ImmutableArray<NsTransferDirective> Directives) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Asm instruction nodes
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A single assembler instruction inside an <c>asm { }</c> block.</summary>
public abstract record AsmInstr(SourceSpan Span) : Node(Span);

/// <summary>A variable declaration inside asm: <c>var x</c> or <c>ref x</c>.</summary>
public sealed record AsmVarDecl(SourceSpan Span, bool IsRef, ImmutableArray<string> Names)
    : AsmInstr(Span);

/// <summary>A label declaration inside asm: <c>label myLabel</c>.</summary>
public sealed record AsmLabelDecl(SourceSpan Span, string Name) : AsmInstr(Span);

/// <summary>
/// A general opcode instruction. <paramref name="OpCode"/> may be null if the instruction
/// name was not recognised (error-tolerant parsing).
/// </summary>
public sealed record AsmOpInstr(
    SourceSpan Span,
    OpCode? OpCode,
    string RawOpName,
    string? Detail,
    AsmArg? Arg0,
    AsmArg? Arg1) : AsmInstr(Span);

/// <summary>An argument to an assembler instruction.</summary>
public abstract record AsmArg(SourceSpan Span) : Node(Span);
public sealed record AsmArgInt(SourceSpan Span, int Value) : AsmArg(Span);
public sealed record AsmArgReal(SourceSpan Span, double Value) : AsmArg(Span);
public sealed record AsmArgBool(SourceSpan Span, bool Value) : AsmArg(Span);
public sealed record AsmArgId(SourceSpan Span, string Name) : AsmArg(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Error / placeholder node
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Used in place of any syntactically invalid construct.
/// Consumers should not pattern-match against this; it is a signal that a parse error occurred.
/// </summary>
public sealed record ErrorNode(SourceSpan Span, string Message) : Expr(Span)
{
    // Also acts as a Stmt and Decl through wrapper nodes below.
}

/// <summary>Wraps an <see cref="ErrorNode"/> where a <see cref="Stmt"/> is expected.</summary>
public sealed record ErrorStmt(SourceSpan Span, string Message) : Stmt(Span);

/// <summary>Wraps an <see cref="ErrorNode"/> where a <see cref="Decl"/> is expected.</summary>
public sealed record ErrorDecl(SourceSpan Span, string Message) : Decl(Span);

/// <summary>Wraps an <see cref="ErrorNode"/> where a <see cref="TypeExpr"/> is expected.</summary>
public sealed record ErrorTypeExpr(SourceSpan Span, string Message) : TypeExpr(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Diagnostics
// ─────────────────────────────────────────────────────────────────────────────

public enum DiagnosticSeverity { Error, Warning }

public sealed record Diagnostic(
    DiagnosticSeverity Severity,
    SourceSpan Span,
    string Message);
