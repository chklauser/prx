/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Prexonite;
using Prexonite.Compiler;
using Prexonite.Helper;

internal partial class Lexer
{
    private readonly StringBuilder buffer = new StringBuilder();

    private Token tok(int kind)
    {
        return tok(kind, yytext());
    }

    private Token tok(int kind, string val)
    {
        var t = new Token
        {
            kind = kind,
            val = val,
            pos = yychar,
            line = yyline,
            col = yycolumn
        };

        return t;
    }

    private readonly Stack<int> yystates = new Stack<int>();

    public void PushState(int state)
    {
        yystates.Push(yystate());
        yybegin(state);
    }

    public void PopState()
    {
        var state = yystates.Count > 0 ? yystates.Pop() : YYINITIAL;
        yybegin(state);
    }

    private string _file = "--unknown--";

    public string File
    {
        get { return _file; }
        set { _file = value; }
    }

    string IScanner.File
    {
        get { return _file; }
    }

    #region fake reference to Lexer.zzAtBOL (too lazy to hack into scanner generator)

    private void _fake1()
    {
        if(!zzAtBOL)
            _fake2();
    }

    private void _fake2()
    {
        if(zzAtBOL)
            _fake1();
    }

    #endregion

    private readonly RandomAccessQueue<Token> _tokenBuffer = new RandomAccessQueue<Token>();
    private int _peekIndex = NO_PEEK;
    private const int NO_PEEK = -1;

    internal void _InjectToken(Token c)
    {
        if (_tokenBuffer.Count == 0)
            _tokenBuffer.Add(c);
        else
            _tokenBuffer.Insert(0, c);
    }

    private Token multiple(params Token[] tokens)
    {
        if (tokens == null)
            throw new ArgumentNullException("tokens");
        if (tokens.Length == 0)
            throw new ArgumentException("Must at least return one token.");

        foreach (var token in tokens)
            _tokenBuffer.Enqueue(token);

        return null;
    }

    private void ret(params Token[] tokens)
    {
        multiple(tokens);
    }

    private void scanNextToken()
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

        var isGlobal = yystate() == YYINITIAL;
        var isLocal = yystate() == Local;

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
            }

        //Global only
        if (isGlobal)
            switch (word)
            {
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
                    //case "to": //Parsed by the scanner.
            }

            //Local only
        else if (isLocal)
            switch (word)
            {
                case "static":
                    return Parser._static;
                case "return":
                    return (Parser._return);
                case "yield":
                    return (Parser._yield);
                case "in":
                    return (Parser._in);
                case "continue":
                    return (Parser._continue);
                case "break":
                    return (Parser._break);
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
                case "null":
                    return Parser._null;
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
                case "using": //Coco/R does not accept "using" as a token name.
                    return Parser._uusing;
            }

        //Is id
        return Parser._id;
    }

    private string unescape_char(string sequence)
    {
        var kind = sequence.Substring(1, 1);
        sequence = sequence.Substring(2);
        int utf32;
        if (
            int.TryParse(
                "0x" + sequence, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out utf32))
            throw new PrexoniteException(
                System.String.Format(
                    "Invalid escape sequence \\{3}{0} on line {1} in {2}.",
                    sequence,
                    yyline,
                    File,
                    kind));
        return char.ConvertFromUtf32(utf32);
    }

    void IScanner.ResetPeek()
    {
        //Try to confuse Peek() so it does no longer know where to start... *muahaha*
        _peekIndex = NO_PEEK;
    }
}