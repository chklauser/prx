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
using System.Diagnostics;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstModifyingAssignment : AstNode, IAstHasExpressions, IAstEffect
    {
        private BinaryOperator _setModifier;
        private AstGetSet _modifyingAssignment;

        public SymbolInterpretations ImplementationInterpretation { get; set; }
        public string ImplementationId { get; set; }

        public AstModifyingAssignment(string file, int line, int column, BinaryOperator setModifier, AstGetSet complex, SymbolInterpretations implementationInterpretation, string implementationId)
            : base(file, line, column)
        {
            _setModifier = setModifier;
            _modifyingAssignment = complex;

            switch (setModifier)
            {
                case BinaryOperator.Addition:
                case BinaryOperator.Subtraction:
                case BinaryOperator.Multiply:
                case BinaryOperator.Division:
                case BinaryOperator.Modulus:
                case BinaryOperator.Power:
                case BinaryOperator.BitwiseAnd:
                case BinaryOperator.BitwiseOr:
                case BinaryOperator.ExclusiveOr:
                case BinaryOperator.Equality:
                case BinaryOperator.Inequality:
                case BinaryOperator.GreaterThan:
                case BinaryOperator.GreaterThanOrEqual:
                case BinaryOperator.LessThan:
                case BinaryOperator.LessThanOrEqual:
                    if(implementationId == null)
                        throw new PrexoniteException("An implementation id is required for the operator " + Enum.GetName(typeof(BinaryOperator), setModifier));
                    break;
            }
            ImplementationInterpretation = implementationInterpretation;
            ImplementationId = implementationId;
        }

        internal static AstModifyingAssignment Create(Parser p, BinaryOperator setModifier, AstGetSet complex)
        {
            var id = OperatorNames.Prexonite.GetName(setModifier);
            var interpretation = id == null ? SymbolInterpretations.Undefined : Resolve(p, id, out id);
            return new AstModifyingAssignment(p.scanner.File, p.t.line, p.t.col, setModifier, complex, interpretation,
                                              id);
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[]{_modifyingAssignment}; }
        }

        public AstGetSet ModifyingAssignment
        {
            get { return _modifyingAssignment; }
            set { _modifyingAssignment = value; }
        }

        public BinaryOperator SetModifier
        {
            get { return _setModifier; }
            set { _setModifier = value; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            IAstExpression newAssignment;
            if (_modifyingAssignment.TryOptimize(target, out newAssignment) && newAssignment is AstGetSet)
                _modifyingAssignment = (AstGetSet) newAssignment;
            expr = null;
            return false;
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        public void EmitCode(CompilerTarget target, bool justEffect)
        {
            switch(_setModifier)
            {
                case BinaryOperator.Coalescence:
                    {
                        AstGetSet assignment = _modifyingAssignment.GetCopy();

                        AstGetSet getVariation = _modifyingAssignment.GetCopy();
                        getVariation.Call = PCall.Get;
                        //remove last argument (the assigned value)
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);

                        var check =
                            new AstTypecheck(
                                File,
                                Line,
                                Column,
                                getVariation,
                                new AstConstantTypeExpression(File, Line, Column, "Null"));

                        if (justEffect)
                        {
                            //Create a traditional condition
                            var cond = new AstCondition(File, Line, Column, check);
                            cond.IfBlock.Add(assignment);
                            cond.EmitCode(target);
                        }
                        else
                        {
                            //Create a conditional expression
                            var cond =
                                new AstConditionalExpression(File, Line, Column, check)
                                {
                                    IfExpression = assignment,
                                    ElseExpression = getVariation
                                };
                            cond.EmitCode(target);
                        }
                    }
                    break;
                case BinaryOperator.Cast:
                    {
                        // a(x,y) ~= T         //a(x,y,~T)~=
                        //to
                        // a(x,y) = a(x,y)~T   //a(x,y,a(x,y)~T)=
                        var assignment = _modifyingAssignment.GetCopy(); //a'(x,y,~T)~=

                        var getVariation = assignment.GetCopy(); //a''(x,y,~T)=
                        getVariation.Call = PCall.Get; //a''(x,y,~String)
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);
                            //a''(x,y)

                        var T =
                            assignment.Arguments[assignment.Arguments.Count - 1] as IAstType; //~T
                        if (T == null)
                            throw new PrexoniteException(
                                String.Format(
                                    "The right hand side of a cast operation must be a type expression (in {0} on line {1}).",
                                    File,
                                    Line));
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstTypecast(File, Line, Column, getVariation, T); //a(x,y,a(x,y)~T)=
                        if(justEffect)
                            assignment.EmitEffectCode(target);
                        else 
                            assignment.EmitCode(target);
                    }
                    break;
                case BinaryOperator.None:
                    if (justEffect)
                        _modifyingAssignment.EmitEffectCode(target);
                    else
                        _modifyingAssignment.EmitCode(target);
                    break;
                default: // +=, *= etc.
                    {
                        if (ImplementationId == null)
                        {
                            target.Loader.Errors.Add(new ParseMessage(ParseMessageSeverity.Error,
                                                                      string.Format(
                                                                          "The assignment modifier {0} is not supported.",
                                                                          Enum.GetName(typeof (BinaryOperator),
                                                                                       SetModifier)),
                                                                      _modifyingAssignment));
                            target.Emit(_modifyingAssignment, OpCode.nop);
                            return;
                        }

                        if(_modifyingAssignment.Arguments.Count < 1)
                        {
                            target.Loader.Errors.Add(new ParseMessage(ParseMessageSeverity.Error, "Invalid modifying assignment: No RHS.",_modifyingAssignment));
                            target.Emit(_modifyingAssignment,OpCode.nop);
                            return;
                        }
                        
                        //Without more detailed information, a Set call with a set modifier has to be expressed using 
                        //  conventional set call and binary operator nodes.
                        //Note that code generator for this original node is completely bypassed.
                        var assignment = _modifyingAssignment.GetCopy();
                        var getVersion = _modifyingAssignment.GetCopy();
                        getVersion.Call = PCall.Get;
                        getVersion.Arguments.RemoveAt(getVersion.Arguments.Count - 1);
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstBinaryOperator(
                                File,
                                Line,
                                Column,
                                getVersion,
                                _setModifier,
                                _modifyingAssignment.Arguments[_modifyingAssignment.Arguments.Count - 1],
                                ImplementationInterpretation,ImplementationId);
                        if (justEffect)
                            assignment.EmitEffectCode(target);
                        else
                            assignment.EmitCode(target);
                    }
                    break;
            }
        }

        #region IAstEffect Members

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        #endregion
    }
}
