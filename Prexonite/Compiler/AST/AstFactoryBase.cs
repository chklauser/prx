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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstFactoryBase : IAstFactory, IIndirectCall, IObject
    {
        protected AstFactoryBase()
        {
            _bridge = new AstFactoryBridge(this);
        }

        protected abstract AstBlock CurrentBlock { get; }

        // TODO: (Ticket #109) TryUseSymbolEntry and NullNode should not be defined on AstFactoryBase
        [NotNull]
        protected abstract AstGetSet CreateNullNode([NotNull] ISourcePosition position);

        protected abstract bool IsOuterVariable([NotNull] string id);

        protected abstract void RequireOuterVariable([NotNull] string id);

        public abstract void ReportMessage(Message message);

        // TODO: Move constant folding out of AST factory and into operator macros
        [NotNull]
        protected abstract CompilerTarget CompileTimeExecutionContext { get; }

        [NotNull]
        private static readonly ISymbolHandler<List<Message>, Symbol> _listMessages = new ListMessagesHandler();

        private class ListMessagesHandler : ISymbolHandler<List<Message>, Symbol>
        {
            #region Implementation of ISymbolHandler<in List<Message>,out Symbol>

            public Symbol HandleReference(ReferenceSymbol self, List<Message> argument)
            {
                return self;
            }

            public Symbol HandleNil(NilSymbol self, List<Message> argument)
            {
                return self;
            }

            public Symbol HandleExpand(ExpandSymbol self, List<Message> argument)
            {
                return _handleWrapped(self, argument);
            }

            public Symbol HandleMessage(MessageSymbol self, List<Message> argument)
            {
                // Add the symbol to the list and unwrap contents
                argument.Add(self.Message);
                return self.InnerSymbol.HandleWith(this, argument);
            }

            public Symbol HandleDereference(DereferenceSymbol self, List<Message> argument)
            {
                return _handleWrapped(self, argument);
            }

            private Symbol _handleWrapped(WrappingSymbol self, List<Message> argument)
            {
                var s = self.InnerSymbol.HandleWith(this, argument);
                if (s == null)
                    return null;
                else if (ReferenceEquals(s, self.InnerSymbol))
                    return self;
                else
                    return self.With(s);
            }

            public Symbol HandleNamespace(NamespaceSymbol self, List<Message> argument)
            {
                return self;
            }

            #endregion
        }

        /// <summary>
        /// A formal usage of a symbol in the program. Reports messages attached to a symbol and returns a copy of the symbol without these messages.
        /// </summary>
        /// <param name="symbol">The symbol to formally "use".</param>
        /// <param name="position">The position in the source program where the symbol was used.</param>
        /// <returns>A <see cref="SymbolUsageResult"/> indicating whether the usage results in an error. 
        /// If and only if the <paramref name="symbol"/> was null, <see cref="SymbolUsageResult.Unresolved"/> is returned.</returns>
        private SymbolUsageResult _useSymbol([CanBeNull] ref Symbol symbol, [NotNull] ISourcePosition position)
        {
            var msgs = new List<Message>(1);
            // symbol could be null.
            if (symbol == null)
                return SymbolUsageResult.Unresolved;
            symbol = symbol.HandleWith(_listMessages, msgs);
            if (msgs.Count > 0)
            {
                var seen = new HashSet<String>();
                foreach (var message in msgs)
                {
                    var c = message.MessageClass;
                    if (c != null)
                        if (!seen.Add(c))
                            continue;

                    ReportMessage(message.Repositioned(position));
                    if (message.Severity == MessageSeverity.Error)
                    {
                        symbol = null;
                        return SymbolUsageResult.Error;
                    }
                }
            }
            return SymbolUsageResult.Successful;
        }

        public AstTypeExpr ConstantType(ISourcePosition position, string typeExpression)
        {
            return new AstConstantTypeExpression(position.File, position.Line, position.Column, typeExpression);
        }

        public AstTypeExpr DynamicType(ISourcePosition position, string typeId, IEnumerable<AstExpr> arguments)
        {
            var t = new AstDynamicTypeExpression(position.File, position.Line,position.Column, typeId);
            t.Arguments.AddRange(arguments);
            return t;
        }

        private static readonly PValue _constZero = new PValue(0,PType.Int);
        private static readonly PValue _constOne = new PValue(1, PType.Int);

        private bool _safeEquals(PValue value,PValue constant)
        {
            PValue result;
            PValue booleanResult;
            return value != null && value.Equality(CompileTimeExecutionContext.Loader, constant, out result) &&
                   result.TryConvertTo(CompileTimeExecutionContext.Loader, PType.Bool, out booleanResult) &&
                   (bool) booleanResult.Value;
        }

        private bool _safeEquals(AstExpr value, PValue constant)
        {
            AstConstant lhs;
            return (lhs = value as AstConstant) != null &&
                   _safeEquals(CompileTimeExecutionContext.Loader.CreateNativePValue(lhs.Constant), constant);
        }

        private bool _safeTypecheck(AstExpr expr, PType type)
        {
            AstConstant lhs;
            return (lhs = expr as AstConstant) != null &&
                   _safeTypecheck(CompileTimeExecutionContext.Loader.CreateNativePValue(lhs.Constant), type);
        }

        private bool _safeTypecheck(PValue expr, PType type)
        {
            return expr != null && expr.Type.IsEqual(type);
        }

        private AstExpr _leftRedundant(AstExpr expr, PValue identity)
        {
            AstIndirectCall callNode;
            if ((callNode = expr as AstIndirectCall) != null
                && callNode.Arguments.Count == 2
                && _safeEquals(callNode.Arguments[0], identity))
            {
                return callNode.Arguments[1];
            }
            else
            {
                return expr;
            }
        }

        private AstExpr _rightRedundant(AstExpr expr, PValue identity)
        {
            AstIndirectCall callNode;
            if ((callNode = expr as AstIndirectCall) != null
                && callNode.Arguments.Count == 2
                && _safeEquals(callNode.Arguments[1], identity))
            {
                return callNode.Arguments[0];
            }
            else
            {
                return expr;
            }
        }

        private void _foldConcatenation(AstStringConcatenation concatenation)
        {
            concatenation._OptimizeInternal(CompileTimeExecutionContext);
        }

        public AstExpr BinaryOperation(ISourcePosition position, AstExpr left, BinaryOperator op, AstExpr right)
        {
            PValue leftNeutral = null;
            PValue rightNeutral = null;
            switch (op)
            {
                case BinaryOperator.Addition:
                    var concatenation = left as AstStringConcatenation;
                    if (concatenation != null)
                    {
                        // The LHS is a concatenation, check if RHS is a concatenation
                        //  as well and absorb it if that is the case.
                        var rightConcatenation = right as AstStringConcatenation;

                        if (rightConcatenation != null)
                        {
                            concatenation.Arguments.AddRange(rightConcatenation.Arguments);
                        }
                        else
                        {
                            concatenation.Arguments.Add(right);
                        }
                    }
                    else
                    {
                        // The RHS could be a concatenation. In this case, we don't need to check whether
                        //  the LHS was a concatenation. If that were the case, we'd already have landed
                        //  in the branch above.

                        concatenation = right as AstStringConcatenation;
                        if (concatenation != null)
                        {
                            concatenation.Arguments.Insert(0, left);
                        }
                        else if (_safeTypecheck(left, PType.String) || _safeTypecheck(right, PType.String))
                        {
                            // One of the two operands is known to be of type string. 
                            //  As a consequence we construct a new Concatenation node.
                            // TODO: Remove hard-coded reference to concat command by re-writing the StringConcatenation node
                            var simpleConcat = (AstGetSet) _resolveImplementation(position, op, expr => expr);
                            var multiConcat = (AstGetSet) _resolveImplementation(position, expr => expr, Engine.ConcatenateAlias);
                            concatenation = new AstStringConcatenation(position,simpleConcat,multiConcat,left,right);
                        }
                    }
                    

                    // If the LH- or RHS was a concatenation, the concatenation variable is set.
                    // Applying other transformations would be wrong. 
                    //  "text" + 0 == "text0"
                    // We don't want to ellide the addition of 0 here, even though 0 is the neutral element of addition.
                    if (concatenation != null)
                    {
                        _foldConcatenation(concatenation);
                        return concatenation;
                    }
                    
                    else
                    {
                        // Assume addition to work like ordinary addition where 0 is the neutral element.
                        leftNeutral = rightNeutral = _constZero;
                    }
                    break;
                case BinaryOperator.Subtraction:
                    rightNeutral = _constZero;
                    break;
                case BinaryOperator.Multiply:
                    leftNeutral = rightNeutral = _constOne;
                    break;
                case BinaryOperator.Division:
                    rightNeutral = _constOne;
                    break;
                case BinaryOperator.Modulus:
                    rightNeutral = _constOne;
                    break;
                case BinaryOperator.Power:
                    if (_safeEquals(right,_constZero))
                    {
                        var b = Block(position);
                        b.Add(left);
                        b.Expression = Constant(position, 1);
                        return b;
                    }
                    else if(_safeEquals(left,_constZero))
                    {
                        var b = Block(position);
                        b.Add(right);
                        b.Expression = Constant(position, 0);
                        return b;
                    }
                    else if (_safeEquals(left, _constOne))
                    {
                        var b = Block(position);
                        b.Add(right);
                        b.Expression = Constant(position, 1);
                        return b;
                    }
                    else
                    {
                        rightNeutral = _constOne;
                    }
                    break;
                case BinaryOperator.BitwiseAnd:
                    break;
                case BinaryOperator.BitwiseOr:
                    break;
                case BinaryOperator.ExclusiveOr:
                    break;
                case BinaryOperator.Equality:
                    break;
                case BinaryOperator.Inequality:
                    break;
                case BinaryOperator.GreaterThan:
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    break;
                case BinaryOperator.LessThan:
                    break;
                case BinaryOperator.LessThanOrEqual:
                    break;
                case BinaryOperator.Coalescence:
                    return Coalescence(position, new[] {left, right});
                case BinaryOperator.Cast:
                    var T = right as AstTypeExpr;
                    if (T == null)
                    {
                        ReportMessage(Message.Error(Resources.AstFactoryBase_BinaryOperation_TypeExprExpected,
                                                    position, MessageClasses.TypeExpressionExpected));
                        return CreateNullNode(position);
                    }
                    else
                    {
                        return Typecast(position, left, T);
                    }
                default:
                    throw new ArgumentOutOfRangeException("op");
            }

            // If control flow ended up here, the operator is going to be implemented
            // according to the corresponding symbol in the symbol table.
            return _resolveImplementation(position, op,
                call =>
                    {
                        call.Arguments.Add(left);
                        call.Arguments.Add(right);
                        AstExpr expr = call;
                        // Now apply left and right identity laws and fold constants.
                        if (leftNeutral != null)
                            expr = _leftRedundant(expr, leftNeutral);
                        if (rightNeutral != null)
                            expr = _rightRedundant(expr, rightNeutral);
                        expr = _foldConstants(expr);
                        return expr;
                    });
        }

        private AstExpr _resolveImplementation(ISourcePosition position, BinaryOperator op, Func<AstGetSet, AstExpr> impl)
        {
            var id = OperatorNames.Prexonite.GetName(op);
            return _resolveImplementation(position, impl, id);
        }

        private AstExpr _resolveImplementation(ISourcePosition position, Func<AstGetSet, AstExpr> impl, string id)
        {
            return _resolveImplementation(position, impl, new QualifiedId(id));
        }

        private AstExpr _resolveImplementation(ISourcePosition position, Func<AstGetSet, AstExpr> impl, QualifiedId qualid)
        {
            var expr = this.ExprFor(position, qualid, CurrentBlock.Symbols);
            var call = expr as AstGetSet;
            if (call == null)
                ReportMessage(Message.Error(string.Format(Resources.AstFactoryBase__resolveImplementation_LValueExpected, qualid), position, MessageClasses.LValueExpected));
            return impl(call);
        }

        [NotNull]
        private AstExpr _foldConstants([NotNull] AstExpr callNode)
        {
            AstIndirectCall indirectCallNode;
            EntityRef entityRef;
            EntityRef.Command command;
            PValue commandImpl;
            PValue result;
            AstExpr constantNode;
            if (callNode.TryMatchCall(out indirectCallNode, out entityRef) 
                && entityRef.TryGetCommand(out command)
                && indirectCallNode.Arguments.All(x => x is AstConstant)
                && command.TryGetEntity(CompileTimeExecutionContext.Loader, out commandImpl)
                && commandImpl.TryIndirectCall(CompileTimeExecutionContext.Loader, _constArgsToValues(indirectCallNode).ToArray(),out result)
                && AstConstant.TryCreateConstant(CompileTimeExecutionContext,callNode.Position,result,out constantNode))
            {
                return constantNode;
            }
            else
            {
                return callNode;
            }
        }

        private IEnumerable<PValue> _constArgsToValues(AstGetSet indirectCallNode)
        {
            return
                indirectCallNode.Arguments.Select(
                    x => CompileTimeExecutionContext.Loader.CreateNativePValue(((AstConstant) x).Constant));
        }

        public AstExpr UnaryOperation(ISourcePosition position, UnaryOperator op, AstExpr operand)
        {
            if (operand == null)
                throw new ArgumentNullException("operand");

            switch (op)
            {
                case UnaryOperator.LogicalNot:
                    {
                        var typecheck = operand as AstTypecheck;
                        if (typecheck != null && typecheck.CheckForPlaceholders() && typecheck.IsInverted)
                        {
                            // Special handling of "? is not Y", where typecheck.IsInverted is set to true.
                            // Note that "not (? is Y)" is not the same thing.

                            var thenCmd = this.Call(position, EntityRef.Command.Create(Engine.ThenAlias));

                            // Create "not ?"
                            var notId = OperatorNames.Prexonite.GetName(UnaryOperator.LogicalNot);
                            Symbol notSymbol;
                            var notOp = CurrentBlock.Symbols.TryGet(notId, out notSymbol)
                                ? ExprFor(position, notSymbol)
                                : new AstUnresolved(position, notId);
                            var notCall = notOp as AstGetSet;
                            if (notCall == null)
                            {
                                ReportMessage(
                                    Message.Error(
                                        Resources.AstFactoryBase_UnaryOperation_NotOperatorForTypecheckRequiresLValue,
                                        position, MessageClasses.LValueExpected));
                                notCall = CreateNullNode(position);
                            }

                            notCall.Arguments.Add(Placeholder(position,0));

                            // Create "? is T"
                            var partialTypecheck = Typecheck(position, typecheck.Subject, typecheck.Type);

                            // Assemble "(? is T) then (not ?)"
                            thenCmd.Arguments.Add(partialTypecheck);
                            thenCmd.Arguments.Add(notCall);

                            return thenCmd;
                        }
                        else
                        {
                            goto case UnaryOperator.UnaryNegation;
                        }
                    }
                case UnaryOperator.UnaryNegation:
                case UnaryOperator.OnesComplement:
                    {
                        var id = OperatorNames.Prexonite.GetName(op);
                        Symbol symbol;
                        CurrentBlock.Symbols.TryGet(id, out symbol);
                        var callExpr = CurrentBlock.Symbols.TryGet(id, out symbol)
                            ? ExprFor(position, symbol)
                            : new AstUnresolved(position, id);
                        var callLValue = callExpr as AstGetSet;
                        if (callLValue == null)
                        {
                            ReportMessage(Message.Error(Resources.AstFactoryBase_UnaryOperation_Target_must_be_LValue,
                                                        position, MessageClasses.LValueExpected));
                        }
                        else
                        {
                            callLValue.Arguments.Add(operand);
                        }
                        return _foldConstants(callExpr);
                    }
                case UnaryOperator.PreIncrement:
                case UnaryOperator.PreDecrement:
                case UnaryOperator.PostIncrement:
                case UnaryOperator.PostDecrement:
                    {
                        var symbolCall = operand as AstIndirectCall;
                        var symbol = symbolCall == null ? null : symbolCall.Subject as AstReference;
                        EntityRef.Variable variableRef;
                        var complex = operand as AstGetSet;

                        var isVariable = symbol != null && symbol.Entity.TryGetVariable(out variableRef);
                        
                        var isAssignable = complex != null;
                        var isPre = op == UnaryOperator.PreDecrement || op == UnaryOperator.PreIncrement;
                        var isIncrement =   op == UnaryOperator.PostIncrement ||
                                            op == UnaryOperator.PreIncrement;

                        if (isVariable)
                        {
                            return new AstUnaryOperator(position, op,operand);
                        }
                        else if (isAssignable)
                        {
                            if (complex.Call != PCall.Get)
                            {
                                ReportMessage(_lValueExpectedErrorMessage(position));
                            }

                            var assignPrototype = complex.GetCopy();
                            assignPrototype.Arguments.Add(Constant(position, 1));
                            assignPrototype.Call = PCall.Set;

                            var assignment = ModifyingAssignment(position, assignPrototype,
                                                                 isIncrement
                                                                     ? BinaryOperator.Addition
                                                                     : BinaryOperator.Subtraction);

                            if (!isPre)
                            {
                                // We need to have the expression evaluated *before* 
                                // the update is performed.
                                // If we just used a block expression, the assignment would be 
                                // performed before the 'value' of the expression has been evaluated.
                                return new AstPostExpression(position, complex, assignment);
                            }
                            else
                            {
                                // The handling of value semantics of set-calls will 
                                // cause the updated value to also be used as the value
                                // of the expression
                                return assignment;
                            }
                        }
                        else
                        {
                            ReportMessage(
                                _lValueExpectedErrorMessage(position));
                            return operand;
                        }
                    }
// ReSharper disable RedundantCaseLabel
                case UnaryOperator.None:
// ReSharper restore RedundantCaseLabel
                default:
                    throw new ArgumentOutOfRangeException("op");
            }
        }

        private static Message _lValueExpectedErrorMessage(ISourcePosition position)
        {
            return Message.Error(
                Resources.AstFactoryBase_UnaryOperation_Target_must_be_LValue, position,
                MessageClasses.LValueExpected);
        }

        public AstExpr ModifyingAssignment(ISourcePosition position, AstGetSet assignPrototype, BinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case BinaryOperator.None:
                    return assignPrototype;
                case BinaryOperator.Coalescence:
                    {
                        // Derive a get call from the assignment prototype
                        var getVariation = assignPrototype.GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);

                        var check = Typecheck(position, getVariation, ConstantType(position, NullPType.Literal));

                        // Given assignPrototype "x = y", produce "if(x is Null) x = y else x"
                        return ConditionalExpression(position, check, assignPrototype, getVariation);
                    }
                case BinaryOperator.Cast:
                    // a(x,y) ~= T         //a(x,y,~T)~=
                    //to
                    // a(x,y) = a(x,y)~T   //a(x,y,a(x,y)~T)=
                    {
                        var assignment = assignPrototype.GetCopy();

                        // Derive a get call from the assignment prototype
                        var getVariation = assignPrototype.GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);

                        var T = assignment.Arguments[assignment.Arguments.Count - 1] as AstTypeExpr;
                        if (T == null)
                        {
                            ReportMessage(Message.Error(Resources.AstFactoryBase_ModifyingAssignment_TypeExpressionExpected,position, MessageClasses.TypeExpressionExpected));
                            T = ConstantType(position, NullPType.Literal);
                        }

                        assignment.Arguments[assignment.Arguments.Count - 1] = Typecast(position, getVariation, T);
                        return assignment;
                    }
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
                    {
                        if (assignPrototype.Arguments.Count < 1)
                        {
                            ReportMessage(Message.Create(MessageSeverity.Error,
                                                            Resources.AstModifyingAssignment_No_RHS, position, MessageClasses.InvalidModifyingAssignment));
                            return CreateNullNode(position);
                        }
                
                        var assignment = assignPrototype.GetCopy();

                        // Create get-version of assignment
                        var getVersion = assignPrototype.GetCopy();
                        getVersion.Call = PCall.Get;
                        getVersion.Arguments.RemoveAt(getVersion.Arguments.Count - 1);

                        // The binary operation constructor will handle resolution of the
                        //  operator implementation
                        var modification = BinaryOperation(position, getVersion, binaryOperator,
                                                            assignPrototype.Arguments[
                                                                assignPrototype.Arguments.Count - 1]);

                        assignment.Arguments[assignment.Arguments.Count - 1] = modification;

                        return assignment;
                    }
                default:
                    throw new ArgumentOutOfRangeException("binaryOperator");
            }
            
        }

        public AstExpr Coalescence(ISourcePosition position, IEnumerable<AstExpr> operands)
        {
            var c = new AstCoalescence(position.File,position.Line, position.Column);
            c.Expressions.AddRange(operands);
            return c;
        }

        public AstExpr ConditionalExpression(ISourcePosition position, AstExpr condition, AstExpr thenExpr, AstExpr elseExpr, bool isNegative = false)
        {
            var c = new AstConditionalExpression(position.File, position.Line, position.Column, condition, isNegative)
                {IfExpression = thenExpr, ElseExpression = elseExpr};
            return c;
        }

        public AstExpr Constant(ISourcePosition position, object constant)
        {
            return new AstConstant(position.File,position.Line,position.Column,constant);
        }

        public AstExpr CreateClosure(ISourcePosition position, EntityRef.Function function)
        {
            return new AstCreateClosure(position,function);
        }

        public AstCreateCoroutine CreateCoroutine(ISourcePosition position, AstExpr function)
        {
            return new AstCreateCoroutine(position.File, position.Line, position.Column) {Expression = function};
        }

        public AstExpr KeyValuePair(ISourcePosition position, AstExpr key, AstExpr value)
        {
            return new AstKeyValuePair(position.File, position.Line, position.Column,key,value);
        }

        public AstExpr ListLiteral(ISourcePosition position, IEnumerable<AstExpr> elements)
        {
            var l = new AstListLiteral(position.File, position.Line, position.Column);
            l.Elements.AddRange(elements);
            return l;
        }

        public AstExpr HashLiteral(ISourcePosition position, IEnumerable<AstExpr> elements)
        {
            var l = new AstHashLiteral(position.File, position.Line, position.Column);
            l.Elements.AddRange(elements);
            return l;
        }

        public AstExpr LogicalAnd(ISourcePosition position, IEnumerable<AstExpr> clauses)
        {
            using (var e = clauses.GetEnumerator())
            {
                if(!e.MoveNext())
                    _throwLogicalNeedsTwoArgs(position);
                var lhs = e.Current;

                if (!e.MoveNext())
                    _throwLogicalNeedsTwoArgs(position);
                var rhs = e.Current;

                var a = new AstLogicalAnd(position.File, position.Line, position.Column, lhs, rhs);

                while (e.MoveNext())
                    a.Conditions.AddLast(e.Current);

                return a;
            }
        }

        public AstExpr LogicalOr(ISourcePosition position, IEnumerable<AstExpr> clauses)
        {
            using (var e = clauses.GetEnumerator())
            {
                if (!e.MoveNext())
                    _throwLogicalNeedsTwoArgs(position);
                var lhs = e.Current;

                if (!e.MoveNext())
                    _throwLogicalNeedsTwoArgs(position);
                var rhs = e.Current;

                var a = new AstLogicalOr(position.File, position.Line, position.Column, lhs, rhs);

                while (e.MoveNext())
                    a.Conditions.AddLast(e.Current);

                return a;
            }
        }

        public AstExpr Null(ISourcePosition position)
        {
            return new AstNull(position.File, position.Line, position.Column);
        }

        public AstObjectCreation CreateObject(ISourcePosition position, AstTypeExpr type)
        {
            return new AstObjectCreation(position.File, position.Line, position.Column, type);
        }

        public AstExpr Typecheck(ISourcePosition position, AstExpr operand, AstTypeExpr type)
        {
            return new AstTypecheck(position.File, position.Line, position.Column,operand,type);
        }

        public AstExpr Typecast(ISourcePosition position, AstExpr operand, AstTypeExpr type)
        {
            return new AstTypecast(position.File, position.Line, position.Column,operand,type);
        }

        public AstExpr Reference(ISourcePosition position, EntityRef entity)
        {
            return new AstReference(position, entity);
        }

        public AstGetSet MemberAccess(ISourcePosition position, AstExpr receiver, string memberId, PCall call = PCall.Get)
        {
            return new AstGetSetMemberAccess(position.File, position.Line, position.Column, call, receiver, memberId);
        }

        public AstGetSet StaticMemberAccess(ISourcePosition position, AstTypeExpr typeExpr, string memberId, PCall call = PCall.Get)
        {
            return new AstGetSetStatic(position.File, position.Line, position.Column,call,typeExpr,memberId);
        }

        public AstGetSet IndirectCall(ISourcePosition position, AstExpr receiver, PCall call = PCall.Get)
        {
            return new AstIndirectCall(position,call,receiver);
        }

        public AstGetSet Expand(ISourcePosition position, EntityRef entity, PCall call = PCall.Get)
        {
            return new AstExpand(position, entity, call);
        }

        public AstGetSet Placeholder(ISourcePosition position, int? index = new int?())
        {
            return new AstPlaceholder(position.File, position.Line, position.Column, index);
        }

        public AstScopedBlock Block(ISourcePosition position)
        {
            return new AstScopedBlock(position,CurrentBlock);
        }

        public AstCondition Condition(ISourcePosition position, AstExpr condition, bool isNegative = false)
        {
            return new AstCondition(position, CurrentBlock, condition, isNegative);
        }

        public AstWhileLoop WhileLoop(ISourcePosition position, bool isPostcondition = false, bool isNegative = false)
        {
            var loop = new AstWhileLoop(position, CurrentBlock, isPostcondition, !isNegative);
            return loop;
        }

        public AstForLoop ForLoop(ISourcePosition position)
        {
            return new AstForLoop(position,CurrentBlock);
        }

        public AstForeachLoop ForeachLoop(ISourcePosition position)
        {
            return new AstForeachLoop(position, CurrentBlock);
        }

        public AstNode Return(ISourcePosition position, AstExpr expression = null, ReturnVariant returnVariant = ReturnVariant.Exit)
        {
            return new AstReturn(position.File, position.Line, position.Column, returnVariant) {Expression = expression};
        }

        public AstNode Throw(ISourcePosition position, AstExpr exceptionExpression)
        {
            return new AstThrow(position.File, position.Line, position.Column){Expression = exceptionExpression};
        }

        public AstTryCatchFinally TryCatchFinally(ISourcePosition position)
        {
            return new AstTryCatchFinally(position, CurrentBlock);
        }

        public AstUsing Using(ISourcePosition position)
        {
            return new AstUsing(position,CurrentBlock);
        }

        public AstExpr ExprFor(ISourcePosition position, Symbol symbol)
        {
            // note that `symbol` could be null (if the symbol was not found in the first place)
            // That case is already handled by `_useSymbol`. It will immediately return `Unsuccessful`

            switch (_useSymbol(ref symbol, position))
            {
                case SymbolUsageResult.Successful:
                    Debug.Assert(symbol != null);
// ReSharper disable PossibleNullReferenceException
                    return symbol.HandleWith(_assembleAst, Tuple.Create(this, PCall.Get, position));
// ReSharper restore PossibleNullReferenceException
                case SymbolUsageResult.Unresolved:
                    return CreateNullNode(position);
                case SymbolUsageResult.Error:
                    // Errors have already been reported by TryUseSymbol
                    return CreateNullNode(position);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static readonly AssembleAstHandler _assembleAst = new AssembleAstHandler();

        private class AssembleAstHandler : ISymbolHandler<Tuple<AstFactoryBase, PCall, ISourcePosition>, AstExpr>
        {
            public AstExpr HandleMessage(MessageSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                return self.InnerSymbol.HandleWith(this, argument);
            }

            public AstExpr HandleDereference(DereferenceSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                return AstIndirectCall.Create(argument.Item3, self.InnerSymbol.HandleWith(this, argument),
                                              argument.Item2);
            }

            #region Implementation of ISymbolHandler<in Tuple<Parser,PCall>,out AstExpr>

            public AstExpr HandleReference(ReferenceSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                EntityRef.Variable.Local local;
                if (self.Entity.TryGetLocalVariable(out local))
                {
                    if (argument.Item1.IsOuterVariable(local.Id))
                        argument.Item1.RequireOuterVariable(local.Id);
                }
                return argument.Item1.Reference(argument.Item3, self.Entity);
            }

            public AstExpr HandleNil(NilSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                // TODO: consider treating Nil as an error (needs to be shadowed by proper error messages)
                return argument.Item1.CreateNullNode(argument.Item3);
            }

            public AstExpr HandleExpand(ExpandSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                ReferenceSymbol refSym;

                var position = argument.Item3;
                var inner = self.InnerSymbol;

                if (self.InnerSymbol.TryGetReferenceSymbol(out refSym))
                {
                    EntityRef.MacroCommand mcmd;
                    EntityRef.Function func;
                    if (refSym.Entity.TryGetMacroCommand(out mcmd) || refSym.Entity.TryGetFunction(out func))
                    {
                        return argument.Item1.Expand(argument.Item3, refSym.Entity);
                    }
                    else
                    {
                        argument.Item1.ReportMessage(
                        Message.Error(string.Format(Resources.Parser_CannotExpandAtCompileTime, inner),
                            position, MessageClasses.NotAMacro));
                        return argument.Item1.CreateNullNode(position);
                    }
                }
                else
                {
                    // TODO: Handle general case with dereferences between Expand and Reference.

                    argument.Item1.ReportMessage(
                        Message.Error(string.Format(Resources.Parser_CannotExpandAtCompileTime, inner),
                            position, MessageClasses.NotAMacro));
                    return argument.Item1.CreateNullNode(position);
                }
            }

            public AstExpr HandleNamespace(NamespaceSymbol self, Tuple<AstFactoryBase, PCall, ISourcePosition> argument)
            {
                return new AstNamespaceUsage(argument.Item3, argument.Item2, self.Namespace);
            }

            #endregion
        }

        private void _throwLogicalNeedsTwoArgs(ISourcePosition position)
        {
            throw new PrexoniteException(string.Format("Lazy logical operators require at least two operands. {0}", position));
        }

        #region IIndirectCall, IObject

        #region Implementation of IIndirectCall

        [NotNull]
        private readonly AstFactoryBridge _bridge;

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _bridge.IndirectCall(sctx, args);
        }

        #endregion

        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            return _bridge.TryDynamicCall(sctx, args, call, id, out result);
        }

        #endregion

        #endregion
    }
}