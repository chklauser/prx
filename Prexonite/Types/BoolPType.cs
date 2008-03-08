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

using Prexonite.Compiler.Cil;

namespace Prexonite.Types
{
    [PTypeLiteral("Bool")]
    public class BoolPType : PType, ICilCompilerAware
    {
        #region Singleton

        private BoolPType()
        {
        }

        static BoolPType()
        {
            instance = new BoolPType();
        }

        private static readonly BoolPType instance;

        public static BoolPType Instance
        {
            get { return instance; }
        }

        #endregion

        #region Static

        public static PValue CreateValue(bool value)
        {
            return new PValue(value, Bool);
        }

        public static PValue CreateValue(object value)
        {
            if (value is bool)
                return CreateValue((bool) value);
            else
                return new PValue(value != null, Bool);
        }

        #endregion

        #region Access interface implementation

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            if (args.Length <= 0)
            {
                result = false;
                return true;
            }
            else
            {
                return args[0].TryConvertTo(sctx, Bool, out result);
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
            if (subject.ToObject().TryDynamicCall(sctx, args, call, id, out result))
                return true;

            return false;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (Object[typeof(bool)].TryStaticCall(sctx, args, call, id, out result))
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
            if (target is ObjectPType)
                return
                    Object[typeof(bool)].TryConvertTo(
                        sctx, subject, target, useExplicit, out result);

            result = null;

            if (target is IntPType && useExplicit)
                result = ((bool) subject.Value) ? 1 : 0;
            else if (target is RealPType && useExplicit)
                result = (bool) subject.Value ? 1.0 : 0.0;
            else if (target is StringPType && useExplicit)
                result = (bool) subject.Value ? bool.TrueString : bool.FalseString;
            else if (target is ObjectPType && ((ObjectPType) target).ClrType == typeof(bool))
                result = Object[typeof(bool)].CreatePValue(subject.Value);
            else
                return false;

            return true;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            result = null;
            if (subject.Type is ObjectPType && ((ObjectPType) subject.Type).ClrType == typeof(bool))
                result = (bool) subject.Value;
            else if (useExplicit && subject.Type is StringPType)
            {
                bool parsed;
                if (bool.TryParse(subject.Value as string, out parsed))
                    result = parsed;
                else
                    result = false;
                return true;
            }
            return false;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is BoolPType;
        }

        #endregion

        #region Operators

        private static bool _tryConvertToBool(StackContext sctx, PValue operand, out bool value)
        {
            value = false;

            switch (operand.Type.ToBuiltIn())
            {
                case BuiltIn.Real:
                case BuiltIn.Int:
                case BuiltIn.Bool:
                    value = (bool) operand.Value;
                    return true;
                case BuiltIn.String:
                    if (!bool.TryParse(operand.Value as string, out value))
                    {
                        value = false;
                        return true;
                    }
                    break;
                case BuiltIn.Object:
                    PValue asBool;
                    if (operand.TryConvertTo(sctx, Bool, out asBool))
                    {
                        value = (bool) asBool.Value;
                        return true;
                    }
                    break;
                case BuiltIn.Null:
                    value = false;
                    return true;
            }

            return false;
        }

        public override bool BitwiseAnd(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            bool left,
                 right;
            if (_tryConvertToBool(sctx, leftOperand, out left) &&
                _tryConvertToBool(sctx, rightOperand, out right))
                result = left & right;
            return result != null;
        }

        public override bool BitwiseOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            bool left,
                 right;
            if (_tryConvertToBool(sctx, leftOperand, out left) &&
                _tryConvertToBool(sctx, rightOperand, out right))
                result = left | right;
            return result != null;
        }

        public override bool Equality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            bool left,
                 right;
            if (_tryConvertToBool(sctx, leftOperand, out left) &&
                _tryConvertToBool(sctx, rightOperand, out right))
                result = left == right;
            return result != null;
        }

        public override bool Inequality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            bool left,
                 right;
            if (_tryConvertToBool(sctx, leftOperand, out left) &&
                _tryConvertToBool(sctx, rightOperand, out right))
                result = left != right;
            return result != null;
        }

        public override bool ExclusiveOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            result = null;
            bool left,
                 right;
            if (_tryConvertToBool(sctx, leftOperand, out left) &&
                _tryConvertToBool(sctx, rightOperand, out right))
                result = left ^ right;
            return result != null;
        }

        public override bool LogicalNot(StackContext sctx, PValue operand, out PValue result)
        {
            result = !(bool) operand.Value;
            return true;
        }

        public override bool OnesComplement(StackContext sctx, PValue operand, out PValue result)
        {
            result = !(bool) operand.Value; //Identical to LogicalNot
            return true;
        }

        public override bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
        {
            result = !(bool) operand.Value; //Identical to UnaryNegation
            return true;
        }

        #endregion

        public const string Literal = "Bool";

        private const int _code = -1181897690;

        public override int GetHashCode()
        {
            return _code;
        }

        public override string ToString()
        {
            return Literal;
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
            state.EmitCall(Compiler.Cil.Compiler.GetBoolPType);
        }

        #endregion
    }
}