using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class Pack : MacroCommand
    {
        public const string Alias = @"macro\pack";

        #region Singleton pattern

        private static readonly Pack _instance = new Pack();

        public static Pack Instance
        {
            get { return _instance; }
        }

        private Pack() : base(Alias)
        {
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if(context.Invocation.Arguments.Count < 1)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    string.Format("Must supply an object to be transported to {0}.", Alias));
                return;
            }

            context.EstablishMacroContext();

            // [| context.StoreForTransport(boxed($arg0)) |]

            var getContext = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalReferenceVariable, PCall.Get, MacroAliases.ContextAlias);
            var boxedArg0 = context.CreateGetSetSymbol(SymbolInterpretations.Command, PCall.Get,
                Engine.BoxedAlias, context.Invocation.Arguments[0]);
            context.Block.Expression = context.CreateGetSetMember(getContext, PCall.Get,
                "StoreForTransport", boxedArg0);
        }

        #endregion
    }
}
