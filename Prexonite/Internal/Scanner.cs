// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;
    //Added for compatibility reasons
using FatalError = Prexonite.Compiler.FatalCompilerException;

namespace Prexonite.Internal
{
    internal class Token
    {
        public int kind; // token kind
        public int pos; // token position in the source text (starting at 0)
        public int col; // token column (starting at 0)
        public int line; // token line (starting at 1)
        public string val; // token value
        public Token next; // ML 2005-03-11 Tokens are kept in linked list

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool includePosition)
        {
            return String.Format("({0})~{1}" + (includePosition ? "/line:{2}/col:{3}" : ""), val,
                Enum.GetName(typeof (Parser.Terminals), (Parser.Terminals) kind), line, col);
        }
    }

    //-----------------------------------------------------------------------------------
    // Buffer
    //-----------------------------------------------------------------------------------
    [NoDebug]
    internal class Buffer : IDisposable
    {
        public const int EOF = char.MaxValue + 1;
        private const int MAX_BUFFER_LENGTH = 64*1024; // 64KB
        private byte[] buf; // input buffer
        private int bufStart; // position of first byte in buffer relative to input stream
        private int bufLen; // length of buffer
        private int fileLen; // length of input stream
        private int pos; // current position in buffer
        private Stream stream; // input stream (seekable)
        private bool isUserStream; // was the stream opened by the user?

        internal Buffer(Stream s, bool isUserStream)
        {
            stream = s;
            this.isUserStream = isUserStream;
            fileLen = bufLen = (int) s.Length;
            if (stream.CanSeek && bufLen > MAX_BUFFER_LENGTH) bufLen = MAX_BUFFER_LENGTH;
            buf = new byte[bufLen];
            bufStart = Int32.MaxValue; // nothing in the buffer so far
            Pos = 0; // setup buffer to position 0 (start)
            if (bufLen == fileLen) Close();
        }

        protected Buffer(Buffer b)
        {
            // called in UTF8Buffer constructor
            buf = b.buf;
            bufStart = b.bufStart;
            bufLen = b.bufLen;
            fileLen = b.fileLen;
            pos = b.pos;
            stream = b.stream;
            b.stream = null;
            isUserStream = b.isUserStream;
        }


        protected void Close()
        {
            Dispose();
        }

        internal virtual int Read()
        {
            if (pos < bufLen)
            {
                return buf[pos++];
            }
            else if (Pos < fileLen)
            {
                Pos = Pos; // shift buffer start to Pos
                return buf[pos++];
            }
            else
            {
                return EOF;
            }
        }

        internal int Peek()
        {
            var curPos = Pos;
            var ch = Read();
            Pos = curPos;
            return ch;
        }

        internal string GetString(int beg, int end)
        {
            var len = end - beg;
            var buf = new char[len];
            var oldPos = Pos;
            Pos = beg;
            for (var i = 0; i < len; i++) buf[i] = (char) Read();
            Pos = oldPos;
            return new String(buf);
        }

        internal int Pos
        {
            get { return pos + bufStart; }
            set
            {
                if (value < 0) value = 0;
                else if (value > fileLen) value = fileLen;
                if (value >= bufStart && value < bufStart + bufLen)
                {
                    // already in buffer
                    pos = value - bufStart;
                }
                else if (stream != null)
                {
                    // must be swapped in
                    stream.Seek(value, SeekOrigin.Begin);
                    bufLen = stream.Read(buf, 0, buf.Length);
                    bufStart = value;
                    pos = 0;
                }
                else
                {
                    pos = fileLen - bufStart; // make Pos return fileLen
                }
            }
        }

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && (!isUserStream) && stream != null)
                {
                    stream.Close();
                }
            }
            disposed = true;
        }

        ~Buffer()
        {
            Dispose(false);
        }
    }

    //-----------------------------------------------------------------------------------
    // UTF8Buffer
    //-----------------------------------------------------------------------------------
    [NoDebug]
    internal class UTF8Buffer : Buffer
    {
        internal UTF8Buffer(Buffer b) : base(b)
        {
        }

        internal override int Read()
        {
            int ch;
            do
            {
                ch = base.Read();
                // until we find a uft8 start (0xxxxxxx or 11xxxxxx)
            } while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
            if (ch < 128 || ch == EOF)
            {
                // nothing to do, first 127 chars are the same in ascii and utf8
                // 0xxxxxxx or end of file character
            }
            else if ((ch & 0xF0) == 0xF0)
            {
                // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                var c1 = ch & 0x07;
                ch = base.Read();
                var c2 = ch & 0x3F;
                ch = base.Read();
                var c3 = ch & 0x3F;
                ch = base.Read();
                var c4 = ch & 0x3F;
                ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
            }
            else if ((ch & 0xE0) == 0xE0)
            {
                // 1110xxxx 10xxxxxx 10xxxxxx
                var c1 = ch & 0x0F;
                ch = base.Read();
                var c2 = ch & 0x3F;
                ch = base.Read();
                var c3 = ch & 0x3F;
                ch = (((c1 << 6) | c2) << 6) | c3;
            }
            else if ((ch & 0xC0) == 0xC0)
            {
                // 110xxxxx 10xxxxxx
                var c1 = ch & 0x1F;
                ch = base.Read();
                var c2 = ch & 0x3F;
                ch = (c1 << 6) | c2;
            }
            return ch;
        }
    }

    //-----------------------------------------------------------------------------------
    // Scanner
    //-----------------------------------------------------------------------------------
    [NoDebug]
    internal class Scanner : IDisposable, IScanner
    {
        private const char EOL = '\n';
        private const int eofSym = 0; /* pdt */
        private const int maxT = 11;
        private const int noSym = 11;
        private char valCh; // current input character (for token.val)

        internal Buffer buffer; // scanner buffer

        private Token t; // current token
        private int ch; // current input character
        private int pos; // byte position of current character
        private int col; // column number of current character
        private int line; // line number of current character
        private int oldEols; // EOLs that appeared in a comment;
        private Dictionary<int, int> start; // maps first token character to start state

        private Token tokens; // list of tokens already peeked (first token is a dummy)
        private Token pt; // current peek token

        private char[] tval = new char[128]; // text of current token
        private int tlen; // length of current token

        private string file = "unknown~";

        public string File
        {
            get { return file; }
        }

        internal Scanner(string fileName)
        {
            try
            {
                file = fileName;
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                buffer = new Buffer(stream, false);
                Init();
            }
            catch (IOException)
            {
                throw new FatalError("Cannot open file " + fileName);
            }
        }

        internal Scanner(Buffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            this.buffer = buffer;
            Init();
        }

        internal Scanner(Buffer buffer, string location) : this(buffer)
        {
            file = location;
        }

        internal static Scanner CreateFromString(string input)
        {
            var str = new MemoryStream(Encoding.UTF8.GetBytes(input));
            return new Scanner(new Buffer(str, false));
        }

        internal static Scanner CreateFromString(string input, string location)
        {
            var str = new MemoryStream(Encoding.UTF8.GetBytes(input));
            return new Scanner(new Buffer(str, false), location);
        }

        internal Scanner(Stream s)
        {
            buffer = new Buffer(s, true);
            Init();
        }

        internal Scanner(Stream s, string location) : this(s)
        {
            file = location;
        }

        private void Init()
        {
            pos = -1;
            line = 1;
            col = 0;
            oldEols = 0;
            NextCh();
            if (ch == 0xEF)
            {
                // check optional byte order mark for UTF-8
                NextCh();
                var ch1 = ch;
                NextCh();
                var ch2 = ch;
                if (ch1 != 0xBB || ch2 != 0xBF)
                {
                    throw new FatalError(String.Format(
                        "illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
                }
                buffer = new UTF8Buffer(buffer);
                col = 0;
                NextCh();
            }
            start = new Dictionary<int, int>(128);
            for (var i = 92; i <= 92; ++i) start[i] = 1;
            for (var i = 95; i <= 95; ++i) start[i] = 1;
            for (var i = 97; i <= 122; ++i) start[i] = 1;
            for (var i = 49; i <= 57; ++i) start[i] = 35;
            start[126] = 2;
            start[48] = 36;
            start[46] = 6;
            start[34] = 18;
            start[64] = 32;
            start[40] = 43;
            start[44] = 44;
            start[41] = 45;
            start[Buffer.EOF] = -1;

            pt = tokens = new Token(); // first token is a dummy
        }

        private void NextCh()
        {
            if (oldEols > 0)
            {
                ch = EOL;
                oldEols--;
            }
            else
            {
                pos = buffer.Pos;
                ch = buffer.Read();
                col++;
                // replace isolated '\r' by '\n' in order to make
                // eol handling uniform across Windows, Unix and Mac
                if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
                if (ch == EOL)
                {
                    line++;
                    col = 0;
                }
            }
            valCh = (char) ch;
            if (ch != Buffer.EOF) ch = char.ToLower((char) ch);
        }

        private void AddCh()
        {
            if (tlen >= tval.Length)
            {
                var newBuf = new char[2*tval.Length];
                Array.Copy(tval, 0, newBuf, 0, tval.Length);
                tval = newBuf;
            }
            tval[tlen++] = valCh;
            NextCh();
        }


        private void CheckLiteral()
        {
            switch (t.val.ToLower())
            {
                case "true":
                    t.kind = 3;
                    break;
                case "false":
                    t.kind = 4;
                    break;
                default:
                    break;
            }
        }

        private Token NextToken()
        {
            while (false || ch >= 9 && ch <= 10 || ch == 13) NextCh();

            var apx = 0;
            t = new Token();
            t.pos = pos;
            t.col = col;
            t.line = line;
            int state;
            try
            {
                state = start[ch];
            }
            catch (KeyNotFoundException)
            {
                state = 0;
            }
            tlen = 0;
            AddCh();

            switch (state)
            {
                case -1:
                    {
                        t.kind = eofSym;
                        break;
                    } // NextCh already done
                case 0:
                    {
                        t.kind = noSym;
                        break;
                    } // NextCh already done
                case 1:
                    if (ch >= '0' && ch <= '9' || ch == 92 || ch == '_' || ch >= 'a' && ch <= 'z')
                    {
                        AddCh();
                        goto case 1;
                    }
                    else
                    {
                        t.kind = 1;
                        t.val = new String(tval, 0, tlen);
                        CheckLiteral();
                        return t;
                    }
                case 2:
                    {
                        t.kind = 2;
                        break;
                    }
                case 3:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 4;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 4:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 4;
                    }
                    else
                    {
                        t.kind = 5;
                        break;
                    }
                case 5:
                    {
                        tlen -= apx;
                        buffer.Pos = t.pos;
                        NextCh();
                        line = t.line;
                        col = t.col;
                        for (var i = 0; i < tlen; i++) NextCh();
                        t.kind = 5;
                        break;
                    }
                case 6:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 7;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 7:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 7;
                    }
                    else if (ch == 'e')
                    {
                        AddCh();
                        goto case 8;
                    }
                    else
                    {
                        t.kind = 6;
                        break;
                    }
                case 8:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 10;
                    }
                    else if (ch == '+' || ch == '-')
                    {
                        AddCh();
                        goto case 9;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 9:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 10;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 10:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 10;
                    }
                    else
                    {
                        t.kind = 6;
                        break;
                    }
                case 11:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 11;
                    }
                    else if (ch == 'e')
                    {
                        AddCh();
                        goto case 12;
                    }
                    else
                    {
                        t.kind = 6;
                        break;
                    }
                case 12:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 14;
                    }
                    else if (ch == '+' || ch == '-')
                    {
                        AddCh();
                        goto case 13;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 13:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 14;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 14:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 14;
                    }
                    else
                    {
                        t.kind = 6;
                        break;
                    }
                case 15:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 17;
                    }
                    else if (ch == '+' || ch == '-')
                    {
                        AddCh();
                        goto case 16;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 16:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 17;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 17:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 17;
                    }
                    else
                    {
                        t.kind = 6;
                        break;
                    }
                case 18:
                    if (ch <= 9 || ch >= 11 && ch <= '!' || ch >= '#' && ch <= '[' ||
                        ch >= ']' && ch <= 65535)
                    {
                        AddCh();
                        goto case 18;
                    }
                    else if (ch == '"')
                    {
                        AddCh();
                        goto case 34;
                    }
                    else if (ch == 92)
                    {
                        AddCh();
                        goto case 37;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 19:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 20;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 20:
                    if (ch <= 9 || ch >= 11 && ch <= '!' || ch >= '#' && ch <= '/' ||
                        ch >= ':' && ch <= '[' || ch >= ']' && ch <= '`' || ch >= 'g' && ch <= 65535)
                    {
                        AddCh();
                        goto case 18;
                    }
                    else if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 38;
                    }
                    else if (ch == '"')
                    {
                        AddCh();
                        goto case 34;
                    }
                    else if (ch == 92)
                    {
                        AddCh();
                        goto case 37;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 21:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 22;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 22:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 23;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 23:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 18;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 24:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 25;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 25:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 26;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 26:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 27;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 27:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 28;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 28:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 29;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 29:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 30;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 30:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 31;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 31:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 18;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 32:
                    if (ch == '"')
                    {
                        AddCh();
                        goto case 33;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 33:
                    if (ch <= '!' || ch >= '#' && ch <= 65535)
                    {
                        AddCh();
                        goto case 33;
                    }
                    else if (ch == '"')
                    {
                        AddCh();
                        goto case 40;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 34:
                    {
                        t.kind = 7;
                        break;
                    }
                case 35:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 35;
                    }
                    else if (ch == '.')
                    {
                        apx++;
                        AddCh();
                        goto case 41;
                    }
                    else if (ch == 'e')
                    {
                        AddCh();
                        goto case 15;
                    }
                    else
                    {
                        t.kind = 5;
                        break;
                    }
                case 36:
                    if (ch >= '0' && ch <= '9')
                    {
                        AddCh();
                        goto case 35;
                    }
                    else if (ch == '.')
                    {
                        apx++;
                        AddCh();
                        goto case 41;
                    }
                    else if (ch == 'x')
                    {
                        AddCh();
                        goto case 3;
                    }
                    else if (ch == 'e')
                    {
                        AddCh();
                        goto case 15;
                    }
                    else
                    {
                        t.kind = 5;
                        break;
                    }
                case 37:
                    if (ch == '"' || ch == 39 || ch == '0' || ch == 92 || ch >= 'a' && ch <= 'b' ||
                        ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v')
                    {
                        AddCh();
                        goto case 18;
                    }
                    else if (ch == 'x')
                    {
                        AddCh();
                        goto case 19;
                    }
                    else if (ch == 'u')
                    {
                        AddCh();
                        goto case 42;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 38:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 39;
                    }
                    else if (ch <= 9 || ch >= 11 && ch <= '!' || ch >= '#' && ch <= '/' ||
                        ch >= ':' && ch <= '[' || ch >= ']' && ch <= '`' ||
                            ch >= 'g' && ch <= 65535)
                    {
                        AddCh();
                        goto case 18;
                    }
                    else if (ch == '"')
                    {
                        AddCh();
                        goto case 34;
                    }
                    else if (ch == 92)
                    {
                        AddCh();
                        goto case 37;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 39:
                    if (ch <= 9 || ch >= 11 && ch <= '!' || ch >= '#' && ch <= '[' ||
                        ch >= ']' && ch <= 65535)
                    {
                        AddCh();
                        goto case 18;
                    }
                    else if (ch == '"')
                    {
                        AddCh();
                        goto case 34;
                    }
                    else if (ch == 92)
                    {
                        AddCh();
                        goto case 37;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 40:
                    if (ch == '"')
                    {
                        AddCh();
                        goto case 33;
                    }
                    else
                    {
                        t.kind = 7;
                        break;
                    }
                case 41:
                    if (ch <= '/' || ch >= ':' && ch <= 65535)
                    {
                        apx++;
                        AddCh();
                        goto case 5;
                    }
                    else if (ch >= '0' && ch <= '9')
                    {
                        apx = 0;
                        AddCh();
                        goto case 11;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 42:
                    if (ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f')
                    {
                        AddCh();
                        goto case 21;
                    }
                    else if (ch == 'l')
                    {
                        AddCh();
                        goto case 24;
                    }
                    else
                    {
                        t.kind = noSym;
                        break;
                    }
                case 43:
                    {
                        t.kind = 8;
                        break;
                    }
                case 44:
                    {
                        t.kind = 9;
                        break;
                    }
                case 45:
                    {
                        t.kind = 10;
                        break;
                    }
            }
            t.val = new String(tval, 0, tlen);
            return t;
        }

        // get the next token (possibly a token already seen during peeking)
        public Token Scan()
        {
            if (tokens.next == null)
            {
                return NextToken();
            }
            else
            {
                pt = tokens = tokens.next;
                return tokens;
            }
        }

        // peek for the next token, ignore pragmas
        public Token Peek()
        {
            if (pt.next == null)
            {
                do
                {
                    pt = pt.next = NextToken();
                } while (pt.kind > maxT); // skip pragmas
            }
            else
            {
                do
                {
                    pt = pt.next;
                } while (pt.kind > maxT);
            }
            return pt;
        }

        // make sure that peeking starts at the current scan position
        public void ResetPeek()
        {
            pt = tokens;
        }

        //Dispose pattern
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && buffer != null)
                {
                    buffer.Dispose();
                }
            }
            disposed = true;
        }

        ~Scanner()
        {
            Dispose(false);
        }
    }

    // end Scanner
}