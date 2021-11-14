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
using System.Collections.Generic;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public interface IAstFactory : IMessageSink
{
    [NotNull]
    AstTypeExpr ConstantType([NotNull] ISourcePosition position, string typeExpression);
    [NotNull]
    AstTypeExpr DynamicType([NotNull] ISourcePosition position, string typeId, IEnumerable<AstExpr> arguments);

    [NotNull]
    AstExpr BinaryOperation([NotNull] ISourcePosition position, [NotNull] AstExpr left, BinaryOperator op, [NotNull] AstExpr right);
    [NotNull]
    AstExpr UnaryOperation([NotNull] ISourcePosition position, UnaryOperator op, [NotNull] AstExpr operand);

    [NotNull]
    AstExpr Coalescence([NotNull] ISourcePosition position, IEnumerable<AstExpr> operands);
    [NotNull]
    AstExpr ConditionalExpression([NotNull] ISourcePosition position, AstExpr condition, AstExpr thenExpr, AstExpr elseExpr, bool isNegative = false);
    [NotNull]
    AstExpr Constant([NotNull] ISourcePosition position, object constant);
    [NotNull]
    AstExpr CreateClosure([NotNull] ISourcePosition position, EntityRef.Function function);
    [NotNull]
    AstCreateCoroutine CreateCoroutine([NotNull] ISourcePosition position, AstExpr function);
    [NotNull]
    AstExpr KeyValuePair([NotNull] ISourcePosition position, AstExpr key, AstExpr value);
    [NotNull]
    AstExpr ListLiteral([NotNull] ISourcePosition position, IEnumerable<AstExpr> elements);
    [NotNull]
    AstExpr HashLiteral([NotNull] ISourcePosition position, IEnumerable<AstExpr> elements);
    [NotNull]
    AstExpr LogicalAnd([NotNull] ISourcePosition position, IEnumerable<AstExpr> clauses);
    [NotNull]
    AstExpr LogicalOr([NotNull] ISourcePosition position, IEnumerable<AstExpr> clauses);
    [NotNull]
    AstExpr Null(ISourcePosition position);
    [NotNull]
    AstObjectCreation CreateObject([NotNull] ISourcePosition position, AstTypeExpr type);
    [NotNull]
    AstExpr Typecheck([NotNull] ISourcePosition position, AstExpr operand, AstTypeExpr type);
    [NotNull]
    AstExpr Typecast([NotNull] ISourcePosition position, AstExpr operand, AstTypeExpr type);

    [NotNull]
    AstExpr Reference([NotNull] ISourcePosition position, EntityRef entity);
    [NotNull]
    AstGetSet MemberAccess([NotNull] ISourcePosition position, AstExpr receiver, string memberId, PCall call = PCall.Get);

    [NotNull]
    AstExpr ArgumentSplice(ISourcePosition position, AstExpr argumentList);
        
    [NotNull]
    AstGetSet StaticMemberAccess([NotNull] ISourcePosition position, AstTypeExpr typeExpr, string memberId,
        PCall call = PCall.Get);

    [NotNull]
    AstGetSet IndirectCall([NotNull] ISourcePosition position, AstExpr receiver, PCall call = PCall.Get);
    [NotNull]
    AstGetSet Expand([NotNull] ISourcePosition position, EntityRef entity, PCall call = PCall.Get);
    [NotNull]
    AstGetSet Placeholder([NotNull] ISourcePosition position, int? index = null);

    [NotNull]
    AstScopedBlock Block(ISourcePosition position);
    [NotNull]
    AstCondition Condition([NotNull] ISourcePosition position, AstExpr condition, bool isNegative = false);

    [NotNull]
    AstWhileLoop WhileLoop([NotNull] ISourcePosition position, bool isPostCondition = false, bool isNegative = false);

    [NotNull]
    AstForLoop ForLoop(ISourcePosition position);

    [NotNull]
    AstForeachLoop ForeachLoop(ISourcePosition position);

    [NotNull]
    AstNode Return([NotNull] ISourcePosition position, AstExpr expression = null, ReturnVariant returnVariant = ReturnVariant.Exit);

    [NotNull]
    AstNode Throw([NotNull] ISourcePosition position, AstExpr exceptionExpression);

    [NotNull]
    AstTryCatchFinally TryCatchFinally(ISourcePosition position);

    [NotNull]
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
    [NotNull]
    AstExpr ExprFor([NotNull] ISourcePosition position, [CanBeNull] Symbol symbol);

    [NotNull]
    AstExpr ModifyingAssignment([NotNull] ISourcePosition position, [NotNull] AstGetSet assignPrototype, BinaryOperator binaryOperator);
}