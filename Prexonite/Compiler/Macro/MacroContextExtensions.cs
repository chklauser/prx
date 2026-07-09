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

using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro;

public static class MacroContextExtensions
{
    extension(MacroContext context)
    {
        /// <summary>
        ///     Creates a instance member access node.
        /// </summary>
    /// <param name = "context">The context for which to generate the AST node.</param>
    /// <param name = "subject">The object on which to invoke the member.</param>
    /// <param name = "call">The call type (get or set)</param>
    /// <param name = "id">The name of the member to invoke</param>
    /// <param name = "args">The arguments to pass as part of the member invocation (optional)</param>
    /// <returns>An instance member access node.</returns>
    public AstGetSetMemberAccess CreateGetSetMember(
        AstExpr subject, PCall call, string id, params AstExpr[] args)
    {
        var mem = new AstGetSetMemberAccess(context.Invocation.File, context.Invocation.Line,
            context.Invocation.Column, call, subject, id);

        mem.Arguments.AddRange(args);

        return mem;
    }
    public AstExpr CreateConstantOrNull(object? constant)
    {
        if(ReferenceEquals(constant, null))
            return new AstNull(context.Invocation.File, context.Invocation.Line, context.Invocation.Column);
        else
        {
            return context.CreateConstant(constant);
        }
    }

    /// <summary>
    ///     Determines whether the "caller" of this macro is a macro itself.
    /// </summary>
    /// <param name = "context"></param>
    /// <returns></returns>
    public bool CallerIsMacro()
    {
        return context.Function.IsMacro || context.GetParentFunctions().Any(f => f.IsMacro);
    }

    /// <summary>
    ///     Ensures that the macro is expanded in another macro, i.e. that the macro context variable is available.
    /// </summary>
    /// <param name = "context">The context of this macro expansion.</param>
    public void EstablishMacroContext()
    {
        if (!context.CallerIsMacro())
        {
            context.ReportMessage(
                Message.Error(
                    Resources.MacroContextExtensions_EstablishMacroContext_OutsideOfMacro,
                    context.Invocation.Position,
                    MessageClasses.MacroContextOutsideOfMacro));
            return;
        }

        if (
            !context.OuterVariables.Contains(MacroAliases.ContextAlias,
                Engine.DefaultStringComparer))
            context.RequireOuterVariable(MacroAliases.ContextAlias);
    }

    #region IAstFactory forwarded methods

    public AstTypeExpr CreateConstantType(string typeExpression)
    {
        return context.Factory.ConstantType(context.Invocation.Position, typeExpression);
    }
    public AstTypeExpr CreateDynamicType(string typeId, IEnumerable<AstExpr> arguments)
    {
        return context.Factory.DynamicType(context.Invocation.Position, typeId, arguments);
    }

    public AstExpr CreateBinaryOperation(AstExpr left, BinaryOperator op, AstExpr right)
    {
        return context.Factory.BinaryOperation(context.Invocation.Position, left, op, right);
    }
    public AstExpr CreateUnaryOperation(UnaryOperator op, AstExpr operand)
    {
        return context.Factory.UnaryOperation(context.Invocation.Position, op, operand);
    }

    public AstExpr CreateCoalescence(IEnumerable<AstExpr> operands)
    {
        return context.Factory.Coalescence(context.Invocation.Position, operands);
    }
    public AstExpr CreateConditionalExpression(AstExpr condition, AstExpr thenExpr,
        AstExpr elseExpr, bool isNegative = false)
    {
        return context.Factory.ConditionalExpression(context.Invocation.Position, condition, thenExpr, elseExpr,
            isNegative);
    }
    public AstExpr CreateConstant(object constant)
    {
        return context.Factory.Constant(context.Invocation.Position, constant);
    }
    public AstExpr CreateCreateClosure(EntityRef.Function function)
    {
        return context.Factory.CreateClosure(context.Invocation.Position, function);
    }
    public AstCreateCoroutine CreateCreateCoroutine(AstExpr function)
    {
        return context.Factory.CreateCoroutine(context.Invocation.Position, function);
    }
    public AstExpr CreateKeyValuePair(AstExpr key, AstExpr value)
    {
        return context.Factory.KeyValuePair(context.Invocation.Position, key, value);
    }
    public AstExpr CreateListLiteral(IEnumerable<AstExpr> elements)
    {
        return context.Factory.ListLiteral(context.Invocation.Position, elements);
    }
    public AstExpr CreateHashLiteral(IEnumerable<AstExpr> elements)
    {
        return context.Factory.HashLiteral(context.Invocation.Position, elements);
    }

    public AstExpr CreateLogicalAnd(IEnumerable<AstExpr> clauses)
    {
        return context.Factory.LogicalAnd(context.Invocation.Position, clauses);
    }
    public AstExpr CreateLogicalOr(IEnumerable<AstExpr> clauses)
    {
        return context.Factory.LogicalOr(context.Invocation.Position, clauses);
    }
    public AstExpr CreateNull()
    {
        return context.Factory.Null(context.Invocation.Position);
    }
    public AstObjectCreation CreateCreateObject(AstTypeExpr type)
    {
        return context.Factory.CreateObject(context.Invocation.Position, type);
    }
    public AstExpr CreateTypecheck(AstExpr operand, AstTypeExpr type)
    {
        return context.Factory.Typecheck(context.Invocation.Position, operand, type);
    }
    public AstExpr CreateTypecast(AstExpr operand, AstTypeExpr type)
    {
        return context.Factory.Typecast(context.Invocation.Position, operand, type);
    }

    public AstExpr CreateReference(EntityRef entity)
    {
        return context.Factory.Reference(context.Invocation.Position, entity);
    }
    public AstGetSet CreateMemberAccess(AstExpr receiver, string memberId,
        PCall call = PCall.Get)
    {
        return context.Factory.MemberAccess(context.Invocation.Position, receiver, memberId, call);
    }

    public AstGetSet CreateStaticMemberAccess(AstTypeExpr typeExpr, string memberId,
        PCall call = PCall.Get)
    {
        return context.Factory.StaticMemberAccess(context.Invocation.Position, typeExpr, memberId, call);
    }

    public AstGetSet CreateIndirectCall(AstExpr receiver, PCall call = PCall.Get)
    {
        return context.Factory.IndirectCall(context.Invocation.Position, receiver, call);
    }
    public AstGetSet CreateExpand(EntityRef entity, PCall call = PCall.Get)
    {
        return context.Factory.Expand(context.Invocation.Position, entity, call);
    }
    public AstGetSet CreatePlaceholder(int? index = null)
    {
        return context.Factory.Placeholder(context.Invocation.Position, index);
    }

    public AstScopedBlock CreateBlock()
    {
        return context.Factory.Block(context.Invocation.Position);
    }

    public AstCondition CreateCondition(AstExpr condition, bool isNegative = false)
    {
        return context.Factory.Condition(context.Invocation.Position, condition, isNegative);
    }

    public AstWhileLoop CreateWhileLoop(bool isPostcondition = false,
        bool isNegative = false)
    {
        return context.Factory.WhileLoop(context.Invocation.Position, isPostcondition, isNegative);
    }

    public AstForLoop CreateForLoop()
    {
        return context.Factory.ForLoop(context.Invocation.Position);
    }

    public AstForeachLoop CreateForeachLoop()
    {
        return context.Factory.ForeachLoop(context.Invocation.Position);
    }

    public AstNode CreateReturn(AstExpr? expression = null,
        ReturnVariant returnVariant = ReturnVariant.Exit)
    {
        return context.Factory.Return(context.Invocation.Position, expression, returnVariant);
    }

    public AstNode Throw(AstExpr exceptionExpression)
    {
        return context.Factory.Throw(context.Invocation.Position, exceptionExpression);
    }

    public AstTryCatchFinally TryCatchFinally()
    {
        return context.Factory.TryCatchFinally(context.Invocation.Position);
    }

    public AstUsing Using()
    {
        return context.Factory.Using(context.Invocation.Position);
    }

    public AstGetSet CreateCall(EntityRef entity, PCall call = PCall.Get, params AstExpr[] arguments)
    {
        return context.Factory.Call(context.Invocation.Position, entity, call, arguments);
    }

    #endregion
    }

    extension<T>(T enumerationValue) where T : struct
    {
        /// <summary>
        ///     Generates an AST node that, when compiled, loads the specified enumeration value.
        /// </summary>
    /// <param name = "enumerationValue">The enumeration value to load</param>
    /// <param name = "position">The source position to associate with the node</param>
    /// <returns>An AST node that represents the specified enumeration value</returns>
    public AstExpr ToExpr(
        ISourcePosition position)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        var member = Enum.GetName(typeof (T), enumerationValue);
        if (member == null)
        {
            var value = new AstConstant(position.File,
                position.Line,
                position.Column,
                Convert.ToInt32(enumerationValue));
            var nativeValueExpr = new AstTypecast(position,
                value,
                new AstConstantTypeExpression(position.File,
                    position.Line,
                    position.Column,
                    PType.Object[Enum.GetUnderlyingType(typeof(T))].ToString()));
            var enumExpr = new AstTypecast(position,
                nativeValueExpr,
                new AstConstantTypeExpression(position.File,
                    position.Line,
                    position.Column,
                    PType.Object[typeof(T)].ToString()));
            return enumExpr;
        }
        else
        {
            var pcallT = new AstConstantTypeExpression(position.File,
                position.Line,
                position.Column,
                PType.Object[typeof(T)].ToString());
            return new AstGetSetStatic(position, PCall.Get, pcallT, member);
        }
    }
    }
}