// Prexonite – ParserV2
// Token kinds mirroring the Flex-based lexer plus new parser-internal kinds.

namespace Prexonite.Compiler.ParserV2.Lexing;

public enum TokenKind
{
    // ── End / Error ──────────────────────────────────────────────
    Eof,
    Error,

    // ── Literals ─────────────────────────────────────────────────
    Integer,       // 42  0xFF
    Real,          // 1.5e-3
    RealLike,      // 1.0  2.3  (could also be start of version)
    Version,       // 1.2.3  1.2.3.4
    StringRaw,     // fully-resolved string content (no interpolation)

    // String-interpolation segment tokens (produced while inside a string)
    StringSegmentText,   // plain text inside "..."
    StringSegmentId,     // $name inside "..."
    StringInterpolStart, // $( — parser should recurse for expr then expect )
    StringEnd,           // closing " (string is done)

    // ── Identifiers / keywords ───────────────────────────────────
    Identifier,    // plain id
    LabelId,       // id followed by `:` (from CONTEXT rule)
    NsId,          // id followed by `::` (namespace path component)

    // Keywords  (IGNORECASE in lex ⇒ all lower-case here for normalisation)
    KwVar,
    KwRef,
    KwTrue,
    KwFalse,
    KwNull,
    KwMod,
    KwIs,
    KwAs,
    KwNot,
    KwEnabled,
    KwDisabled,
    KwFunction,
    KwCommand,
    KwAsm,
    KwDeclare,
    KwBuild,
    KwReturn,
    KwIn,
    KwTo,
    KwAdd,
    KwContinue,
    KwBreak,
    KwYield,
    KwOr,
    KwAnd,
    KwXor,
    KwLabel,
    KwGoto,
    KwStatic,
    KwIf,
    KwUnless,
    KwElse,
    KwNew,
    KwCoroutine,
    KwFrom,
    KwDo,
    KwDoes,
    KwWhile,
    KwUntil,
    KwFor,
    KwForeach,
    KwTry,
    KwCatch,
    KwFinally,
    KwThrow,
    KwThen,
    KwUsing,       // "using" (stored as KwUsing, source spells "using")
    KwMacro,
    KwLazy,
    KwLet,
    KwMethod,
    KwThis,
    KwNamespace,
    KwExport,

    // ── Punctuation / operators ───────────────────────────────────
    BitAnd,        // &
    Assign,        // =
    Comma,         // ,
    Dec,           // --
    Div,           // /
    Dot,           // .
    Eq,            // ==
    Gt,            // >
    Ge,            // >=
    Inc,           // ++
    LBrace,        // {
    LBrack,        // [
    LParen,        // (
    Lt,            // <
    Le,            // <=
    Minus,         // -
    Ne,            // !=
    BitOr,         // |
    Plus,          // +
    Pow,           // ^
    RBrace,        // }
    RBrack,        // ]
    RParen,        // )
    Tilde,         // ~
    Times,         // *
    Semicolon,     // ;
    Colon,         // :
    DoubleColon,   // ::
    Coalescence,   // ??
    Question,      // ?
    Pointer,       // ->
    Implementation,// =>
    At,            // @
    AppendLeft,    // <<
    AppendRight,   // >>
    DeltaRight,    // |>
    DeltaLeft,     // <|
    InterpreterLine, // #!/...

    // ── Operator-as-identifier ───────────────────────────────────
    // Tokens like (+), (-), (*), (==), etc. come through as Identifier with
    // the operator name as the text. No special TokenKind needed.
}
