using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CallSub : MacroCommand
    {
        public const string Alias = @"call\sub";

        #region Singleton pattern

        private static readonly CallSub _instance = new CallSub();

        public static CallSub Instance
        {
            get { return _instance; }
        }

        private CallSub() : base(Alias)
        {
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            var perform = context.CreateGetSetSymbol(SymbolInterpretations.Command, PCall.Get,
                                                     Engine.CallSubPerformAlias,
                                                     context.Invocation.Arguments.ToArray());
            var interpret = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                                                   context.Invocation.Column, CallSubInterpret.Alias,
                                                   SymbolInterpretations.MacroCommand);
            interpret.Arguments.Add(perform);

            context.Block.Expression = interpret;
        }

        #endregion
    }
}
