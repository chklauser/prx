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
#line 15

#region Shared Source License

// The above license text has been added by an automated tool. 
//  However, for this particular file a different license is in effect:

/* **********************************************************************************
*
* Copyright (c) Microsoft Corporation. All rights reserved.
*
* This source code is subject to terms and conditions of the Shared Source License
* for IronPython. A copy of the license can be found in the License.html file
* at the root of this distribution. If you can not locate the Shared Source License
* for IronPython, please send an email to ironpy@microsoft.com.
* By using this source code in any fashion, you are agreeing to be bound by
* the terms of the Shared Source License for IronPython.
*
* You must not remove this notice, or any other, from this software.
*
* **********************************************************************************/

#endregion

/*
 * Adaption for use as a general purpose console wrapper.
 * Copyright of changes Christian Klauser
 * Changes are marked with (-P)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Prx.Win32;

//Removed references to IronPython (-P)

namespace Prx;

//made class abstract
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class SuperConsole
{
    public ConsoleColor PromptColor = Console.ForegroundColor;
    public ConsoleColor OutColor = Console.ForegroundColor;
    public ConsoleColor ErrorColor = Console.ForegroundColor;

    public void SetupColors()
    {
        PromptColor = ConsoleColor.DarkGray;
        OutColor = ConsoleColor.DarkBlue;
        ErrorColor = ConsoleColor.DarkRed;
    }

    /// <summary>
    ///     Class managing the command history.
    /// </summary>
    class History
    {
        List<string> list = new();
        int current;
        bool increment; // increment on Next()

        string Current => current >= 0 && current < list.Count ? list[current] : string.Empty;

        public void Add(string? line, bool setCurrentAsLast)
        {
            if (line != null && line.Length > 0)
            {
                var oldCount = list.Count;
                list.Add(line);
                if (setCurrentAsLast || current == oldCount)
                {
                    current = list.Count;
                }
                else
                {
                    current++;
                }
                // Do not increment on the immediately following Next()
                increment = false;
            }
        }

        public string Previous()
        {
            if (current > 0)
            {
                current--;
                increment = true;
            }
            return Current;
        }

        public string Next()
        {
            if (current + 1 < list.Count)
            {
                if (increment)
                    current++;
                increment = true;
            }
            return Current;
        }
    }

    /// <summary>
    ///     List of available options
    /// </summary>
    class SuperConsoleOptions
    {
        ArrayList list = new();
        int current;

        public int Count => list.Count;

        string? Current => current >= 0 && current < list.Count ? (string?) list[current] : string.Empty;

        public void Clear()
        {
            list.Clear();
            current = -1;
        }

        public void Add(string? line)
        {
            if (line != null && line.Length > 0)
            {
                list.Add(line);
            }
        }

        public string? Previous()
        {
            if (list.Count > 0)
            {
                current = (current - 1 + list.Count)%list.Count;
            }
            return Current;
        }

        public string? Next()
        {
            if (list.Count > 0)
            {
                current = (current + 1)%list.Count;
            }
            return Current;
        }

        public string Root { get; set; } = "";
    }

    /// <summary>
    ///     Cursor position management
    /// </summary>
    struct Cursor
    {
        /// <summary>
        ///     Beginning position of the cursor - top coordinate.
        /// </summary>
        public int Top { get; private set; }

        /// <summary>
        ///     Beginning position of the cursor - left coordinate.
        /// </summary>
        public int Left { get; private set; }

        public void Anchor()
        {
            Top = Console.CursorTop;
            Left = Console.CursorLeft;
        }

        public void Reset()
        {
            Console.CursorTop = Top;
            Console.CursorLeft = Left;
        }

        public void Place(int index)
        {
            Console.CursorLeft = (Left + index)%Console.BufferWidth;
            var cursorTop = Top + (Left + index)/Console.BufferWidth;
            if (cursorTop >= Console.BufferHeight)
            {
                Top -= cursorTop - Console.BufferHeight + 1;
                cursorTop = Console.BufferHeight - 1;
            }
            Console.CursorTop = cursorTop;
        }

        public void Move(int delta)
        {
            var position = Console.CursorTop*Console.BufferWidth + Console.CursorLeft + delta;

            Console.CursorLeft = position%Console.BufferWidth;
            Console.CursorTop = position/Console.BufferWidth;
        }
    }

    /// <summary>
    ///     The console input buffer.
    /// </summary>
    StringBuilder input = new();

    /// <summary>
    ///     Current position - index into the input buffer
    /// </summary>
    int current;

    //removed autoIndentSize  (-P)

    /// <summary>
    ///     Length of the output currently rendered on screen.
    /// </summary>
    int rendered;

    //private bool changed = true;
    /// <summary>
    ///     Input has changed.
    /// </summary>
    /// <summary>
    ///     Command history
    /// </summary>
    History history = new();

    /// <summary>
    ///     Tab options available in current context
    /// </summary>
    SuperConsoleOptions options = new();

    /// <summary>
    ///     Cursort anchor - position of cursor when the routine was called
    /// </summary>
    Cursor cursor;

    //Removed "PythonEngine engine" an all references/assignments to it. (-P)

    AutoResetEvent ctrlCEvent;

    public SuperConsole(bool colorfulConsole)
    {
        Console.CancelKeyPress += Console_CancelKeyPress;
        ctrlCEvent = new(false);
        if (colorfulConsole)
            SetupColors();
    }

    void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (e.SpecialKey == ConsoleSpecialKey.ControlC)
        {
            e.Cancel = true;
            ctrlCEvent.Set();
            Environment.Exit(2);
        }
    }

    public abstract bool IsPartOfIdentifier(char c);

    bool GetOptions()
    {
        options.Clear();

        int len;
        for (len = input.Length; len > 0; len--)
        {
            var c = input[len - 1];
            if (IsPartOfIdentifier(c))
            {
            }
            else
            {
                break;
            }
        }

        var name = input.ToString(len, input.Length - len);
        if (name.Trim().Length > 0)
        {
            var lastDot = name.LastIndexOf('.');
            string attr,
                pref,
                root;
            if (lastDot < 0)
            {
                attr = string.Empty;
                pref = name;
                root = input.ToString(0, len);
            }
            else
            {
                attr = name[..lastDot];
                pref = name[(lastDot + 1)..];
                root = input.ToString(0, len + lastDot + 1);
            }

            try
            {
                //Moved tab handling to abstract method (-P)
                options.Root = root;
                foreach (var option in OnTab(attr, pref, root))
                {
                    //Console.Write("{0} proposed and ", option);

                    if (option.StartsWith(pref, StringComparison.OrdinalIgnoreCase))
                    {
                        options.Add(option);
                        //Console.WriteLine("accepted");
                    }
                    else
                    {
                        //Console.WriteLine("rejected");
                    }
                }
            }
            catch (Exception exc)
            {
                WriteLine(exc.ToString(), Style.Error);
                options.Clear();
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public abstract IEnumerable<string> OnTab(
        string attr, string pref, string root);

    void SetInput(string line)
    {
        input.Length = 0;
        input.Append(line);

        current = input.Length;

        Render();
    }

    void Initialize()
    {
        cursor.Anchor();
        input.Length = 0;
        current = 0;
        rendered = 0;
        //changed = false;
    }

    //Removed special backspace handling for autoindention  (-P)

    void Backspace()
    {
        if (input.Length > 0 && current > 0)
        {
            input.Remove(current - 1, 1);
            current--;
            Render();
        }
    }

    void Delete()
    {
        if (input.Length > 0 && current < input.Length)
        {
            input.Remove(current, 1);
            Render();
        }
    }

    void Insert(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.F6)
        {
            Debug.Assert(FinalLineText.Length == 1);

            Insert(FinalLineText[0]);
        }
        else
        {
            //c = key.KeyChar;
            var us = User32.ToUnicode(key, true);
            if (us.Length > 0)
                Insert(us[0]);
        }
    }

    void Insert(char c)
    {
        if (current == input.Length)
        {
            if (char.IsControl(c))
            {
                var s = MapCharacter(c);
                current++;
                input.Append(c);
                Console.Write(s);
                rendered += s.Length;
            }
            else
            {
                current++;
                input.Append(c);
                Console.Write(c);
                rendered++;
            }
        }
        else
        {
            input.Insert(current, c);
            current++;
            Render();
        }
    }

    //Made the following two methods static  (-P)

    static string MapCharacter(char c)
    {
        if (c == 13)
            return "\r\n";
        if (c <= 26)
            return "^" + (char) (c + 'A' - 1);

        return "^?";
        //return c.ToString();
    }

    static int GetCharacterSize(char c)
    {
        if (char.IsControl(c))
        {
            return MapCharacter(c).Length;
        }
        else
        {
            return 1;
        }
    }

    void Render()
    {
        cursor.Reset();
        var output = new StringBuilder();
        var position = -1;
        for (var i = 0; i < input.Length; i++)
        {
            if (i == current)
            {
                position = output.Length;
            }
            var c = input[i];
            if (char.IsControl(c))
            {
                output.Append(MapCharacter(c));
            }
            else
            {
                output.Append(c);
            }
        }

        if (current == input.Length)
        {
            position = output.Length;
        }

        var text = output.ToString();
        Console.Write(text);

        if (text.Length < rendered)
        {
            Console.Write(new string(' ', rendered - text.Length));
        }
        rendered = text.Length;
        cursor.Place(position);
    }

    void MoveLeft(ConsoleModifiers keyModifiers)
    {
        if ((keyModifiers & ConsoleModifiers.Control) != 0)
        {
            // move back to the start of the previous word
            if (input.Length > 0 && current != 0)
            {
                var nonLetter = IsSeperator(input[current - 1]);
                while (current > 0 && current - 1 < input.Length)
                {
                    MoveLeft();

                    if (IsSeperator(input[current]) != nonLetter)
                    {
                        if (!nonLetter)
                        {
                            MoveRight();
                            break;
                        }

                        nonLetter = false;
                    }
                }
            }
        }
        else
        {
            MoveLeft();
        }
    }

    //Made IsSeperator static  (-P)

    static bool IsSeperator(char ch)
    {
        return !char.IsLetter(ch);
    }

    void MoveRight(ConsoleModifiers keyModifiers)
    {
        if ((keyModifiers & ConsoleModifiers.Control) != 0)
        {
            // move to the next word
            if (input.Length != 0 && current < input.Length)
            {
                var nonLetter = IsSeperator(input[current]);
                while (current < input.Length)
                {
                    MoveRight();

                    if (current == input.Length)
                        break;
                    if (IsSeperator(input[current]) != nonLetter)
                    {
                        if (nonLetter)
                            break;

                        nonLetter = true;
                    }
                }
            }
        }
        else
        {
            MoveRight();
        }
    }

    void MoveRight()
    {
        if (current < input.Length)
        {
            var c = input[current];
            current++;
            cursor.Move(GetCharacterSize(c));
        }
    }

    void MoveLeft()
    {
        if (current > 0 && current - 1 < input.Length)
        {
            current--;
            var c = input[current];
            cursor.Move(-GetCharacterSize(c));
        }
    }

    const int TabSize = 4;

    void InsertTab()
    {
        for (var i = TabSize - current%TabSize; i > 0; i--)
        {
            Insert(' ');
        }
    }

    void MoveHome()
    {
        current = 0;
        cursor.Reset();
    }

    void MoveEnd()
    {
        current = input.Length;
        cursor.Place(rendered);
    }

    public bool DoBeep { get; set; }

    //Removed autoIndentSizeInput parameter and usages (-P)

    public string? ReadLine()
    {
        Initialize();

        var inputChanged = false;
        var optionsObsolete = false;

        for (;;)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    Backspace();
                    inputChanged = optionsObsolete = true;
                    break;
                case ConsoleKey.Delete:
                    Delete();
                    inputChanged = optionsObsolete = true;
                    break;
                case ConsoleKey.Enter:
                    Console.Write("\n");
                    var line = input.ToString();
                    if (line == FinalLineText)
                        return null;
                    if (line.Length > 0)
                    {
                        history.Add(line, inputChanged);
                    }
                    return line;
                case ConsoleKey.Tab:
                {
                    var prefix = false;
                    if (optionsObsolete)
                    {
                        prefix = GetOptions();
                        optionsObsolete = false;
                    }

                    if (options.Count > 0)
                    {
                        var part = (key.Modifiers & ConsoleModifiers.Shift) != 0
                            ? options.Previous()
                            : options.Next();
                        SetInput(options.Root + part);
                    }
                    else
                    {
                        if (!prefix)
                        {
                            InsertTab();
                        }
                    }
                    inputChanged = true;
                    break;
                }
                case ConsoleKey.UpArrow:
                    SetInput(history.Previous());
                    optionsObsolete = true;
                    inputChanged = false;
                    break;
                case ConsoleKey.DownArrow:
                    SetInput(history.Next());
                    optionsObsolete = true;
                    inputChanged = false;
                    break;
                case ConsoleKey.RightArrow:
                    MoveRight(key.Modifiers);
                    optionsObsolete = true;
                    break;
                case ConsoleKey.LeftArrow:
                    MoveLeft(key.Modifiers);
                    optionsObsolete = true;
                    break;
                case ConsoleKey.Escape:
                    SetInput(string.Empty);
                    inputChanged = optionsObsolete = true;
                    break;
                case ConsoleKey.Home:
                    MoveHome();
                    optionsObsolete = true;
                    break;
                case ConsoleKey.End:
                    MoveEnd();
                    optionsObsolete = true;
                    break;
                case ConsoleKey.LeftWindows:
                case ConsoleKey.RightWindows:
                    // ignore these
                    continue;
                default:
                    if (key.KeyChar == '\x0D')
                        goto case ConsoleKey.Enter; // Ctrl-M
                    if (key.KeyChar == '\x08')
                        goto case ConsoleKey.Backspace; // Ctrl-H
                    Insert(key);
                    inputChanged = optionsObsolete = true;
                    break;
            }
        }
    }

    public string? ReadLineInteractive()
    {
        Initialize();

        var inputChanged = false;
        var optionsObsolete = false;

        for (;;)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    Backspace();
                    inputChanged = optionsObsolete = true;
                    GetOptions();
                    break;
                case ConsoleKey.Delete:
                    Delete();
                    inputChanged = optionsObsolete = true;
                    GetOptions();
                    break;
                case ConsoleKey.Enter:
                    Console.Write("\n");
                    var line = input.ToString();
                    if (line == FinalLineText)
                        return null;
                    if (line.Length > 0)
                    {
                        history.Add(line, inputChanged);
                    }
                    return line;
                case ConsoleKey.Tab:
                {
                    var prefix = false;
                    if (optionsObsolete)
                    {
                        prefix = GetOptions();
                        optionsObsolete = false;
                    }

                    if (options.Count > 0)
                    {
                        var part = (key.Modifiers & ConsoleModifiers.Shift) != 0
                            ? options.Previous()
                            : options.Next();
                        SetInput(options.Root + part);
                    }
                    else
                    {
                        if (!prefix)
                        {
                            InsertTab();
                        }
                        else
                        {
                        }
                    }
                    inputChanged = true;
                    break;
                }
                case ConsoleKey.UpArrow:
                    SetInput(history.Previous());
                    optionsObsolete = true;
                    inputChanged = false;
                    break;
                case ConsoleKey.DownArrow:
                    SetInput(history.Next());
                    optionsObsolete = true;
                    inputChanged = false;
                    break;
                case ConsoleKey.RightArrow:
                    MoveRight(key.Modifiers);
                    optionsObsolete = true;
                    break;
                case ConsoleKey.LeftArrow:
                    MoveLeft(key.Modifiers);
                    optionsObsolete = true;
                    break;
                case ConsoleKey.Escape:
                    SetInput(string.Empty);
                    inputChanged = optionsObsolete = true;
                    break;
                case ConsoleKey.Home:
                    MoveHome();
                    optionsObsolete = true;
                    break;
                case ConsoleKey.End:
                    MoveEnd();
                    optionsObsolete = true;
                    break;
                case ConsoleKey.LeftWindows:
                case ConsoleKey.RightWindows:
                    // ignore these
                    continue;
                default:
                    if (key.KeyChar == '\x0D')
                        goto case ConsoleKey.Enter; // Ctrl-M
                    if (key.KeyChar == '\x08')
                        goto case ConsoleKey.Backspace; // Ctrl-H
                    Insert(key);
                    inputChanged = optionsObsolete = true;
                    GetOptions();
                    break;
            }
        }
    }

    //Made FinalLineText static  (-P)

    static string FinalLineText => Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04";

    public void Write(string text, Style style)
    {
        switch (style)
        {
            case Style.Prompt:
                WriteColor(text, PromptColor);
                break;
            case Style.Out:
                WriteColor(text, OutColor);
                break;
            case Style.Error:
                WriteColor(text, ErrorColor);
                break;
        }
    }

    public void WriteLine(string text, Style style)
    {
        Write(text + Environment.NewLine, style);
    }

    static void WriteColor(string s, ConsoleColor c)
    {
        var origColor = Console.ForegroundColor;
        Console.ForegroundColor = c;
        Console.Write(s);
        Console.ForegroundColor = origColor;
    }
}

public enum Style
{
    Prompt,
    Out,
    Error,
}