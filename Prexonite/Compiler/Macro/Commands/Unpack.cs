using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class Unpack : MacroCommand
    {

        public const string Alias = @"macro\unpack";

        #region Singleton pattern

        private static readonly Unpack _instance = new Unpack();

        public static Unpack Instance
        {
            get { return _instance; }
        }

        private Unpack() : base(Alias)
        {
        }

        public static IEnumerable<KeyValuePair<string, PCommand>> GetHelperCommands(Loader ldr)
        {
            yield return
                new KeyValuePair<string, PCommand>(Impl.Alias, Impl.Instance);
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if(context.Invocation.Arguments.Count < 1)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    string.Format(
                        "{0} requires at least one argument, the id of the object to unpack.", Alias));
                return;
            }

            context.EstablishMacroContext();

            // [| macro\unpack\impl(context, $arg0) |]

            var getContext = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalReferenceVariable, PCall.Get, MacroAliases.ContextAlias);

            context.Block.Expression = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                PCall.Get, Impl.Alias, getContext, context.Invocation.Arguments[0]);
        }

        #endregion

        private class Impl : PCommand
        {
            public const string Alias = @"macro\unpack\impl";

            #region Singleton pattern

            private static readonly Impl _instance = new Impl();

            public static Impl Instance
            {
                get { return _instance; }
            }

            private Impl()
            {
            }

            #endregion

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                MacroContext context;
                if (args.Length < 2 || !(args[0].Type is ObjectPType) || (context = args[0].Value as MacroContext) == null)
                    throw new PrexoniteException(_getUsage());

                int id;
                if (args[1].TryConvertTo(sctx, true, out id))
                    return context.RetrieveFromTransport(id);

                AstConstant constant;
                if(!(args[1].Type is ObjectPType) || (constant = args[1].Value as AstConstant) == null || !(constant.Constant is int))
                    throw new PrexoniteException(_getUsage());

                return context.RetrieveFromTransport((int) constant.Constant);
            }

            private static string _getUsage()
            {
                return string.Format("usage {0}(context, id)", Alias);
            }

            #endregion
        }
    }
}
