// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    internal class ParserAstFactory : IAstFactory
    {
        private readonly Parser _parser;

        public ParserAstFactory(Parser parser)
        {
            if (parser == null)
                throw new System.ArgumentNullException("parser");
            _parser = parser;
        }

        #region Implementation of IAstFactory

        public AstTypeExpr ConstantTypeExpression(ISourcePosition position, string typeExpression)
        {
            return new AstConstantTypeExpression(position.File, position.Line, position.Column, typeExpression);
        }

        public AstTypeExpr DynamicTypeExpression(ISourcePosition position, string typeId, IEnumerable<AstExpr> arguments)
        {
            var t = new AstDynamicTypeExpression(position.File, position.Line,position.Column, typeId);
            t.Arguments.AddRange(arguments);
            return t;
        }

        public AstExpr BinaryOperation(ISourcePosition position, AstExpr left, BinaryOperator op, AstExpr right)
        {
            var id = OperatorNames.Prexonite.GetName(op);
            SymbolEntry entry;
            if(_parser._TryUseSymbolEntry(id, out entry))
            {
                return new AstBinaryOperator(position.File, position.Line, position.Column, left, op, right, entry,
                                      _parser.CurrentBlock);
            }
            else
            {
                return _parser._NullNode(position);
            }
        }

        public AstExpr UnaryOperation(ISourcePosition position, UnaryOperator op, AstExpr operand)
        {
            var id = OperatorNames.Prexonite.GetName(op);
            SymbolEntry entry;
            if(_parser._TryUseSymbolEntry(id, out entry))
            {
                return new AstUnaryOperator(position.File,position.Line, position.Column, op,operand,entry);
            }
            else
            {
                return _parser._NullNode(position);
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
            return new AstCreateClosure(position.File, position.Line, position.Column,function.ToSymbolEntry());
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

        
        private void _throwLogicalNeedsTwoArgs(ISourcePosition position)
        {
            throw new PrexoniteException(string.Format("Lazy logical operators require at least two operands. {0}", position));
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

        public AstExpr ObjectCreation(ISourcePosition position, AstTypeExpr type)
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

        public AstGetSet Entity(ISourcePosition position, EntityRef entity, PCall call = PCall.Get)
        {
            throw new System.NotImplementedException();
        }

        public AstGetSet MemberAccess(ISourcePosition position, AstExpr receiver, string memberId, PCall call = PCall.Get)
        {
            throw new System.NotImplementedException();
        }

        public AstGetSet StaticMemberAccess(ISourcePosition position, AstTypeExpr typeExpr, string memberId, PCall call = PCall.Get)
        {
            throw new System.NotImplementedException();
        }

        public AstGetSet IndirectCall(ISourcePosition position, AstExpr receiver, PCall call = PCall.Get)
        {
            throw new System.NotImplementedException();
        }

        public AstGetSet Placeholder(ISourcePosition position, int? index = new int?())
        {
            throw new System.NotImplementedException();
        }

        public AstBlock Block(ISourcePosition position)
        {
            throw new System.NotImplementedException();
        }

        public AstCondition Condition(ISourcePosition position, AstExpr condition, bool isNegative = false)
        {
            throw new System.NotImplementedException();
        }

        public AstLoop WhileLoop(ISourcePosition position, bool isPostcondition = false, bool isNegative = false)
        {
            throw new System.NotImplementedException();
        }

        public AstForLoop ForLoop(ISourcePosition position)
        {
            throw new System.NotImplementedException();
        }

        public AstForeachLoop ForeachLoop(ISourcePosition position)
        {
            throw new System.NotImplementedException();
        }

        public AstNode Return(ISourcePosition position, ReturnVariant returnVariant = ReturnVariant.Exit, AstExpr expression = null)
        {
            throw new System.NotImplementedException();
        }

        public AstNode Throw(ISourcePosition position, AstExpr exceptionExpression)
        {
            throw new System.NotImplementedException();
        }

        public AstTryCatchFinally TryCatchFinally(ISourcePosition position)
        {
            throw new System.NotImplementedException();
        }

        public AstUsing Using(ISourcePosition position)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}