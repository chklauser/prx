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

#region

using System;
using System.Globalization;
using System.Reflection;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerStepThroughAttribute;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("Char")]
    public class CharPType : PType, ICilCompilerAware
    {
        #region Singleton

        private static readonly CharPType instance;

        public static CharPType Instance
        {
            [NoDebug]
            get { return instance; }
        }

        static CharPType()
        {
            instance = new CharPType();
        }

        [NoDebug]
        private CharPType()
        {
        }

        #endregion

        #region Static: CreatePValue

        public static PValue CreatePValue(char c)
        {
            return new PValue(c, instance);
        }

        public static PValue CreatePValue(int i)
        {
            return new PValue((char) i, instance);
        }

        #endregion

        #region PType interface

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            char c;
            result = null;

            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            PValue v;

            if (args.Length < 1 || args[0].IsNull)
            {
                c = '\0';
            }
            else if (args[0].TryConvertTo(sctx, Char, out v))
            {
                c = (char) v.Value;
            }
            else if (args[0].TryConvertTo(sctx, Int, false, out v))
            {
                c = (char) (int) v.Value;
            }
            else
            {
                c = '\0';
            }

            result = c;
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
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (args == null)
                throw new ArgumentNullException("args");
            if (id == null)
                id = "";
            var c = (char) subject.Value;
            CultureInfo ci;
            switch (id.ToLowerInvariant())
            {
                case "getnumericvalue":
                    result = System.Char.GetNumericValue(c);
                    break;
                case "getunicodecategory":
                    result = sctx.CreateNativePValue(System.Char.GetUnicodeCategory(c));
                    break;
                case "iscontrol":
                    result = System.Char.IsControl(c);
                    break;
                case "isdigit":
                    result = System.Char.IsDigit(c);
                    break;
                case "ishighsurrogate":
                    result = System.Char.IsHighSurrogate(c);
                    break;
                case "isletter":
                    result = System.Char.IsLetter(c);
                    break;
                case "isletterordigit":
                    result = System.Char.IsLetterOrDigit(c);
                    break;
                case "islower":
                    result = System.Char.IsLower(c);
                    break;
                case "islowsurrogate":
                    result = System.Char.IsLowSurrogate(c);
                    break;
                case "isnumber":
                    result = System.Char.IsNumber(c);
                    break;
                case "ispunctuation":
                    result = System.Char.IsPunctuation(c);
                    break;
                case "issurrogate":
                    result = System.Char.IsSurrogate(c);
                    break;
                case "issymbol":
                    result = System.Char.IsSymbol(c);
                    break;
                case "isupper":
                    result = System.Char.IsUpper(c);
                    break;
                case "iswhitespace":
                    result = System.Char.IsWhiteSpace(c);
                    break;
                case "tolower":
                    if (args.Length > 0 && args[0].TryConvertTo(sctx, false, out ci))
                        result = System.Char.ToLower(c, ci);
                    else
                        result = System.Char.ToLower(c);
                    break;
                case "toupper":
                    if (args.Length > 0 && args[0].TryConvertTo(sctx, false, out ci))
                        result = System.Char.ToUpper(c, ci);
                    else
                        result = System.Char.ToUpper(c);
                    break;
                case "tolowerinvariant":
                    result = System.Char.ToLowerInvariant(c);
                    break;
                case "toupperinvariant":
                    result = System.Char.ToUpperInvariant(c);
                    break;
                case "length":
                    result = 1;
                    break;

                default:
                    //Try CLR dynamic call
                    var clrint = Object[subject.ClrType];
                    if (!clrint.TryDynamicCall(sctx, subject, args, call, id, out result))
                        result = null;
                    break;
            }

            return result != null;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            //Try CLR static call
            var clrint = Object[typeof (int)];
            if (clrint.TryStaticCall(sctx, args, call, id, out result))
                return true;

            return false;
        }

        protected override bool InternalConvertTo(
            StackContext sctx, PValue subject, PType target, bool useExplicit, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (subject == null || subject.IsNull)
                throw new ArgumentNullException("subject");
            if ((object) target == null)
                throw new ArgumentNullException("target");

            result = null;
            var c = (char) subject.Value;
            var bi = target.ToBuiltIn();

            if (useExplicit)
            {
                switch (bi)
                {
                    case BuiltIn.Object:
                        var clrType = ((ObjectPType) target).ClrType;
                        switch (Type.GetTypeCode(clrType))
                        {
                            case TypeCode.Byte:
                                result = new PValue(Convert.ToByte(c), target);
                                break;
                        }
                        break;
                }
            }

            if (result == null)
            {
                switch (bi)
                {
                    case BuiltIn.Int:
                        result = (int) c;
                        break;
                    case BuiltIn.String:
                        result = c.ToString();
                        break;
                    case BuiltIn.Object:
                        var clrType = ((ObjectPType) target).ClrType;
                        switch (Type.GetTypeCode(clrType))
                        {
                            case TypeCode.Char:
                                result = new PValue(c, target);
                                break;
                            case TypeCode.Int32:
                                result = new PValue((Int32) c, target);
                                break;
                        }
                        break;
                }
            }

            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx, PValue subject, bool useExplicit, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (subject == null || subject.IsNull)
                throw new ArgumentNullException("subject");

            var source = subject.Type;
            var bi = source.ToBuiltIn();

            result = null;

            if (useExplicit)
            {
                switch (bi)
                {
                    case BuiltIn.String:
                        var s = (string) subject.Value;
                        if (s.Length == 1)
                            result = s[0];
                        break;
                }
            }

            if (result == null)
            {
                switch (bi)
                {
                    case BuiltIn.Int:
                        result = (char) (int) subject.Value;
                        break;
                    case BuiltIn.Object:
                        var clrType = ((ObjectPType) source).ClrType;
                        var tc = Type.GetTypeCode(clrType);
                        switch (tc)
                        {
                            case TypeCode.Byte:
                                result = (char) subject.Value;
                                break;
                            case TypeCode.Int32:
                                result = (char) (Int32) subject.Value;
                                break;
                            case TypeCode.Char:
                                result = (char) subject.Value;
                                break;
                        }

                        if (result == null &&
                            source.TryConvertTo(sctx, subject, Object[typeof (char)], useExplicit,
                                out result))
                        {
                            result = (char) result.Value;
                        }
                        break;
                }
            }

            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is CharPType;
        }

        private const int _hashcode = 361633961;
        public const string Literal = "Char";

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return Literal;
        }

        #endregion

        #region Operators

        public override PValue CreatePValue(object value)
        {
            return Convert.ToChar(value);
        }

        private bool _tryConvert(StackContext sctx, PValue pv, out char c)
        {
            c = '\0';
            switch (pv.Type.ToBuiltIn())
            {
                case BuiltIn.Char:
                    c = (char) pv.Value;
                    return true;

                case BuiltIn.Int:
                    c = (char) (int) pv.Value;
                    return true;

                case BuiltIn.Null:
                    return true;

                case BuiltIn.String:
                    var s = (string) pv.Value;
                    if (s.Length == 1)
                    {
                        c = s[0];
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case BuiltIn.Object:
                    if (pv.TryConvertTo(sctx, Char, false, out pv))
                        return _tryConvert(sctx, pv, out c);
                    else
                        return false;

                case BuiltIn.Structure:
                case BuiltIn.Hash:
                case BuiltIn.List:
                case BuiltIn.Bool:
                case BuiltIn.None:
                case BuiltIn.Real:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equality(StackContext sctx, PValue leftOperand, PValue rightOperand,
            out PValue result)
        {
            result = null;

            char left;
            char right;

            if (_tryConvert(sctx, leftOperand, out left) &&
                _tryConvert(sctx, rightOperand, out right))
                result = left == right;

            return result != null;
        }

        public override bool Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand,
            out PValue result)
        {
            char left;
            char right;

            if (!(_tryConvert(sctx, leftOperand, out left)) ||
                !(_tryConvert(sctx, rightOperand, out right)))
                result = false;
            else
                result = left != right;

            return true;
        }

        #endregion

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

        private static readonly MethodInfo _getCharPType =
            typeof (PType).GetProperty("Char").GetGetMethod();

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(_getCharPType);
        }

        #endregion
    }
}