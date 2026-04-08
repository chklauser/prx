// Prexonite – ParserV2 AST – Statements

using System.Collections.Immutable;

namespace Prexonite.Compiler.ParserV2.Ast;

// ─────────────────────────────────────────────────────────────────────────────
//  Simple statements
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>An expression used as a statement.</summary>
public sealed record ExprStmt(SourceSpan Span, Expr Expression) : Stmt(Span);

/// <summary><c>return [expr]</c></summary>
public sealed record ReturnStmt(SourceSpan Span, Expr? Expression) : Stmt(Span);

/// <summary><c>yield [expr]</c></summary>
public sealed record YieldStmt(SourceSpan Span, Expr? Expression) : Stmt(Span);

/// <summary><c>break</c></summary>
public sealed record BreakStmt(SourceSpan Span) : Stmt(Span);

/// <summary><c>continue</c></summary>
public sealed record ContinueStmt(SourceSpan Span) : Stmt(Span);

/// <summary><c>goto label</c></summary>
public sealed record GotoStmt(SourceSpan Span, string Label) : Stmt(Span);

/// <summary>An explicit label: <c>myLabel:</c></summary>
public sealed record LabelStmt(SourceSpan Span, string Name) : Stmt(Span);

/// <summary><c>throw expr</c></summary>
public sealed record ThrowStmt(SourceSpan Span, Expr Expression) : Stmt(Span);

/// <summary>
/// A <c>let</c> binding statement: <c>let x = expr, y = expr</c>.
/// Each binding may have an initializer.
/// </summary>
public sealed record LetBindingStmt(
    SourceSpan Span,
    ImmutableArray<LetBinding> Bindings) : Stmt(Span);

public sealed record LetBinding(SourceSpan Span, string Name, Expr? Initializer) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Structured statements
// ─────────────────────────────────────────────────────────────────────────────

/// <summary><c>if/unless (cond) stmt [else stmt]</c></summary>
public sealed record IfStmt(
    SourceSpan Span,
    bool IsNegated,
    Expr Condition,
    Block Then,
    Block? Else) : Stmt(Span);

/// <summary>
/// <c>while/until (cond) body</c> or <c>do body while/until (cond)</c>.
/// </summary>
public sealed record WhileStmt(
    SourceSpan Span,
    bool IsNegated,
    bool IsPostCondition,
    Expr Condition,
    Block Body) : Stmt(Span);

/// <summary>
/// <c>for (init; [while/until] cond; next) body</c>  (C-style, IsPostCondition = false)
/// <c>for (init; do next; while/until cond) body</c>  (Prexonite-style, IsPostCondition = true)
/// </summary>
public sealed record ForStmt(
    SourceSpan Span,
    Block Init,
    bool IsNegated,
    bool IsPostCondition,
    Expr? Condition,
    Block Next,
    Block Body) : Stmt(Span);

/// <summary><c>foreach (element in list) body</c></summary>
public sealed record ForeachStmt(
    SourceSpan Span,
    Expr Element,   // typically a LocalVarDecl or assignment target
    Expr List,
    Block Body) : Stmt(Span);

/// <summary>
/// Try/catch/finally. Catch and finally are both optional; at least one must be present.
/// </summary>
public sealed record TryCatchFinallyStmt(
    SourceSpan Span,
    Block Try,
    CatchClause? Catch,
    Block? Finally) : Stmt(Span);

public sealed record CatchClause(SourceSpan Span, Expr ExceptionVar, Block Body) : Node(Span);

/// <summary><c>using (resource) body</c></summary>
public sealed record UsingStmt(
    SourceSpan Span,
    Expr Resource,
    Block Body) : Stmt(Span);

/// <summary>
/// Inline asm block as a statement: <c>asm { … }</c>.
/// </summary>
public sealed record AsmStmt(SourceSpan Span, ImmutableArray<AsmInstr> Instructions) : Stmt(Span);

/// <summary>
/// A nested function definition inside a function body.
/// Semantically, this introduces a local variable bound to the function.
/// </summary>
public sealed record NestedFunctionStmt(SourceSpan Span, FunctionDecl Function) : Stmt(Span);
