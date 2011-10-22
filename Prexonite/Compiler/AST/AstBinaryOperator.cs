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

using System;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    ///     Represents all binary operators.
    /// </summary>
    public class AstBinaryOperator : AstNode,
                                     IAstExpression,
                                     IAstHasExpressions
    {
        private IAstExpression _leftOperand;
        private IAstExpression _rightOperand;
        private BinaryOperator _operator;
        private SymbolEntry _implementation;

        internal static AstBinaryOperator Create(Parser parser, IAstExpression leftOperand,
            BinaryOperator op,
            IAstExpression rightOperand)
        {
            string id;
            var impl = Resolve(parser, OperatorNames.Prexonite.GetName(op));
            return new AstBinaryOperator(parser.scanner.File, parser.t.line, parser.t.col,
                leftOperand, op,
                rightOperand,impl);
        }

        /// <summary>
        ///     Creates a new binary operator node.
        /// </summary>
        /// <param name = "file">The file that caused this node to be created.</param>
        /// <param name = "line">The line that caused this node to be created.</param>
        /// <param name = "column">The column that caused this node to be created.</param>
        /// <param name = "leftOperand">The left operand of the expression.</param>
        /// <param name = "op">The operator.</param>
        /// <param name = "rightOperand">The right operand of the expression.</param>
        /// <param name = "implementation">Describes the Prexonite entity that is used for the implementation (a command or function entry in most cases)</param>
        /// <seealso cref = "BinaryOperator" />
        /// <seealso cref = "IAstExpression" />
        public AstBinaryOperator(
            string file,
            int line,
            int column,
            IAstExpression leftOperand,
            BinaryOperator op,
            IAstExpression rightOperand,
            SymbolEntry implementation)
            : base(file, line, column)
        {
            if (implementation == null)
                throw new ArgumentNullException("implementation");

            _leftOperand = leftOperand;
            _rightOperand = rightOperand;
            _implementation = implementation;
            _operator = op;
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {_leftOperand, _rightOperand}; }
        }

        public IAstExpression LeftOperand
        {
            get { return _leftOperand; }
            set { _leftOperand = value; }
        }

        public IAstExpression RightOperand
        {
            get { return _rightOperand; }
            set { _rightOperand = value; }
        }

        public BinaryOperator Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public SymbolEntry Implementation
        {
            get { return _implementation; }
        }

        #endregion

        /// <summary>
        ///     Emits the code for this node.
        /// </summary>
        /// <param name = "target">The target to which to write the code to.</param>
        protected override void DoEmitCode(CompilerTarget target)
        {
            var call = new AstGetSetSymbol(File, Line, Column, PCall.Get, Implementation);
            call.Arguments.Add(_leftOperand);
            call.Arguments.Add(_rightOperand);
            call.EmitCode(target);
        }

        #region Optimization

        /// <summary>
        ///     Tries to optimize the binary operator expression.
        /// </summary>
        /// <param name = "target">The context in which to perform the optimization.</param>
        /// <param name = "expr">A possible replacement for the called node.</param>
        /// <returns>True, if <paramref name = "expr" /> contains a replacement for the called node. 
        ///     False otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         Note that <c>false</c> as the return value doesn't mean that the node cannot be 
        ///         optimized. It just cannot be replaced by <paramref name = "expr" />.
        ///     </para>
        ///     <para>
        ///         Also, <paramref name = "expr" /> is only defined if the method call returns <c>true</c>. Don't use it otherwise.
        ///     </para>
        /// </remarks>
        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //The coalecence and cast operators are handled separately.
            if (_operator == BinaryOperator.Coalescence)
            {
                var coal = new AstCoalescence(File, Line, Column);
                coal.Expressions.Add(_leftOperand);
                coal.Expressions.Add(_rightOperand);
                expr = coal;
                _OptimizeNode(target, ref expr);
                return true;
            }
            else if (_operator == BinaryOperator.Cast)
            {
                var T = _rightOperand as IAstType;
                if (T == null)
                    throw new PrexoniteException(
                        String.Format(
                            "The right hand side of a cast operation must be a type expression (in {0} on line {1}).",
                            File,
                            Line));
                expr = new AstTypecast(File, Line, Column, _leftOperand, T);
                _OptimizeNode(target, ref expr);
                return true;
            }

            expr = null;

            //Let children do optimization
            _OptimizeNode(target, ref _leftOperand);
            _OptimizeNode(target, ref _rightOperand);

            //Constant folding
            var leftConstant = _leftOperand as AstConstant;
            var rightConstant = _rightOperand as AstConstant;
            var constant = leftConstant != null && rightConstant != null;

            var left = leftConstant != null ? leftConstant.ToPValue(target) : null;
            var right = rightConstant != null ? rightConstant.ToPValue(target) : null;
            PValue result;

            PValue neutral;

            switch (_operator)
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
                            expr = leftConstant ?? _leftOperand;
                            return true;
                        }
                    }
                    else if (left != null)
                    {
                        if ((left.Equality(target.Loader, new PValue(0, PType.Int), out neutral) &&
                            (bool) neutral.Value))
                        {
                            //left operand is the neutral element 0 => 0 + right = right
                            expr = rightConstant ?? _rightOperand;
                            return true;
                        }
                    }

                    //Check for already existing concat nodes.
                    AstStringConcatenation concat;
                    //left is concat?
                    if ((concat = _leftOperand as AstStringConcatenation) != null)
                    {
                        concat.Arguments.Add(_rightOperand);
                        expr = _GetOptimizedNode(target, concat);
                        return true;
                    } //right is concat?
                    else if ((concat = _rightOperand as AstStringConcatenation) != null)
                    {
                        concat.Arguments.Insert(0, _leftOperand);
                        expr = _GetOptimizedNode(target, concat);
                        return true;
                    }

                    //Check if a new concat can be created
                    if (left != null && left.Type is StringPType)
                    {
                        //Can create concat
                        expr = _GetOptimizedNode(
                            target,
                            new AstStringConcatenation(
                                File,
                                Line,
                                Column,
                                Implementation,
                                rightConstant ?? _rightOperand));

                        return true;
                    }
                    else if (right != null && right.Type is StringPType)
                    {
                        //Can create concat
                        expr = _GetOptimizedNode(
                            target,
                            new AstStringConcatenation(
                                File,
                                Line,
                                Column,
                                Implementation,
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
                        expr = leftConstant ?? _leftOperand;
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
                        expr = rightConstant ?? _rightOperand;
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
                        //right operand is the neutral element 1 => left / 1 = left
                        expr = leftConstant ?? _leftOperand;
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
                        result = Math.Pow(Convert.ToDouble(rleft.Value),
                            Convert.ToDouble(rright.Value));
                        goto emitConstant;
                    }
                    if (right != null)
                    {
                        neutral = new PValue(1, PType.Int);
                        if ((right.Equality(target.Loader, neutral, out neutral) &&
                            (bool) neutral.Value))
                        {
                            //right operand is the neutral element 1 => left ^ 1 = left
                            expr = leftConstant ?? _leftOperand;
                            return true;
                        }
                        else
                        {
                            goto emitFull;
                        }
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
        ///     Returns a string representation of the AstNode.
        /// </summary>
        /// <returns>A string representation of the AstNode.</returns>
        public override string ToString()
        {
            return
                String.Format(
                    "({0}) {1} ({2})",
                    _leftOperand,
                    Enum.GetName(typeof (BinaryOperator), _operator),
                    _rightOperand);
        }
    }

    /// <summary>
    ///     Represents the binary operators supported by <see cref = "AstBinaryOperator" />.
    /// </summary>
    /// <seealso cref = "AstBinaryOperator" />
    /// <seealso cref = "AstUnaryOperator" />
    /// <seealso cref = "AstNode" />
    /// <seealso cref = "IAstExpression" />
    public enum BinaryOperator
    {
        /// <summary>
        ///     No operator, invalid in most contexts.
        /// </summary>
        None,
        //Binary operators with a direct equivalent in assembler code

        /// <summary>
        ///     The addition operator
        /// </summary>
        Addition,

        /// <summary>
        ///     The subtraction operator
        /// </summary>
        Subtraction,

        /// <summary>
        ///     The multiplication operator
        /// </summary>
        Multiply,

        /// <summary>
        ///     The division operator
        /// </summary>
        Division,

        /// <summary>
        ///     The modulus division operator
        /// </summary>
        Modulus,

        /// <summary>
        ///     The exponential operator
        /// </summary>
        Power,

        /// <summary>
        ///     The bitwise AND operator
        /// </summary>
        BitwiseAnd,

        /// <summary>
        ///     The bitwise OR operator
        /// </summary>
        BitwiseOr,

        /// <summary>
        ///     The bitwise XOR operator
        /// </summary>
        ExclusiveOr,

        /// <summary>
        ///     The equality operator
        /// </summary>
        Equality,

        /// <summary>
        ///     The inequality operator
        /// </summary>
        Inequality,

        /// <summary>
        ///     The greater than operator
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     The greater than or equal operator
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        ///     The less than operator
        /// </summary>
        LessThan,

        /// <summary>
        ///     The less than or equal operator
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        ///     The ??-operator.
        /// </summary>
        /// <remarks>
        ///     Will result in a AstCoalescence node when optimized.
        /// </remarks>
        Coalescence,

        /// <summary>
        ///     Entry for ~= expressions.
        /// </summary>
        /// <remarks>
        ///     Will result in a AstTypecast node when optimized.
        /// </remarks>
        Cast
    }
}