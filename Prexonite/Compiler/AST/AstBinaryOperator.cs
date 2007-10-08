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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// Represents all binary operators.
    /// </summary>
    public class AstBinaryOperator : AstNode,
                                     IAstExpression
    {
        public IAstExpression LeftOperand;
        public IAstExpression RightOperand;
        public BinaryOperator Operator;

        internal AstBinaryOperator(
            Parser p, IAstExpression leftOperand, BinaryOperator op, IAstExpression rightOperand)
            : this(p.scanner.File, p.t.line, p.t.col, leftOperand, op, rightOperand)
        {
        }

        /// <summary>
        /// Creates a new binary operator node.
        /// </summary>
        /// <param name="file">The file that caused this node to be created.</param>
        /// <param name="line">The line that caused this node to be created.</param>
        /// <param name="column">The column that caused this node to be created.</param>
        /// <param name="leftOperand">The left operand of the expression.</param>
        /// <param name="op">The operator.</param>
        /// <param name="rightOperand">The right operand of the expression.</param>
        /// <seealso cref="BinaryOperator"/>
        /// <seealso cref="IAstExpression"/>
        public AstBinaryOperator(
            string file,
            int line,
            int column,
            IAstExpression leftOperand,
            BinaryOperator op,
            IAstExpression rightOperand)
            : base(file, line, column)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            Operator = op;
        }

        /// <summary>
        /// Emits the instruction corresponding to the supplied binary operator.
        /// </summary>
        /// <param name="target">The target to which to write the instruction to.</param>
        /// <param name="op">Any binary operator.</param>
        /// <seealso cref="BinaryOperator"/>
        public static void EmitOperator(CompilerTarget target, BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Addition:
                    target.Emit(OpCode.add);
                    break;
                case BinaryOperator.Subtraction:
                    target.Emit(OpCode.sub);
                    break;
                case BinaryOperator.Multiply:
                    target.Emit(OpCode.mul);
                    break;
                case BinaryOperator.Division:
                    target.Emit(OpCode.div);
                    break;
                case BinaryOperator.Modulus:
                    target.Emit(OpCode.mod);
                    break;
                case BinaryOperator.Power:
                    target.Emit(OpCode.pow);
                    break;
                case BinaryOperator.BitwiseAnd:
                    target.Emit(OpCode.and);
                    break;
                case BinaryOperator.BitwiseOr:
                    target.Emit(OpCode.or);
                    break;
                case BinaryOperator.ExclusiveOr:
                    target.Emit(OpCode.xor);
                    break;
                case BinaryOperator.Equality:
                    target.Emit(OpCode.ceq);
                    break;
                case BinaryOperator.Inequality:
                    target.Emit(OpCode.cne);
                    break;
                case BinaryOperator.GreaterThan:
                    target.Emit(OpCode.cgt);
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    target.Emit(OpCode.cge);
                    break;
                case BinaryOperator.LessThan:
                    target.Emit(OpCode.clt);
                    break;
                case BinaryOperator.LessThanOrEqual:
                    target.Emit(OpCode.cle);
                    break;
            }
        }

        /// <summary>
        /// Emits the operator instruction only
        /// </summary>
        /// <param name="target">The target to which to write the instruction to.</param>
        public void EmitOperator(CompilerTarget target)
        {
            EmitOperator(target, Operator);
        }

        /// <summary>
        /// Emits the code for this node.
        /// </summary>
        /// <param name="target">The target to which to write the code to.</param>
        public override void EmitCode(CompilerTarget target)
        {
            LeftOperand.EmitCode(target);
            RightOperand.EmitCode(target);
            EmitOperator(target);
        }

        #region Optimization

        /// <summary>
        /// Tries to optimize the binary operator expression.
        /// </summary>
        /// <param name="target">The context in which to perform the optimization.</param>
        /// <param name="expr">A possible replacement for the called node.</param>
        /// <returns>True, if <paramref name="expr"/> contains a replacement for the called node. 
        /// False otherwise.</returns>
        /// <remarks>
        /// <para>
        ///     Note that <c>false</c> as the return value doesn't mean that the node cannot be 
        ///     optimized. It just cannot be replaced by <paramref name="expr"/>.
        /// </para>
        /// <para>
        ///     Also, <paramref name="expr"/> is only defined if the method call returns <c>true</c>. Don't use it otherwise.
        /// </para>
        /// </remarks>
        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //The coalecence operator is handled separately.
            if (Operator == BinaryOperator.Coalescence)
            {
                AstCoalescence coal = new AstCoalescence(File, Line, Column);
                coal.Expressions.Add(LeftOperand);
                coal.Expressions.Add(RightOperand);
                expr = coal;
                OptimizeNode(target, ref expr);
                return true;
            }

            expr = null;

            //Let children do optimization
            OptimizeNode(target, ref LeftOperand);
            OptimizeNode(target, ref RightOperand);

            //Constant folding
            AstConstant leftConstant = LeftOperand as AstConstant;
            AstConstant rightConstant = RightOperand as AstConstant;
            bool constant = leftConstant != null && rightConstant != null;

            PValue left = leftConstant != null ? leftConstant.ToPValue(target) : null;
            PValue right = rightConstant != null ? rightConstant.ToPValue(target) : null;
            PValue result;

            PValue neutral;

            switch (Operator)
            {
                case BinaryOperator.Addition:
                    //Constant folding
                    if (constant && left.Addition(target.Loader, right, out result))
                        goto emitConstant;

                    additionRedundancy:
                    if (right != null)
                    {
                        //DO NOT CHECK FOR THE EMPTY STRING! It can change the type of an expression -> handled by StringConcat
                        if ((right.Equality(target.Loader, new PValue(0, PType.Int), out neutral) &&
                             (bool) neutral.Value))
                        {
                            //right operand is the neutral element 0 => left + 0 = left
                            expr = leftConstant == null ? LeftOperand : leftConstant;
                            return true;
                        }
                    }
                    else if (left != null)
                    {
                        if ((left.Equality(target.Loader, new PValue(0, PType.Int), out neutral) &&
                             (bool) neutral.Value))
                        {
                            //left operand is the neutral element 0 => 0 + right = right
                            expr = rightConstant == null ? RightOperand : rightConstant;
                            return true;
                        }
                    }

                    //Check for already existing concat nodes.
                    AstStringConcatenation concat;
                    //left is concat?
                    if ((concat = LeftOperand as AstStringConcatenation) != null)
                    {
                        concat.Arguments.Add(RightOperand);
                        expr = GetOptimizedNode(target, concat);
                        return true;
                    } //right is concat?
                    else if ((concat = RightOperand as AstStringConcatenation) != null)
                    {
                        concat.Arguments.Insert(0, LeftOperand);
                        expr = GetOptimizedNode(target, concat);
                        return true;
                    }

                    //Check if a new concat can be created
                    if (left != null && left.Type is StringPType)
                    {
                        //Can create concat
                        expr = GetOptimizedNode(
                            target,
                            new AstStringConcatenation(
                                File,
                                Line,
                                Column,
                                leftConstant,
                                rightConstant == null ? RightOperand : rightConstant));

                        return true;
                    }
                    else if (right != null && right.Type is StringPType)
                    {
                        //Can create concat
                        expr = GetOptimizedNode(
                            target,
                            new AstStringConcatenation(
                                File,
                                Line,
                                Column,
                                leftConstant == null ? LeftOperand : leftConstant,
                                rightConstant));
                        return true;
                    }
                    else
                        goto emitFull;

                case BinaryOperator.Subtraction:
                    //Constant folding
                    if (constant && left.Subtraction(target.Loader, right, out result))
                        goto emitConstant;

                    //Subtraction shares redundancy code with addition
                    goto additionRedundancy;
                case BinaryOperator.Multiply:
                    //Constant folding
                    if (constant && left.Multiply(target.Loader, right, out result))
                        goto emitConstant;

                    //multiply redundancy
                    if (right != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if (
                            !(right.Equality(target.Loader, neutral, out neutral) &&
                              (bool) neutral.Value))
                            goto emitFull;
                        //right operand is the neutral element 1 => left * 1 = left
                        expr = leftConstant == null ? LeftOperand : leftConstant;
                        return true;
                    }
                    else if (left != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if (
                            !(left.Equality(target.Loader, neutral, out neutral) &&
                              (bool) neutral.Value))
                            goto emitFull;
                        //left operand is the neutral element 1 => 1 * right = right
                        expr = rightConstant == null ? RightOperand : rightConstant;
                        return true;
                    }
                    else
                        goto emitFull;

                case BinaryOperator.Division:
                    //Constant folding
                    if (constant && left.Division(target.Loader, right, out result))
                        goto emitConstant;

                    //multiply redundancy
                    divisionRedundancy:
                    if (right != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if (
                            !(right.Equality(target.Loader, neutral, out neutral) &&
                              (bool) neutral.Value))
                            goto emitFull;
                        //right operand is the neutral element 1 => left * 1 = left
                        expr = leftConstant == null ? LeftOperand : leftConstant;
                        return true;
                    }
                    else
                        goto emitFull;
                case BinaryOperator.Modulus:
                    //Constant folding
                    if (constant && left.Modulus(target.Loader, right, out result))
                        goto emitConstant;

                    //Modulus shares redundancy code with multiply
                    goto divisionRedundancy;
                case BinaryOperator.Power:
                    //Constant folding
                    PValue rleft,
                           rright;
                    if (constant &&
                        left.TryConvertTo(target.Loader, PType.Real, out rleft) &&
                        right.TryConvertTo(target.Loader, PType.Real, out rright))
                    {
                        result = Math.Pow((double) rleft.Value, (double) rright.Value);
                        goto emitConstant;
                    }
                    if (right != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if (
                            !(right.Equality(target.Loader, neutral, out neutral) &&
                              (bool) neutral.Value))
                            goto emitFull;
                        //right operand is the neutral element 1 => left ^ 1 = left
                        expr = leftConstant == null ? LeftOperand : leftConstant;
                        return true;
                    }
                    else if (left != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if (
                            !(left.Equality(target.Loader, neutral, out neutral) &&
                              (bool) neutral.Value))
                            goto emitFull;
                        //left operand is the neutral element 1 => 1 ^ right = 1
                        expr = leftConstant;
                        return true;
                    }
                    else
                        goto emitFull;
                case BinaryOperator.BitwiseAnd:
                    //Constant folding
                    if (constant && left.BitwiseAnd(target.Loader, right, out result))
                        goto emitConstant;

                    if (right != null)
                    {
                        if (!(right.Type is BoolPType))
                            goto emitFull;
                        result = right;
                        goto emitConstant;
                    }
                    else if (left != null)
                    {
                        if (!(left.Type is BoolPType))
                            goto emitFull;
                        result = left;
                        goto emitConstant;
                    }
                    else
                        goto emitFull;
                case BinaryOperator.BitwiseOr:
                    //Constant folding
                    if (constant && left.BitwiseOr(target.Loader, right, out result))
                        goto emitConstant;

                    if (right != null)
                    {
                        if (!(right.Type is BoolPType))
                            goto emitFull;
                        if (right.UnaryNegation(target.Loader, out result))
                            goto emitConstant;
                    }
                    else if (left != null)
                    {
                        if (!(left.Type is BoolPType))
                            goto emitFull;
                        if (left.UnaryNegation(target.Loader, out result))
                            goto emitConstant;
                    }

                    goto emitFull;
                case BinaryOperator.ExclusiveOr:
                    //Constant folding
                    if (constant && left.ExclusiveOr(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.Equality:
                    //Constant folding
                    if (constant && left.Equality(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.Inequality:
                    //Constant folding
                    if (constant && left.Inequality(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.GreaterThan:
                    //Constant folding
                    if (constant && left.GreaterThan(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.GreaterThanOrEqual:
                    //Constant folding
                    if (constant && left.GreaterThanOrEqual(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.LessThan:
                    //Constant folding
                    if (constant && left.LessThan(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
                case BinaryOperator.LessThanOrEqual:
                    //Constant folding
                    if (constant && left.LessThanOrEqual(target.Loader, right, out result))
                        goto emitConstant;
                    else
                        goto emitFull;
            }
            goto emitFull;

            emitConstant:
            return AstConstant.TryCreateConstant(target, this, result, out expr);

            emitFull:
            return false;
        }

        #endregion

        /// <summary>
        /// Returns a string representation of the AstNode.
        /// </summary>
        /// <returns>A string representation of the AstNode.</returns>
        public override string ToString()
        {
            return
                String.Format(
                    "({0}) {1} ({2})",
                    LeftOperand,
                    Enum.GetName(typeof(BinaryOperator), Operator),
                    RightOperand);
        }
    }

    /// <summary>
    /// Represents the binary operators supported by <see cref="AstBinaryOperator"/>.
    /// </summary>
    /// <seealso cref="AstBinaryOperator"/>
    /// <seealso cref="AstUnaryOperator"/>
    /// <seealso cref="AstNode"/>
    /// <seealso cref="IAstExpression"/>
    public enum BinaryOperator
    {
        /// <summary>
        /// No operator, invalid in most contexts.
        /// </summary>
        None,
        //Binary operators with a direct equivalent in assembler code

        /// <summary>
        /// The addition operator
        /// </summary>
        Addition,

        /// <summary>
        /// The subtraction operator
        /// </summary>
        Subtraction,

        /// <summary>
        /// The multiplication operator
        /// </summary>
        Multiply,

        /// <summary>
        /// The division operator
        /// </summary>
        Division,

        /// <summary>
        /// The modulus division operator
        /// </summary>
        Modulus,

        /// <summary>
        /// The exponential operator
        /// </summary>
        Power,

        /// <summary>
        /// The bitwise AND operator
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// The bitwise OR operator
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// The bitwise XOR operator
        /// </summary>
        ExclusiveOr,

        /// <summary>
        /// The equality operator
        /// </summary>
        Equality,

        /// <summary>
        /// The inequality operator
        /// </summary>
        Inequality,

        /// <summary>
        /// The greater than operator
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The greater than or equal operator
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// The less than operator
        /// </summary>
        LessThan,

        /// <summary>
        /// The less than or equal operator
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// The ??-operator.
        /// </summary>
        Coalescence
    }
}