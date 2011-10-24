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
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro
{
    public static class MacroContextExtensions
    {
        /// <summary>
        ///     Creates a symbol access node.
        /// </summary>
        /// <param name = "context">The context for which to generate the AST node.</param>
        /// <param name="implementation"></param>
        /// <param name = "call">The call type (get or set)</param>
        /// <param name = "args">The arguments to pass as part of teh access (optional)</param>
        /// <returns>A symbol access node.</returns>
        public static AstGetSetSymbol CreateGetSetSymbol(this MacroContext context, SymbolEntry implementation, PCall call, params IAstExpression[] args)
        {
            var sym = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, call, implementation);
            sym.Arguments.AddRange(args);

            return sym;
        }

        /// <summary>
        ///     Creates a local variable access node.
        /// </summary>
        /// <param name = "context">The context for which to generate the AST node.</param>
        /// <param name = "id">The (physical) name of the local variable.</param>
        /// <param name = "call">The access type (get or set)</param>
        /// <returns>A local variable access node</returns>
        public static AstGetSetSymbol CreateGetSetLocal(this MacroContext context, string id,
            PCall call = PCall.Get)
        {
            return CreateGetSetSymbol(context, new SymbolEntry(SymbolInterpretations.LocalObjectVariable,id,null), call);
        }

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
            IAstExpression subject, PCall call, string id, params IAstExpression[] args)
        {
            var mem = new AstGetSetMemberAccess(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, call, subject, id);

            mem.Arguments.AddRange(args);

            return mem;
        }

        /// <summary>
        ///     Creates a constant literal node.
        /// </summary>
        /// <param name = "context">The context for which to generate the constant node.</param>
        /// <param name = "constant">The constant value. Must be an integer, a double, a boolean value or a string.</param>
        /// <returns></returns>
        public static AstConstant CreateConstant(this MacroContext context, object constant)
        {
            return new AstConstant(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, constant);
        }

        public static IAstExpression CreateConstantOrNull(this MacroContext context, object constant)
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
        public static IAstExpression EnumToExpression<T>(this T enumerationValue,
            ISourcePosition position) where T : struct
        {
            if (position == null)
                throw new ArgumentNullException("position");

            var member = Enum.GetName(typeof (T), enumerationValue);
            var pcallT = new AstConstantTypeExpression(position.File,
                position.Line,
                position.Column,
                PType.Object[typeof (T)].ToString());
            return new AstGetSetStatic(position.File, position.Line,
                position.Column, PCall.Get, pcallT, member);
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
                context.ReportMessage(ParseMessageSeverity.Error,
                    "Cannot establish macro context outside of macro.");
                return;
            }

            if (
                !context.OuterVariables.Contains(MacroAliases.ContextAlias,
                    Engine.DefaultStringComparer))
                context.RequireOuterVariable(MacroAliases.ContextAlias);
        }

        /// <summary>
        ///     Creates a macro invocation node
        /// </summary>
        /// <param name = "context">The context for which to generate the AST node.</param>
        /// <param name = "callType">The call type (get or set)</param>
        /// <param name="implementation"></param>
        /// <param name = "args">The arguments to pass as part of teh access (optional)</param>
        /// <returns>A symbol access node.</returns>
        public static AstMacroInvocation CreateMacroInvocation(this MacroContext context, PCall callType, SymbolEntry implementation, params IAstExpression[] args)
        {
            var m = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, implementation) {Call = callType};
            m.Arguments.AddRange(args);
            return m;
        }
    }
}