using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CallMacro : MacroCommand
    {
        public const string Alias = @"call\macro";
        public CallMacro() : base(Alias)
        {
        }

        #region Call\Macro\MakeClosure

        private class MakeClosure : PCommand
        {
            private readonly Loader _loader;

            public MakeClosure(Loader loader)
            {
                _loader = loader;
            }

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                if (args.Length < 4)
                    throw new PrexoniteException("Id of macro implementation, effect flag, call type and/or context missing.");

                var id = args[0].Value as string;
                if(args[0].Type != PType.String || id == null)
                    throw new PrexoniteException("First argument must be id.");

                var func = _loader.ParentApplication.Functions[id];
                if(func == null)
                    throw new PrexoniteException("Macro implementation does not exist.");

                var context = args[1].Value as MacroContext;
                if(!(args[1].Type is ObjectPType) || context == null)
                    throw new PrexoniteException("Macro context is missing.");

                var target = _loader.FunctionTargets[context.Function];

                if(!(args[2].Type is ObjectPType && args[2].Value is PCall))
                    throw new PrexoniteException("Call type is missing.");
                var call = (PCall) args[2].Value;

                if(args[3].Type != PType.Bool)
                    throw new PrexoniteException("Effect flag is missing.");
                var justEffect = (bool) args[3].Value;

                var argList = Call.FlattenArguments(sctx, args, 4);
                var offender =
                    argList.FirstOrDefault(
                        p => !(p.Type is ObjectPType) || !(p.Value is IAstExpression));
                if (offender != null)
                    throw new PrexoniteException(
                        string.Format(
                            "Macros cannot have runtime values as arguments. {0} is not an AST node.",
                            offender));

                var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                                                 context.Invocation.Column, id,
                                                 SymbolInterpretations.Function);
                inv.Arguments.AddRange(argList.Select(p => (IAstExpression) p.Value));
                inv.Call = call;

                var subContext = new MacroContext(target.CurrentMacroSession, inv, justEffect);
                var macro = MacroSession.PrepareMacroImplementation(sctx, func, subContext);

                return sctx.CreateNativePValue(macro);
            }

            #endregion
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                                      "call\\macro must be supplied a macro reference.");
                return;
            }

            var inv = context.Invocation;
            var macroReference = inv.Arguments[0];

            if (!context.CallerIsMacro())
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                                      string.Format(
                                          "call\\macro called from {0}. " +
                                          "call\\macro can only be called from a macro context, i.e., from a macro function or an " +
                                          "inner function of a macro.", context.Function.LogicalId));
                return;
            }

            context.EstablishMacroContext();
            SymbolInterpretations macroInterpretation;
            string macroId;
            
            if(!_parseReference(context, macroReference, out macroId, out macroInterpretation))
                return;

            switch (macroInterpretation)
            {
                case SymbolInterpretations.Function:
                    //code for macro as function
                    var theMacro = new AstCreateClosure(inv.File, inv.Line, inv.Column, macroId);
                    var c = new AstIndirectCall(inv.File, inv.Line, inv.Column, PCall.Get, theMacro);
                    break;
                case SymbolInterpretations.MacroCommand:
                    break;
                default:
                    context.ReportMessage(ParseMessageSeverity.Error, "Macro resolves to invalid interpretation.");
                    return;
            }
        }

        private bool _parseReference(MacroContext context, IAstExpression macroReference, out string macroId, out SymbolInterpretations macroInterpretation)
        {
            macroId = null;

            var reference = macroReference as AstGetSetReference;
            var constant = macroReference as AstConstant;
            string constantId;
            SymbolEntry symbolEntry;
            if (reference != null && _isMacro(context, reference.Interpretation, reference.Id))
            {
                macroId = reference.Id;
                macroInterpretation = reference.Interpretation;
            }
            else if (constant != null
                     && (constantId = constant.Constant as string) != null
                     && context.GlobalSymbols.TryGetValue(constantId, out symbolEntry)
                     && _isMacro(context, symbolEntry.Interpretation, symbolEntry.Id))
            {
                macroId = symbolEntry.Id;
                macroInterpretation = symbolEntry.Interpretation;
                context.ReportMessage(ParseMessageSeverity.Warning,
                                      "Call to macro specified as string. Local declarations not taken into account. Use references where possible.");
            }
            else
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                                      string.Format(
                                          "Cannot infer macro from {0}. call\\macro requires the macro reference to be passed as a reference that is known at compile-time.",
                                          macroReference));
                macroInterpretation = SymbolInterpretations.Undefined;
                return false;
            }

            if (macroId == null)
            {
                context.ReportMessage(ParseMessageSeverity.Error, "Unable to infer macro id.");
                return false;
            }

            return true;
        }

        private bool _isMacro(MacroContext context, SymbolInterpretations interpretation, string id)
        {
            if (interpretation == SymbolInterpretations.Function)
            {
                PFunction func;
                return context.Application.Functions.TryGetValue(id, out func) && func.IsMacro;
            }
            else
            {
                return interpretation == SymbolInterpretations.MacroCommand;
            }
        }

        #endregion
    }
}
