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

        #region Singleton pattern

        private static readonly CallMacro _instance = new CallMacro();

        public static CallMacro Instance
        {
            get { return _instance; }
        }

        private CallMacro() : base(Alias)
        {
        }

        #endregion

        public static IEnumerable<KeyValuePair<string,PCommand>> GetHelperCommands(Loader ldr)
        {
            yield return
                new KeyValuePair<string, PCommand>(PrepareMacro.Alias, new PrepareMacro(ldr));
        }

        #region Call\Macro\PrepareMacro

        private class PrepareMacro : PCommand
        {
            public const string Alias = @"call\macro\prepare_macro";
            private readonly Loader _loader;

            public PrepareMacro(Loader loader)
            {
                _loader = loader;
            }

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                const int varargOffset = 5;

                if (args.Length < varargOffset)
                    throw new PrexoniteException("Id of macro implementation, effect flag, call type and/or context missing.");

                var id = args[0].Value as string;
                if(args[0].Type != PType.String || id == null)
                    throw new PrexoniteException("First argument must be id.");

                if(!(args[1].Type is ObjectPType) || !(args[1].Value is SymbolInterpretations))
                    throw new PrexoniteException("Second argument must be symbol interpretation.");
                var macroInterpretation = (SymbolInterpretations) args[1].Value;

                var context = args[2].Value as MacroContext;
                if(!(args[2].Type is ObjectPType) || context == null)
                    throw new PrexoniteException("Macro context is missing.");

                var target = _loader.FunctionTargets[context.Function];

                if(!(args[3].Type is ObjectPType && args[3].Value is PCall))
                    throw new PrexoniteException("Call type is missing.");
                var call = (PCall) args[3].Value;

                if(args[4].Type != PType.Bool)
                    throw new PrexoniteException("Effect flag is missing.");
                var justEffect = (bool) args[4].Value;

                var argList = Call.FlattenArguments(sctx, args, varargOffset);
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
                                                 macroInterpretation);
                inv.Arguments.AddRange(argList.Select(p => (IAstExpression) p.Value));
                inv.Call = call;

                return
                    CompilerTarget.CreateFunctionValue(
                        (callSite, _) =>
                            {
                                MacroSession macroSession = null;

                                try
                                {
                                    macroSession = target.AcquireMacroSession();

                                    return callSite.CreateNativePValue(
                                        macroSession.ExpandMacro(inv, justEffect));
                                }
                                finally
                                {
                                    if(macroSession != null)
                                        target.ReleaseMacroSession(macroSession);
                                }
                            });
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
            PCall call;
            bool justEffect;
            IAstExpression[] args;
            
            if(!_parseReference(context, macroReference, out macroId, out macroInterpretation, out call, out justEffect, out args))
                return;

            // [| call\macro\prepare_macro("$macroId", $macroInterpretation, context, $call, $justEffect, $args...).() |]
            var prepareCall = _prepareMacro(context, inv, macroId, macroInterpretation, call, justEffect, args);

            var invocation = new AstIndirectCall(inv.File, inv.Line, inv.Column,
                                                 context.Call, prepareCall);

            context.Block.Expression = invocation;
        }

        private AstGetSetSymbol _prepareMacro(MacroContext context, AstMacroInvocation inv, string macroId, SymbolInterpretations macroInterpretation, PCall call, bool justEffect, IAstExpression[] args)
        {
            var macroIdConst = new AstConstant(inv.File, inv.Line, inv.Column, macroId);
            var macroInterpretationExpr = macroInterpretation.ToExpression(context);
            var getContext = context.CreateGetSetLocal(MacroAliases.ContextAlias);
            var callExpr = call.ToExpression(context);
            var justEffectExpr = context.CreateConstant(justEffect);
            var prepareCall = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                                                         PCall.Get, PrepareMacro.Alias,
                                                         macroIdConst,
                                                         macroInterpretationExpr,
                                                         getContext, callExpr,
                                                         justEffectExpr);
            prepareCall.Arguments.AddRange(args);
            return prepareCall;
        }

        private bool _parseReference(
            MacroContext context, 
            IAstExpression macroReference, 
            out string macroId, 
            out SymbolInterpretations macroInterpretation,
            out PCall call,
            out bool justEffect,
            out IAstExpression[] args)
        {
            macroId = null;
            call = PCall.Get;
            justEffect = false;
            args = new IAstExpression[0];

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
