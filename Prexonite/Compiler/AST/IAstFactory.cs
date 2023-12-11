// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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

using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public interface IAstFactory : IMessageSink
{
    AstTypeExpr ConstantType(ISourcePosition position, string typeExpression);
    AstTypeExpr DynamicType(ISourcePosition position, string typeId, IEnumerable<AstExpr> arguments);

    AstExpr BinaryOperation(ISourcePosition position, AstExpr left, BinaryOperator op, AstExpr right);
    AstExpr UnaryOperation(ISourcePosition position, UnaryOperator op, AstExpr operand);

    AstExpr Coalescence(ISourcePosition position, IEnumerable<AstExpr> operands);
    AstExpr ConditionalExpression(ISourcePosition position, AstExpr condition, AstExpr thenExpr, AstExpr elseExpr, bool isNegative = false);
    AstExpr Constant(ISourcePosition position, object? constant);
    AstExpr CreateClosure(ISourcePosition position, EntityRef.Function function);
    AstCreateCoroutine CreateCoroutine(ISourcePosition position, AstExpr function);
    AstExpr KeyValuePair(ISourcePosition position, AstExpr key, AstExpr value);
    AstExpr ListLiteral(ISourcePosition position, IEnumerable<AstExpr> elements);
    AstExpr HashLiteral(ISourcePosition position, IEnumerable<AstExpr> elements);
    AstExpr LogicalAnd(ISourcePosition position, IEnumerable<AstExpr> clauses);
    AstExpr LogicalOr(ISourcePosition position, IEnumerable<AstExpr> clauses);
    AstExpr Null(ISourcePosition position);
    AstObjectCreation CreateObject(ISourcePosition position, AstTypeExpr type);
    AstExpr Typecheck(ISourcePosition position, AstExpr operand, AstTypeExpr type);
    AstExpr Typecast(ISourcePosition position, AstExpr operand, AstTypeExpr type);

    AstExpr Reference(ISourcePosition position, EntityRef entity);
    AstGetSet MemberAccess(ISourcePosition position, AstExpr receiver, string memberId, PCall call = PCall.Get);

    AstExpr ArgumentSplice(ISourcePosition position, AstExpr argumentList);
        
    AstGetSet StaticMemberAccess(ISourcePosition position, AstTypeExpr typeExpr, string memberId,
        PCall call = PCall.Get);

    AstGetSet IndirectCall(ISourcePosition position, AstExpr receiver, PCall call = PCall.Get);
    AstGetSet Expand(ISourcePosition position, EntityRef entity, PCall call = PCall.Get);
    AstGetSet Placeholder(ISourcePosition position, int? index = null);

    AstScopedBlock Block(ISourcePosition position);
    AstCondition Condition(ISourcePosition position, AstExpr condition, bool isNegative = false);

    AstWhileLoop WhileLoop(ISourcePosition position, bool isPostCondition = false, bool isNegative = false);

    AstForLoop ForLoop(ISourcePosition position);

    AstForeachLoop ForeachLoop(ISourcePosition position);

    AstNode Return(ISourcePosition position, AstExpr? expression = null, ReturnVariant returnVariant = ReturnVariant.Exit);

    AstExpr Throw(ISourcePosition position, AstExpr exceptionExpression);

    AstTryCatchFinally TryCatchFinally(ISourcePosition position);

    AstUsing Using(ISourcePosition position);

    /// <summary>
    /// Assembles an expression that is appropriate for the supplied symbol.
    /// </summary>
    /// <param name="position">The position at which the symbol was used in the source code.</param>
    /// <param name="symbol">The symbol to assemble an expression node for. Can be null to indicate an unresolved symbol.</param>
    /// <remarks>
    ///     <para><see cref="ExprFor"/> reports all messages generated by the use of the symbol (errors, warnings, etc.)</para>
    ///     <para>
    ///         The parameter <paramref name="symbol"/> can be null, indicating that the symbol was not found in the first place. 
    ///         <see cref="ExprFor"/> does <em>not</em> treat this as an error. 
    ///         The caller must make sure that a corresponding error message has been emitted.
    ///     </para>
    /// </remarks>
    /// <returns>An expression that implements a usage of the supplied <paramref name="symbol"/>. Never null.</returns>
    AstExpr ExprFor(ISourcePosition position, Symbol? symbol);

    AstExpr ModifyingAssignment(ISourcePosition position, AstGetSet assignPrototype, BinaryOperator binaryOperator);
}