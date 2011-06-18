using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CallStar : PartialMacroCommand
    {
        #region Singleton pattern

        private static readonly CallStar _instance = new CallStar();

        public static CallStar Instance
        {
            get { return _instance; }
        }

        private CallStar()
            : base(@"call\star")
        {
        }

        #endregion

        #region Overrides of PartialMacroCommand

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            if (context.Invocation.Arguments.Count < 1)
            {
                context.ReportMessage(ParseMessageSeverity.Error, "{0} requires at least one argument, the call\\* command/function to invoke.");
                return true;
            }

            int passThrough;
            List<IAstExpression> arguments;
            _determinePassThrough(context, out passThrough, out arguments);

            _expandPartialApplication(context, passThrough, arguments);

            return true;
        }

        private void _expandPartialApplication(MacroContext context, int passThrough, List<IAstExpression> arguments)
        {
            var flatArgs = new List<IAstExpression>(arguments.Count);
            var directives = new List<int>(arguments.Count);

            //The call target is a "non-argument" in partial application terms. Do not include it in the
            //  stream of directives.
            flatArgs.Add(arguments[0]);

            var opaqueSpan = 0;
            for(var i = 1; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                AstListLiteral lit;
                if(i < passThrough || !_isPartialList(arg, out lit))
                {
                    flatArgs.Add(arg);
                    opaqueSpan++;
                }
                else
                {
                    flatArgs.AddRange(lit.Elements);
                    if(opaqueSpan > 0)
                    {
                        directives.Add(opaqueSpan);
                        opaqueSpan = 0;
                    }
                    directives.Add(-lit.Elements.Count);
                }
            }

            var ppArgv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(flatArgs);

            var argc = ppArgv.Count;
            var mappings8 = new int[argc + directives.Count + 1];
            var closedArguments = new List<IAstExpression>(argc);

            AstPartiallyApplicable.GetMapping(ppArgv, mappings8, closedArguments);
            _mergeDirectivesIntoMappings(directives, mappings8, argc);
            var mappings32 = PartialApplicationCommandBase.PackMappings32(mappings8);

            var implCall = context.CreateGetSetSymbol(SymbolInterpretations.Command, context.Call,
                PartialCallStarImplCommand.Alias);
            implCall.Arguments.AddRange(closedArguments);
            
            implCall.Arguments.AddRange(mappings32.Select(m => context.CreateConstant(m)));

            context.Block.Expression = implCall;
        }

        private static void _mergeDirectivesIntoMappings(List<int> directives, int[] mappings8, int argc)
        {
            var mi = argc;
            foreach (var directive in directives)
            {
                mappings8[mi++] = directive;
            }

            mappings8[mappings8.Length - 1] = directives.Count;
        }

        #endregion

        #region Overrides of MacroCommand

        private static bool _isPartialList(IAstExpression expr)
        {
            AstListLiteral lit;
            return _isPartialList(expr, out lit);
        }

        private static bool _isPartialList(IAstExpression expr, out AstListLiteral lit)
        {
            lit = expr as AstListLiteral;
            return lit != null && lit.CheckForPlaceholders();
        }

        protected override void DoExpand(MacroContext context)
        {
            if(context.Invocation.Arguments.Count < 1)
            {
                context.ReportMessage(ParseMessageSeverity.Error, "{0} requires at least one argument, the call\\* command/function to invoke.");
                return;
            }

            int passThrough;
            List<IAstExpression> arguments;
            _determinePassThrough(context, out passThrough, out arguments);

            if(arguments.Skip(passThrough).Any(_isPartialList))
            {
                _expandPartialApplication(context, passThrough, arguments);
                return;
            }

            // "Fallback" direct invocation
            var ic = new AstIndirectCall(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, context.Invocation.Call, arguments[0]);
            ic.Arguments.AddRange(arguments.Skip(1));
            context.Block.Expression = ic;
        }

        private static void _determinePassThrough(MacroContext context, out int passThrough, out List<IAstExpression> arguments)
        {
            var arg0 = context.Invocation.Arguments[0];
            var passThroughNode = arg0 as AstConstant;
            if (passThroughNode != null && passThroughNode.Constant is int)
            {
                arguments = new List<IAstExpression>(context.Invocation.Arguments.Skip(1));
                passThrough = (int)passThroughNode.Constant;
            }
            else
            {
                arguments = new List<IAstExpression>(context.Invocation.Arguments);
                passThrough = 1;
            }

            if (passThrough < 1)
                context.ReportMessage(ParseMessageSeverity.Error,
                    "call\\star must at least pass through one argument (the call target). It has been instructed to pass through " +
                        passThrough + " arguments.", passThroughNode);
        }

        #endregion
    }
}
