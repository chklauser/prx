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
        public static AstGetSetSymbol CreateGetSetSymbol(this MacroContext context, SymbolInterpretations interpretation, PCall call, string id, params IAstExpression[] args)
        {
            var sym = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                                          context.Invocation.Column, call, id, interpretation);
            sym.Arguments.AddRange(args);

            return sym;
        }

        public static AstGetSetMemberAccess CreateGetSetMember(this MacroContext context, IAstExpression subject, PCall call, string id, params IAstExpression[] args)
        {
            var mem = new AstGetSetMemberAccess(context.Invocation.File, context.Invocation.Line,
                                                context.Invocation.Column, call, subject, id);

            mem.Arguments.AddRange(args);

            return mem;
        }
    }
}
