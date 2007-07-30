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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Prexonite.Commands;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;
using UInt8 = System.Byte;

namespace Prexonite.Types
{
    [PTypeLiteral("String")]
    public class StringPType : PType
    {
        #region Singleton Pattern

        private static StringPType instance;

        public static StringPType Instance
        {
            [NoDebug()]
            get { return instance; }
        }

        static StringPType()
        {
            instance = new StringPType();
        }

        [NoDebug()]
        private StringPType()
        {
        }

        #endregion

        #region Static

        [NoDebug()]
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
            StringBuilder buffer = new StringBuilder(unescaped.Length + 10);
            for (int i = 0; i < unescaped.Length; i++)
            {
                char curr = unescaped[i];
                if (curr == '\\')
                    buffer.Append(@"\\");
                else if (curr == '"')
                    buffer.Append("\\\"");
                else if (curr == '$')
                    buffer.Append("\\$");
                else if (curr >= 20 && curr < 127)
                    buffer.Append(curr);
                else //Non-printable characters
                    switch (curr)
                    {
                        case '\0':
                            buffer.Append("\\0");
                            break;
                        case '\a':
                            buffer.Append("\\a");
                            break;
                        case '\b':
                            buffer.Append("\\b");
                            break;
                        case '\f':
                            buffer.Append("\\f");
                            break;
                        case '\n':
                            buffer.Append("\\n");
                            break;
                        case '\r':
                            buffer.Append("\\r");
                            break;
                        case '\t':
                            buffer.Append("\\t");
                            break;
                        case '\v':
                            buffer.Append("\\v");
                            break;
                        default:
                            UInt32 utf32 = curr;
                            UInt16 utf16 = curr;
                            UInt8 utf8 = (UInt8)curr;
                            if (utf32 > UInt16.MaxValue)
                                //Use \U notation
                                buffer.AppendFormat("\\U{0:00000000}", utf32.ToString("X"));
                            else if (utf32 > UInt8.MaxValue)
                                //Use \u notation
                                buffer.AppendFormat("\\u{0:0000}", utf16.ToString("X"));
                            else
                                //Use \x notation
                                buffer.AppendFormat("\\x{0:00}", utf8.ToString("X"));
                            break;
                    }
            }
            return buffer.ToString();
        }

        public static string Unescape(string escaped)
        {
            char[] esc = escaped.ToCharArray();
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < esc.Length; i++)
            {
                //Is escape char
                if (esc[i] == '\\')
                {
                    i++;
                    if (i >= esc.Length)
                        throw new ArgumentException("Incomplete escape sequence at the end of the string");
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
                            for (int j = 0; i < esc.Length && j < 3; i++, j++)
                            {
                                char curr = esc[i];
                                if (
                                    !(char.IsDigit(curr) ||
                                      (char.IsLetter(curr) && ((char.IsLower(curr) && curr < 'g') || curr < 'G'))))
                                {
                                    i--;
                                    break;
                                }
                                hex.Append(esc[i]);
                            }
                            if (
                                !int.TryParse(hex.ToString(), NumberStyles.HexNumber,
                                              CultureInfo.InvariantCulture, out utf32))
                                throw new ArgumentException("Invalid escape character sequence. (\"\\x" +
                                                            hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
                            break;
                        case 'u':
                            if (i + 4 >= esc.Length)
                                goto add; //Ignore this sequence
                            hex = new StringBuilder();
                            i++;
                            for (int j = 0; i < esc.Length && j < 3; i++, j++)
                            {
                                hex.Append(esc[i]);
                            }
                            if (
                                !int.TryParse(hex.ToString(), NumberStyles.HexNumber,
                                              CultureInfo.InvariantCulture, out utf32))
                                throw new ArgumentException("Invalid escape character sequence. (\"\\u" +
                                                            hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
                            break;
                        case 'U':
                            if (i + 4 >= esc.Length)
                                goto add; //Ignore this sequence
                            hex = new StringBuilder();
                            i++;
                            for (int j = 0; i < esc.Length && j < 7; i++, j++)
                            {
                                hex.Append(esc[i]);
                            }
                            if (
                                !int.TryParse(hex.ToString(), NumberStyles.HexNumber,
                                              CultureInfo.InvariantCulture, out utf32))
                                throw new ArgumentException("Invalid escape character sequence. (\"\\U" +
                                                            hex.ToString().Substring(2) + "\")");
                            buffer.Append(char.ConvertFromUtf32(utf32));
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

        private static Regex idLetters =
            new Regex(
                @"^[\w\\][\w\d\\]{0,}$", RegexOptions.Compiled);

        private const int anArbitraryIdLengthLimit = 255;

        public static string ToIdOrLiteral(string raw)
        {
            if (raw == null)
                raw = "";
            if (
                //Empty strings cannot be represented as Ids
                raw.Length == 0)
                return "\"\"";
            else if (
                raw.Length > anArbitraryIdLengthLimit ||
                !idLetters.IsMatch(raw) ||
                IsReservedWord(raw) ||
                char.IsDigit(raw, 0)
                )
                return "\"" + Escape(raw) + "\"";
            else
                return raw;
        }

        #endregion

        #region IsReservedWord

        public static bool IsReservedWord(string word)
        {
            //The shortest word is "f" and the longest "coroutine". 
            //Spaces are most common in strings, but forbidden in Ids
            if (word.Length < 1 || word.Length > 9 || word.Contains(" "))
                return false;
            string[] reservedWords = new string[]
                {
                    #region list of reserved words
                    "mod",
                    "is",
                    "not",
                    "enabled",
                    "disabled",
                    "function",
                    "inline",
                    "true",
                    "false",
                    "asm",
                    "ref",
                    "declare",
                    "do",
                    "does",
                    "build",
                    "return",
                    "in",
                    "to",
                    "add",
                    "continue",
                    "break",
                    "or",
                    "and",
                    "xor",
                    "label",
                    "goto",
                    "local",
                    "static",
                    "var",
                    "null",
                    "for",
                    "foreach",
                    "while",
                    "until",
                    "if",
                    "unless"
                    #endregion
                };
            foreach (string reservedWord in reservedWords)
                if (Engine.StringsAreEqual(word, reservedWord))
                    return true;
            return false;
        }

        #endregion

        #endregion

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            PValue arg;
            result = null;
            if (args.Length <= 0)
                result = String.CreatePValue("");
            else if (args[0].TryConvertTo(sctx, String, out arg))
                result = String.CreatePValue(arg.Value as String);
            else
                return false;
            return true;
        }

        public override bool TryDynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
                                            out PValue result)
        {
            result = null;
            switch ((id == null) ? "" : id.ToLowerInvariant())
            {
                case "":
                    if (args.Length < 1)
                    return false;
                    PValue nArg = args[0];
                    PValue rArg;
                    if (!nArg.TryConvertTo(sctx, Int, out rArg))
                        return false;
                    result = ((string) subject.Value)[(int) rArg.Value].ToString();
                    break;
                case "unescape":
                    result = Unescape((string)subject.Value);
                    break;
                case "format":
                    string[] objs = new string[args.Length];
                    for (int i = 0; i < args.Length; i++)
                        objs[i] = args[i].CallToString(sctx);
                    result = System.String.Format((string) subject.Value, objs);
                    break;
                case "escape":
                    result = Escape((string)subject.Value);
                    break;
                case "isreservedword":
                    result = IsReservedWord((string)subject.Value);
                    break;
                case "toidorliteral":
                    result = ToIdOrLiteral((string)subject.Value);
                    break;
                case "tostring":
                    result = subject;
                    break;
                case "tolower":
                    result = ((string) subject.Value).ToLower();
                    break;
                case "toupper":
                    result = ((string)subject.Value).ToUpper();
                    break;
                case "substring":
                    if(args.Length == 0)
                        return false;
                    else if(args.Length == 1)
                        result = ((string) subject.Value).Substring((int) args[0].ConvertTo(sctx, Int).Value);
                    else
                        result = ((string)subject.Value).Substring((int)args[0].ConvertTo(sctx, Int).Value, (int)args[1].ConvertTo(sctx, Int).Value);
                    break;
                default:
                    return Object[typeof(string)].TryDynamicCall(sctx, subject, args, call, id, out result);
            }
            return result != null;
        }

        public override bool TryStaticCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (args.Length >= 1 && Engine.StringsAreEqual(id, "unescape"))
            {
                result = Unescape(args[0].ConvertTo(sctx, String).Value as string);
                return true;
            }
            else if (args.Length >= 1 && Engine.StringsAreEqual(id, "escape"))
            {
                result = Escape(args[0].ConvertTo(sctx, String).Value as string);
                return true;
            }
            else if (args.Length > 1 && Engine.StringsAreEqual(id, "format"))
            {
                object[] oargs = new object[args.Length - 1];
                string format = args[0].CallToString(sctx);
                for (int i = 0; i < oargs.Length; i++)
                    oargs[i] = args[i + 1].CallToString(sctx);
                result = System.String.Format(format, oargs);
                return true;
            }
            else
            {
                return Object[typeof(string)].TryStaticCall(sctx, args, call, id, out result);
            }
        }

        public override bool IndirectCall(StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            result = null;
            string str = subject.Value as string;
            PFunction func;
            PCommand cmd;
            Application app = sctx.ParentApplication;
            Engine eng = sctx.ParentEngine;
            if (app.Functions.TryGetValue(str, out func))
            {
                FunctionContext fctx = func.CreateFunctionContext(eng, args);
                eng.Process(fctx);
                result = fctx.ReturnValue ?? Null.CreatePValue();
            }
            else if (eng.Commands.TryGetValue(str, out cmd))
            {
                result = cmd.Run(sctx, args);
            }

            return result != null;
        }

        public override bool Increment(StackContext sctx, PValue operand, out PValue result)
        {
            String sop = operand.Value as String;
            if (sop == null)
                throw new PrexoniteException(operand + " cannot be supplied to ~String.Increment");
            if (sop.Length == 0)
                result = String.CreatePValue("");
            else
                result = String.CreatePValue(System.String.Concat(sop, sop));
            return true;
        }

        public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
        {
            String sop = operand.Value as String;
            if (sop == null)
                throw new PrexoniteException(operand + " cannot be supplied to ~String.Decrement");
            if (sop.Length == 0)
                result = String.CreatePValue("");
            else
                result = String.CreatePValue(sop.Substring(0, sop.Length - 1));
            return true;
        }

        public override bool Addition(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            String left = leftOperand.CallToString(sctx);
            String right = rightOperand.CallToString(sctx);
            result = System.String.Concat(left, right);
            return true;
        }

        public override bool Multiply(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            String left;
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
                throw new PrexoniteException("~String.Multiply requires one operand to be a string.");

            if (left.Length == 0)
            {
                result = String.CreatePValue("");
                return true;
            }
            else if (right > -1)
            {
                StringBuilder res = new StringBuilder();
                for (Int32 i = right; i > 0; i--)
                    res.Append(left);
                result = res.ToString();
                return true;
            }
            else
                return false;
        }

        public override bool Equality(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) == 0;
            return true;
        }

        public override bool Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) != 0;
            return true;
        }

        public override bool GreaterThan(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) > 0;
            return true;
        }

        public override bool GreaterThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand,
                                                out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) >= 0;
            return true;
        }

        public override bool LessThan(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) < 0;
            return true;
        }

        public override bool LessThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand,
                                             out PValue result)
        {
            result =
                StringComparer.Ordinal.Compare(leftOperand.CallToString(sctx), rightOperand.CallToString(sctx)) <= 0;
            return true;
        }

        protected override bool InternalConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit,
                                                  out PValue result)
        {
            result = null;
            if (target is ObjectPType)
            {
                switch (Type.GetTypeCode((target as ObjectPType).ClrType))
                {
                    case TypeCode.String:
                        result = CreateObject((string) subject.Value);
                        goto ret;
                }
            }

            ret:
            return result != null;
        }

        protected override bool InternalConvertFrom(StackContext sctx, PValue subject, bool useExplicit,
                                                    out PValue result)
        {
            result = null;
            ObjectPType subjT = subject.Type as ObjectPType;
            if (subjT != null)
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
    }
}