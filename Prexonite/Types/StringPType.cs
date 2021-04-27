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
#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;
using UInt8 = System.Byte;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("String")]
    public class StringPType : PType, ICilCompilerAware
    {
        #region Singleton Pattern

        public static StringPType Instance { [DebuggerStepThrough] get; }

        static StringPType()
        {
            Instance = new StringPType();
        }

        [DebuggerStepThrough]
        private StringPType()
        {
        }

        #endregion

        #region Static

        [DebuggerStepThrough]
        public override PValue CreatePValue(object value)
        {
            if (value == null)
                value = "";
            return new PValue(value.ToString(), Instance);
        }

        #region Escape/Unescape

        public static string UnescapeMinimal(string escaped)
        {
            return escaped.Replace("\"\"", "\"");
        }

        public static string Escape(string unescaped)
        {
            //The initial capacity is just a random guess
            var buffer = new StringBuilder(unescaped.Length + 16);
            Escape(unescaped, new StringWriter(buffer));
            return buffer.ToString();
        }

        public static void Escape(string unescaped, TextWriter buffer)
        {
            for (var i = 0; i < unescaped.Length; i++)
            {
                var curr = unescaped[i];
                bool nextIsHex;
                if (i + 1 < unescaped.Length)
                {
                    var c = unescaped[i + 1];
                    nextIsHex = '0' <= c && c <= '9'
                                || 'a' <= c && c <= 'f'
                                || 'A' <= c && c <= 'F';
                }
                else
                {
                    nextIsHex = false;
                }


                switch (curr)
                {
                    case '\\':
                        buffer.Write(@"\\");
                        break;
                    case '"':
                        buffer.Write("\\\"");
                        break;
                    case '$':
                        buffer.Write("\\$");
                        break;
                    case '\0':
                        buffer.Write("\\0");
                        break;
                    case '\a':
                        buffer.Write("\\a");
                        break;
                    case '\b':
                        buffer.Write("\\b");
                        break;
                    case '\f':
                        buffer.Write("\\f");
                        break;
                    case '\n':
                        buffer.Write("\\n");
                        break;
                    case '\r':
                        buffer.Write("\\r");
                        break;
                    case '\t':
                        buffer.Write("\\t");
                        break;
                    case '\v':
                        buffer.Write("\\v");
                        break;
                    default:
                        if (curr >= 20 && curr < 127)
                            buffer.Write(curr);
                        else //Non-printable characters
                        {
                            uint utf32 = curr;
                            ushort utf16 = curr;
                            var utf8 = (byte) curr;
                            if (utf32 > ushort.MaxValue)
                                //Use \U notation
                                buffer.Write("\\U{0:X8}", utf32);
                            else if (utf32 > byte.MaxValue || nextIsHex)
                                //Use \u notation
                                buffer.Write("\\u{0:X4}", utf16);
                            else
                                //Use \x notation
                                buffer.Write("\\x{0:X2}", utf8);
                        }
                        break;
                }
            }
        }

        public static string Unescape(string escaped)
        {
            var esc = escaped.ToCharArray();
            var buffer = new StringBuilder();
            for (var i = 0; i < esc.Length; i++)
            {
                //Is escape char
                if (esc[i] == '\\')
                {
                    i++;
                    if (i >= esc.Length)
                        throw new ArgumentException(
                            "Incomplete escape sequence at the end of the string");
                    StringBuilder hex;
                    int utf32;
                    switch (esc[i])
                    {
                        case '\'':
                        case '\"':
                        case '\\':
                        case '$':
                            goto add;
                        case '0':
                            buffer.Append('\0');
                            break;
                        case 'a':
                        case 'A':
                            buffer.Append('\a');
                            break;
                        case 'b':
                        case 'B':
                            buffer.Append('\b');
                            break;
                        case 'f':
                        case 'F':
                            buffer.Append('\f');
                            break;
                        case 'n':
                        case 'N':
                            buffer.Append('\n');
                            break;
                        case 'r':
                        case 'R':
                            buffer.Append('\r');
                            break;
                        case 't':
                        case 'T':
                            buffer.Append('\t');
                            break;
                        case 'v':
                        case 'V':
                            buffer.Append('\v');
                            break;
                        case 'x':
                        case 'X':
                            //Require at least two additional characters
                            if (i + 2 >= esc.Length)
                                goto add; //Ignore this sequence
                            i++;
                            hex = new StringBuilder();
                            for (var j = 0; i < esc.Length && j < 3; i++, j++)
                            {
                                var curr = esc[i];
                                if (
                                    !(char.IsDigit(curr) ||
                                        char.IsLetter(curr) &&
                                        (char.IsLower(curr) && curr < 'g' || curr < 'G')))
                                {
                                    i--;
                                    break;
                                }
                                hex.Append(esc[i]);
                            }
                            if (
                                !int.TryParse(
                                    hex.ToString(),
                                    NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture,
                                    out utf32))
                                throw new ArgumentException(
                                    "Invalid escape character sequence. (\"\\x" +
                                        hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
                            break;
                        case 'u':
                            if (i + 4 >= esc.Length)
                                goto add; //Ignore this sequence
                            hex = new StringBuilder();
                            i++;
                            for (var j = 0; i < esc.Length && j < 4; i++, j++)
                                hex.Append(esc[i]);
                            if (
                                !int.TryParse(
                                    hex.ToString(),
                                    NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture,
                                    out utf32))
                                throw new ArgumentException(
                                    "Invalid escape character sequence. (\"\\u" +
                                        hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
                            i--; //i will be incremented by the for-loop
                            break;
                        case 'U':
                            if (i + 8 >= esc.Length)
                                goto add; //Ignore this sequence
                            hex = new StringBuilder();
                            i++;
                            for (var j = 0; i < esc.Length && j < 8; i++, j++)
                            {
                                hex.Append(esc[i]);
                            }
                            if (
                                !int.TryParse(
                                    hex.ToString(),
                                    NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture,
                                    out utf32))
                                throw new ArgumentException(
                                    "Invalid escape character sequence. (\"\\U" +
                                        hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
                            i--; //i will be incremented by the for-loop
                            break;
                    }
                    goto next;
                }
                add: //Add verbatim
                buffer.Append(esc[i]);
                next:
                ;
            }
            return buffer.ToString();
        }

        #endregion

        #region IdOrLiteral

        private static readonly Regex IdLetters = new( @"^[\w\\][\w\d\\']{0,}$");

        private const int AnArbitraryIdLengthLimit = 255;

        public static string ToIdOrLiteral(string raw)
        {
            raw ??= "";
            if (
                //Empty strings cannot be represented as Ids
                raw.Length == 0)
                return "\"\"";
            if (
                raw.Length > AnArbitraryIdLengthLimit ||
                    !IdLetters.IsMatch(raw) ||
                        IsReservedWord(raw) ||
                            char.IsDigit(raw, 0)
                )
                return "\"" + Escape(raw) + "\"";
            return raw;
        }

        public static string ToIdLiteral(string physicalId)
        {
            return OperatorCommands.TryGetLiteral(physicalId, out var literal) ? literal : physicalId;
        }

        #endregion

        #region IsReservedWord

        public static bool IsReservedWord(string word)
        {
            //The shortest word is "f" and the longest "coroutine". 
            //Spaces are most common in strings, but forbidden in Ids
            if (word.Length < 1 || word.Length > 9 || word.Contains(" "))
                return false;
            var reservedWords = new[]
                {
                    #region list of reserved words
                    "add",
                    "and",
                    "as",
                    "asm",
                    "break",
                    "build",
                    "command",
                    "continue",
                    "coroutine",
                    "catch",
                    "declare",
                    "disabled",
                    "do",
                    "does",
                    "enabled",
                    "else",
                    "false",
                    "finally",
                    "for",
                    "foreach",
                    "function",
                    "goto",
                    "if",
                    "in",
                    "inline",
                    "is",
                    "label",
                    "lazy",
                    "let",
                    "local",
                    "macro",
                    "mod",
                    "not",
                    "null",
                    "new",
                    "or",
                    "ref",
                    "return",
                    "static",
                    "to",
                    "true",
                    "try",
                    "then",
                    "throw",
                    "unless",
                    "until",
                    "using",
                    "var",
                    "while",
                    "xor",
                    "yield",

                    #endregion
                };
            return reservedWords.Any(reservedWord => Engine.StringsAreEqual(word, reservedWord));
        }

        #endregion

        #endregion

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            result = null;
            if (args.Length <= 0)
                result = String.CreatePValue("");
            else if (args[0].TryConvertTo(sctx, String, out var arg))
                result = String.CreatePValue(arg.Value as string);
            else
                return false;
            return true;
        }

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            result = null;
            var str = (string) subject.Value;
            switch (id?.ToLowerInvariant() ?? "")
            {
                case "":
                    if (args.Length < 1)
                        return false;
                    var nArg = args[0];
                    if (!nArg.TryConvertTo(sctx, Int, out var rArg))
                        return false;
                    result = str[(int) rArg.Value].ToString();
                    break;
                case "unescape":
                    result = Unescape(str);
                    break;
                case "format":
                    var objs = new object[args.Length];
                    for (var i = 0; i < args.Length; i++)
                        objs[i] = args[i].Value;
                    result = string.Format(str, objs);
                    break;
                case "escape":
                    result = Escape(str);
                    break;
                case "isreservedword":
                    result = IsReservedWord(str);
                    break;
                case "toidorliteral":
                    result = ToIdOrLiteral(str);
                    break;
                case "tostring":
                    result = subject;
                    break;
                case "tolower":
                    result = str.ToLower();
                    break;
                case "toupper":
                    result = str.ToUpper();
                    break;
                case "substring":
                    switch (args.Length)
                    {
                        case 0:
                            return false;
                        case 1:
                            result =
                                str.Substring(
                                    (int) args[0].ConvertTo(sctx, Int).Value);
                            break;
                        default:
                            result =
                                str.Substring(
                                    (int) args[0].ConvertTo(sctx, Int).Value,
                                    (int) args[1].ConvertTo(sctx, Int).Value);
                            break;
                    }
                    break;
                case "split":
                    if (args.Length == 0 || args[0].IsNull)
                    {
                        result =
                            (PValue)
                                _wrap_strings(str.Split(null));
                        return true;
                    }
                    //Try to interpret as params char[] or fall back to params string[]
                    var sch = new List<char>();
                    List<string> sst = null;

                    var isParams = true;

                    _resolve_params(sctx, args, ref isParams, sch, ref sst, false);

                    if (isParams)
                    {
                        if (sst != null)
                        {
                            result =
                                (PValue)
                                    _wrap_strings(
                                        str.Split(sst.ToArray(), StringSplitOptions.None));
                        }
                        else
                        {
                            result =
                                (PValue)
                                    _wrap_strings(
                                        str.Split(sch.ToArray(), StringSplitOptions.None));
                        }
                        return true;
                    }

                    if (!args[0].TryConvertTo(sctx, List, true, out var list))
                        throw new PrexoniteException(
                            "String.Split requires a list as its first argument.");

                    sch.Clear();
                    sst = null;
                    var isValid = true;
                    _resolve_params(sctx, ((List<PValue>) list.Value).ToArray(), ref isValid, sch,
                        ref sst, true);

                    if (!isValid)
                        throw new PrexoniteException(
                            "String.Split only accepts lists of strings or chars.");

                    if (sst != null)
                    {
                        result =
                            (PValue)
                                _wrap_strings(
                                    str.Split(sst.ToArray(), StringSplitOptions.None));
                    }
                    else
                    {
                        result =
                            (PValue)
                                _wrap_strings(
                                    str.Split(sch.ToArray(), StringSplitOptions.None));
                    }
                    return true;
                default:
                    return
                        Object[typeof (string)].TryDynamicCall(
                            sctx, subject, args, call, id, out result);
            }
            return result != null;
        }

        private static List<PValue> _wrap_strings(ICollection<string> xs)
        {
            var lst = new List<PValue>(xs.Count);
            foreach (var x in xs)
                lst.Add(x);
            return lst;
        }

        private static void _resolve_params(
            StackContext sctx,
            IEnumerable<PValue> args,
            ref bool isParams,
            ICollection<char> sch,
            ref List<string> sst,
            bool useExplicit)
        {
            foreach (var arg in args)
            {
                PValue v;
                if (sst != null)
                {
                    if (! arg.TryConvertTo(sctx, String, useExplicit, out v))
                    {
                        isParams = false;
                        break;
                    }
                    sst.Add((string) v.Value);
                }
                else if (arg.TryConvertTo(sctx, out char c))
                {
                    sch.Add(c);
                }
                else if (arg.TryConvertTo(sctx, String, useExplicit, out v))
                {
                    sst = new List<string> {(string) v.Value};
                }
                else
                {
                    isParams = false;
                    break;
                }
            }

            if (isParams && sst != null)
            {
                var isChars = true;
                sch.Clear();

                foreach (var s in sst)
                {
                    if (s.Length != 1)
                    {
                        isChars = false;
                        break;
                    }
                    sch.Add(s[0]);
                }

                if (isChars)
                    sst = null;
            }
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (args.Length >= 1 && Engine.StringsAreEqual(id, "unescape"))
            {
                result = Unescape(args[0].ConvertTo(sctx, String).Value as string);
                return true;
            }
            if (args.Length >= 1 && Engine.StringsAreEqual(id, "escape"))
            {
                result = Escape(args[0].ConvertTo(sctx, String).Value as string);
                return true;
            }
            if (args.Length > 1 && Engine.StringsAreEqual(id, "format"))
            {
                var oargs = new object[args.Length - 1];
                var format = args[0].CallToString(sctx);
                for (var i = 0; i < oargs.Length; i++)
                    oargs[i] = args[i + 1].CallToString(sctx);
                result = string.Format(format, oargs);
                return true;
            }
            return Object[typeof (string)].TryStaticCall(sctx, args, call, id, out result);
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            result = null;
            var str = (string) subject.Value;
            var app = sctx.ParentApplication;
            var eng = sctx.ParentEngine;
            if (app.Functions.TryGetValue(str, out var func))
            {
                var fctx = func.CreateFunctionContext(sctx, args);
                result = eng.Process(fctx);
            }
            else if (eng.Commands.TryGetValue(str, out var cmd))
            {
                result = cmd.Run(sctx, args);
            }

            return result != null;
        }

        public override bool Increment(StackContext sctx, PValue operand, out PValue result)
        {
            if (!(operand.Value is string sop))
                throw new PrexoniteException(operand + " cannot be supplied to ~String.Increment");
            result = sop.Length == 0
                ? String.CreatePValue("")
                : String.CreatePValue(string.Concat(sop, sop));
            return true;
        }

        public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
        {
            if (!(operand.Value is string sop))
                throw new PrexoniteException(operand + " cannot be supplied to ~String.Decrement");
            result = sop.Length == 0
                ? String.CreatePValue("")
                : String.CreatePValue(sop.Substring(0, sop.Length - 1));
            return true;
        }

        public override bool Addition(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            var left = leftOperand.CallToString(sctx);
            var right = rightOperand.CallToString(sctx);
            result = string.Concat(left, right);
            return true;
        }

        public override bool Multiply(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            string left;
            int right;
            if (leftOperand.Type is StringPType && rightOperand.Type is IntPType)
            {
                left = leftOperand.Value as string;
                right = (int) rightOperand.Value;
            }
            else if (rightOperand.Type is StringPType && leftOperand.Type is IntPType)
            {
                left = rightOperand.Value as string;
                right = (int) leftOperand.Value;
            }
            else
                return false;

            if (left == null)
                throw new PrexoniteException(
                    "~String.Multiply requires one operand to be a string.");

            if (left.Length == 0)
            {
                result = String.CreatePValue("");
                return true;
            }
            if (right > -1)
            {
                var res = new StringBuilder();
                for (var i = right; i > 0; i--)
                    res.Append(left);
                result = res.ToString();
                return true;
            }
            throw new PrexoniteException("String multiplication requires positive values. (Not " +
                right + ")");
        }

        public override bool Equality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) == 0;
            return true;
        }

        public override bool Inequality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) != 0;
            return true;
        }

        public override bool GreaterThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) > 0;
            return true;
        }

        public override bool GreaterThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) >= 0;
            return true;
        }

        public override bool LessThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) < 0;
            return true;
        }

        public override bool LessThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(
                    leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) <= 0;
            return true;
        }

        protected override bool InternalConvertTo(
            StackContext sctx,
            PValue subject,
            PType target,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            var s = (string) subject.Value;
            var builtInT = target.ToBuiltIn();
            if (useExplicit)
            {
                switch (builtInT)
                {
                    case BuiltIn.List:
                        var lst = _toPCharList(s);
                        result = (PValue) lst;
                        break;
                }
            }

            if (result == null)
            {
                switch (builtInT)
                {
                    case BuiltIn.Object:
                        var clrType = ((ObjectPType) target).ClrType;
                        var typeC = Type.GetTypeCode(clrType);
                        switch (typeC)
                        {
                            case TypeCode.String:
                                result = CreateObject(s);
                                break;
                            case TypeCode.Object:
                                if (clrType == typeof (IEnumerable<PValue>))
                                    result = (PValue) _toPCharList(s);
                                else if (clrType == typeof (char[]) ||
                                    clrType == typeof (IEnumerable<char>) ||
                                        clrType == typeof (ICollection<char>) ||
                                            clrType == typeof (IList<char>))
                                    result = new PValue(s.ToCharArray(), target);
                                else if(clrType.IsInstanceOfType(s)) 
                                    result = new PValue(s, Object[clrType]);
                                break;
                        }
                        break;
                }
            }

            return result != null;
        }

        private static List<PValue> _toPCharList(string s)
        {
            var lst = new List<PValue>(s.Length);
            foreach (var c in s)
                lst.Add(c);
            return lst;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            var subjT = subject.Type as ObjectPType;
            if ((object) subjT != null)
            {
                switch (Type.GetTypeCode(subjT.ClrType))
                {
                    case TypeCode.String:
                        result = subject.Value as string;
                        goto ret;
                }
            }

            ret:
            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is StringPType;
        }

        public const string Literal = "String";

        public override string ToString()
        {
            return Literal;
        }

        private const int _code = -631020829;

        public override int GetHashCode()
        {
            return _code;
        }

        #region ICilCompilerAware Members

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(Compiler.Cil.Compiler.GetStringPType);
        }

        #endregion
    }
}