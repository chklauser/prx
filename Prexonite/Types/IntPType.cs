#region

using System;
using System.Collections.Generic;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("Int")]
    public sealed class IntPType : PType, ICilCompilerAware
    {
        #region Singleton Pattern

        private static readonly IntPType instance;

        public static IntPType Instance
        {
            [NoDebug]
            get { return instance; }
        }

        static IntPType()
        {
            instance = new IntPType();
        }

        [NoDebug]
        private IntPType()
        {
        }

        #endregion

        #region Static

        [NoDebug]
        public PValue CreatePValue(byte value)
        {
            return new PValue(value, Instance);
        }

        [NoDebug]
        public PValue CreatePValue(short value)
        {
            return new PValue(value, Instance);
        }

        [NoDebug]
        public PValue CreatePValue(int value)
        {
            return new PValue(value, Instance);
        }

        [NoDebug]
        public PValue CreatePValue(long value)
        {
            return new PValue(value, Instance);
        }

        #endregion

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
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
                throw new ArgumentNullException("sctx");

            result = null;

            switch(id.ToUpperInvariant())
            {
                case "TO":
                    if(args.Length < 1)
                        break;
                    var upperLimitPV = args[0].ConvertTo(sctx, Int,true);
                    var stepPV = args.Length > 1 ? args[1].ConvertTo(sctx, Int, true) : 1;

                    var lowerLimit = (int)subject.Value;
                    var upperLimit = (int) upperLimitPV.Value;
                    var step = (int) stepPV.Value;

                    Console.WriteLine();


                    result = sctx.CreateNativePValue
                        (new Coroutine(new CoroutineContext(sctx, _generateIntegerRange(lowerLimit, step, upperLimit))));
                    break;
            }

            if(result != null)
                return true;

            //Try CLR dynamic call
            var clrint = Object[subject.ClrType];
            if (clrint.TryDynamicCall(sctx, subject, args, call, id, out result))
                return true;

            return false;
        }

        private IEnumerable<PValue> _generateIntegerRange(int lowerLimit, int step, int upperLimit)
        {
            for (var i = lowerLimit; i <= upperLimit; i += step)
                yield return i;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            //Try CLR static call
            ObjectPType clrint = Object[typeof (int)];
            if (clrint.TryStaticCall(sctx, args, call, id, out result))
                return true;

            return false;
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
                    Type clrType = ((ObjectPType) target).ClrType;
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
                else if (target is ObjectPType)
                {
                    Type clrType = ((ObjectPType) target).ClrType;
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
            PType subjectType = subject.Type;
            if (subjectType is StringPType)
            {
                int value;
                if (int.TryParse(subject.Value as string, out value))
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
            else if (subjectType is ObjectPType)
            {
                if (useExplicit)
                    switch (Type.GetTypeCode((subjectType as ObjectPType).ClrType))
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
                            result = ((bool) subject.Value) ? 1 : 0;
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

        [NoDebug]
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
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferCustomImplementation;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(Compiler.Cil.Compiler.GetIntPType);
        }

        #endregion
    }
}