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
using System.Reflection;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerStepThroughAttribute;

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

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            char c;
            result = null;

            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            PValue v;

            if(args.Length < 1 || args[0].IsNull)
            {
                c = '\0';
            }
            else if(args[0].TryConvertTo(sctx, Char, out v))
            {
                c = (char) v.Value;
            }
            else if(args[0].TryConvertTo(sctx, Int, false,out v))
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
            char c = (char) subject.Value;
            CultureInfo ci;
            switch(id.ToLowerInvariant())
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
                    ObjectPType clrint = Object[subject.ClrType];
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
            ObjectPType clrint = Object[typeof(int)];
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
            if ((object)target == null)
                throw new ArgumentNullException("target"); 

            result = null;
            char c = (char) subject.Value;
            BuiltIn bi = target.ToBuiltIn();

            if(useExplicit)
            {
                switch(bi)
                {
                    case BuiltIn.Object:
                        Type clrType = ((ObjectPType) target).ClrType;
                        switch (Type.GetTypeCode(clrType))
                        {
                            case TypeCode.Byte:
                                result = new PValue(Convert.ToByte(c), target);
                                break;
                        }
                        break;
                }
            }

            if(result == null)
            {
                switch(bi)
                {
                    case BuiltIn.Int:
                        result = (int) c;
                        break;
                    case BuiltIn.String:
                        result = c.ToString();
                        break;
                    case BuiltIn.Object:
                        Type clrType = ((ObjectPType)target).ClrType;
                        switch (Type.GetTypeCode(clrType))
                        {
                            case TypeCode.Char:
                                result = new PValue(c, target);
                                break;
                            case TypeCode.Int32:
                                result = new PValue((Int32)c, target);
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
            
            PType source = subject.Type;
            BuiltIn bi = source.ToBuiltIn();

            result = null;

            if(useExplicit)
            {
                switch(bi)
                {
                    case BuiltIn.String:
                        string s = (string) subject.Value;
                        if (s.Length == 1)
                            result = s[0];
                        break;
                }
            }

            if(result == null)
            {
                switch (bi)
                {
                    case BuiltIn.Int:
                        result = (char) (int) subject.Value;
                        break;
                    case BuiltIn.Object:
                        Type clrType = ((ObjectPType)source).ClrType;
                        TypeCode tc = Type.GetTypeCode(clrType);
                        switch (tc)
                        {
                            case TypeCode.Byte:
                                result = (char)subject.Value;
                                break;
                            case TypeCode.Int32:
                                result = (char) (Int32) subject.Value;
                                break;
                            case TypeCode.Char:
                                result = (char)subject.Value;
                                break;

                        }

                        if(result == null && 
                            source.TryConvertTo(sctx, subject, Object[typeof(char)],useExplicit,out result))
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

        #endregion

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferCustomImplementation;
        }

        private static readonly MethodInfo GetCharPType = typeof(PType).GetProperty("Char").GetGetMethod();

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(GetCharPType);
        }

        #endregion
    }
}
