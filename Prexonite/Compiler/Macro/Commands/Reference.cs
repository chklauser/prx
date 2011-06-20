using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class Reference : MacroCommand
    {
        public const string Alias = @"macro\reference";

        #region Singleton pattern

        private static readonly Reference _instance = new Reference();

        public static Reference Instance
        {
            get { return _instance; }
        }

        private Reference() : base(Alias)
        {
        }

        public static IEnumerable<KeyValuePair<string, PCommand>> GetHelperCommands(Loader ldr)
        {
            yield return
                new KeyValuePair<string, PCommand>(Impl.Alias, new Impl(ldr));
        }

        #endregion

        private class Impl : PCommand
        {
            public const string Alias = Reference.Alias + @"\impl";

            private readonly Loader _loader;

            public Impl(Loader loader)
            {
                _loader = loader;
            }

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                if(args.Length < 2)
                    throw new PrexoniteException(string.Format("{0} requires at least 2 arguments.", Alias));

                var id = args[0].CallToString(sctx);
                var interpretation = (SymbolInterpretations) args[1].Value;

                switch (interpretation)
                {
                    case SymbolInterpretations.Function:
                        return sctx.CreateNativePValue(sctx.ParentApplication.Functions[id]);
                    case SymbolInterpretations.MacroCommand:
                        return sctx.CreateNativePValue(_loader.MacroCommands[id]);
                    default:
                        throw new PrexoniteException(
                            string.Format("Unknown macro interpretation {0} in {1}.",
                                Enum.GetName(typeof (SymbolInterpretations), interpretation), Alias));
                }
            }

            #endregion
        }

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if(!context.CallerIsMacro())
            {
                context.ReportMessage(ParseMessageSeverity.Error, string.Format("{0} can only be used in a macro context.", Alias));
                return;
            }

            if(context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(ParseMessageSeverity.Error, "{0} requires at least one argument.");
                return;
            }

            var prototype = context.Invocation.Arguments[0] as AstMacroInvocation;
            if(prototype == null)
            {
                context.ReportMessage(ParseMessageSeverity.Error, "{0} requires argument to be a prototype of a macro invocation.");
                return;
            }

            context.Block.Expression = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                PCall.Get, Impl.Alias,
                context.CreateConstant(prototype.MacroId),
                prototype.Interpretation.EnumToExpression(prototype));
        }

        #endregion
    }
}
