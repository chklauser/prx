// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast
{
    public class AstUnaryOperator : AstExpr,
                                    IAstHasExpressions,
                                    IAstPartiallyApplicable
    {
        private readonly AstExpr _operand;
        private readonly UnaryOperator _operator;

        public AstUnaryOperator(ISourcePosition position, UnaryOperator op, AstExpr operand)
            : base(position)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");
            
            _operator = op;
            _operand = operand;
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return new[] {_operand}; }
        }

        public UnaryOperator Operator
        {
            get { return _operator; }
        }

        public AstExpr Operand
        {
            get { return _operand; }
        }

        #endregion

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;
            var operand = _operand;
            _OptimizeNode(target, ref operand);
            var constOperand = operand as AstConstant;
            if (constOperand != null) 
            {
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
                return AstConstant.TryCreateConstant(target, Position, result, out expr);
                emitFull:
                return false;
            }

            //Try other optimizations
            switch (_operator)
            {
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.LogicalNot:
                case UnaryOperator.OnesComplement:
                    var doubleNegation = operand as AstUnaryOperator;
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
            return false;
        }

        #endregion

        private void _emitIncrementDecrementCode(CompilerTarget target, StackSemantics value)
        {
            var symbolCall = _operand as AstIndirectCall;
            var symbol = symbolCall == null ? null : symbolCall.Subject as AstReference;
            EntityRef.Variable variableRef = null;
            var isVariable = symbol != null && symbol.Entity.TryGetVariable(out variableRef);
            var isPre = _operator == UnaryOperator.PreDecrement || _operator == UnaryOperator.PreIncrement;
            switch (_operator)
            {
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PostIncrement:
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostDecrement:
                    var isIncrement = 
                        _operator == UnaryOperator.PostIncrement ||
                        _operator == UnaryOperator.PreIncrement;
                    if (isVariable)
                    {
                        Debug.Assert(variableRef != null);
                        EntityRef.Variable.Local localRef;
                        Action loadVar;
                        Action perform;
                        EntityRef.Variable.Global globalRef;

                        // First setup the two actions
                        if (variableRef.TryGetLocalVariable(out localRef))
                        {
                            loadVar = () => target.EmitLoadLocal(Position, localRef.Id);
                            perform =
                                () => target.Emit(Position, isIncrement ? OpCode.incloc : OpCode.decloc, localRef.Id);
                        }
                        else if(variableRef.TryGetGlobalVariable(out globalRef))
                        {
                            loadVar = () => target.EmitLoadGlobal(Position, globalRef.Id, globalRef.ModuleName);

                            perform =
                                () =>
                                target.Emit(Position, isIncrement ? OpCode.incglob : OpCode.decglob, globalRef.Id,
                                            globalRef.ModuleName);
                        }
                        else
                        {
                            throw new InvalidOperationException("Found variable entity that is neither a global nor a local variable.");
                        }

                        // Then decide in what order to apply them.
                        if (!isPre && value == StackSemantics.Value)
                        {
                            loadVar();
                        }

                        perform();

                        if (isPre && value == StackSemantics.Value)
                        {
                            loadVar();
                        }
                    }
                    else
                        throw new PrexoniteException(
                            "Node of type " + _operand.GetType() +
                                " does not support increment/decrement operators.");
                    break;
                // ReSharper disable RedundantCaseLabel
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.LogicalNot:
                case UnaryOperator.OnesComplement:
                // ReSharper restore RedundantCaseLabel
                // ReSharper disable RedundantEmptyDefaultSwitchBranch
                default:
                    break; //No effect
                // ReSharper restore RedundantEmptyDefaultSwitchBranch
            }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            switch (stackSemantics)
            {
                case StackSemantics.Value:
                    _emitValueCode(target);
                    break;
                case StackSemantics.Effect:
                    _emitIncrementDecrementCode(target, stackSemantics);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("stackSemantics");
            }
        }

        private void _emitValueCode(CompilerTarget target)
        {
            switch (_operator)
            {
                case UnaryOperator.LogicalNot:
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.OnesComplement:
                    target.Loader.ReportMessage(
                        Message.Error(
                            Resources.AstUnaryOperator__NonIncrementDecrement,
                            Position, MessageClasses.ParserInternal));
                    break;
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PreIncrement:
                    _emitIncrementDecrementCode(target, StackSemantics.Value);
                    break;
                case UnaryOperator.PostDecrement:
                case UnaryOperator.PostIncrement:
                    _emitIncrementDecrementCode(target, StackSemantics.Value);
                    break;
            }
        }

        public override bool CheckForPlaceholders()
        {
            AstTypecheck typecheck;
            return base.CheckForPlaceholders() || Operand.IsPlaceholder() ||
                (Operator == UnaryOperator.LogicalNot
                    && (typecheck = Operand as AstTypecheck) != null &&
                        typecheck.CheckForPlaceholders());
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            //Just emit the operator normally, the appropriate mechanism will kick in
            // further down the AST
            DoEmitCode(target,StackSemantics.Value);
        }
    }

    public enum UnaryOperator
    {
        [PublicAPI]
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