// Prexonite – ParserV2 AST – Top-level declarations

using System.Collections.Immutable;

namespace Prexonite.Compiler.ParserV2.Ast;

// ─────────────────────────────────────────────────────────────────────────────
//  Top-level file structure
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// The root of a parsed Prexonite Script file.
/// Contains the optional interpreter line followed by all top-level declarations.
/// </summary>
public sealed record CompilationUnit(
    SourceSpan Span,
    string? InterpreterLine,
    ImmutableArray<Decl> Declarations,
    ImmutableArray<Diagnostic> Diagnostics) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Function declarations
// ─────────────────────────────────────────────────────────────────────────────

public enum FunctionKind { Function, Lazy, Coroutine, Macro }

/// <summary>How a function body is introduced syntactically.</summary>
public enum FunctionBodyStyle { Brace, Does, Arrow, Assign }

public abstract record FunctionBodyNode(SourceSpan Span, FunctionBodyStyle Style) : Node(Span);

/// <summary>A block body: <c>{ stmts }</c> or <c>does stmt</c> or <c>=&gt; { stmts }</c>.</summary>
public sealed record FunctionBlockBody(
    SourceSpan Span,
    FunctionBodyStyle Style,
    Block Statements) : FunctionBodyNode(Span, Style);

/// <summary>An expression body: <c>= expr;</c> or <c>=&gt; expr;</c>.</summary>
public sealed record FunctionExprBody(
    SourceSpan Span,
    FunctionBodyStyle Style,
    Expr Expression) : FunctionBodyNode(Span, Style);

/// <summary>
/// A function declaration (top-level or nested).
/// Also used for inline functions declared inside other functions
/// (in that case <see cref="IsNested"/> = true).
/// </summary>
public sealed record FunctionDecl(
    SourceSpan Span,
    FunctionKind Kind,
    string? PrimaryName,                         // null if only aliases provided
    ImmutableArray<string> Aliases,
    ImmutableArray<FormalParam> Parameters,
    ImmutableArray<MetaEntry> Meta,              // [is test; …]
    NsImportClause? ImportClause,                // namespace import …
    FunctionBodyNode Body,
    bool IsNested = false) : Decl(Span);

/// <summary>The <c>namespace import spec1, spec2</c> clause on a function declaration.</summary>
public sealed record NsImportClause(
    SourceSpan Span,
    ImmutableArray<NsTransferSpec> Specs) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Global variable declarations
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A global variable declaration: <c>var name</c> or <c>ref name</c>,
/// with optional aliases, optional meta block, and optional initializer.
/// </summary>
public sealed record GlobalVarDecl(
    SourceSpan Span,
    bool IsRef,
    string? PrimaryName,
    ImmutableArray<string> Aliases,
    ImmutableArray<MetaEntry> Meta,
    Expr? Initializer) : Decl(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Namespace declarations
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A namespace declaration block.
/// </summary>
public sealed record NamespaceDecl(
    SourceSpan Span,
    QualifiedName Name,
    ImmutableArray<NsTransferSpec> ImportSpecs,  // after `import`
    ImmutableArray<Decl> Body,                   // may be empty for skeleton namespaces
    NsExportSpec? Export) : Decl(Span);

/// <summary>The <c>export(…)</c> or <c>export.* …</c> clause at end of a namespace.</summary>
public abstract record NsExportSpec(SourceSpan Span) : Node(Span);

/// <summary>Explicit export directives: <c>export(a, b =&gt; c, *)</c>.</summary>
public sealed record NsExportDirectives(
    SourceSpan Span,
    ImmutableArray<NsTransferDirective> Directives) : NsExportSpec(Span);

/// <summary>Export everything: <c>export.*</c> (implicit when no export clause).</summary>
public sealed record NsExportAll(SourceSpan Span) : NsExportSpec(Span);

/// <summary>Export with additional transfer specs: <c>export spec1, spec2</c>.</summary>
public sealed record NsExportSpecs(
    SourceSpan Span,
    ImmutableArray<NsTransferSpec> Specs) : NsExportSpec(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Namespace import statement (top-level)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Top-level <c>namespace import spec1, spec2;</c></summary>
public sealed record NamespaceImportDecl(
    SourceSpan Span,
    ImmutableArray<NsTransferSpec> Specs) : Decl(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  declare statement
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A <c>declare</c> statement (various forms).</summary>
public abstract record DeclareDecl(SourceSpan Span) : Decl(Span);

/// <summary>
/// Inline declaration list: <c>declare [ref] [function|var|…] name [as alias], …;</c>
/// </summary>
public sealed record DeclareListDecl(
    SourceSpan Span,
    bool IsRef,
    bool IsPointer,
    string? EntityKind,           // "var", "function", "command", "macro function", etc.; null = auto
    ImmutableArray<DeclareItem> Items) : DeclareDecl(Span);

public sealed record DeclareItem(
    SourceSpan Span,
    string Name,
    string? ModuleName,           // after /
    string? Alias,                // after as
    ImmutableArray<DeclareMessage> Messages) : Node(Span);

public sealed record DeclareMessage(
    SourceSpan Span,
    DiagnosticSeverity Severity,
    string? MessageClass,
    string Message) : Node(Span);

/// <summary>Block form: <c>declare { … }</c></summary>
public sealed record DeclareBlockDecl(
    SourceSpan Span,
    string? UsingModule,
    ImmutableArray<DeclareListDecl> Entries) : DeclareDecl(Span);

/// <summary>MExpr form: <c>declare (alias = MExpr, …)</c></summary>
public sealed record DeclareMExprDecl(
    SourceSpan Span,
    ImmutableArray<MExprBinding> Bindings) : DeclareDecl(Span);

public sealed record MExprBinding(SourceSpan Span, string Alias, MExprNode Expr) : Node(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Build block
// ─────────────────────────────────────────────────────────────────────────────

/// <summary><c>build [does] { … }</c> – executed at compile time.</summary>
public sealed record BuildBlockDecl(SourceSpan Span, Block Body) : Decl(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Global code block
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A bare <c>{ … }</c> block at global scope (initialization code).</summary>
public sealed record GlobalCodeDecl(SourceSpan Span, Block Body) : Decl(Span);

// ─────────────────────────────────────────────────────────────────────────────
//  Module meta assignments
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A top-level meta assignment: <c>Name "foo";</c>, <c>is Test;</c>, etc.</summary>
public sealed record ModuleMetaDecl(SourceSpan Span, MetaEntry Entry) : Decl(Span);
