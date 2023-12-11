// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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

using System.Globalization;
using System.Text;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Internal;
using Prexonite.Properties;
using Parser = Prexonite.Compiler.Parser;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
partial class Lexer
// ReSharper restore CheckNamespace
{
    readonly StringBuilder buffer = new();

    Token tok(int kind)
    {
        return tok(kind, yytext());
    }

    Token tok(int kind, string val)
    {
        var t = new Token
            {
                kind = kind,
                val = val,
                pos = yychar,
                line = yyline,
                col = yycolumn,
            };

        return t;
    }

    readonly Stack<int> yystates = new();

    public void PushState(int state)
    {
        yystates.Push(yystate());
        yybegin(state);
    }

    int _surroundingLocalState
    {
        get
        {
            foreach (var state in yystates)
            {
                if (state == Local || state == LocalShell)
                {
                    return state;
                }
            }
            // Conservatively assume Local (instead of LocalShell)
            return Local;
        }
    }

    public void PopState()
    {
        var state = yystates.Count > 0 ? yystates.Pop() : YYINITIAL;
        yybegin(state);
    }

    public string File { get; set; } = "--unknown--";

    public void Abort()
    {
        yyclose();
    }

    string IScanner.File => File;

    #region fake reference to Lexer.zzAtBOL (too lazy to hack into scanner generator)

    void _fake1()
    {
        if (!zzAtBOL)
            _fake2();
    }

    void _fake2()
    {
        if (zzAtBOL)
            _fake1();
    }

    #endregion

    readonly RandomAccessQueue<Token> _tokenBuffer = new();
    int _peekIndex = NO_PEEK;

    const int NO_PEEK = -1;


    internal void _InjectToken(Token c)
    {
        if (_tokenBuffer.Count == 0)
            _tokenBuffer.Add(c);
        else
            _tokenBuffer.Insert(0, c);
    }

    Token? multiple(params Token?[] tokens)
    {
        if (tokens == null)
            throw new ArgumentNullException(nameof(tokens));
        if (tokens.Length == 0)
            throw new ArgumentException("Must at least return one token.");

        foreach (var token in tokens)
            if (token != null)
                _tokenBuffer.Enqueue(token);

        return null;
    }

    void ret(params Token[] tokens)
    {
        multiple(tokens);
    }

    void scanNextToken()
    {
        var count = _tokenBuffer.Count;
        var next = Scan();
        if (next == null)
            if (_tokenBuffer.Count == count)
                throw new FatalCompilerException("Invalid (null) token returned by lexer.");
            else
            {
                //Tokens got added by multiple()
            }
        else
            _tokenBuffer.Enqueue(next);
    }

    Token IScanner.Scan()
    {
        _peekIndex = NO_PEEK;
        if (_tokenBuffer.Count == 0)
            scanNextToken();
        return _tokenBuffer.Dequeue();
    }

    Token IScanner.Peek()
    {
        _peekIndex++;
        if (_peekIndex >= _tokenBuffer.Count)
            scanNextToken();
        return _tokenBuffer[_peekIndex];
    }

    public int checkKeyword(string word)
    {
        word = word.ToLowerInvariant();

        //Any lexer state
        switch (word)
        {
            case "false":
                return Parser._false;
            case "true":
                return Parser._true;
        }

        var current_state = yystate();
        var isGlobal = current_state == YYINITIAL || current_state == Transfer;
        var isLocal = current_state == Local || current_state == LocalShell;

        //Not assembler
        if (isGlobal || isLocal)
            switch (word)
            {
                case "as":
                    return Parser._as;
                case "asm":
                    return Parser._asm;
                case "command":
                    return Parser._command;
                case "coroutine":
                    return Parser._coroutine;
                case "function":
                    return Parser._function;
                case "method":
                    return Parser._method;
                case "declare":
                    return Parser._declare;
                case "is":
                    return Parser._is;
                case "not":
                    return Parser._not;
                case "macro":
                    return Parser._macro;
                case "lazy":
                    return Parser._lazy;
                case "let":
                    return Parser._let;
                case "namespace":
                    return Parser._namespace;
                case "null":
                    return Parser._null;
            }

        //Global only
        if (isGlobal)
            switch (word)
            {
                //case "to": //Parsed by the scanner.
                case "add":
                    return Parser._add;
                case "build":
                    return Parser._build;
                case "disabled":
                    return Parser._disabled;
                case "does":
                    return Parser._does;
                case "enabled":
                    return Parser._enabled;
                case "export":
                    return Parser._export;
            }

        //Local only
        else if (isLocal)
            switch (word)
            {
                case "static":
                    return Parser._static;
                case "return":
                    return Parser._return;
                case "yield":
                    return Parser._yield;
                case "in":
                    return Parser._in;
                case "continue":
                    return Parser._continue;
                case "break":
                    return Parser._break;
                case "mod":
                    return Parser._mod;
                case "or":
                    return Parser._or;
                case "and":
                    return Parser._and;
                case "xor":
                    return Parser._xor;
                case "goto":
                    return Parser._goto;
                case "if":
                    return Parser._if;
                case "unless":
                    return Parser._unless;
                case "else":
                    return Parser._else;
                case "new":
                    return Parser._new;
                    /* //Not currently used.
                case "from":
                    return Parser._from;
                //*/
                case "do":
                    return Parser._do;
                case "while":
                    return Parser._while;
                case "until":
                    return Parser._until;
                case "foreach":
                    return Parser._foreach;
                case "for":
                    return Parser._for;
                case "try":
                    return Parser._try;
                case "catch":
                    return Parser._catch;
                case "finally":
                    return Parser._finally;
                case "throw":
                    return Parser._throw;
                case "then":
                    return Parser._then;
                case "using": //Coco/R does not accept "using" as a token name.
                    return Parser._uusing;
                case "this":
                    return Parser._this;
            }

        //Is id
        return Parser._id;
    }

    string _unescapeChar(string sequence)
    {
        var kind = sequence.Substring(1, 1);
        sequence = sequence[2..];
        if (
            !int.TryParse(sequence, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var utf32))
            throw new PrexoniteException(
                string.Format(
                    "Invalid escape sequence \\{3}{0} on line {1} in {2}.",
                    sequence,
                    yyline,
                    File,
                    kind));
        try
        {
            return char.ConvertFromUtf32(utf32);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new PrexoniteException(
                "Failed to convert string escape sequence " + sequence +
                $" to string on line {yyline} in {File}", ex);
        }
    }

    /// <summary>
    ///     Takes a raw smart string identifier and splits any \& sequences into the buffer, 
    ///     returning the actual identifier. The part clipped off is returned seperately (excluding the \&)
    /// </summary>
    /// <param name = "rawIdentifier">a $identifier as it was recognized by the lexer (including the $).</param>
    /// <param name = "clipped">The part that was clipped off, excluding the \&</param>
    /// <returns>The actual identifier, ready to be used.</returns>
    string _pruneSmartStringIdentifier(string rawIdentifier, out string? clipped)
    {
        if (rawIdentifier.EndsWith("\\&"))
        {
            clipped = null;
            return rawIdentifier.Substring(1, rawIdentifier.Length - 3);
        }
        else
        {
            var hasAmp = rawIdentifier.EndsWith("&");
            clipped = hasAmp ? "&" : null;
            return rawIdentifier.Substring(1, rawIdentifier.Length - (hasAmp ? 2 : 1));
        }
    }

    void IScanner.ResetPeek()
    {
        //Try to confuse Peek() so it does no longer know where to start... *muahaha*
        _peekIndex = NO_PEEK;
    }
}
// ReSharper restore InconsistentNaming