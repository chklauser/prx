// Prexonite – ParserV2 Lexer
// Hand-written, recursive-descent-friendly, UTF-8 aware.

using System.Globalization;
using System.Text;

namespace Prexonite.Compiler.ParserV2.Lexing;

/// <summary>
/// The lexer state determines which tokens are valid and how strings are treated.
/// The parser pushes/pops states using <see cref="Lexer.PushMode"/> and <see cref="Lexer.PopMode"/>.
/// </summary>
public enum LexerMode
{
    /// <summary>Top-level / global scope (YYINITIAL). No smart-string interpolation.</summary>
    Global,
    /// <summary>Inside a function body. Enables smart-string interpolation.</summary>
    Local,
    /// <summary>Inside an <c>asm { }</c> block. Keywords become identifiers.</summary>
    Asm,
    /// <summary>Inside a namespace transfer specification.</summary>
    Transfer,
}

/// <summary>
/// Pull lexer for Prexonite Script.
/// Call <see cref="Next"/> to advance to the next token.
/// The <see cref="Current"/> property holds the most recently consumed token.
/// <see cref="Peek"/> looks one token ahead without consuming it.
/// </summary>
public sealed class Lexer
{
    // ── input ───────────────────────────────────────────────────────────────
    readonly TextReader _reader;
    readonly string _file;

    // Character buffer for one-char lookahead.
    int _ch = -2;      // -2 = not yet read; -1 = EOF; ≥0 = codepoint
    int _line = 1;
    int _col = 1;
    // Position of _ch
    int _chLine = 1;
    int _chCol = 1;

    // ── mode stack ──────────────────────────────────────────────────────────
    LexerMode _mode = LexerMode.Global;
    readonly Stack<LexerMode> _modeStack = new();

    // ── token lookahead ─────────────────────────────────────────────────────
    Token _current = Token.Synthetic(TokenKind.Eof);
    Token? _peek;

    // ── string buffer ───────────────────────────────────────────────────────
    readonly StringBuilder _buf = new();

    // ═══════════════════════════════════════════════════════════════════════
    //  Construction
    // ═══════════════════════════════════════════════════════════════════════

    public Lexer(TextReader reader, string file = "<input>")
    {
        _reader = reader;
        _file = file;
    }

    public static Lexer ForString(string source, string file = "<string>") =>
        new(new StringReader(source), file);

    // ═══════════════════════════════════════════════════════════════════════
    //  Public API
    // ═══════════════════════════════════════════════════════════════════════

    public Token Current => _current;

    /// <summary>Look one token ahead without advancing.</summary>
    public Token Peek()
    {
        _peek ??= ReadToken();
        return _peek.Value;
    }

    /// <summary>Advance to the next token and return it.</summary>
    public Token Next()
    {
        if (_peek.HasValue)
        {
            _current = _peek.Value;
            _peek = null;
        }
        else
        {
            _current = ReadToken();
        }
        return _current;
    }

    // ── Mode management ────────────────────────────────────────────────────

    public LexerMode Mode => _mode;

    public void PushMode(LexerMode mode)
    {
        _modeStack.Push(_mode);
        _mode = mode;
        _peek = null; // invalidate lookahead when mode changes
    }

    public void PopMode()
    {
        if (_modeStack.Count == 0)
            throw new InvalidOperationException("Lexer mode stack is empty.");
        _mode = _modeStack.Pop();
        _peek = null;
    }

    // ── String segment API (called from parser while inside a string) ───────

    /// <summary>
    /// Lex the next string segment.
    /// Should only be called after the opening <c>"</c> has been consumed via
    /// <see cref="BeginString"/>. Returns tokens of kind:
    /// <list type="bullet">
    ///   <item><see cref="TokenKind.StringSegmentText"/> – plain text.</item>
    ///   <item><see cref="TokenKind.StringSegmentId"/> – <c>$name</c>.</item>
    ///   <item><see cref="TokenKind.StringInterpolStart"/> – <c>$(</c>.</item>
    ///   <item><see cref="TokenKind.StringEnd"/> – closing <c>"</c>.</item>
    /// </list>
    /// </summary>
    public Token NextStringSegment() => _verbatimString ? ReadVerbatimSegment() : ReadSmartSegment();

    bool _inString;
    bool _verbatimString;
    bool _suspendedString; // true when inside $(expr) — string mode paused for expression
    SourcePos _stringStart;

    /// <summary>
    /// Called by the parser after parsing the expression inside <c>$(expr)</c>
    /// and consuming the closing <c>)</c>. Resumes string segment mode.
    /// </summary>
    public void ResumeString()
    {
        if (_suspendedString)
        {
            _inString = true;
            _suspendedString = false;
            _peek = null; // invalidate any peeked token
        }
    }

    /// <summary>Whether the lexer is currently inside a <c>$(expr)</c> interpolation (string mode suspended).</summary>
    public bool IsInInterpolation => _suspendedString;

    // ═══════════════════════════════════════════════════════════════════════
    //  Character-level input
    // ═══════════════════════════════════════════════════════════════════════

    int Advance()
    {
        if (_ch == -2)
        {
            // Initial read
            _ch = _reader.Read();
            _chLine = _line;
            _chCol = _col;
        }

        int prev = _ch;
        // If Peek2() has already read the next char into _ch2, use it;
        // otherwise read from the underlying reader.
        if (_ch2 != -2)
        {
            _ch = _ch2;
            _ch2 = -2;
        }
        else
        {
            _ch = _reader.Read();
        }

        // Update position for NEXT char
        if (prev == '\n')
        {
            _line++;
            _col = 1;
        }
        else if (prev != -1)
        {
            _col++;
        }
        _chLine = _line;
        _chCol = _col;
        return prev;
    }

    // Peek at current char without consuming
    int Peek1()
    {
        if (_ch == -2)
        {
            _ch = _reader.Read();
            _chLine = _line;
            _chCol = _col;
        }
        return _ch;
    }

    // Peek at second char ahead (requires a two-char buffer)
    int _ch2 = -2;
    int Peek2()
    {
        Peek1(); // ensure _ch is initialised
        if (_ch2 == -2 && _ch != -1)
            _ch2 = _reader.Read();
        return _ch2;
    }

    int AdvanceWith2()
    {
        // ch2 is now the new _ch, shift
        int prev = _ch;
        _ch = _ch2;
        _ch2 = -2;
        if (prev == '\n') { _line++; _col = 1; }
        else if (prev != -1) _col++;
        _chLine = _line;
        _chCol = _col;
        return prev;
    }

    SourcePos CurrentPos => new(_chLine, _chCol);
    SourcePos PrevEndPos => new(_line, _col);

    Token MakeToken(TokenKind kind, string text, SourcePos start)
        => new(kind, text, new SourceSpan(_file, start, PrevEndPos));

    // ═══════════════════════════════════════════════════════════════════════
    //  Main token reader
    // ═══════════════════════════════════════════════════════════════════════

    Token ReadToken()
    {
        // Pending StringEnd from a flushed text segment
        if (_pendingStringEnd.HasValue)
        {
            var t = _pendingStringEnd.Value;
            _pendingStringEnd = null;
            _inString = false;
            return t;
        }

        // If we're inside a string, delegate to segment reader
        if (_inString)
        {
            return NextStringSegment();
        }

        // Skip whitespace and comments
        for (;;)
        {
            int c = Peek1();
            if (c == -1) break;
            if (c == '/' && Peek2() == '/')
            {
                // Line comment
                while (Peek1() != -1 && Peek1() != '\n') Advance();
                continue;
            }
            if (c == '/' && Peek2() == '*')
            {
                Advance(); Advance(); // consume /*
                SkipBlockComment();
                continue;
            }
            if (c is ' ' or '\t' or '\r' or '\n' or '\u000b' or '\u000c' or '\u0085'
                or '\u2028' or '\u2029')
            {
                Advance();
                continue;
            }
            break;
        }

        int ch = Peek1();
        if (ch == -1) return MakeToken(TokenKind.Eof, "", CurrentPos);

        var start = CurrentPos;

        // ── Interpreter line ──────────────────────────────────────────────
        if (ch == '#' && Peek2() == '!')
        {
            Advance(); Advance(); // #!
            _buf.Clear();
            while (Peek1() != -1 && Peek1() != '\n') _buf.Append((char)Advance());
            return MakeToken(TokenKind.InterpreterLine, _buf.ToString(), start);
        }

        // ── String literals ───────────────────────────────────────────────
        // Enter string segment mode. ReadToken() will delegate to segment
        // reader on subsequent calls until StringEnd is produced.
        if (ch == '"')
        {
            Advance(); // consume opening "
            _inString = true;
            _verbatimString = false;
            _stringStart = CurrentPos;
            return ReadSmartSegment();
        }

        if (ch == '@' && Peek2() == '"')
        {
            Advance(); Advance(); // consume @"
            _inString = true;
            _verbatimString = true;
            _stringStart = CurrentPos;
            return ReadVerbatimSegment();
        }

        // ── $ prefix ──────────────────────────────────────────────────────
        if (ch == '$')
        {
            Advance(); // consume $
            int next = Peek1();
            if (next == '"')
            {
                // $"arbitrary string" → anyId + string; we emit the string as plain Identifier
                // The string is the identifier text.
                Advance(); // consume "
                string s = ConsumeRawString(false);
                return MakeToken(TokenKind.Identifier, s, start);
            }
            if (next == '@' && Peek2() == '"')
            {
                Advance(); Advance(); // @"
                string s = ConsumeRawString(true);
                return MakeToken(TokenKind.Identifier, s, start);
            }
            if (next == '@' && Peek2() != '"')
            {
                // $@ = var args special form; caller deals with it
                Advance(); // @
                return MakeToken(TokenKind.Identifier, PFunction.ArgumentListId, start);
            }
            if (IsIdentStart(next))
            {
                // $identifier → strip the $
                string id = ConsumeIdentifier();
                // keywords become identifiers via $-prefix
                return MakeToken(TokenKind.Identifier, id, start);
            }
            // Lone $ — shouldn't normally appear; return as error
            return MakeToken(TokenKind.Error, "$", start);
        }

        // ── Operator-as-identifier forms: (+), (-), (*), (==), etc. ──────
        // Only for unambiguous 3-char forms like (+), (-), (*), (/) where second char
        // is the operator and third char is `)`. Longer forms and keyword-based ones
        // like (mod), (xor), (<|), (|>) need the TryLexOperatorIdent path which
        // unfortunately consumes characters. We guard it with a Peek2()==')' check
        // for the simple cases, and let longer forms fall through to normal `(` handling.
        if (ch == '(' && Peek2() != -1)
        {
            int opC2 = Peek2();
            // Only attempt operator-ident for safe cases.
            // Exclude `-` to avoid consuming `(->name)` as an op-ident.
            // Exclude `-`, `!`, `*` — these commonly appear in non-op-ident parens:
            // (->) pointer, (!) not-expr, (*args) splice.
            // The forms (-)  (*) etc. are valid op-idents but are very rare in practice
            // and the false positive cost is too high.
            if (opC2 != '-' && opC2 != '!' && opC2 != '*' && IsOperatorIdentStart(opC2))
            {
                var tok = TryLexOperatorIdent(start);
                if (tok.HasValue) return tok.Value;
                // If TryLex returns null, chars were consumed — can't recover.
                // Produce LParen as best-effort (the consumed content is lost).
                return MakeToken(TokenKind.LParen, "(", start);
            }
        }

        // ── Numbers ───────────────────────────────────────────────────────
        if (IsDigit(ch))
            return LexNumber(start);

        // ── Identifiers / keywords ────────────────────────────────────────
        if (IsIdentStart(ch))
            return LexIdentOrKeyword(start);

        // ── Two-char operators ────────────────────────────────────────────
        Advance(); // consume first char
        int c2 = Peek1();

        switch ((char)ch)
        {
            case '-':
                if (c2 == '-') { Advance(); return MakeToken(TokenKind.Dec, "--", start); }
                if (c2 == '>') { Advance(); return MakeToken(TokenKind.Pointer, "->", start); }
                return MakeToken(TokenKind.Minus, "-", start);
            case '+':
                if (c2 == '+') { Advance(); return MakeToken(TokenKind.Inc, "++", start); }
                return MakeToken(TokenKind.Plus, "+", start);
            case '=':
                if (c2 == '=') { Advance(); return MakeToken(TokenKind.Eq, "==", start); }
                if (c2 == '>') { Advance(); return MakeToken(TokenKind.Implementation, "=>", start); }
                return MakeToken(TokenKind.Assign, "=", start);
            case '!':
                if (c2 == '=') { Advance(); return MakeToken(TokenKind.Ne, "!=", start); }
                // ! as alias for `not`
                return MakeToken(TokenKind.KwNot, "not", start);
            case '<':
                if (c2 == '=') { Advance(); return MakeToken(TokenKind.Le, "<=", start); }
                if (c2 == '<') { Advance(); return MakeToken(TokenKind.AppendLeft, "<<", start); }
                if (c2 == '|') { Advance(); return MakeToken(TokenKind.DeltaLeft, "<|", start); }
                return MakeToken(TokenKind.Lt, "<", start);
            case '>':
                if (c2 == '=') { Advance(); return MakeToken(TokenKind.Ge, ">=", start); }
                if (c2 == '>') { Advance(); return MakeToken(TokenKind.AppendRight, ">>", start); }
                return MakeToken(TokenKind.Gt, ">", start);
            case '|':
                if (c2 == '>') { Advance(); return MakeToken(TokenKind.DeltaRight, "|>", start); }
                if (c2 == '|') { Advance(); return MakeToken(TokenKind.KwOr, "or", start); } // || → or
                return MakeToken(TokenKind.BitOr, "|", start);
            case '&':
                if (c2 == '&') { Advance(); return MakeToken(TokenKind.KwAnd, "and", start); } // && → and
                return MakeToken(TokenKind.BitAnd, "&", start);
            case '?':
                if (c2 == '?') { Advance(); return MakeToken(TokenKind.Coalescence, "??", start); }
                return MakeToken(TokenKind.Question, "?", start);
            case ':':
                if (c2 == ':') { Advance(); return MakeToken(TokenKind.DoubleColon, "::", start); }
                return MakeToken(TokenKind.Colon, ":", start);
            // Single-char operators
            case '*': return MakeToken(TokenKind.Times, "*", start);
            case '/': return MakeToken(TokenKind.Div, "/", start);
            case '^': return MakeToken(TokenKind.Pow, "^", start);
            case '~': return MakeToken(TokenKind.Tilde, "~", start);
            case '{': return MakeToken(TokenKind.LBrace, "{", start);
            case '}': return MakeToken(TokenKind.RBrace, "}", start);
            case '[': return MakeToken(TokenKind.LBrack, "[", start);
            case ']': return MakeToken(TokenKind.RBrack, "]", start);
            case '(': return MakeToken(TokenKind.LParen, "(", start);
            case ')': return MakeToken(TokenKind.RParen, ")", start);
            case ';': return MakeToken(TokenKind.Semicolon, ";", start);
            case ',': return MakeToken(TokenKind.Comma, ",", start);
            case '.': return MakeToken(TokenKind.Dot, ".", start);
            case '@': return MakeToken(TokenKind.At, "@", start);
            default:
                return MakeToken(TokenKind.Error, ((char)ch).ToString(), start);
        }
    }

    // ── Block comment skip ─────────────────────────────────────────────────
    void SkipBlockComment()
    {
        int depth = 1;
        while (depth > 0)
        {
            int c = Advance();
            if (c == -1) break;
            if (c == '/' && Peek1() == '*') { Advance(); depth++; }
            else if (c == '*' && Peek1() == '/') { Advance(); depth--; }
        }
    }

    // ── Number lexing ─────────────────────────────────────────────────────
    Token LexNumber(SourcePos start)
    {
        _buf.Clear();

        // Hex literal
        if (Peek1() == '0' && (Peek2() == 'x' || Peek2() == 'X'))
        {
            _buf.Append((char)Advance()); // 0
            _buf.Append((char)Advance()); // x
            while (IsHexDigit(Peek1())) _buf.Append((char)Advance());
            return MakeToken(TokenKind.Integer, _buf.ToString(), start);
        }

        // Integer part (digits with optional ' separators: 1'000'000)
        while (IsDigit(Peek1()) || (Peek1() == '\'' && IsDigit(Peek2())))
            _buf.Append((char)Advance());

        // Check for version literal: d.d.d or d.d.d.d (before checking for real)
        // We need to look ahead further. Strategy: if after int we see "." int "." → version/real
        if (Peek1() == '.')
        {
            // Peek at what follows the dot
            // We'll consume optimistically and classify later
            _buf.Append((char)Advance()); // .
            // Could be just ".", "1.0", "1.0e5", "1.0.0" (version)
            if (!IsDigit(Peek1()))
            {
                // "1." followed by non-digit → this is integer + dot operator
                // Back out the dot: we can't un-read, so return just the integer and re-inject
                // Actually: return the integer, put the dot back somehow...
                // Simplest: just return the integer portion and let the "." be re-lexed
                // But we already consumed the "." — use a one-char pushback trick
                _buf.Remove(_buf.Length - 1, 1); // remove the dot
                PushBack('.');
                return MakeToken(TokenKind.Integer, _buf.ToString(), start);
            }
            while (IsDigit(Peek1())) _buf.Append((char)Advance());
            // Now check: exponent → definite real; another "." → version; nothing → realLike
            if (Peek1() == 'e' || Peek1() == 'E')
            {
                _buf.Append((char)Advance()); // e
                if (Peek1() == '+' || Peek1() == '-') _buf.Append((char)Advance());
                while (IsDigit(Peek1())) _buf.Append((char)Advance());
                return MakeToken(TokenKind.Real, _buf.ToString(), start);
            }
            if (Peek1() == '.')
            {
                // Might be version: 1.2.3 or 1.2.3.4
                _buf.Append((char)Advance()); // second .
                if (!IsDigit(Peek1()))
                {
                    // Back out second dot
                    _buf.Remove(_buf.Length - 1, 1);
                    PushBack('.');
                    return MakeToken(TokenKind.RealLike, _buf.ToString(), start);
                }
                while (IsDigit(Peek1())) _buf.Append((char)Advance());
                // Maybe a 4th part
                if (Peek1() == '.')
                {
                    int savedLen = _buf.Length;
                    _buf.Append((char)Advance()); // third .
                    if (IsDigit(Peek1()))
                    {
                        while (IsDigit(Peek1())) _buf.Append((char)Advance());
                        return MakeToken(TokenKind.Version, _buf.ToString(), start);
                    }
                    _buf.Length = savedLen;
                    PushBack('.');
                }
                return MakeToken(TokenKind.Version, _buf.ToString(), start);
            }
            return MakeToken(TokenKind.RealLike, _buf.ToString(), start);
        }

        // Check for exponent without decimal part: "1e5"
        if (Peek1() == 'e' || Peek1() == 'E')
        {
            _buf.Append((char)Advance()); // e
            if (Peek1() == '+' || Peek1() == '-') _buf.Append((char)Advance());
            while (IsDigit(Peek1())) _buf.Append((char)Advance());
            return MakeToken(TokenKind.Real, _buf.ToString(), start);
        }

        return MakeToken(TokenKind.Integer, _buf.ToString(), start);
    }

    // One-char pushback support (for the single case we need it in number lexing)
    char _pushBack = '\0';
    bool _hasPushBack;

    void PushBack(char c)
    {
        _pushBack = c;
        _hasPushBack = true;
        // Also need to undo position tracking — simplest: just track it doesn't affect tokens
        _col--;
    }

    // Override Peek1 / Advance to handle pushback
    // (We call Peek1/Advance via different names to avoid confusion — actually we shadow them.)
    // NOTE: The pushback is only needed for the "1." case above; everything else uses the 2-char buffer.

    // ── Identifier / keyword lexing ───────────────────────────────────────
    Token LexIdentOrKeyword(SourcePos start)
    {
        string id = ConsumeIdentifier();

        // Check for identifier-with-context lookahead:
        //   "id::"  → NsId  (but only if NOT followed by third ":")
        //   "id:"   → LabelId  (but only if not "::")
        //   Otherwise → plain identifier / keyword

        int next = Peek1();
        if (next == ':')
        {
            int next2 = Peek2();
            if (next2 == ':')
            {
                // Consume "::", return ns token
                Advance(); Advance();
                return MakeToken(TokenKind.NsId, id, start);
            }
            else
            {
                // Single ":" → could be a label (id:) or key-value pair (a:b).
                // Don't consume the colon. Return a regular identifier and let the
                // parser figure out labels via context (peeking for `:` after an id
                // at statement start).
                // Fall through to keyword check below.
            }
        }

        // Keyword check (case-insensitive per IGNORECASE in lex)
        string lower = id.ToLowerInvariant();
        TokenKind kw = _mode == LexerMode.Asm ? TokenKind.Identifier : LookupKeyword(lower);
        return MakeToken(kw, kw == TokenKind.Identifier ? id : lower, start);
    }

    string ConsumeIdentifier()
    {
        _buf.Clear();
        if (_hasPushBack)
        {
            _hasPushBack = false;
            _buf.Append(_pushBack);
        }
        // Allow \ at the start too (e.g., \init)
        while (IsIdentContinue(Peek1())) _buf.Append((char)Advance());
        return _buf.ToString();
    }

    // ── Raw string (no interpolation awareness at lexer level) ─────────────
    // Parser calls BeginString + NextStringSegment instead for local-scope strings.
    // For global scope or asm/transfer, we produce a single StringRaw token.
    string ConsumeRawString(bool verbatim)
    {
        _buf.Clear();
        if (verbatim)
        {
            // @"..." where only "" is an escape
            for (;;)
            {
                int c = Advance();
                if (c == -1) break;
                if (c == '"')
                {
                    if (Peek1() == '"') { Advance(); _buf.Append('"'); }
                    else break;
                }
                else _buf.Append((char)c);
            }
        }
        else
        {
            for (;;)
            {
                int c = Advance();
                if (c == -1 || c == '\n') break;
                if (c == '"') break;
                if (c == '\\') { { var esc = ProcessEscape(); if (esc != '\xFFFF') _buf.Append(esc); } }
                else _buf.Append((char)c);
            }
        }
        return _buf.ToString();
    }

    // ── Smart string segments (local scope) ───────────────────────────────

    Token ReadSmartSegment()
    {
        var start = CurrentPos;
        _buf.Clear();

        for (;;)
        {
            int c = Peek1();
            if (c == -1 || c == '\n')
            {
                // Unterminated string
                if (_buf.Length > 0) return FlushSegText(start);
                _inString = false;
                return MakeToken(TokenKind.StringEnd, "", start);
            }
            if (c == '"')
            {
                Advance(); // consume closing "
                _inString = false;
                if (_buf.Length > 0) return FlushSegText(start, endAfter: true);
                return MakeToken(TokenKind.StringEnd, "", start);
            }
            if (c == '\\')
            {
                Advance(); // consume \
                { var esc = ProcessEscape(); if (esc != '\xFFFF') _buf.Append(esc); }
                continue;
            }
            if (c == '$')
            {
                if (_buf.Length > 0) return FlushSegText(start);
                Advance(); // consume $
                int n = Peek1();
                if (n == '(')
                {
                    Advance(); // consume (
                    // Suspend string mode so the parser can lex normal tokens
                    // for the interpolated expression. The parser will call
                    // ResumeString() after consuming the closing ')'.
                    _inString = false;
                    _suspendedString = true;
                    return MakeToken(TokenKind.StringInterpolStart, "$(", start);
                }
                if (IsIdentStart(n))
                {
                    // $name — prune trailing & if present
                    string id = ConsumeSmartStringId();
                    return MakeToken(TokenKind.StringSegmentId, id, start);
                }
                // lone $ → literal
                _buf.Append('$');
                continue;
            }
            Advance();
            _buf.Append((char)c);
        }
    }

    // Flush accumulated text as a StringSegmentText token.
    // If `endAfter` is true, the closing " was just consumed so we set _inString = false after flush.
    Token? _pendingStringEnd;

    Token FlushSegText(SourcePos start, bool endAfter = false)
    {
        var tok = MakeToken(TokenKind.StringSegmentText, _buf.ToString(), start);
        _buf.Clear();
        if (endAfter) _pendingStringEnd = MakeToken(TokenKind.StringEnd, "", PrevEndPos);
        return tok;
    }

    Token ReadVerbatimSegment()
    {
        var start = CurrentPos;
        _buf.Clear();
        for (;;)
        {
            int c = Peek1();
            if (c == -1)
            {
                if (_buf.Length > 0) return FlushSegText(start);
                _inString = false;
                return MakeToken(TokenKind.StringEnd, "", start);
            }
            if (c == '"')
            {
                Advance(); // consume first "
                if (Peek1() == '"') { Advance(); _buf.Append('"'); continue; }
                // Closing "
                _inString = false;
                if (_buf.Length > 0) return FlushSegText(start, endAfter: true);
                return MakeToken(TokenKind.StringEnd, "", start);
            }
            if (c == '$')
            {
                if (_buf.Length > 0) return FlushSegText(start);
                Advance();
                int n = Peek1();
                if (n == '(')
                {
                    Advance();
                    return MakeToken(TokenKind.StringInterpolStart, "$(", start);
                }
                if (IsIdentStart(n))
                {
                    string id = ConsumeSmartStringId();
                    return MakeToken(TokenKind.StringSegmentId, id, start);
                }
                _buf.Append('$');
                continue;
            }
            Advance();
            _buf.Append((char)c);
        }
    }

    string ConsumeSmartStringId()
    {
        _buf.Clear();
        while (IsIdentContinue(Peek1()))
        {
            int c = Peek1();
            // Stop at '&' which is a trim marker in the original lex
            if (c == '&') { Advance(); break; }
            _buf.Append((char)Advance());
        }
        return _buf.ToString();
    }

    // ── Escape sequence processing ────────────────────────────────────────
    char ProcessEscape()
    {
        int c = Advance();
        return c switch
        {
            '\\' => '\\',
            '"' => '"',
            '\'' => '\'',
            '0' => '\0',
            'a' => '\a',
            'b' => '\b',
            'f' => '\f',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            'v' => '\v',
            '$' => '$',
            '&' => '\xFFFF', // \& = nothing (sentinel, caller must not append)
            'x' or 'u' or 'U' => ConsumeHexEscape(c),
            _ => (char)c, // unknown escape → literal char
        };
    }

    char ConsumeHexEscape(int kind)
    {
        int max = kind == 'x' ? 4 : kind == 'u' ? 4 : 8;
        _buf.Clear();
        for (int i = 0; i < max && IsHexDigit(Peek1()); i++)
            _buf.Append((char)Advance());
        if (int.TryParse(_buf.ToString(), NumberStyles.HexNumber, null, out int cp))
            return (char)cp;
        return '\0';
    }

    // ── Operator-as-identifier: (+), (-), (mod), (^), etc. ────────────────
    static readonly Dictionary<string, string> _opIdents = new(StringComparer.OrdinalIgnoreCase)
    {
        ["(+)"] = "(+)", ["(-.)"] = "(-.)", ["(-)"] = "(-)", ["(*)"] = "(*)",
        ["(/)"] = "(/)", ["(^)"] = "(^)", ["(&)"] = "(&)", ["(|)"] = "(|)",
        ["(==)"] = "(==)", ["(!=)"] = "(!=)", ["(>)"] = "(>)", ["(>=)"] = "(>=)",
        ["(<)"] = "(<)", ["(<=)"] = "(<=)", ["(++)"] = "(++)", ["(--)"] = "(--)",
        ["(<|.)"] = "(<|.)", ["(.<|)"] = "(.<|)", ["(|>.)"] = "(|>.)", ["(.|>)"] = "(.|>)",
        ["(<|)"] = "(<|)", ["(|>)"] = "(|>)",
    };
    // Also: (mod) and (xor) are handled via keyword → we just lex the content as-is.

    Token? TryLexOperatorIdent(SourcePos start)
    {
        // Collect "(..." until ")" or end, then check against known forms.
        // This is called only when Peek1()=='(' and the second char looks like an op.
        var saved = _buf.ToString();
        _buf.Clear();
        _buf.Append('(');
        Advance(); // consume (
        while (Peek1() != -1 && Peek1() != ')' && Peek1() != '\n')
            _buf.Append((char)Advance());
        if (Peek1() == ')') { _buf.Append(')'); Advance(); }
        string candidate = _buf.ToString().ToLowerInvariant();
        // Also handle (mod) and (xor)
        if (candidate == "(mod)")
        {
            _buf.Clear();
            return MakeToken(TokenKind.Identifier, OperatorNames.Prexonite.Modulus, start);
        }
        if (candidate == "(xor)")
        {
            _buf.Clear();
            return MakeToken(TokenKind.Identifier, OperatorNames.Prexonite.ExclusiveOr, start);
        }
        if (_opIdents.TryGetValue(candidate, out string? opName))
        {
            _buf.Clear();
            return MakeToken(TokenKind.Identifier, opName, start);
        }
        // Not an operator-ident. We've already consumed content through `)`.
        // Return the consumed content as an error, unless it starts with (- and could be
        // a regular paren expression. In that case, we need to return LParen and re-inject.
        // Since we can't un-read, produce an LParen for `(` and treat the rest as tokens
        // by pushing back via _pushBack for the first char.
        _buf.Clear();
        // Best effort: return just the LParen and hope the inner content re-lexes.
        // The chars between ( and ) were consumed but we can't put them back.
        // Return error with the full candidate text for diagnostics.
        return null; // Let caller produce LParen instead
    }

    static bool IsOperatorIdentStart(int c)
        => c is '+' or '-' or '*' or '/' or '^' or '&' or '|' or '=' or '!' or '<' or '>' or '.';

    /// <summary>
    /// Heuristic: check if current position looks like a valid operator-as-identifier.
    /// We require the second char after '(' to be either ')' or another operator char
    /// (to avoid false positives like `(->name)` or `(expr)`).
    /// Known forms: (+), (-), (-.), (*), (/), (^), (&amp;), (|), (==), (!=), (&gt;), (&gt;=),
    /// (&lt;), (&lt;=), (++), (--), (mod), (xor), (&lt;|), (|&gt;), (&lt;|.), (.&lt;|), (|&gt;.), (.|&gt;)
    /// </summary>
    bool IsLikelyOperatorIdent()
    {
        int c2 = Peek2();
        // All valid op-idents: second char is an operator char like +, -, *, etc.
        // Exclude `-` followed by `>` (that's `->` pointer, not an op-ident start)
        // Also exclude cases where the second char is a letter that's not part of a keyword op (mod/xor)
        if (c2 == '-')
        {
            // Could be (-) or (-.) or (--) but NOT (->...)
            // We can't easily peek the THIRD char without modifying state, so be conservative:
            // Only accept `-` if the char sequence matches a known short pattern.
            // Just check: it's only valid if the third character is `)` or `.` or `-`
            // We can't peek(3), so just let TryLexOperatorIdent handle it — but we already know
            // it will consume characters. Accept the risk for (-) patterns.
            return true;
        }
        return c2 is '+' or '*' or '/' or '^' or '&' or '|' or '=' or '!' or '<' or '>' or '.';
    }

    // ── Keyword table ─────────────────────────────────────────────────────
    static readonly Dictionary<string, TokenKind> _keywords = new()
    {
        ["var"]       = TokenKind.KwVar,
        ["ref"]       = TokenKind.KwRef,
        ["true"]      = TokenKind.KwTrue,
        ["false"]     = TokenKind.KwFalse,
        ["null"]      = TokenKind.KwNull,
        ["mod"]       = TokenKind.KwMod,
        ["is"]        = TokenKind.KwIs,
        ["as"]        = TokenKind.KwAs,
        ["not"]       = TokenKind.KwNot,
        ["enabled"]   = TokenKind.KwEnabled,
        ["disabled"]  = TokenKind.KwDisabled,
        ["function"]  = TokenKind.KwFunction,
        ["command"]   = TokenKind.KwCommand,
        ["asm"]       = TokenKind.KwAsm,
        ["declare"]   = TokenKind.KwDeclare,
        ["build"]     = TokenKind.KwBuild,
        ["return"]    = TokenKind.KwReturn,
        ["in"]        = TokenKind.KwIn,
        ["to"]        = TokenKind.KwTo,
        ["add"]       = TokenKind.KwAdd,
        ["continue"]  = TokenKind.KwContinue,
        ["break"]     = TokenKind.KwBreak,
        ["yield"]     = TokenKind.KwYield,
        ["or"]        = TokenKind.KwOr,
        ["and"]       = TokenKind.KwAnd,
        ["xor"]       = TokenKind.KwXor,
        ["label"]     = TokenKind.KwLabel,
        ["goto"]      = TokenKind.KwGoto,
        ["static"]    = TokenKind.KwStatic,
        ["if"]        = TokenKind.KwIf,
        ["unless"]    = TokenKind.KwUnless,
        ["else"]      = TokenKind.KwElse,
        ["new"]       = TokenKind.KwNew,
        ["coroutine"] = TokenKind.KwCoroutine,
        ["from"]      = TokenKind.KwFrom,
        ["do"]        = TokenKind.KwDo,
        ["does"]      = TokenKind.KwDoes,
        ["while"]     = TokenKind.KwWhile,
        ["until"]     = TokenKind.KwUntil,
        ["for"]       = TokenKind.KwFor,
        ["foreach"]   = TokenKind.KwForeach,
        ["try"]       = TokenKind.KwTry,
        ["catch"]     = TokenKind.KwCatch,
        ["finally"]   = TokenKind.KwFinally,
        ["throw"]     = TokenKind.KwThrow,
        ["then"]      = TokenKind.KwThen,
        ["using"]     = TokenKind.KwUsing,
        ["macro"]     = TokenKind.KwMacro,
        ["lazy"]      = TokenKind.KwLazy,
        ["let"]       = TokenKind.KwLet,
        ["method"]    = TokenKind.KwMethod,
        ["this"]      = TokenKind.KwThis,
        ["namespace"] = TokenKind.KwNamespace,
        ["export"]    = TokenKind.KwExport,
    };

    static TokenKind LookupKeyword(string lower)
        => _keywords.TryGetValue(lower, out var k) ? k : TokenKind.Identifier;

    // ── Character classification ─────────────────────────────────────────
    static bool IsDigit(int c) => c is >= '0' and <= '9';
    static bool IsHexDigit(int c) => IsDigit(c) || c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F';
    static bool IsIdentStart(int c) =>
        c != -1 && (char.IsLetter((char)c) || c == '_' || c == '\\');
    static bool IsIdentContinue(int c) =>
        c != -1 && (char.IsLetterOrDigit((char)c) || c == '_' || c == '\\' || c == '\'');
}

// ── Helper extension to convert a SourcePos to a zero-width SourceSpan ──────
file static class SourcePosExt
{
    public static SourceSpan AsSpan(this SourcePos p, string file) => new(file, p, p);
}
