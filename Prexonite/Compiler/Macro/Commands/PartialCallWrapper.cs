using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro.Commands
{
    public class PartialCallWrapper : PartialMacroCommand
    {
        private readonly string _callImplementationId;
        private readonly SymbolInterpretations _callImplementetaionInterpretation;

        public string CallImplementationId
        {
            [DebuggerStepThrough]
            get { return _callImplementationId; }
        }

        public SymbolInterpretations CallImplementetaionInterpretation
        {
            [DebuggerStepThrough]
            get { return _callImplementetaionInterpretation; }
        }

        public PartialCallWrapper(string alias, string callImplementationId, SymbolInterpretations callImplementetaionInterpretation)
            : base(alias)
        {
            if (callImplementationId == null)
                throw new ArgumentNullException("callImplementationId");

            _callImplementationId = callImplementationId;
            _callImplementetaionInterpretation = callImplementetaionInterpretation;
        }

        #region Overrides of MacroCommand

        private static bool _hasPlaceholder(IAstExpression expr)
        {
            var lit = expr as AstListLiteral;
            return expr.IsPlaceholder() || (lit != null && lit.CheckForPlaceholders());
        }

        protected override void DoExpand(MacroContext context)
        {
            if(context.Invocation.Arguments.Count == 0)
            {
                // Call with no arguments returns null.
                // The macro system will supply that null.
                return;
            }

            var p = context.Invocation.Arguments[0] as AstPlaceholder;
            if(context.Invocation.Arguments.Count == 1 
                && p != null 
                && (p.Index.GetValueOrDefault(0) == 0))
            {
                // call(?0) ⇒ call\perform(?0)

                var cp = GetTrivialPartialApplication(context);
                context.Block.Expression = cp;
                return;
            }

            if(!context.Invocation.Arguments.Any(_hasPlaceholder))
            {
                // no placeholders, invoke call\perform directly

                var call = context.CreateGetSetSymbol(_callImplementetaionInterpretation,
                    context.Call, _callImplementationId, GetCallArguments(context).ToArray());
                context.Block.Expression = call;
                return;
            }

            var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column,
                CallStar.Instance.Id,
                SymbolInterpretations.MacroCommand);

            // Protected the first two arguments
            inv.Arguments.Add(context.CreateConstant(GetPassThroughArguments(context)));

            // Indicate the kind of call by passing `call(?)`, a partial application of call
            var paCall = context.CreateGetSetSymbol(_callImplementetaionInterpretation,
                context.Invocation.Call, _callImplementationId,
                new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                    context.Invocation.Column, 0));
            inv.Arguments.Add(paCall);

            // Pass all the other arguments through
            inv.Arguments.AddRange(GetCallArguments(context));

            context.Block.Expression = inv;
        }

        protected virtual AstGetSetSymbol GetTrivialPartialApplication(MacroContext context)
        {
            var cp = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, context.Invocation.Call, _callImplementationId,
                _callImplementetaionInterpretation);
            cp.Arguments.Add(new AstPlaceholder(context.Invocation.File, context.Invocation.Line, context.Invocation.Column,0));
            return cp;
        }

        protected virtual IEnumerable<IAstExpression> GetCallArguments(MacroContext context)
        {
            return context.Invocation.Arguments;
        }

        protected virtual int GetPassThroughArguments(MacroContext context)
        {
            return 2;
        }

        #endregion

        #region Overrides of PartialMacroCommand

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            DoExpand(context);
            return true;
        }

        #endregion
    }
}