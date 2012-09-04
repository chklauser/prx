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
using System.Diagnostics;
using Prexonite.Compiler.Symbolic.Compatibility;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstModifyingAssignment : AstScopedExpr, IAstHasExpressions
    {
        private BinaryOperator _setModifier;
        private AstGetSet _modifyingAssignment;

        public SymbolEntry Implementation { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="AstModifyingAssignment"/>.
        /// </summary>
        /// <param name="file">The source file that contains the code that is the basis for this AST node.</param>
        /// <param name="line">The line in the source file that contains the code that is the basis for this AST node.</param>
        /// <param name="column">The offset at which the the code that is the basis for this AST node begins.</param>
        /// <param name="setModifier">The operator used in the modifying assignment.</param>
        /// <param name="complex">The left-hand side of the modifying assignment.</param>
        /// <param name="implementation">The implementation for the operator (only for arithmetic operators)</param>
        /// <param name="lexicalScope"> </param>
        public AstModifyingAssignment(string file, int line, int column, BinaryOperator setModifier, AstGetSet complex, SymbolEntry implementation, AstBlock lexicalScope)
            : base(file, line, column,lexicalScope)
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
                    if (implementation == null)
                        throw new PrexoniteException(
                            "An implementation id is required for the operator " +
                                Enum.GetName(typeof (BinaryOperator), setModifier));
                    break;
            }
            Implementation = implementation;
        }

        // Warning: This method is used on the parser, automatic refactoring will fail.
        internal static AstExpr _Create(Parser p, BinaryOperator setModifier,
            AstGetSet complex)
        {
            var id = OperatorNames.Prexonite.GetName(setModifier);

            var impl = id == null ? null : _ResolveOperator(p, id).ToSymbolEntry();

            return new AstModifyingAssignment(p.scanner.File, p.t.line, p.t.col, setModifier,
                    complex, impl, p.CurrentBlock);   
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return new AstExpr[] {_modifyingAssignment}; }
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

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            AstExpr newAssignment;
            if (_modifyingAssignment.TryOptimize(target, out newAssignment) &&
                newAssignment is AstGetSet)
                _modifyingAssignment = (AstGetSet) newAssignment;
            expr = null;
            return false;
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var justEffect = stackSemantics == StackSemantics.Effect;
            switch (_setModifier)
            {
                case BinaryOperator.Coalescence:
                    {
                        var assignment = _modifyingAssignment.GetCopy();

                        var getVariation = _modifyingAssignment.GetCopy();
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
                            var cond = new AstCondition(this, LexicalScope,check);
                            cond.IfBlock.Add(assignment);
                            cond.EmitEffectCode(target);
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
                            cond.EmitValueCode(target);
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
                            assignment.Arguments[assignment.Arguments.Count - 1] as AstTypeExpr; //~T
                        if (T == null)
                            throw new PrexoniteException(
                                String.Format(
                                    "The right hand side of a cast operation must be a type expression (in {0} on line {1}).",
                                    File,
                                    Line));
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstTypecast(File, Line, Column, getVariation, T); //a(x,y,a(x,y)~T)=
                        assignment.EmitCode(target, stackSemantics);
                    }
                    break;
                case BinaryOperator.None:
                    _modifyingAssignment.EmitCode(target, stackSemantics);
                    break;
                default: // +=, *= etc.
                    {
                        if (Implementation == null)
                        {
                            target.Loader.Errors.Add(Message.Create(MessageSeverity.Error,
                                                            string.Format(
                                                                Resources.AstModifyingAssignment_AssignmentModifierNotSupported,
                                                                Enum.GetName(typeof(BinaryOperator),
                                                                             SetModifier)),
                                                            _modifyingAssignment,MessageClasses.InvalidModifyingAssignment));
                            target.Emit(_modifyingAssignment, OpCode.nop);
                            return;
                        }

                        if (_modifyingAssignment.Arguments.Count < 1)
                        {
                            target.Loader.Errors.Add(Message.Create(MessageSeverity.Error,
                                                            Resources.AstModifyingAssignment_No_RHS, _modifyingAssignment,MessageClasses.InvalidModifyingAssignment));
                            target.Emit(_modifyingAssignment, OpCode.nop);
                            return;
                        }

                        //Without more detailed information, a Set call with a set modifier has to be expressed using 
                        //  conventional set call and binary operator nodes.
                        //Note that code generator for this original node is completely bypassed.
                        var assignment = _modifyingAssignment.GetCopy();
                        var getVersion = _modifyingAssignment.GetCopy();
                        Debug.Assert(getVersion.Call == PCall.Set,"Assuming that _modifyingAssignment is a Set call.");
                        getVersion.Call = PCall.Get;
                        getVersion.Arguments.RemoveAt(getVersion.Arguments.Count - 1);

                        var modification = new AstBinaryOperator(
                            File,
                            Line,
                            Column,
                            getVersion,
                            _setModifier,
                            _modifyingAssignment.Arguments[
                                _modifyingAssignment.Arguments.Count - 1],
                            Implementation, LexicalScope);

                        assignment.Arguments[assignment.Arguments.Count - 1] = modification;
                        
                        assignment.EmitCode(target, stackSemantics);
                    }
                    break;
            }

        }
    }
}