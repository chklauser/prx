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
    public class AstUnaryOperator : AstNode,
                                    IAstEffect,
                                    IAstHasExpressions,
                                    IAstPartiallyApplicable
    {
        private IAstExpression _operand;
        private UnaryOperator _operator;

        public SymbolInterpretations ImplementationInterpretation { get; set; }

        public string ImplementationId { get; set; }

        public AstUnaryOperator(
            string file, int line, int column, UnaryOperator op, IAstExpression operand, SymbolInterpretations implementationInterpretation, string implementationId)
            : base(file, line, column)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");
            _operator = op;
            _operand = operand;
            ImplementationInterpretation = implementationInterpretation;
            ImplementationId = implementationId;
        }

        internal static AstUnaryOperator Create(Parser p, UnaryOperator op, IAstExpression operand)
        {
            string id;
            SymbolInterpretations interpretation;
            
            switch (op)
            {
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PostIncrement:
                    interpretation = Resolve(p, OperatorNames.Prexonite.Addition, out id);
                    break;
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostDecrement:
                    interpretation = Resolve(p, OperatorNames.Prexonite.Subtraction, out id);
                    break;
                default:
                    interpretation = Resolve(p, OperatorNames.Prexonite.GetName(op), out id);
                    break;
            }
            return new AstUnaryOperator(p.scanner.File, p.t.line, p.t.col, op, operand, interpretation,id);
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {_operand}; }
        }

        public UnaryOperator Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public IAstExpression Operand
        {
            get { return _operand; }
            set { _operand = value; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            _OptimizeNode(target, ref _operand);
            if (_operand is AstConstant)
            {
                var constOperand = (AstConstant) _operand;
                var valueOperand = constOperand.ToPValue(target);
                PValue result;
                switch (_operator)
                {
                    case UnaryOperator.UnaryNegation:
                        if (valueOperand.UnaryNegation(target.Loader, out result))
                            goto emitConstant;
                        break;
                    case UnaryOperator.LogicalNot:
                        if (valueOperand.LogicalNot(target.Loader, out result))
                            goto emitConstant;
                        break;
                    case UnaryOperator.OnesComplement:
                        if (valueOperand.OnesComplement(target.Loader, out result))
                            goto emitConstant;
                        break;
                    case UnaryOperator.PreIncrement:
                        if (valueOperand.Increment(target.Loader, out result))
                            goto emitConstant;
                        break;
                    case UnaryOperator.PreDecrement:
                        if (valueOperand.Decrement(target.Loader, out result))
                            goto emitConstant;
                        break;
                    case UnaryOperator.PostIncrement:
                    case UnaryOperator.PostDecrement:
                        //No optimization allowed/needed here
                        break;
                }
                goto emitFull;

                emitConstant:
                return AstConstant.TryCreateConstant(target, this, result, out expr);
                emitFull:
                return false;
            }

            //Try other optimizations
            switch (_operator)
            {
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.LogicalNot:
                case UnaryOperator.OnesComplement:
                    var doubleNegation = _operand as AstUnaryOperator;
                    if (doubleNegation != null && doubleNegation._operator == _operator)
                    {
                        expr = doubleNegation._operand;
                        return true;
                    }
                    break;
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostIncrement:
                case UnaryOperator.PostDecrement:
                    //No optimization
                    break;
            }
            expr = null;
            return false;
        }

        #endregion

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            var symbol = _operand as AstGetSetSymbol;
            var isVariable = symbol != null && symbol.IsObjectVariable;
            var complex = _operand as AstGetSet;
            var isAssignable = complex != null;
            switch (_operator)
            {
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PostIncrement:
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostDecrement:
                    if (isVariable) //The easy way
                    {
                        OpCode opc;
                        if (_operator == UnaryOperator.PostIncrement ||
                            _operator == UnaryOperator.PreIncrement)
                            if (symbol.Interpretation == SymbolInterpretations.GlobalObjectVariable)
                                opc = OpCode.incglob;
                            else
                                opc = OpCode.incloc;
                        else if (symbol.Interpretation == SymbolInterpretations.GlobalObjectVariable)
                            opc = OpCode.decglob;
                        else
                            opc = OpCode.decloc;
                        target.Emit(this, opc, symbol.Id);
                    }
                    else if (isAssignable)
                    {
                        //The get/set fallback
                        complex = complex.GetCopy();
                        var assignment = new AstModifyingAssignment(
                            complex.File, complex.Line, complex.Column, _operator ==
                                                                        UnaryOperator.PostIncrement ||
                                                                        _operator == UnaryOperator.PreIncrement
                                                                            ?
                                                                                BinaryOperator.Addition
                                                                            : BinaryOperator.Subtraction, complex,ImplementationInterpretation,ImplementationId);
                        if (complex.Call == PCall.Get)
                            complex.Arguments.Add(new AstConstant(File, Line, Column, 1));
                        else
                            complex.Arguments[complex.Arguments.Count - 1] =
                                new AstConstant(File, Line, Column, 1);
                        complex.Call = PCall.Set;
                        assignment.EmitCode(target);
                    }
                    else
                        throw new PrexoniteException(
                            "Node of type " + _operand.GetType() +
                            " does not support increment/decrement operators.");
                    break;
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.LogicalNot:
                case UnaryOperator.OnesComplement:
                default:
                    break; //No effect
            }
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            switch (_operator)
            {
                case UnaryOperator.LogicalNot:
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.OnesComplement:
                    var call = new AstGetSetSymbol(File, Line, Column, PCall.Get, ImplementationId,
                                                   ImplementationInterpretation);
                    call.Arguments.Add(_operand);
                    call.EmitCode(target);
                    break;
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PreIncrement:
                    ((IAstEffect)this).DoEmitEffectCode(target);
                    _operand.EmitCode(target);
                    break;
                case UnaryOperator.PostDecrement:
                case UnaryOperator.PostIncrement:
                    _operand.EmitCode(target);
                    ((IAstEffect)this).DoEmitEffectCode(target);
                    break;
            }
        }

        public override bool CheckForPlaceholders()
        {
            AstTypecheck typecheck;
            return base.CheckForPlaceholders() || Operand.IsPlaceholder() ||
                   (Operator == UnaryOperator.LogicalNot 
                   && (typecheck = Operand as AstTypecheck) != null && typecheck.CheckForPlaceholders());
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var typecheck = Operand as AstTypecheck;
            //Special handling of `? is not {TypeExpr}`
            //  the typecheck.IsInverted flag is only set for the
            //      `? is not {TypeExpr}`
            //  syntax, and not for
            //      `not ? is {TypeExpr}`
            //  for consistency reasons
            if (Operator == UnaryOperator.LogicalNot && typecheck != null && typecheck.IsInverted)
            {
                //Expression is something like (? is not T)
                //emit ((? is T) then (not ?))
                var thenCmd = new AstGetSetSymbol(File, Line, Column, PCall.Get, Engine.ThenAlias,
                                                  SymbolInterpretations.Command);
                var notOp = new AstUnaryOperator(File, Line, Column, UnaryOperator.LogicalNot,
                                                 new AstPlaceholder(File, Line, Column, 0), ImplementationInterpretation,
                                                 ImplementationId);
                var partialTypecheck = new AstTypecheck(File, Line, Column, typecheck.Subject, typecheck.Type);
                thenCmd.Arguments.Add(partialTypecheck);
                thenCmd.Arguments.Add(notOp);

                thenCmd.EmitCode(target);
            }
            else
            {
                //Just emit the operator normally, the appropriate mechanism will kick in
                DoEmitCode(target);
            }
        }
    }

    public enum UnaryOperator
    {
        None,
        UnaryNegation,
        LogicalNot,
        OnesComplement,
        PreIncrement,
        PreDecrement,
        PostIncrement,
        PostDecrement
    }
}