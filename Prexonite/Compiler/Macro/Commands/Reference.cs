using System;
using System.Collections.Generic;
using Prexonite.Commands;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Properties;

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

        private Reference()
            : base(Alias) { }

        public static IEnumerable<KeyValuePair<string, PCommand>> GetHelperCommands(Loader ldr)
        {
            yield return new KeyValuePair<string, PCommand>(Impl.Alias, new Impl(ldr));
        }

        #endregion

        private class Impl : PCommand
        {
            // ReSharper disable MemberHidesStaticFromOuterClass // not an issue
            public const string Alias = Reference.Alias + @"\impl";

            // ReSharper restore MemberHidesStaticFromOuterClass

            private readonly Loader _loader;

            public Impl(Loader loader)
            {
                _loader = loader;
            }

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
            {
                if (args.Length < 3)
                    throw new PrexoniteException(
                        string.Format("{0} requires at least 3 arguments.", Alias)
                    );

                var id = args[0].CallToString(sctx);
                var interpretation = (SymbolInterpretations)args[1].Value!;
                var module = args[2].Value as ModuleName;
                if (module == null)
                {
                    var moduleRaw = args[2].Value as string;
                    if (moduleRaw != null)
                    {
                        if (!ModuleName.TryParse(moduleRaw, out module))
                            throw new PrexoniteException(
                                "Invalid module name \"" + moduleRaw + "\"."
                            );
                    }
                }

                switch (interpretation)
                {
                    case SymbolInterpretations.Function:
                        PFunction? func;
                        if (module == null)
                        {
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot create reference to function {0}. Module name is missing.",
                                    id
                                )
                            );
                        }
                        else if (sctx.ParentApplication.TryGetFunction(id, module, out func))
                        {
                            return sctx.CreateNativePValue(func);
                        }
                        else
                        {
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot create reference to function {0} from module {1}. Function or module is missing from context.",
                                    id,
                                    module
                                )
                            );
                        }
                    case SymbolInterpretations.MacroCommand:
                        return sctx.CreateNativePValue(_loader.MacroCommands[id]);
                    default:
                        throw new PrexoniteException(
                            string.Format(
                                "Unknown macro interpretation {0} in {1}.",
                                Enum.GetName(typeof(SymbolInterpretations), interpretation),
                                Alias
                            )
                        );
                }
            }

            #endregion
        }

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(
                    Message.Error(
                        string.Format(Resources.Reference_requires_at_least_one_argument, Alias),
                        context.Invocation.Position,
                        MessageClasses.ReferenceUsage
                    )
                );
                return;
            }

            var prototype = context.Invocation.Arguments[0] as AstExpand;
            if (prototype == null)
            {
                context.ReportMessage(
                    Message.Error(
                        string.Format(
                            Resources.Reference_requires_argument_to_be_a_prototype_of_a_macro_invocation,
                            Alias
                        ),
                        context.Invocation.Position,
                        MessageClasses.ReferenceUsage
                    )
                );
            }
            else
            {
                context.Block.Expression = _assembleImplCall(
                    context,
                    prototype.Entity.ToSymbolEntry(),
                    prototype.Position
                );
            }
        }

        private static AstGetSet _assembleImplCall(
            MacroContext context,
            SymbolEntry implementationSymbolEntry,
            ISourcePosition position
        )
        {
            var internalId = context.CreateConstant(implementationSymbolEntry.InternalId!);
            var interpretation = implementationSymbolEntry.Interpretation.ToExpr(position);
            var moduleNameOpt = context.CreateConstantOrNull(implementationSymbolEntry.Module);
            var implCall = context.Factory.IndirectCall(
                context.Invocation.Position,
                context.Factory.Reference(
                    context.Invocation.Position,
                    EntityRef.Command.Create(Impl.Alias)
                )
            );
            implCall.Arguments.Add(internalId);
            implCall.Arguments.Add(interpretation);
            implCall.Arguments.Add(moduleNameOpt);
            return implCall;
        }

        #endregion
    }
}
