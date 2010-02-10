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

/*
 * Adaption for use as a general purpose console wrapper.
 * Copyright of changes Christian Klauser
 * Changes are marked with (-P)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
//Removed references to IronPython (-P)

namespace Prx
{
    //made class abstract
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
        /// Class managing the command history.
        /// </summary>
        private class History
        {
            private ArrayList list = new ArrayList();
            private int current = 0;
            private bool increment = false; // increment on Next()

            private string Current
            {
                get
                {
                    return
                        current >= 0 && current < list.Count ? (string) list[current] : String.Empty;
                }
            }

            public void Add(string line, bool setCurrentAsLast)
            {
                if (line != null && line.Length > 0)
                {
                    int oldCount = list.Count;
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
        /// List of available options
        /// </summary>
        private class SuperConsoleOptions
        {
            private ArrayList list = new ArrayList();
            private int current = 0;
            private string root;

            public int Count
            {
                get { return list.Count; }
            }

            private string Current
            {
                get
                {
                    return
                        current >= 0 && current < list.Count ? (string) list[current] : String.Empty;
                }
            }

            public void Clear()
            {
                list.Clear();
                current = -1;
            }

            public void Add(string line)
            {
                if (line != null && line.Length > 0)
                {
                    list.Add(line);
                }
            }

            public string Previous()
            {
                if (list.Count > 0)
                {
                    current = ((current - 1) + list.Count)%list.Count;
                }
                return Current;
            }

            public string Next()
            {
                if (list.Count > 0)
                {
                    current = (current + 1)%list.Count;
                }
                return Current;
            }

            public string Root
            {
                get { return root; }
                set { root = value; }
            }
        }

        /// <summary>
        /// Cursor position management
        /// </summary>
        private struct Cursor
        {
            /// <summary>
            /// Beginning position of the cursor - top coordinate.
            /// </summary>
            private int anchorTop;

            /// <summary>
            /// Beginning position of the cursor - left coordinate.
            /// </summary>
            private int anchorLeft;

            public int Top
            {
                get { return anchorTop; }
            }

            public int Left
            {
                get { return anchorLeft; }
            }

            public void Anchor()
            {
                anchorTop = Console.CursorTop;
                anchorLeft = Console.CursorLeft;
            }

            public void Reset()
            {
                Console.CursorTop = anchorTop;
                Console.CursorLeft = anchorLeft;
            }

            public void Place(int index)
            {
                Console.CursorLeft = (anchorLeft + index)%Console.BufferWidth;
                int cursorTop = anchorTop + (anchorLeft + index)/Console.BufferWidth;
                if (cursorTop >= Console.BufferHeight)
                {
                    anchorTop -= cursorTop - Console.BufferHeight + 1;
                    cursorTop = Console.BufferHeight - 1;
                }
                Console.CursorTop = cursorTop;
            }

            public void Move(int delta)
            {
                int position = Console.CursorTop*Console.BufferWidth + Console.CursorLeft + delta;

                Console.CursorLeft = position%Console.BufferWidth;
                Console.CursorTop = position/Console.BufferWidth;
            }
        } ;

        /// <summary>
        /// The console input buffer.
        /// </summary>
        private StringBuilder input = new StringBuilder();

        /// <summary>
        /// Current position - index into the input buffer
        /// </summary>
        private int current = 0;

        //removed autoIndentSize  (-P)

        /// <summary>
        /// Length of the output currently rendered on screen.
        /// </summary>
        private int rendered = 0;

        /// <summary>
        /// Input has changed.
        /// </summary>
        //private bool changed = true;
        /// <summary>
        /// Command history
        /// </summary>
        private History history = new History();

        /// <summary>
        /// Tab options available in current context
        /// </summary>
        private SuperConsoleOptions options = new SuperConsoleOptions();

        /// <summary>
        /// Cursort anchor - position of cursor when the routine was called
        /// </summary>
        private Cursor cursor;

        //Removed "PythonEngine engine" an all references/assignments to it. (-P)

        private AutoResetEvent ctrlCEvent;
        private Thread MainEngineThread = Thread.CurrentThread;

        public SuperConsole(bool colorfulConsole)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            ctrlCEvent = new AutoResetEvent(false);
            if (colorfulConsole)
                SetupColors();
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                e.Cancel = true;
                ctrlCEvent.Set();
                MainEngineThread.Abort();
            }
        }

        public abstract bool IsPartOfIdentifier(char c);

        private bool GetOptions()
        {
            options.Clear();

            int len;
            for (len = input.Length; len > 0; len--)
            {
                char c = input[len - 1];
                if (IsPartOfIdentifier(c))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }

            string name = input.ToString(len, input.Length - len);
            if (name.Trim().Length > 0)
            {
                int lastDot = name.LastIndexOf('.');
                string attr,
                       pref,
                       root;
                if (lastDot < 0)
                {
                    attr = String.Empty;
                    pref = name;
                    root = input.ToString(0, len);
                }
                else
                {
                    attr = name.Substring(0, lastDot);
                    pref = name.Substring(lastDot + 1);
                    root = input.ToString(0, len + lastDot + 1);
                }

                try
                {
                    //Moved tab handling to abstract method (-P)
                    options.Root = root;
                    foreach (string option in OnTab(attr, pref, root))
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

        private void SetInput(string line)
        {
            input.Length = 0;
            input.Append(line);

            current = input.Length;

            Render();
        }

        private void Initialize()
        {
            cursor.Anchor();
            input.Length = 0;
            current = 0;
            rendered = 0;
            //changed = false;
        }

        //Removed special backspace handling for autoindention  (-P)

        private void Backspace()
        {
            if (input.Length > 0 && current > 0)
            {
                input.Remove(current - 1, 1);
                current--;
                Render();
            }
        }

        private void Delete()
        {
            if (input.Length > 0 && current < input.Length)
            {
                input.Remove(current, 1);
                Render();
            }
        }

        private void Insert(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.F6)
            {
                Debug.Assert(FinalLineText.Length == 1);

                Insert(FinalLineText[0]);
            }
            else
            {
                //c = key.KeyChar;
                var us = Win32.User32.ToUnicode(key,true);
                if(us.Length > 0)
                    Insert(us[0]);
            }
        }

        private void Insert(char c)
        {
            if (current == input.Length)
            {
                if (Char.IsControl(c))
                {
                    string s = MapCharacter(c);
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

        private static string MapCharacter(char c)
        {
            if (c == 13)
                return "\r\n";
            if (c <= 26)
                return "^" + ((char) (c + 'A' - 1));

            return "^?";
            //return c.ToString();
        }

        private static int GetCharacterSize(char c)
        {
            if (Char.IsControl(c))
            {
                return MapCharacter(c).Length;
            }
            else
            {
                return 1;
            }
        }

        private void Render()
        {
            cursor.Reset();
            StringBuilder output = new StringBuilder();
            int position = -1;
            for (int i = 0; i < input.Length; i++)
            {
                if (i == current)
                {
                    position = output.Length;
                }
                char c = input[i];
                if (Char.IsControl(c))
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

            string text = output.ToString();
            Console.Write(text);

            if (text.Length < rendered)
            {
                Console.Write(new String(' ', rendered - text.Length));
            }
            rendered = text.Length;
            cursor.Place(position);
        }

        private void MoveLeft(ConsoleModifiers keyModifiers)
        {
            if ((keyModifiers & ConsoleModifiers.Control) != 0)
            {
                // move back to the start of the previous word
                if (input.Length > 0 && current != 0)
                {
                    bool nonLetter = IsSeperator(input[current - 1]);
                    while (current > 0 && (current - 1 < input.Length))
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

        private static bool IsSeperator(char ch)
        {
            return !Char.IsLetter(ch);
        }

        private void MoveRight(ConsoleModifiers keyModifiers)
        {
            if ((keyModifiers & ConsoleModifiers.Control) != 0)
            {
                // move to the next word
                if (input.Length != 0 && current < input.Length)
                {
                    bool nonLetter = IsSeperator(input[current]);
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

        private void MoveRight()
        {
            if (current < input.Length)
            {
                char c = input[current];
                current++;
                cursor.Move(GetCharacterSize(c));
            }
        }

        private void MoveLeft()
        {
            if (current > 0 && (current - 1 < input.Length))
            {
                current--;
                char c = input[current];
                cursor.Move(-GetCharacterSize(c));
            }
        }

        private const int TabSize = 4;

        private void InsertTab()
        {
            for (int i = TabSize - (current%TabSize); i > 0; i--)
            {
                Insert(' ');
            }
        }

        private void MoveHome()
        {
            current = 0;
            cursor.Reset();
        }

        private void MoveEnd()
        {
            current = input.Length;
            cursor.Place(rendered);
        }

        public bool DoBeep
        {
            get { return _doBeep; }
            set { _doBeep = value; }
        }

        private bool _doBeep = false;

        //Removed autoIndentSizeInput parameter and usages (-P)

        public string ReadLine()
        {
            Initialize();

            bool inputChanged = false;
            bool optionsObsolete = false;

            for (;;)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

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
                        string line = input.ToString();
                        if (line == FinalLineText)
                            return null;
                        if (line.Length > 0)
                        {
                            history.Add(line, inputChanged);
                        }
                        return line;
                    case ConsoleKey.Tab:
                        {
                            bool prefix = false;
                            if (optionsObsolete)
                            {
                                prefix = GetOptions();
                                optionsObsolete = false;
                            }

                            if (options.Count > 0)
                            {
                                string part = (key.Modifiers & ConsoleModifiers.Shift) != 0
                                                  ? options.Previous()
                                                  : options.Next();
                                SetInput(options.Root + part);
                            }
                            else
                            {
                                if (prefix)
                                {
                                    if (_doBeep)
                                        Console.Beep();
                                }
                                else
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
                        SetInput(String.Empty);
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

        //Made FinalLineText static  (-P)

        private static string FinalLineText
        {
            get { return Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04"; }
        }

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

        private static void WriteColor(string s, ConsoleColor c)
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
        Error
    }
}