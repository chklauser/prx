using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro
{
    public static class MacroContextExtensions
    {
        /// <summary>
        /// Creates a symbol access node.
        /// </summary>
        /// <param name="context">The context for which to generate the AST node.</param>
        /// <param name="interpretation">The interpretation of the symbol to apply (function, command, etc.)</param>
        /// <param name="call">The call type (get or set)</param>
        /// <param name="id">The (physical) id of the entity to access</param>
        /// <param name="args">The arguments to pass as part of teh access (optional)</param>
        /// <returns>A symbol access node.</returns>
        public static AstGetSetSymbol CreateGetSetSymbol(this MacroContext context, SymbolInterpretations interpretation, PCall call, string id, params IAstExpression[] args)
        {
            var sym = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                                          context.Invocation.Column, call, id, interpretation);
            sym.Arguments.AddRange(args);

            return sym;
        }

        /// <summary>
        /// Creates a local variable access node.
        /// </summary>
        /// <param name="context">The context for which to generate the AST node.</param>
        /// <param name="id">The (physical) name of the local variable.</param>
        /// <param name="call">The access type (get or set)</param>
        /// <returns>A local variable access node</returns>
        public static AstGetSetSymbol CreateGetSetLocal(this MacroContext context, string id, PCall call = PCall.Get)
        {
            return CreateGetSetSymbol(context, SymbolInterpretations.LocalObjectVariable, call, id);
        }

        /// <summary>
        /// Creates a instance member access node.
        /// </summary>
        /// <param name="context">The context for which to generate the AST node.</param>
        /// <param name="subject">The object on which to invoke the member.</param>
        /// <param name="call">The call type (get or set)</param>
        /// <param name="id">The name of the member to invoke</param>
        /// <param name="args">The arguments to pass as part of the member invocation (optional)</param>
        /// <returns>An instance member access node.</returns>
        public static AstGetSetMemberAccess CreateGetSetMember(this MacroContext context, IAstExpression subject, PCall call, string id, params IAstExpression[] args)
        {
            var mem = new AstGetSetMemberAccess(context.Invocation.File, context.Invocation.Line,
                                                context.Invocation.Column, call, subject, id);

            mem.Arguments.AddRange(args);

            return mem;
        }

        /// <summary>
        /// Creates a constant literal node.
        /// </summary>
        /// <param name="context">The context for which to generate the constant node.</param>
        /// <param name="constant">The constant value. Must be an integer, a double, a boolean value or a string.</param>
        /// <returns></returns>
        public static AstConstant CreateConstant(this MacroContext context, object constant)
        {
            return new AstConstant(context.Invocation.File, context.Invocation.Line,
                                   context.Invocation.Column, constant);
        }

        /// <summary>
        /// Generates an AST node that, when compiled, loads the specified enumeration value.
        /// </summary>
        /// <param name="enumerationValue">The enumeration value to load</param>
        /// <param name="position">The source position to associate with the node</param>
        /// <returns>An AST node that represents the specified enumeration value</returns>
        public static IAstExpression EnumToExpression<T>(this T enumerationValue, ISourcePosition position) where T : struct
        {
            var member = Enum.GetName(typeof (T), enumerationValue);
            var pcallT = new AstConstantTypeExpression(position.File,
                                                       position.Line,
                                                       position.Column,
                                                       PType.Object[typeof(T)].ToString());
            return new AstGetSetStatic(position.File, position.Line,
                                       position.Column, PCall.Get, pcallT, member);
        }

        /// <summary>
        /// Determines whether the "caller" of this macro is a macro itself.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool CallerIsMacro(this MacroContext context)
        {
            return context.Function.IsMacro || context.GetParentFunctions().Any(f => f.IsMacro);
        }

        /// <summary>
        /// Ensures that the macro is expanded in another macro, i.e. that the macro context variable is available. 
        /// </summary>
        /// <param name="context">The context of this macro expansion.</param>
        public static void EstablishMacroContext(this MacroContext context)
        {
            if(!CallerIsMacro(context))
            {
                context.ReportMessage(ParseMessageSeverity.Error, "Cannot establish macro context outside of macro.");
                return;
            }

            if(!context.OuterVariables.Contains(MacroAliases.ContextAlias,Engine.DefaultStringComparer))
                context.RequireOuterVariable(MacroAliases.ContextAlias);
        }
    }
}
