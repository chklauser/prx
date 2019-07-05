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
using System.Diagnostics.CodeAnalysis;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("Int")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    public sealed class IntPType : PType, ICilCompilerAware
    {
        #region Singleton Pattern

        private static readonly IntPType instance;

        public static IntPType Instance
        {
            [DebuggerStepThrough]
            get { return instance; }
        }

        static IntPType()
        {
            instance = new IntPType();
        }

        [DebuggerStepThrough]
        private IntPType()
        {
        }

        #endregion

        #region Static

        [DebuggerStepThrough]
        public PValue CreatePValue(byte value)
        {
            return new PValue(value, Instance);
        }

        [DebuggerStepThrough]
        public PValue CreatePValue(short value)
        {
            return new PValue(value, Instance);
        }

        [DebuggerStepThrough]
        public PValue CreatePValue(int value)
        {
            return new PValue(value, Instance);
        }

        [DebuggerStepThrough]
        public PValue CreatePValue(long value)
        {
            return new PValue(value, Instance);
        }

        #endregion

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            if (args.Length < 1)
            {
                result = Int.CreatePValue(0);
                return true;
            }
            else
            {
                return args[0].TryConvertTo(sctx, Int, out result);
            }
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
                throw new ArgumentNullException(nameof(sctx));

            result = null;

            switch (id.ToUpperInvariant())
            {
                case "TO":
                    if (args.Length < 1)
                        break;
                    var upperLimitPV = args[0].ConvertTo(sctx, Int, true);
                    var stepPV = args.Length > 1 ? args[1].ConvertTo(sctx, Int, true) : 1;

                    var lowerLimit = (int) subject.Value;
                    var upperLimit = (int) upperLimitPV.Value;
                    var step = (int) stepPV.Value;

                    result = sctx.CreateNativePValue
                        (new Coroutine(new CoroutineContext(sctx,
                            _generateIntegerRange(lowerLimit, step, upperLimit))));
                    break;
            }

            if (result != null)
                return true;

            //Try CLR dynamic call
            var clrint = Object[subject.ClrType];
            return clrint.TryDynamicCall(sctx, subject, args, call, id, out result);
        }

        private static IEnumerable<PValue> _generateIntegerRange(int lowerLimit, int step,
            int upperLimit)
        {
            for (var i = lowerLimit; i <= upperLimit; i += step)
                yield return i;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            //Try CLR static call
            var clrint = Object[typeof (int)];
            return clrint.TryStaticCall(sctx, args, call, id, out result);
        }

        protected override bool InternalConvertTo(
            StackContext sctx,
            PValue subject,
            PType target,
            bool useExplicit,
            out PValue result)
        {
            result = null;

            if (useExplicit)
            {
                if (target is ObjectPType)
                {
                    var clrType = ((ObjectPType) target).ClrType;
                    if (clrType == typeof (Byte))
                        result = CreateObject((Byte) (Int32) subject.Value);
                    else if (clrType == typeof (Char))
                        result = CreateObject(Convert.ToChar((Int32) subject.Value));
                    else if (clrType == typeof (SByte))
                        result = CreateObject((SByte) (Int32) subject.Value);
                    else if (clrType == typeof (Int16))
                        result = CreateObject((Int16) (Int32) subject.Value);
                    else if (clrType == typeof (UInt16))
                        result = CreateObject((UInt16) (Int32) subject.Value);
                }
            }

            // (implicit or explicit
            if (result == null)
            {
                if (target is StringPType)
                    result = String.CreatePValue(subject.Value.ToString());
                else if (target is RealPType)
                    result = Real.CreatePValue((int) subject.Value);
                else if (target is BoolPType)
                    result = Bool.CreatePValue(((int) subject.Value) != 0);
                else if (target is ObjectPType objectType)
                {
                    var clrType = objectType.ClrType;
                    if (clrType == typeof (Int32))
                        result = CreateObject((Int32) subject.Value);
                    else if (clrType == typeof (Double))
                        result = CreateObject((Double) (Int32) subject.Value);
                    else if (clrType == typeof (Single))
                        result = CreateObject((Single) (Int32) subject.Value);
                    else if (clrType == typeof (Decimal))
                        result = CreateObject((Decimal) (Int32) subject.Value);
                    else if (clrType == typeof (Int64))
                        result = CreateObject((Int64) (Int32) subject.Value);
                    else if (clrType == typeof (UInt32))
                        result = CreateObject((UInt32) (Int32) subject.Value);
                    else if (clrType == typeof (UInt64))
                        result = CreateObject((UInt64) (Int32) subject.Value);
                    else if (clrType == typeof(object))
                        result = new PValue(subject.Value, Object[typeof(object)]);
                }
            }

            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            var subjectType = subject.Type;
            if (subjectType is StringPType)
            {
                if (int.TryParse(subject.Value as string, out var value))
                    result = value;
                else if (useExplicit)
                    return false; //Conversion required, provoke error
                else
                    result = 0;
            }
            else if (subjectType is RealPType)
            {
                result = Convert.ToInt32(subject.Value);
            }
            else if (subjectType is ObjectPType objectType)
            {
                if (useExplicit)
                    switch (Type.GetTypeCode(objectType.ClrType))
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Char:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            result = (int) subject.Value;
                            break;
                        case TypeCode.Boolean:
                            result = (bool) subject.Value ? 1 : 0;
                            break;
                    }

                if (result != null)
                {
                    //(!useExplicit || useExplicit)
                    switch (Type.GetTypeCode(objectType.ClrType))
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            result = (int) subject.Value;
                            break;
                    }
                }
            }

            return result != null;
        }

        #region Operators

        private static bool _tryConvertToInt(StackContext sctx, PValue operand, out int value)
        {
            return _tryConvertToInt(sctx, operand, out value, true);
        }

        private static bool _tryConvertToInt(
            StackContext sctx, PValue operand, out int value, bool allowNull)
        {
            value = -1337; //should never surface as value is only used if the method returns true

            switch (operand.Type.ToBuiltIn())
            {
                case BuiltIn.Int:
                    value = Convert.ToInt32(operand.Value);
                    return true;
                    /*
                case BuiltIn.String:
                    string rawRight = operand.Value as string;
                    if (!int.TryParse(rawRight, out value))
                        value = 0;
                    return true; //*/
                case BuiltIn.Object:
                    PValue pvRight;
                    if (operand.TryConvertTo(sctx, Int, out pvRight))
                    {
                        value = (int) pvRight.Value;
                        return true;
                    }
                    break;
                case BuiltIn.Null:
                    value = 0;
                    return allowNull;
                default:
                    break;
            }

            return false;
        }

        public override bool Addition(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left + right;

            return result != null;
        }

        public override bool Subtraction(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left - right;

            return result != null;
        }

        public override bool Multiply(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left*right;

            return result != null;
        }

        public override bool Division(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left/right;

            return result != null;
        }

        public override bool Modulus(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left%right;

            return result != null;
        }

        public override bool BitwiseAnd(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left & right;

            return result != null;
        }

        public override bool BitwiseOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left | right;

            return result != null;
        }

        public override bool ExclusiveOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left ^ right;

            return result != null;
        }

        public override bool Equality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left, false) &&
                _tryConvertToInt(sctx, rightOperand, out right, false))
                result = left == right;

            return result != null;
        }

        public override bool Inequality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left, false) &&
                _tryConvertToInt(sctx, rightOperand, out right, false))
                result = left != right;

            return result != null;
        }

        public override bool GreaterThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left > right;

            return result != null;
        }

        public override bool GreaterThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left >= right;

            return result != null;
        }

        public override bool LessThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left < right;

            return result != null;
        }

        public override bool LessThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            result = null;
            int left;
            int right;

            if (_tryConvertToInt(sctx, leftOperand, out left) &&
                _tryConvertToInt(sctx, rightOperand, out right))
                result = left <= right;

            return result != null;
        }

        public override bool OnesComplement(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            int op;
            if (_tryConvertToInt(sctx, operand, out op))
                result = ~op;

            return result != null;
        }

        public override bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            int op;
            if (_tryConvertToInt(sctx, operand, out op))
                result = -op;

            return result != null;
        }

        public override bool Increment(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            int op;
            if (_tryConvertToInt(sctx, operand, out op))
                result = op + 1;

            return result != null;
        }

        public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            int op;
            if (_tryConvertToInt(sctx, operand, out op))
                result = op - 1;

            return result != null;
        }

        #endregion

        public const string Literal = "Int";

        public override string ToString()
        {
            return Literal;
        }

        [DebuggerStepThrough]
        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is IntPType;
        }

        private const int _code = -408434186;

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
            state.EmitCall(Compiler.Cil.Compiler.GetIntPType);
        }

        #endregion
    }
}