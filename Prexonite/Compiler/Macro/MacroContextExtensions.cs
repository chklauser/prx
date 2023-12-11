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
    /// <summary>
    ///     Creates a instance member access node.
    /// </summary>
    /// <param name = "context">The context for which to generate the AST node.</param>
    /// <param name = "subject">The object on which to invoke the member.</param>
    /// <param name = "call">The call type (get or set)</param>
    /// <param name = "id">The name of the member to invoke</param>
    /// <param name = "args">The arguments to pass as part of the member invocation (optional)</param>
    /// <returns>An instance member access node.</returns>
    public static AstGetSetMemberAccess CreateGetSetMember(this MacroContext context,
        AstExpr subject, PCall call, string id, params AstExpr[] args)
    {
        var mem = new AstGetSetMemberAccess(context.Invocation.File, context.Invocation.Line,
            context.Invocation.Column, call, subject, id);

        mem.Arguments.AddRange(args);

        return mem;
    }
    public static AstExpr CreateConstantOrNull(this MacroContext context, object? constant)
    {
        if(ReferenceEquals(constant, null))
            return new AstNull(context.Invocation.File, context.Invocation.Line, context.Invocation.Column);
        else
        {
            return CreateConstant(context, constant);
        }
    }

    /// <summary>
    ///     Generates an AST node that, when compiled, loads the specified enumeration value.
    /// </summary>
    /// <param name = "enumerationValue">The enumeration value to load</param>
    /// <param name = "position">The source position to associate with the node</param>
    /// <returns>An AST node that represents the specified enumeration value</returns>
    public static AstExpr ToExpr<T>(this T enumerationValue,
        ISourcePosition position) where T : struct
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

    /// <summary>
    ///     Determines whether the "caller" of this macro is a macro itself.
    /// </summary>
    /// <param name = "context"></param>
    /// <returns></returns>
    public static bool CallerIsMacro(this MacroContext context)
    {
        return context.Function.IsMacro || context.GetParentFunctions().Any(f => f.IsMacro);
    }

    /// <summary>
    ///     Ensures that the macro is expanded in another macro, i.e. that the macro context variable is available.
    /// </summary>
    /// <param name = "context">The context of this macro expansion.</param>
    public static void EstablishMacroContext(this MacroContext context)
    {
        if (!CallerIsMacro(context))
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

    public static AstTypeExpr CreateConstantType(this MacroContext context, string typeExpression)
    {
        return context.Factory.ConstantType(context.Invocation.Position, typeExpression);
    }
    public static AstTypeExpr CreateDynamicType(this MacroContext context, string typeId, IEnumerable<AstExpr> arguments)
    {
        return context.Factory.DynamicType(context.Invocation.Position, typeId, arguments);
    }

    public static AstExpr CreateBinaryOperation(this MacroContext context, AstExpr left, BinaryOperator op, AstExpr right)
    {
        return context.Factory.BinaryOperation(context.Invocation.Position, left, op, right);
    }
    public static AstExpr CreateUnaryOperation(this MacroContext context, UnaryOperator op, AstExpr operand)
    {
        return context.Factory.UnaryOperation(context.Invocation.Position, op, operand);
    }

    public static AstExpr CreateCoalescence(this MacroContext context, IEnumerable<AstExpr> operands)
    {
        return context.Factory.Coalescence(context.Invocation.Position, operands);
    }
    public static AstExpr CreateConditionalExpression(this MacroContext context, AstExpr condition, AstExpr thenExpr,
        AstExpr elseExpr, bool isNegative = false)
    {
        return context.Factory.ConditionalExpression(context.Invocation.Position, condition, thenExpr, elseExpr,
            isNegative);
    }
    public static AstExpr CreateConstant(this MacroContext context, object constant)
    {
        return context.Factory.Constant(context.Invocation.Position, constant);
    }
    public static AstExpr CreateCreateClosure(this MacroContext context, EntityRef.Function function)
    {
        return context.Factory.CreateClosure(context.Invocation.Position, function);
    }
    public static AstCreateCoroutine CreateCreateCoroutine(this MacroContext context, AstExpr function)
    {
        return context.Factory.CreateCoroutine(context.Invocation.Position, function);
    }
    public static AstExpr CreateKeyValuePair(this MacroContext context, AstExpr key, AstExpr value)
    {
        return context.Factory.KeyValuePair(context.Invocation.Position, key, value);
    }
    public static AstExpr CreateListLiteral(this MacroContext context, IEnumerable<AstExpr> elements)
    {
        return context.Factory.ListLiteral(context.Invocation.Position, elements);
    }
    public static AstExpr CreateHashLiteral(this MacroContext context, IEnumerable<AstExpr> elements)
    {
        return context.Factory.HashLiteral(context.Invocation.Position, elements);
    }

    public static AstExpr CreateLogicalAnd(this MacroContext context, IEnumerable<AstExpr> clauses)
    {
        return context.Factory.LogicalAnd(context.Invocation.Position, clauses);
    }
    public static AstExpr CreateLogicalOr(this MacroContext context, IEnumerable<AstExpr> clauses)
    {
        return context.Factory.LogicalOr(context.Invocation.Position, clauses);
    }
    public static AstExpr CreateNull(this MacroContext context)
    {
        return context.Factory.Null(context.Invocation.Position);
    }
    public static AstObjectCreation CreateCreateObject(this MacroContext context, AstTypeExpr type)
    {
        return context.Factory.CreateObject(context.Invocation.Position, type);
    }
    public static AstExpr CreateTypecheck(this MacroContext context, AstExpr operand, AstTypeExpr type)
    {
        return context.Factory.Typecheck(context.Invocation.Position, operand, type);
    }
    public static AstExpr CreateTypecast(this MacroContext context, AstExpr operand, AstTypeExpr type)
    {
        return context.Factory.Typecast(context.Invocation.Position, operand, type);
    }

    public static AstExpr CreateReference(this MacroContext context, EntityRef entity)
    {
        return context.Factory.Reference(context.Invocation.Position, entity);
    }
    public static AstGetSet CreateMemberAccess(this MacroContext context, AstExpr receiver, string memberId,
        PCall call = PCall.Get)
    {
        return context.Factory.MemberAccess(context.Invocation.Position, receiver, memberId, call);
    }

    public static AstGetSet CreateStaticMemberAccess(this MacroContext context, AstTypeExpr typeExpr, string memberId,
        PCall call = PCall.Get)
    {
        return context.Factory.StaticMemberAccess(context.Invocation.Position, typeExpr, memberId, call);
    }

    public static AstGetSet CreateIndirectCall(this MacroContext context, AstExpr receiver, PCall call = PCall.Get)
    {
        return context.Factory.IndirectCall(context.Invocation.Position, receiver, call);
    }
    public static AstGetSet CreateExpand(this MacroContext context, EntityRef entity, PCall call = PCall.Get)
    {
        return context.Factory.Expand(context.Invocation.Position, entity, call);
    }
    public static AstGetSet CreatePlaceholder(this MacroContext context, int? index = null)
    {
        return context.Factory.Placeholder(context.Invocation.Position, index);
    }

    public static AstScopedBlock CreateBlock(this MacroContext context)
    {
        return context.Factory.Block(context.Invocation.Position);
    }

    public static AstCondition CreateCondition(this MacroContext context, AstExpr condition, bool isNegative = false)
    {
        return context.Factory.Condition(context.Invocation.Position, condition, isNegative);
    }

    public static AstWhileLoop CreateWhileLoop(this MacroContext context, bool isPostcondition = false,
        bool isNegative = false)
    {
        return context.Factory.WhileLoop(context.Invocation.Position, isPostcondition, isNegative);
    }

    public static AstForLoop CreateForLoop(this MacroContext context)
    {
        return context.Factory.ForLoop(context.Invocation.Position);
    }

    public static AstForeachLoop CreateForeachLoop(this MacroContext context)
    {
        return context.Factory.ForeachLoop(context.Invocation.Position);
    }

    public static AstNode CreateReturn(this MacroContext context, AstExpr? expression = null,
        ReturnVariant returnVariant = ReturnVariant.Exit)
    {
        return context.Factory.Return(context.Invocation.Position, expression, returnVariant);
    }

    public static AstNode Throw(this MacroContext context, AstExpr exceptionExpression)
    {
        return context.Factory.Throw(context.Invocation.Position, exceptionExpression);
    }

    public static AstTryCatchFinally TryCatchFinally(this MacroContext context)
    {
        return context.Factory.TryCatchFinally(context.Invocation.Position);
    }

    public static AstUsing Using(this MacroContext context)
    {
        return context.Factory.Using(context.Invocation.Position);
    }

    public static AstGetSet CreateCall(this MacroContext context, EntityRef entity, PCall call = PCall.Get, params AstExpr[] arguments)
    {
        return context.Factory.Call(context.Invocation.Position, entity, call, arguments);
    }

    #endregion

}