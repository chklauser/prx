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
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Types;

/// <summary>
///     Representation of a Type in the Prexonite virtual machine.
/// </summary>
public abstract class PType : IObject
{
    #region Built-In Types

    /// <summary>
    ///     Lightweight identification of built-in types.
    /// </summary>
    public enum BuiltIn
    {
        /// <summary>
        ///     Not a built-in type
        /// </summary>
        None,

        /// <summary>
        ///     <see cref = "PType.Real" />
        /// </summary>
        Real,

        /// <summary>
        ///     <see cref = "PType.Int" />
        /// </summary>
        Int,

        /// <summary>
        ///     <see cref = "PType.String" />
        /// </summary>
        String,

        /// <summary>
        ///     <see cref = "PType.Null" />
        /// </summary>
        Null,

        /// <summary>
        ///     <see cref = "PType.Bool" />
        /// </summary>
        Bool,

        /// <summary>
        ///     <see cref = "PType.Object" />
        /// </summary>
        Object,

        /// <summary>
        ///     <see cref = "PType.List" />
        /// </summary>
        List,

        /// <summary>
        ///     <see cref = "PType.Hash" />
        /// </summary>
        Hash,

        /// <summary>
        ///     <see cref = "PType.Char" />
        /// </summary>
        Char,

        /// <summary>
        ///     <see cref = "PType.Structure" />
        /// </summary>
        Structure
    }

    /// <summary>
    ///     Indicates whether the current type belongs to the set of <see cref = "BuiltIn" /> types.
    /// </summary>
    public bool IsBuiltIn
    {
        [DebuggerStepThrough]
        get => ToBuiltIn() != BuiltIn.None;
    }

    /// <summary>
    ///     Reduces the current type to a <see cref = "BuiltIn" /> enumeration value.
    /// </summary>
    /// <returns>A <see cref = "BuiltIn" /> enumeration value.</returns>
    public BuiltIn ToBuiltIn()
    {
        var thisType = GetType();
        if (thisType == typeof (RealPType))
            return BuiltIn.Real;
        if (thisType == typeof (IntPType))
            return BuiltIn.Int;
        if (thisType == typeof (StringPType))
            return BuiltIn.String;
        if (thisType == typeof (NullPType))
            return BuiltIn.Null;
        if (thisType == typeof (BoolPType))
            return BuiltIn.Bool;
        if (thisType == typeof (ObjectPType))
            return BuiltIn.Object;
        if (thisType == typeof (ListPType))
            return BuiltIn.List;
        if (thisType == typeof (HashPType))
            return BuiltIn.Hash;
        if (thisType == typeof (CharPType))
            return BuiltIn.Char;
        if (thisType == typeof (StructurePType))
            return BuiltIn.Structure;

        return BuiltIn.Null;
    }

    /// <summary>
    ///     Returns a reference to a built-in runtime type.
    /// </summary>
    /// <param name = "biType">The built-in type.</param>
    /// <returns>A reference to a built-in runtime type.</returns>
    public static PType GetBuiltIn(BuiltIn biType)
    {
        return biType switch
        {
            BuiltIn.Real => Real,
            BuiltIn.Int => Int,
            BuiltIn.String => String,
            BuiltIn.Null => Null,
            BuiltIn.Bool => Bool,
            BuiltIn.Object => Object[typeof(object)],
            BuiltIn.List => List,
            BuiltIn.Hash => Hash,
            BuiltIn.Char => Char,
            BuiltIn.Structure => Structure,
            _ => null
        };
    }

    public static RealPType Real
    {
        [DebuggerStepThrough]
        get => RealPType.Instance;
    }

    public static IntPType Int
    {
        [DebuggerStepThrough]
        get => IntPType.Instance;
    }

    public static StringPType String
    {
        [DebuggerStepThrough]
        get => StringPType.Instance;
    }

    public static NullPType Null
    {
        [DebuggerStepThrough]
        get => NullPType.Instance;
    }

    public static BoolPType Bool
    {
        [DebuggerStepThrough]
        get => BoolPType.Instance;
    }

    public static ListPType List
    {
        [DebuggerStepThrough]
        get => ListPType.Instance;
    }

    public static HashPType Hash
    {
        [DebuggerStepThrough]
        get => HashPType.Instance;
    }

    public static CharPType Char
    {
        [DebuggerStepThrough]
        get => CharPType.Instance;
    }

    public static StructurePType Structure
    {
        [DebuggerStepThrough]
        get => StructurePType.Instance;
    }

    public const string StaticCallFromStackId = "StaticCall\\FromStack";
    public const string ConstructFromStackId = "Construct\\FromStack";

    public static PrexoniteObjectTypeProxy Object { [DebuggerStepThrough] get; } = new();

    /// <summary>
    ///     A proxy that manages instances of the <see cref = "PType.Object" /> type.
    /// </summary>
    public class PrexoniteObjectTypeProxy
    {
        private readonly ObjectPType _charObj = new(typeof (char));
        private readonly ObjectPType _byteObj = new(typeof (byte));
        private readonly ObjectPType _sByteObj = new(typeof (sbyte));
        private readonly ObjectPType _int16Obj = new(typeof (short));
        private readonly ObjectPType _uInt16Obj = new(typeof (ushort));
        private readonly ObjectPType _int32Obj = new(typeof (int));
        private readonly ObjectPType _uInt32Obj = new(typeof (uint));
        private readonly ObjectPType _int64Obj = new(typeof (long));
        private readonly ObjectPType _uInt64Obj = new(typeof (ulong));
        private readonly ObjectPType _booleanObj = new(typeof (bool));
        private readonly ObjectPType _singleObj = new(typeof (float));
        private readonly ObjectPType _doubleObj = new(typeof (double));
        private readonly ObjectPType _stringObj = new(typeof (string));
        private readonly ObjectPType _decimalObj = new(typeof (decimal));
        private readonly ObjectPType _dateTimeObj = new(typeof (DateTime));
        private readonly ObjectPType _timeSpanObj = new(typeof (TimeSpan));
        private readonly ObjectPType _listOfPTypeObj = new(typeof (List<PValue>));

        internal PrexoniteObjectTypeProxy()
        {
        }

        public PValue CreatePValue(object value)
        {
            if (value == null)
                return Null.CreatePValue();
            return this[value.GetType()].CreatePValue(value);
        }

        internal static void _ImplementInCil(CompilerState state, Type clrType)
        {
            if (clrType == typeof (PValueHashtable))
            {
                state.EmitCall(_getPValueHashTableObjectType);
            }
            else if (clrType == typeof (PValueKeyValuePair))
            {
                state.EmitCall(_getPValueKeyValuePairObjectType);
            }
            else
            {
                state.EmitCall(Compiler.Cil.Compiler.GetObjectProxy);
                state.EmitLoadClrType(clrType);
                state.EmitCall(_getAnyObjectType);
            }
        }

        private static readonly MethodInfo _getPValueHashTableObjectType =
            typeof (PValueHashtable).GetProperty("ObjectType").GetGetMethod();

        private static readonly MethodInfo _getPValueKeyValuePairObjectType =
            typeof (PValueKeyValuePair).GetProperty("ObjectType").GetGetMethod();

        private static readonly MethodInfo _getAnyObjectType =
            typeof (PrexoniteObjectTypeProxy).GetProperty("Item", new[] {typeof (Type)}).
                GetGetMethod();

        public ObjectPType this[Type clrType]
        {
            get
            {
                if (clrType == typeof (char))
                    return _charObj;
                if (clrType == typeof (byte))
                    return _byteObj;
                if (clrType == typeof (sbyte))
                    return _sByteObj;
                if (clrType == typeof (short))
                    return _int16Obj;
                if (clrType == typeof (ushort))
                    return _uInt16Obj;
                if (clrType == typeof (int))
                    return _int32Obj;
                if (clrType == typeof (uint))
                    return _uInt32Obj;
                if (clrType == typeof (long))
                    return _int64Obj;
                if (clrType == typeof (ulong))
                    return _uInt64Obj;
                if (clrType == typeof (float))
                    return _singleObj;
                else if (clrType == typeof (double))
                    return _doubleObj;
                else if (clrType == typeof (bool))
                    return _booleanObj;
                else if (clrType == typeof (string))
                    return _stringObj;
                else if (clrType == typeof (decimal))
                    return _decimalObj;
                else if (clrType == typeof (DateTime))
                    return _dateTimeObj;
                else if (clrType == typeof (TimeSpan))
                    return _timeSpanObj;
                else if (clrType == typeof (List<PValue>))
                    return _listOfPTypeObj;
                else if (clrType ==
                         typeof (PValueHashtable))
                    return
                        PValueHashtable.
                            ObjectType;
                else if (clrType ==
                         typeof (
                             PValueKeyValuePair))
                    return
                        PValueKeyValuePair.
                            ObjectType;
                else
                    return
                        new ObjectPType(
                            clrType);
            }
        }

        public ObjectPType this[StackContext sctx, string clrTypeName] => new(sctx, clrTypeName);
    }

    #endregion

    /// <summary>
    ///     Wraps the object in a PValue of the current type.
    /// </summary>
    /// <param name = "value">The CLR object to wrap.</param>
    /// <returns>A PValue object that wraps the supplied object.</returns>
    /// <exception cref = "ArgumentException">Value incompatible with type.</exception>
    [DebuggerStepThrough]
    public virtual PValue CreatePValue(object value)
    {
        return new(value, this);
    }

    #region Type interface

    public abstract bool TryDynamicCall(
        StackContext sctx,
        PValue subject,
        PValue[] args,
        PCall call,
        string id,
        out PValue result);

    public abstract bool TryStaticCall(
        StackContext sctx, PValue[] args, PCall call, string id, out PValue result);

    public abstract bool TryConstruct(StackContext sctx, PValue[] args, out PValue result);

    [DebuggerStepThrough]
    public virtual PValue Construct(StackContext sctx, PValue[] args)
    {
        if (TryConstruct(sctx, args, out var result))
            return result;
        else
        {
            var sb = new StringBuilder();
            sb.Append("Cannot construct a ");
            sb.Append(ToString());
            sb.Append(" with (");
            foreach (var arg in args)
            {
                sb.Append(arg);
                sb.Append(", ");
            }
            if (args.Length > 0)
                sb.Length -= 2;
            sb.Append(").");
            throw new InvalidCallException(sb.ToString());
        }
    }

    #region Indirect Call

    public virtual bool IndirectCall(
        StackContext sctx, PValue subject, PValue[] args, out PValue result)
    {
        result = null;
        return false;
    }

    public PValue IndirectCall(StackContext sctx, PValue subject, PValue[] args)
    {
        if (IndirectCall(sctx, subject, args, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " (" + ToString() +
                ") does not support indirect calls.");
    }

    #endregion

    #region Operators

    #region Failing-Variations

    public PValue UnaryNegation(StackContext sctx, PValue operand)
    {
        if (UnaryNegation(sctx, operand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name +
                " does not support the UnaryNegation operator.");
    }

    public PValue LogicalNot(StackContext sctx, PValue operand)
    {
        if (LogicalNot(sctx, operand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the LogicalNot operator.");
    }

    public PValue OnesComplement(StackContext sctx, PValue operand)
    {
        if (OnesComplement(sctx, operand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name +
                " does not support the OnesComplement operator.");
    }

    public PValue Increment(StackContext sctx, PValue operand)
    {
        if (Increment(sctx, operand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Increment operator.");
    }

    public PValue Decrement(StackContext sctx, PValue operand)
    {
        if (Decrement(sctx, operand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Decrement operator.");
    }

    //Binary
    public PValue Addition(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Addition(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Addition operator.");
    }

    public PValue Subtraction(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Subtraction(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Subtraction operator.");
    }

    public PValue Multiply(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Multiply(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Multiply operator.");
    }

    public PValue Division(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Division(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Division operator.");
    }

    public PValue Modulus(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Modulus(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Modulus operator.");
    }

    public PValue BitwiseAnd(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (BitwiseAnd(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the BitwiseAnd operator.");
    }

    public PValue BitwiseOr(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (BitwiseOr(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the BitwiseOr operator.");
    }

    public PValue ExclusiveOr(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (ExclusiveOr(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the ExclusiveOr operator.");
    }

    public PValue Equality(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Equality(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Equality operator.");
    }

    public PValue Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (Inequality(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the Inequality operator.");
    }

    public PValue GreaterThan(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (GreaterThan(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the GreaterThan operator.");
    }

    public PValue GreaterThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (GreaterThanOrEqual(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name +
                " does not support the GreaterThanOrEqual operator.");
    }

    public PValue LessThan(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (LessThan(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name + " does not support the LessThan operator.");
    }

    public PValue LessThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand)
    {
        if (LessThanOrEqual(sctx, leftOperand, rightOperand, out var ret))
            return ret;
        else
            throw new InvalidCallException(
                "PType " + GetType().Name +
                " does not support the LessThanOrEqual operator.");
    }

    #endregion //Failing-Variantions

    #region Try-Variations

    //Note that overriding operators is optional.
    //
    //Unary
    //
    public virtual bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool LogicalNot(StackContext sctx, PValue operand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool OnesComplement(StackContext sctx, PValue operand, out PValue result)
    {
        result = null;
        return false;
    }

    //Inc/Decrement
    public virtual bool Increment(StackContext sctx, PValue operand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool Decrement(StackContext sctx, PValue operand, out PValue result)
    {
        result = null;
        return false;
    }

    //
    //Binary
    //
    //Arithmetic
    public virtual bool Addition(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool Subtraction(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool Multiply(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool Division(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool Modulus(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    //Bitwise
    public virtual bool BitwiseAnd(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool BitwiseOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool ExclusiveOr(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    //Comparision
    public virtual bool Equality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        if (ReferenceEquals(leftOperand.Value, rightOperand.Value) &&
            leftOperand.Type == rightOperand.Type)
        {
            result = true;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public virtual bool Inequality(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        //Types must be the same for this shortcut to be valid.
        if (ReferenceEquals(leftOperand.Value, rightOperand.Value) &&
            leftOperand.Type == rightOperand.Type)
        {
            result = false;
            return true;
        }
        else
        {
            //We're not sure yet, delegate to more complete implementation of concrete PType.
            result = null;
            return false;
        }
    }

    public virtual bool GreaterThan(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool GreaterThanOrEqual(
        StackContext sctx,
        PValue leftOperand,
        PValue rightOperand,
        out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool LessThan(
        StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
    {
        result = null;
        return false;
    }

    public virtual bool LessThanOrEqual(
        StackContext sctx,
        PValue leftOperand,
        PValue rightOperand,
        out PValue result)
    {
        result = null;
        return false;
    }

    #endregion //Try-Variants

    #endregion //Operators

    public virtual PValue DynamicCall(
        StackContext sctx, PValue subject, PValue[] args, PCall call, string id)
    {
        if (TryDynamicCall(sctx, subject, args, call, id, out var result))
            return result;
        else
            throw new InvalidCallException(
                "Cannot call '" + id + "' on object of PType " + ToString() + ".");
    }

    [DebuggerStepThrough]
    public virtual PValue StaticCall(StackContext sctx, PValue[] args, PCall call, string id)
    {
        if (TryStaticCall(sctx, args, call, id, out var result))
            return result;
        else
            throw new InvalidCallException(
                "Cannot call '" + id + "' on PType " + ToString() + ".");
    }

    #endregion //Type Interface

    #region xConvertTo methods

    /// <summary>
    ///     Converts a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit)
    {
        if (TryConvertTo(sctx, subject, target, useExplicit, out var result))
            return result;
        else
            throw new InvalidConversionException(
                "Cannot " + (useExplicit ? "explicitly" : "implicitly") +
                " convert " + subject.Type + " to " + target +
                ".");
    }

    /// <summary>
    ///     Converts a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ConvertTo(StackContext sctx, PValue subject, PType target)
    {
        return ConvertTo(sctx, subject, target, false);
    }

    /// <summary>
    ///     Explicitly converts a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ExplicitlyConvertTo(StackContext sctx, PValue subject, PType target)
    {
        return ConvertTo(sctx, subject, target, true);
    }

    /// <summary>
    ///     Converts a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "clrTarget">The clr type to convert the value to.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ConvertTo(StackContext sctx, PValue subject, Type clrTarget, bool useExplicit)
    {
        if (clrTarget == null)
            throw new ArgumentNullException(nameof(clrTarget));
        return ConvertTo(sctx, subject, Object[clrTarget], useExplicit);
    }

    /// <summary>
    ///     Converts a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "clrTarget">The clr type to convert the value to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ConvertTo(StackContext sctx, PValue subject, Type clrTarget)
    {
        return ConvertTo(sctx, subject, clrTarget, false);
    }

    /// <summary>
    ///     Explicitly converts a value of this type to a value of a supplie other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "clrTarget">The clr type to convert the value to.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref = "PrexoniteException">Conversion failed.</exception>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public PValue ExplicitlyConvertTo(StackContext sctx, PValue subject, Type clrTarget)
    {
        return ConvertTo(sctx, subject, clrTarget, true);
    }

    /// <summary>
    ///     Tries to convert a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public bool TryConvertTo(
        StackContext sctx, PValue subject, PType target, bool useExplicit, out PValue result)
    {
        //Check if a conversion is needed
        if (subject.Type.IsEqual(target))
        {
            result = subject;
            return true;
        }
        else
            return
                InternalConvertTo(sctx, subject, target, useExplicit, out result) ||
                target.InternalConvertFrom(sctx, subject, useExplicit, out result);
    }

    /// <summary>
    ///     Tries to convert a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public bool TryConvertTo(StackContext sctx, PValue subject, PType target, out PValue result)
    {
        return TryConvertTo(sctx, subject, target, false, out result);
    }

    /// <summary>
    ///     Tries to convert a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "clrTarget">The clr type to convert the value to.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public bool TryConvertTo(
        StackContext sctx, PValue subject, Type clrTarget, bool useExplicit, out PValue result)
    {
        if (clrTarget == null)
            throw new ArgumentNullException(nameof(clrTarget));
        return TryConvertTo(sctx, subject, Object[clrTarget], useExplicit, out result);
    }

    /// <summary>
    ///     Tries to convert a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "clrTarget">The type to convert the value to.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: Use the <see cref = "InternalConvertTo" /> and <see cref = "InternalConvertFrom" /> methods to customise 
    ///     the conversion behaviour of your types.
    /// </remarks>
    [DebuggerStepThrough]
    public bool TryConvertTo(
        StackContext sctx, PValue subject, Type clrTarget, out PValue result)
    {
        return TryConvertTo(sctx, subject, clrTarget, false, out result);
    }

    /// <summary>
    ///     Tries to convert a value of this type to a value of a supplied other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "target">The type to convert the value to.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: <paramref name = "result" /> MUST be null if the conversion fails and MUST NOT be 
    ///     null (but possibly an instance of <see cref = "Null" />) if the conversion succeeds.
    /// </remarks>
    protected abstract bool InternalConvertTo(
        StackContext sctx,
        PValue subject,
        PType target,
        bool useExplicit,
        out PValue result);

    /// <summary>
    ///     Tries to convert a value of the supplied type to a value of the current other type.
    /// </summary>
    /// <param name = "sctx">The stack context in which to try to perform the conversion.</param>
    /// <param name = "subject">The value to convert.</param>
    /// <param name = "useExplicit">True if explicit conversions are to be used. False otherwise.</param>
    /// <param name = "result">The converted value if the conversion was successful, null otherwise.</param>
    /// <returns>True if the conversion succeeded, false otherwise.</returns>
    /// <remarks>
    ///     For implementors: <paramref name = "result" /> MUST be null if the conversion fails and MUST NOT be 
    ///     null (but possibly an instance of <see cref = "Null" />) if the conversion succeeds.
    /// </remarks>
    protected abstract bool InternalConvertFrom(
        StackContext sctx,
        PValue subject,
        bool useExplicit,
        out PValue result);

    #endregion

    #region Comparision

    /// <summary>
    ///     Determines if two types are equal. Also used by <see cref = "Equals" />.
    /// </summary>
    /// <param name = "otherType">The type to check for equality.</param>
    /// <returns>True if the two types are equal. False otherwise.</returns>
    /// <remarks>
    ///     The types are tested for <em>equality</em>, not <em>Identity</em>.
    /// </remarks>
    [DebuggerStepThrough]
    public bool IsEqual(PType otherType)
    {
        if ((object) otherType == null)
            throw new ArgumentNullException(nameof(otherType));
        if (ReferenceEquals(this, otherType))
            return true;
        if (GetHashCode() != otherType.GetHashCode())
            return false;
        if (InternalIsEqual(otherType))
            return true;
        else
            return otherType.InternalIsEqual(this);
    }

    /// <summary>
    ///     Provides the type specific part of equality checking. Reference has already been checked and hashcode is guaranteed to match.
    /// </summary>
    /// <param name = "otherType">A reference to a non-identical type.</param>
    /// <returns>True if the two types are equal. False otherwise.</returns>
    protected abstract bool InternalIsEqual(PType otherType);

    /// <summary>
    ///     Checks two types for equality.
    /// </summary>
    /// <param name = "left">A type</param>
    /// <param name = "right">A type</param>
    /// <returns>True, if the types are equal. False otherwise,</returns>
    [DebuggerStepThrough]
    public static bool operator ==(PType left, PType right)
    {
        if ((object) left == null && (object) right == null)
            return true;
        else if ((object) left == null || (object) right == null)
            return false;
        return left.IsEqual(right);
    }

    /// <summary>
    ///     Checks two types for inequality.
    /// </summary>
    /// <param name = "left">A type</param>
    /// <param name = "right">A type</param>
    /// <returns>True, if the types are inequal. False otherwise,</returns>
    [DebuggerStepThrough]
    public static bool operator !=(PType left, PType right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Checks a CLR type and a Prexonite type for equality.
    /// </summary>
    /// <param name = "left">A clr type</param>
    /// <param name = "right">A type</param>
    /// <returns>True, if the types are equal. False otherwise,</returns>
    public static bool operator ==(Type left, PType right)
    {
        ObjectPType objT;
        if ((object) right == null && left == null)
            return true;
        else if ((object) right == null || left == null)
            return false;
        else if ((object) (objT = right as ObjectPType) == null)
            return false;
        else
            return objT.ClrType == left;
    }

    /// <summary>
    ///     Checks a CLR type and a Prexonite type for inequality.
    /// </summary>
    /// <param name = "left">A clr type</param>
    /// <param name = "right">A type</param>
    /// <returns>True, if the types are inequal. False otherwise,</returns>
    public static bool operator !=(Type left, PType right)
    {
        ObjectPType objT;
        if ((object) right == null && left == null)
            return false;
        else if ((object) right == null || left == null)
            return true;
        else if ((object) (objT = right as ObjectPType) == null)
            return false;
        else
            return objT.ClrType != left;
    }

    /// <summary>
    ///     Checks a CLR type and a Prexonite type for equality.
    /// </summary>
    /// <param name = "left">A type</param>
    /// <param name = "right">A clr type</param>
    /// <returns>True, if the types are equal. False otherwise,</returns>
    public static bool operator ==(PType left, Type right)
    {
        ObjectPType objT;
        if ((object) left == null && right == null)
            return true;
        else if ((object) left == null || right == null)
            return false;
        else if ((object) (objT = left as ObjectPType) == null)
            return false;
        else
            return objT.ClrType == right;
    }

    /// <summary>
    ///     Checks a CLR type and a Prexonite type for inequality.
    /// </summary>
    /// <param name = "left">A type</param>
    /// <param name = "right">A clr type</param>
    /// <returns>True, if the types are inequal. False otherwise,</returns>
    public static bool operator !=(PType left, Type right)
    {
        ObjectPType objT;
        if ((object) left == null && right == null)
            return false;
        else if ((object) left == null || right == null)
            return true;
        else if ((object) (objT = left as ObjectPType) == null)
            return false;
        else
            return objT.ClrType != right;
    }

    /// <summary>
    ///     Determines whether two objects are equal. Uses <see cref = "IsEqual" />.
    /// </summary>
    /// <param name = "obj">The object to check for equality.</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        else if (obj is PType)
            return this == (PType) obj;
        else if (obj is Type)
            return this == (Type) obj;
        else
            return base.Equals(obj);
    }

    /// <summary>
    ///     Returns a 32 bit checksum for the type. Non-parametrized types are to return a constant checksum.
    /// </summary>
    /// <returns>A 32 bit checksum for the type.</returns>
    public abstract override int GetHashCode();

    #endregion

    /// <summary>
    ///     Returns a string representation of the type.
    /// </summary>
    /// <returns>A string representation of the type.</returns>
    public abstract override string ToString();

    /// <summary>
    ///     Checks if the supplied value is a runtime type reference.
    /// </summary>
    /// <param name = "clrType">The value to be checked.</param>
    /// <returns>True if the value is a runtime type reference. False otherwise.</returns>
    public static bool IsPType(PValue clrType)
    {
        if (clrType == null)
            throw new ArgumentNullException(nameof(clrType));
        if (clrType.IsNull)
            return false;
        else
            return IsPType(clrType.ClrType);
    }

    /// <summary>
    ///     Checks if the supplied value is a runtime type reference.
    /// </summary>
    /// <param name = "clrType">The value to be checked.</param>
    /// <returns>True if the value is a runtime type reference. False otherwise.</returns>
    public static bool IsPType(ObjectPType clrType)
    {
        if ((object) clrType == null)
            throw new ArgumentNullException(nameof(clrType));
        return IsPType(clrType.ClrType);
    }

    /// <summary>
    ///     Checks if the supplied value is a runtime type reference.
    /// </summary>
    /// <param name = "clrType">The value to be checked.</param>
    /// <returns>True if the value is a runtime type reference. False otherwise.</returns>
    public static bool IsPType(Type clrType)
    {
        return clrType != null && typeof (PType).IsAssignableFrom(clrType);
    }

    /// <summary>
    ///     Creates a new ClrObject[T] by casting subject.Value to T.
    /// </summary>
    /// <typeparam name = "T">The clrType of the object to create</typeparam>
    /// <param name = "value">The value to create an object PValue for.</param>
    /// <returns>The newly created ClrObject[T]</returns>
    protected internal static PValue CreateObject<T>(T value)
    {
        return Object[typeof (T)].CreatePValue(value);
    }


    bool IObject.TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
        out PValue result)
    {
        result = null;

        if (Engine.StringsAreEqual(id, ConstructFromStackId))
        {
            //Try calling the constructor
            if (!TryConstruct(sctx, args, out result))
                result = null;
        }
        else if (Engine.StringsAreEqual(id, StaticCallFromStackId))
        {
            //Try perform static call using the first arguments as the member id.
            var actualArgs = new PValue[args.Length - 1];
            Array.Copy(args, 1, actualArgs, 0, actualArgs.Length);
            var actualId = args[0].CallToString(sctx);
            if (!TryStaticCall(sctx, actualArgs, call, actualId, out result))
                result = null;
        }

        return result != null;
    }
}

/// <summary>
///     Defines whether a call is a "set-call" or a "get-call"
/// </summary>
public enum PCall
{
    /// <summary>
    ///     A "get-call". Has a return value.
    /// </summary>
    Get = 0,

    /// <summary>
    ///     A "set-call". Has at least one argument and no return value;
    /// </summary>
    Set = 1
}