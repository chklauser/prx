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

using System.Globalization;
using System.Reflection;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerStepThroughAttribute;

#endregion

namespace Prexonite.Types;

[PTypeLiteral(nameof(Char))]
public class CharPType : PType, ICilCompilerAware
{
    #region Singleton

    public static CharPType Instance { [NoDebug] get; }

    static CharPType()
    {
        Instance = new();
    }

    [NoDebug]
    CharPType()
    {
    }

    #endregion

    #region Static: CreatePValue

    public static PValue CreatePValue(char c)
    {
        return new(c, Instance);
    }

    public static PValue CreatePValue(int i)
    {
        return new((char) i, Instance);
    }

    #endregion

    #region PType interface

    public override bool TryConstruct(StackContext sctx, PValue[] args, [NotNullWhen(true)] out PValue? result)
    {
        char c;
        result = null;

        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1 || args[0].IsNull)
        {
            c = '\0';
        }
        else if (args[0].TryConvertTo(sctx, Char, out var v))
        {
            c = (char) v.Value!;
        }
        else if (args[0].TryConvertTo(sctx, Int, false, out v))
        {
            c = (char) (int) v.Value!;
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
        string? id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));
        if (args == null)
            throw new ArgumentNullException(nameof(args));
        if (id == null)
            id = "";
        var c = (char) subject.Value!;
        CultureInfo? ci;
        switch (id.ToLowerInvariant())
        {
            case "getnumericvalue":
                result = char.GetNumericValue(c);
                break;
            case "getunicodecategory":
                result = sctx.CreateNativePValue(char.GetUnicodeCategory(c));
                break;
            case "iscontrol":
                result = char.IsControl(c);
                break;
            case "isdigit":
                result = char.IsDigit(c);
                break;
            case "ishighsurrogate":
                result = char.IsHighSurrogate(c);
                break;
            case "isletter":
                result = char.IsLetter(c);
                break;
            case "isletterordigit":
                result = char.IsLetterOrDigit(c);
                break;
            case "islower":
                result = char.IsLower(c);
                break;
            case "islowsurrogate":
                result = char.IsLowSurrogate(c);
                break;
            case "isnumber":
                result = char.IsNumber(c);
                break;
            case "ispunctuation":
                result = char.IsPunctuation(c);
                break;
            case "issurrogate":
                result = char.IsSurrogate(c);
                break;
            case "issymbol":
                result = char.IsSymbol(c);
                break;
            case "isupper":
                result = char.IsUpper(c);
                break;
            case "iswhitespace":
                result = char.IsWhiteSpace(c);
                break;
            case "tolower":
                if (args.Length > 0 && args[0].TryConvertTo(sctx, false, out ci))
                    result = char.ToLower(c, ci);
                else
                    result = char.ToLower(c);
                break;
            case "toupper":
                if (args.Length > 0 && args[0].TryConvertTo(sctx, false, out ci))
                    result = char.ToUpper(c, ci);
                else
                    result = char.ToUpper(c);
                break;
            case "tolowerinvariant":
                result = char.ToLowerInvariant(c);
                break;
            case "toupperinvariant":
                result = char.ToUpperInvariant(c);
                break;
            case "length":
                result = 1;
                break;

            default:
                //Try CLR dynamic call
                var clrint = Object[subject.ClrType!];
                if (!clrint.TryDynamicCall(sctx, subject, args, call, id, out result))
                    result = null;
                break;
        }

        return result != null;
    }

    public override bool TryStaticCall(
        StackContext sctx, PValue[] args, PCall call, string id, [NotNullWhen(true)] out PValue? result)
    {
        //Try CLR static call
        var clrint = Object[typeof (int)];
        if (clrint.TryStaticCall(sctx, args, call, id, out result))
            return true;

        return false;
    }

    protected override bool InternalConvertTo(
        StackContext sctx, PValue subject, PType target, bool useExplicit, [NotNullWhen(true)] out PValue? result)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (subject == null || subject.IsNull)
            throw new ArgumentNullException(nameof(subject));
        if ((object) target == null)
            throw new ArgumentNullException(nameof(target));

        result = null;
        var c = (char) subject.Value!;
        var bi = target.ToBuiltIn();

        if (useExplicit)
        {
            switch (bi)
            {
                case BuiltIn.Object:
                    var clrType = ((ObjectPType) target).ClrType;
                    result = Type.GetTypeCode(clrType) switch
                    {
                        TypeCode.Byte => new(Convert.ToByte(c), target),
                        _ => result,
                    };
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
                    result = Type.GetTypeCode(clrType) switch
                    {
                        TypeCode.Char => new(c, target),
                        TypeCode.Int32 => new((int) c, target),
                        TypeCode.Object =>
                            // explicit boxing
                            new(c, Object[typeof(object)]),
                        _ => result,
                    };
                    break;
            }
        }

        return result != null;
    }

    protected override bool InternalConvertFrom(
        StackContext sctx, PValue subject, bool useExplicit, [NotNullWhen(true)] out PValue? result)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (subject == null || subject.IsNull)
            throw new ArgumentNullException(nameof(subject));

        var source = subject.Type;
        var bi = source.ToBuiltIn();

        result = null;

        if (useExplicit)
        {
            switch (bi)
            {
                case BuiltIn.String:
                    var s = (string) subject.Value!;
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
                    result = (char) (int) subject.Value!;
                    break;
                case BuiltIn.Object:
                    var clrType = ((ObjectPType) source).ClrType;
                    var tc = Type.GetTypeCode(clrType);
                    result = tc switch
                    {
                        TypeCode.Byte => (char) subject.Value!,
                        TypeCode.Int32 => (char) (int) subject.Value!,
                        TypeCode.Char => (char) subject.Value!,
                        _ => result,
                    };

                    if (result == null &&
                        source.TryConvertTo(sctx, subject, Object[typeof (char)], useExplicit,
                            out result))
                    {
                        result = (char) result.Value!;
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

    const int Hashcode = 361633961;
    public const string Literal = nameof(Char);

    public override int GetHashCode()
    {
        return Hashcode;
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

    bool _tryConvert(StackContext sctx, PValue pv, out char c)
    {
        c = '\0';
        switch (pv.Type.ToBuiltIn())
        {
            case BuiltIn.Char:
                c = (char) pv.Value!;
                return true;

            case BuiltIn.Int:
                c = (char) (int) pv.Value!;
                return true;

            case BuiltIn.Null:
                return true;

            case BuiltIn.String:
                var s = (string) pv.Value!;
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
                if (pv.TryConvertTo(sctx, Char, false, out var converted))
                    return _tryConvert(sctx, converted, out c);
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
        [NotNullWhen(true)] out PValue? result)
    {
        result = null;

        if (_tryConvert(sctx, leftOperand, out var left) &&
            _tryConvert(sctx, rightOperand, out var right))
            result = left == right;

        return result != null;
    }

    public override bool Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand,
        out PValue result)
    {
        if (!_tryConvert(sctx, leftOperand, out var left) ||
            !_tryConvert(sctx, rightOperand, out var right))
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

    static readonly MethodInfo _getCharPType =
        typeof(PType).GetProperty(nameof(Char))?.GetGetMethod() ??
        throw new InvalidOperationException($"Cannot find property {nameof(PType)}.{nameof(Char)} getter.");

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