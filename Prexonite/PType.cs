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
using System.Runtime.Serialization;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Types
{

    public abstract class PType
    {
        #region Built-In Types

        public enum BuiltIn
        {
            None,
            Real,
            Int,
            String,
            Null,
            Bool,
            Object,
            List,
            Hash
        }

        public bool IsBuiltIn
        {
            [NoDebug]
            get { return ToBuiltIn() != BuiltIn.None; }
        }

        public BuiltIn ToBuiltIn()
        {
            Type thisType = GetType();
            if (thisType == typeof(RealPType))
                return BuiltIn.Real;
            if (thisType == typeof(IntPType))
                return BuiltIn.Int;
            if (thisType == typeof(StringPType))
                return BuiltIn.String;
            if (thisType == typeof(NullPType))
                return BuiltIn.Null;
            if (thisType == typeof(BoolPType))
                return BuiltIn.Bool;
            if (thisType == typeof(ObjectPType))
                return BuiltIn.Object;
            if (thisType == typeof(ListPType))
                return BuiltIn.List;
            if(thisType == typeof(HashPType))
                return BuiltIn.Hash;

            return BuiltIn.Null;
        }

        public static PType GetBuiltIn(BuiltIn biType)
        {
            switch (biType)
            {
                case BuiltIn.Real:
                    return Real;
                case BuiltIn.Int:
                    return Int;
                case BuiltIn.String:
                    return String;
                case BuiltIn.Null:
                    return Null;
                case BuiltIn.Bool:
                    return Bool;
                case BuiltIn.Object:
                    return Object[typeof(object)];
                case BuiltIn.List:
                    return List;
                case BuiltIn.Hash:
                    return Hash;
                default:
                    return null;
            }
        }

        public static RealPType Real
        {
            [NoDebug()]
            get { return RealPType.Instance; }
        }

        public static IntPType Int
        {
            [NoDebug()]
            get { return IntPType.Instance; }
        }

        public static StringPType String
        {
            [NoDebug()]
            get { return StringPType.Instance; }
        }

        public static NullPType Null
        {
            [NoDebug()]
            get { return NullPType.Instance; }
        }

        public static BoolPType Bool
        {
            [NoDebug()]
            get { return BoolPType.Instance; }
        }

        public static ListPType List
        {
            [NoDebug]
            get { return ListPType.Instance; }
        }

        public static HashPType Hash
        {
            [NoDebug]
            get { return HashPType.Instance; }
        }

        private static PrexoniteObjectTypeProxy pobjfacade = new PrexoniteObjectTypeProxy();

        public static PrexoniteObjectTypeProxy Object
        {
            [NoDebug()]
            get { return pobjfacade; }
        }

        //[NoDebug()]
        public class PrexoniteObjectTypeProxy
        {
            private readonly ObjectPType CharObj = new ObjectPType(typeof(Char));
            private readonly ObjectPType ByteObj = new ObjectPType(typeof(Byte));
            private readonly ObjectPType SByteObj = new ObjectPType(typeof(SByte));
            private readonly ObjectPType Int16Obj = new ObjectPType(typeof(Int16));
            private readonly ObjectPType UInt16Obj = new ObjectPType(typeof(UInt16));
            private readonly ObjectPType Int32Obj = new ObjectPType(typeof(Int32));
            private readonly ObjectPType UInt32Obj = new ObjectPType(typeof(UInt32));
            private readonly ObjectPType Int64Obj = new ObjectPType(typeof(Int64));
            private readonly ObjectPType UInt64Obj = new ObjectPType(typeof(UInt64));
            private readonly ObjectPType BooleanObj = new ObjectPType(typeof(Boolean));
            private readonly ObjectPType StringObj = new ObjectPType(typeof(String));
            private readonly ObjectPType DecimalObj = new ObjectPType(typeof(Decimal));
            private readonly ObjectPType DateTimeObj = new ObjectPType(typeof(DateTime));
            private readonly ObjectPType TimeSpanObj = new ObjectPType(typeof(TimeSpan));
            private readonly ObjectPType ListOfPTypeObj = new ObjectPType(typeof(List<PValue>));

            internal PrexoniteObjectTypeProxy()
            {
            }

            public PValue CreatePValue(object value)
            {
                if (value == null)
                    return Null.CreatePValue();
                else
                    return this[value.GetType()].CreatePValue(value);
            }

            public ObjectPType this[Type clrType]
            {
                get
                {
                    if (clrType == typeof(Char))
                        return CharObj;
                    else if (clrType == typeof(Byte))
                        return ByteObj;
                    else if (clrType == typeof(SByte))
                        return SByteObj;
                    else if (clrType == typeof(Int16))
                        return Int16Obj;
                    else if (clrType == typeof(UInt16))
                        return UInt16Obj;
                    else if (clrType == typeof(Int32))
                        return Int32Obj;
                    else if (clrType == typeof(UInt32))
                        return UInt32Obj;
                    else if (clrType == typeof(Int64))
                        return Int64Obj;
                    else if (clrType == typeof(UInt64))
                        return UInt64Obj;
                    else if (clrType == typeof(Boolean))
                        return BooleanObj;
                    else if (clrType == typeof(String))
                        return StringObj;
                    else if (clrType == typeof(Decimal))
                        return DecimalObj;
                    else if (clrType == typeof(DateTime))
                        return DateTimeObj;
                    else if (clrType == typeof(TimeSpan))
                        return TimeSpanObj;
                    else if (clrType == typeof(List<PValue>))
                        return ListOfPTypeObj;
                    else if (clrType == typeof(PValueHashtable))
                        return PValueHashtable.ObjectType;
                    else if (clrType == typeof(PValueKeyValuePair))
                        return PValueKeyValuePair.ObjectType;
                    else
                        return new ObjectPType(clrType);
                }
            }

            public ObjectPType this[StackContext sctx, string clrTypeName]
            {
                get { return new ObjectPType(sctx, clrTypeName); }
            }
        }

        #endregion

        [NoDebug()]
        public virtual PValue CreatePValue(object value)
        {
            return new PValue(value, this);
        }

        #region Type interface

        public abstract bool TryDynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
                                            out PValue result);

        public abstract bool TryStaticCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result);
        public abstract bool TryContruct(StackContext sctx, PValue[] args, out PValue result);

        [NoDebug()]
        public virtual PValue Construct(StackContext sctx, PValue[] args)
        {
            PValue result;
            if (TryContruct(sctx, args, out result))
                return result;
            else
                throw new InvalidCallException("Cannot construct a " + ToString() + " with the supplied arguments.");
        }

        #region Indirect Call

        public virtual bool IndirectCall(StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            result = null;
            return false;
        }

        public PValue IndirectCall(StackContext sctx, PValue subject, PValue[] args)
        {
            PValue ret;
            if (IndirectCall(sctx, subject, args, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " (" + ToString() + ") does not support indirect calls.");
        }

        #endregion

        #region Operators

        #region Failing-Variations

        public PValue UnaryNegation(StackContext sctx, PValue operand)
        {
            PValue ret;
            if (UnaryNegation(sctx, operand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name +
                                               " does not support the UnaryNegation operator.");
        }

        public PValue LogicalNot(StackContext sctx, PValue operand)
        {
            PValue ret;
            if (LogicalNot(sctx, operand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the LogicalNot operator.");
        }

        public PValue OnesComplement(StackContext sctx, PValue operand)
        {
            PValue ret;
            if (OnesComplement(sctx, operand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name +
                                               " does not support the OnesComplement operator.");
        }

        public PValue Increment(StackContext sctx, PValue operand)
        {
            PValue ret;
            if (Increment(sctx, operand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Increment operator.");
        }

        public PValue Decrement(StackContext sctx, PValue operand)
        {
            PValue ret;
            if (Decrement(sctx, operand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Decrement operator.");
        }

        //Binary
        public PValue Addition(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Addition(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Addition operator.");
        }

        public PValue Subtraction(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Subtraction(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Subtraction operator.");
        }

        public PValue Multiply(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Multiply(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Multiply operator.");
        }

        public PValue Division(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Division(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Division operator.");
        }

        public PValue Modulus(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Modulus(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Modulus operator.");
        }

        public PValue BitwiseAnd(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (BitwiseAnd(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the BitwiseAnd operator.");
        }

        public PValue BitwiseOr(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (BitwiseOr(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the BitwiseOr operator.");
        }

        public PValue ExclusiveOr(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (ExclusiveOr(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the ExclusiveOr operator.");
        }

        public PValue Equality(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Equality(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Equality operator.");
        }

        public PValue Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (Inequality(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the Inequality operator.");
        }

        public PValue GreaterThan(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (GreaterThan(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the GreaterThan operator.");
        }

        public PValue GreaterThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (GreaterThanOrEqual(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name +
                                               " does not support the GreaterThanOrEqual operator.");
        }

        public PValue LessThan(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (LessThan(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name + " does not support the LessThan operator.");
        }

        public PValue LessThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand)
        {
            PValue ret;
            if (LessThanOrEqual(sctx, leftOperand, rightOperand, out ret))
                return ret;
            else
                throw new InvalidCallException("PType " + GetType().Name +
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
        public virtual bool Addition(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool Subtraction(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool Multiply(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool Division(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool Modulus(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        //Bitwise
        public virtual bool BitwiseAnd(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool BitwiseOr(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool ExclusiveOr(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        //Comparision
        public virtual bool Equality(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool Inequality(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool GreaterThan(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool GreaterThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand,
                                               out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool LessThan(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            return false;
        }

        public virtual bool LessThanOrEqual(StackContext sctx, PValue leftOperand, PValue rightOperand,
                                            out PValue result)
        {
            result = null;
            return false;
        }

        #endregion //Try-Variants

        #endregion //Operators

        [NoDebug()]
        public virtual PValue DynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id)
        {
            PValue result;
            if (TryDynamicCall(sctx, subject, args, call, id, out result))
                return result;
            else
                throw new InvalidCallException("Cannot call '" + id + "' on object of PType " + ToString() + ".");
        }

        [NoDebug()]
        public virtual PValue StaticCall(StackContext sctx, PValue[] args, PCall call, string id)
        {
            PValue result;
            if (TryStaticCall(sctx, args, call, id, out result))
                return result;
            else
                throw new InvalidCallException("Cannot call '" + id + "' on PType " + ToString() + ".");
        }

        #endregion //Type Interface

        #region xConvertTo methods

        [NoDebug()]
        public PValue ConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit)
        {
            PValue result;
            if (TryConvertTo(sctx, subject, target, useExplicit, out result))
                return result;
            else
                throw new InvalidConversionException("Cannot" + (useExplicit ? "explicitly" : "implicitly") +
                                                     " convert " + subject.Type + " to " + target +
                                                     ".");
        }

        [NoDebug()]
        public PValue ConvertTo(StackContext sctx, PValue subject, PType target)
        {
            return ConvertTo(sctx, subject, target, false);
        }

        [NoDebug()]
        public PValue ExplicitlyConvertTo(StackContext sctx, PValue subject, PType target)
        {
            return ConvertTo(sctx, subject, target, true);
        }

        [NoDebug()]
        public PValue ConvertTo(StackContext sctx, PValue subject, Type clrTarget, bool useExplicit)
        {
            if (clrTarget == null)
                throw new ArgumentNullException("clrTarget");
            return ConvertTo(sctx, subject, Object[clrTarget], useExplicit);
        }

        [NoDebug()]
        public PValue ConvertTo(StackContext sctx, PValue subject, Type clrTarget)
        {
            return ConvertTo(sctx, subject, clrTarget, false);
        }

        [NoDebug()]
        public PValue ExplicitlyConvertTo(StackContext sctx, PValue subject, Type clrTarget)
        {
            return ConvertTo(sctx, subject, clrTarget, true);
        }

        [NoDebug()]
        public bool TryConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit, out PValue result)
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

        [NoDebug()]
        public bool TryConvertTo(StackContext sctx, PValue subject, PType target, out PValue result)
        {
            return TryConvertTo(sctx, subject, target, false, out result);
        }

        [NoDebug()]
        public bool TryConvertTo(StackContext sctx, PValue subject, Type clrTarget, bool useExplicit, out PValue result)
        {
            if (clrTarget == null)
                throw new ArgumentNullException("clrTarget");
            return TryConvertTo(sctx, subject, Object[clrTarget], useExplicit, out result);
        }

        [NoDebug()]
        public bool TryConvertTo(StackContext sctx, PValue subject, Type clrTarget, out PValue result)
        {
            return TryConvertTo(sctx, subject, clrTarget, false, out result);
        }

        protected abstract bool InternalConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit,
                                                  out PValue result);

        protected abstract bool InternalConvertFrom(StackContext sctx, PValue subject, bool useExplicit,
                                                    out PValue result);

        #endregion

        #region Comparision

        [NoDebug()]
        public bool IsEqual(PType otherType)
        {
            if (GetHashCode() != otherType.GetHashCode())
                return false;
            if (InternalIsEqual(otherType))
                return true;
            else
                return otherType.InternalIsEqual(this);
        }

        protected abstract bool InternalIsEqual(PType otherType);

        [NoDebug]
        public static bool operator ==(PType left, PType right)
        {
            if ((object) left == null && (object) right == null)
                return true;
            else if ((object) left == null || (object) right == null)
                return false;
            return left.IsEqual(right);
        }

        [NoDebug]
        public static bool operator !=(PType left, PType right)
        {
            return !(left == right);
        }

        [NoDebug]
        public override bool Equals(object obj)
        {
            if (obj is PType)
                return IsEqual(obj as PType);
            else
                return base.Equals(obj);
        }

        public abstract override int GetHashCode();

        #endregion

        public abstract override string ToString();

        public static bool IsPType(PValue clrType)
        {
            if (clrType == null)
                throw new ArgumentNullException("clrType");
            if (clrType.IsNull)
                return false;
            else
                return IsPType(clrType.ClrType);
        }

        public static bool IsPType(ObjectPType clrType)
        {
            if (clrType == null)
                throw new ArgumentNullException("clrType");
            return IsPType(clrType.ClrType);
        }

        public static bool IsPType(Type clrType)
        {
            if (clrType == null)
                return false;
            return typeof(PType).IsAssignableFrom(clrType);
        }

        public static object[] ToObjectArray(PValue[] input)
        {
            if (input == null)
                return null;
            object[] output = new object[input.Length];
            for (int i = input.Length - 1; i > -1; i--)
                output[i] = input[i].Value;
            return output;
        }

        public static PValue[] ToClrObjectArray(object[] input)
        {
            if (input == null)
                return null;
            PValue[] output = new PValue[input.Length];
            for (int i = input.Length - 1; i > -1; i--)
                output[i] = Object.CreatePValue(input[i]);
            return output;
        }

        protected internal static bool PTypeToClrType<T>(PValue subject, PType target, out PValue result)
        {
            result = null;
            if (target is ObjectPType && ((ObjectPType) target).ClrType == typeof(T))
                result = Object[typeof(T)].CreatePValue(subject.Value);
            return result != null;
        }

        /// <summary>
        /// Creates a new ClrObject[T] by casting subject.Value to T.
        /// </summary>
        /// <typeparam name="T">The clrType of the object to create</typeparam>
        /// <param name="value">The value to create an object PValue for.</param>
        /// <returns>The newly created ClrObject[T]</returns>
        protected internal static PValue CreateObject<T>(T value)
        {
            return Object[typeof(T)].CreatePValue(value);
        }

        static internal int _CombineHashes(int a, int b)
        {
            long h = Math.BigMul(a, b);
            bool isNegative = h < 0;
            if(isNegative)
                h = -h;
            h = h%Int32.MaxValue;
            return (int) (isNegative ? -h : h);
        }
    }

    /// <summary>
    /// Defines whether a call is a "set-call" or a "get-call"
    /// </summary>
    public enum PCall
    {
        /// <summary>
        /// A "get-call". Has a return value.
        /// </summary>
        Get = 0,

        /// <summary>
        /// A "set-call". Has at least one argument and no return value;
        /// </summary>
        Set = 1
    }
}