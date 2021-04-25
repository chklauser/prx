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
using System.Globalization;
using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("Real")]
    public class RealPType : PType, ICilCompilerAware
    {
        #region Singleton pattern

        public static RealPType Instance { get; }

        static RealPType()
        {
            Instance = new RealPType();
        }

        private RealPType()
        {
        }

        #endregion

        #region Static

        public PValue CreatePValue(double value)
        {
            return new(value, Instance);
        }

        public PValue CreatePValue(float value)
        {
            return new((double) value, Instance);
        }

        #endregion

        #region Access interface implementation

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            if (args.Length <= 1)
            {
                result = Real.CreatePValue(0.0);
                return true;
            }
            return args[0].TryConvertTo(sctx, Real, out result);
        }

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            if (Engine.StringsAreEqual(id, "ToString") && args.Length == 0)
            {
                args = new[] { sctx.CreateNativePValue(CultureInfo.InvariantCulture) };
            }
            Object[typeof (double)].TryDynamicCall(sctx, subject, args, call, id, out result);

            return result != null;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            Object[typeof (double)].TryStaticCall(sctx, args, call, id, out result);

            return result != null;
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
                    result = Type.GetTypeCode(((ObjectPType) target).ClrType) switch
                    {
                        TypeCode.Byte => CreateObject((byte) (double) subject.Value),
                        TypeCode.SByte => CreateObject((sbyte) (double) subject.Value),
                        TypeCode.Int32 => CreateObject((int) (double) subject.Value),
                        TypeCode.UInt32 => CreateObject((uint) (double) subject.Value),
                        TypeCode.Int16 => CreateObject((short) (double) subject.Value),
                        TypeCode.UInt16 => CreateObject((ushort) (double) subject.Value),
                        TypeCode.Int64 => CreateObject((long) (double) subject.Value),
                        TypeCode.UInt64 => CreateObject((ulong) (double) subject.Value),
                        TypeCode.Single => CreateObject((float) (double) subject.Value),
                        _ => result
                    };
                }
            }

            // (!useImplicit)
            if (result == null)
            {
                if (target is StringPType)
                    result = String.CreatePValue(((double)subject.Value).ToString(CultureInfo.InvariantCulture));
                else if (target is RealPType)
                    result = Real.CreatePValue((double) subject.Value);
                else if (target is BoolPType)
                    result = Bool.CreatePValue(Math.Abs((double) subject.Value) < double.Epsilon);
                else if (target is ObjectPType objectType)
                {
                    result = Type.GetTypeCode(objectType.ClrType) switch
                    {
                        TypeCode.Double => CreateObject((double) subject.Value),
                        TypeCode.Decimal => CreateObject((decimal) subject.Value),
                        TypeCode.Object => new PValue(subject.Value, Object[typeof(object)]),
                        _ => result
                    };
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
                if (double.TryParse(subject.Value as string, out var value))
                    result = value; //Conversion succeeded
                else if (useExplicit)
                    return false; //Conversion required, provoke error
                else
                    result = 0; //Conversion not required, return default value
            }
            else if (subjectType is ObjectPType)
            {
                if (useExplicit)
                    switch (Type.GetTypeCode((subjectType as ObjectPType).ClrType))
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Char:
                            result = (double) subject.Value;
                            break;
                        case TypeCode.Boolean:
                            result = (bool) subject.Value ? 1.0 : 0.0;
                            break;
                    }

                if (result != null)
                {
                    //(!useExplicit || useExplicit)
                    switch (Type.GetTypeCode((subjectType as ObjectPType).ClrType))
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            result = (double) subject.Value;
                            break;
                    }
                }
            }

            return result != null;
        }

        private static bool _tryConvertToReal(StackContext sctx, PValue operand, out double value)
        {
            return _tryConvertToReal(sctx, operand, out value, true);
        }

        private static bool _tryConvertToReal(
            StackContext sctx, PValue operand, out double value, bool allowNull)
        {
            value = -133.7; //should never surface as value is only used if the method returns true

            switch (operand.Type.ToBuiltIn())
            {
                case BuiltIn.Int:
                case BuiltIn.Real:
                    value = Convert.ToDouble(operand.Value);
                    return true;
                    /* 
                case BuiltIn.String:
                    string rawRight = operand.Value as string;
                    if (!double.TryParse(rawRight, out value))
                        value = 0.0;
                    return true; //*/
                case BuiltIn.Object:
                    if (operand.TryConvertTo(sctx, Real, out var pvRight))
                    {
                        value = (int) pvRight.Value;
                        return true;
                    }
                    break;
                case BuiltIn.Null:
                    value = 0.0;
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

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left + right;

            return result != null;
        }

        public override bool Subtraction(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left - right;

            return result != null;
        }

        public override bool Multiply(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left*right;

            return result != null;
        }

        public override bool Division(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left/right;

            return result != null;
        }

        public override bool Modulus(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left%right;

            return result != null;
        }

        public override bool Equality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left, false) &&
                _tryConvertToReal(sctx, rightOperand, out var right, false))
                result = left == right;

            return result != null;
        }

        public override bool Inequality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left, false) &&
                _tryConvertToReal(sctx, rightOperand, out var right, false))
                result = left != right;

            return result != null;
        }

        public override bool GreaterThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
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

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left >= right;

            return result != null;
        }

        public override bool LessThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
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

            if (_tryConvertToReal(sctx, leftOperand, out var left) &&
                _tryConvertToReal(sctx, rightOperand, out var right))
                result = left <= right;

            return result != null;
        }

        public override bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            if (_tryConvertToReal(sctx, operand, out var op))
                result = -op;

            return result != null;
        }

        public override bool Increment(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            if (_tryConvertToReal(sctx, operand, out var op))
                result = op + 1.0;

            return result != null;
        }

        public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
        {
            result = null;
            if (_tryConvertToReal(sctx, operand, out var op))
                result = op - 1.0;

            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is RealPType;
        }

        private const int _code = -2035946599;

        public override int GetHashCode()
        {
            return _code;
        }

        #endregion

        public const string Literal = "Real";

        public override string ToString()
        {
            return Literal;
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
            state.EmitCall(Compiler.Cil.Compiler.GetRealPType);
        }

        #endregion
    }
}