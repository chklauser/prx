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
            yield return
                new KeyValuePair<string, PCommand>(InferInterpretation.Alias, InferInterpretation.Instance);
        }

        #region Call\Macro\PrepareMacro

        private class InferInterpretation : PCommand
        {
// ReSharper disable MemberHidesStaticFromOuterClass
            public const string Alias = @"call\macro\infer_interpretation";


            #region Singleton pattern

            private static readonly InferInterpretation _instance = new InferInterpretation();

            public static InferInterpretation Instance
            {
                get { return _instance; }
            }

            private InferInterpretation()
            {
            }
// ReSharper restore MemberHidesStaticFromOuterClass

            #endregion

            #region Overrides of PCommand

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                if(args.Length < 1)
                    throw new PrexoniteException(Alias + " requires at least one argument.");

                if(!(args[0].Type is ObjectPType))
                    throw new PrexoniteException(string.Format("Cannot infer interpretation of macro from {0}.", args[0]));

                var raw = args[0].Value;
                var cmd = raw as MacroCommand;
                var func = raw as PFunction;

                if (cmd != null)
                    return sctx.CreateNativePValue(SymbolInterpretations.MacroCommand);
                else if (func != null && func.IsMacro)
                    return sctx.CreateNativePValue(SymbolInterpretations.Function);
                else
                    throw new PrexoniteException(string.Format(
                        "{0} is not recognizable as a macro.", raw));
            }

            #endregion
        }

        private class PrepareMacro : PCommand
        {
// ReSharper disable MemberHidesStaticFromOuterClass
            public const string Alias = @"call\macro\prepare_macro";
// ReSharper restore MemberHidesStaticFromOuterClass
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
                    throw new PrexoniteException(string.Format("First argument must be id in call to {0}.", Alias));

                if(!(args[1].Type is ObjectPType) || !(args[1].Value is SymbolInterpretations))
                    throw new PrexoniteException(string.Format("Second argument must be symbol interpretation in call to {0}.", Alias));
                var macroInterpretation = (SymbolInterpretations) args[1].Value;

                var context = args[2].Value as MacroContext;
                if(!(args[2].Type is ObjectPType) || context == null)
                    throw new PrexoniteException(string.Format("Macro context is missing in call to {0}.", Alias));

                var target = _loader.FunctionTargets[context.Function];

                if(!(args[3].Type is ObjectPType && args[3].Value is PCall))
                    throw new PrexoniteException(string.Format("Call type is missing in call to {0}.", Alias));
                var call = (PCall) args[3].Value;

                if(args[4].Type != PType.Bool)
                    throw new PrexoniteException(string.Format("Effect flag is missing in call to {0}.", Alias));
                var justEffect = (bool) args[4].Value;

                var argList = Call.FlattenArguments(sctx, args, varargOffset);
                var offender =
                    argList.FirstOrDefault(
                        p => !(p.Type is ObjectPType) || !(p.Value is IAstExpression));
                if (offender != null)
                    throw new PrexoniteException(
                        string.Format(
                            "Macros cannot have runtime values as arguments in call to {1}. {0} is not an AST node.",
                            offender,Alias));

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

            IAstExpression macroInterpretation;
            IAstExpression macroId;
            IAstExpression call;
            IAstExpression justEffect;
            IAstExpression[] args;
            
            if(!_parseArguments(context, out macroId, out macroInterpretation, out call, out justEffect, out args))
                return;

            // [| call\macro\prepare_macro("$macroId", $macroInterpretation, context, $call, $justEffect, $args...).() |]
            var prepareCall = _prepareMacro(context, macroId, macroInterpretation, call, justEffect, args);

            var invocation = new AstIndirectCall(inv.File, inv.Line, inv.Column,
                                                 context.Call, prepareCall);

            context.Block.Expression = invocation;
        }

        private AstGetSetSymbol _prepareMacro(MacroContext context, IAstExpression macroId, IAstExpression macroInterpretation, IAstExpression call, IAstExpression justEffect, IAstExpression[] args)
        {
            var getContext = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalReferenceVariable, PCall.Get, MacroAliases.ContextAlias);
            var prepareCall = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                                                         PCall.Get, PrepareMacro.Alias,
                                                         macroId,
                                                         macroInterpretation,
                                                         getContext, call,
                                                         justEffect);
            prepareCall.Arguments.AddRange(args);
            return prepareCall;
        }

        private bool _parseArguments(
            MacroContext context, 
            out IAstExpression macroId, 
            out IAstExpression macroInterpretation,
            out IAstExpression call,
            out IAstExpression justEffect,
            out IAstExpression[] args)
        {
            /* call(macroRef,...) = call([],macroRef,[false],...);
             * call([],macroRef,[je],...) = call([],macroRef,[je,context.Call],...);
             * call([],macroRef,[je,c],...) = { macroId := macroRef.Id; 
             *                                  macroInterpretation := interpretation(macroRef); 
             *                                  call := c; 
             *                                  justEffect := je 
             *                                }
             * call([proto(...1)],...2) = call([],from_proto(proto),[false,PCall.Get],[...1],...2);
             * call([proto(...1) = x],...2) = call([],from_proto(proto),[false,PCall.Set],[...1],...2,[x]);
             * call([proto(...1),je],...2) = call([],from_proto(proto),[je,PCall.Get],[...1],...2);
             * call([proto(...1) = x,je],...2) = call([],from_proto(proto),[je,PCall.Set],[...1],...2,[x]);
             * call([proto(...1),je,call],...2) = call([],from_proto(proto),[je,call],[...1],...2);
             */

            var inv = context.Invocation;
            justEffect = new AstConstant(inv.File, inv.Line,
                                         inv.Column, false);
            call = PCall.Get.EnumToExpression(context.Invocation);

            var invokeSpec = inv.Arguments[0];
            var listSpec = invokeSpec as AstListLiteral;
            if (listSpec == null)
            {
                // - Macro reference specified as expression that evaluates to an actual macro reference
                _parseReference(context, inv.Arguments[0], out macroId, out macroInterpretation);
                args = inv.Arguments.Skip(1).ToArray();
                return true;
            }
            else if (listSpec.Elements.Count == 0)
            {
                // - Macro reference specified as expression that evaluates to an actual macro reference
                // - followed by a list of options

                AstListLiteral optionsRaw;
                if (inv.Arguments.Count < 3 ||
                    (optionsRaw = inv.Arguments[2] as AstListLiteral) == null)
                {
                    _errorUsageFullRef(context);
                    args = null;
                    macroId = null;
                    macroInterpretation = null;
                    return false;
                }

                //first option: justEffect
                if (optionsRaw.Elements.Count >= 1)
                    justEffect = optionsRaw.Elements[0];

                //second option: call type
                if (optionsRaw.Elements.Count >= 2)
                    call = optionsRaw.Elements[1];

                _parseReference(context, inv.Arguments[1], out macroId, out macroInterpretation);

                //args: except first 3
                args = inv.Arguments.Skip(3).ToArray();
                return true;
            }
            else
            {
                // - Macro reference specified as a prototype
                // - includes list of options

                var proto = listSpec.Elements[0] as AstMacroInvocation;
                if(proto == null)
                {
                    _errorUsagePrototype(context);
                    args = null;
                    macroId = null;
                    macroInterpretation = null;
                    return false;
                }

                //first option: justEffect
                if (listSpec.Elements.Count >= 2)
                    justEffect = listSpec.Elements[1];

                //second option: call type
                if (listSpec.Elements.Count >= 3)
                    call = listSpec.Elements[2];
                else
                {
                    call = proto.Call.EnumToExpression(proto);
                }

                //macroId: as a constant
                macroId = context.CreateConstant(proto.MacroId);

                //macroInterpretation: as an expression
                macroInterpretation = proto.Interpretation.EnumToExpression(proto);

                //args: lift and pass prototype arguments, special care for set
                var setArgs = proto.Call == PCall.Set
                    ? proto.Arguments.Last()
                    : null;
                var getArgs = proto.Call == PCall.Set
                    ? proto.Arguments.Take(proto.Arguments.Count - 1)
                    : proto.Arguments;

                var getArgsLit = new AstListLiteral(proto.File, proto.Line, proto.Column);
                getArgsLit.Elements.AddRange(getArgs);

                IEnumerable<IAstExpression> setArgsLit;
                if(setArgs != null)
                {
                    var lit = new AstListLiteral(setArgs.File, setArgs.Line, setArgs.Column);
                    lit.Elements.Add(setArgs);
                    setArgsLit = lit.Singleton();
                }
                else
                {
                    setArgsLit = Enumerable.Empty<IAstExpression>();
                }

                args = getArgsLit.Singleton()
                    .Append(inv.Arguments.Skip(1))
                    .Append(setArgsLit).ToArray();

                return true;
            }
        }

        private void _errorUsagePrototype(MacroContext context)
        {
            context.ReportMessage(ParseMessageSeverity.Error,
                                  string.Format(
                                      "Used in this way, {0} has the form {0}([macroPrototype(...),justEffect?,call?],...).",
                                      Alias));
        }

        private void _parseReference(MacroContext context, IAstExpression macroRef, out IAstExpression macroId, out IAstExpression macroInterpretation)
        {
            var macroRefGetSet = macroRef as AstGetSet;
            if (macroRefGetSet != null)
                macroRefGetSet = macroRefGetSet.GetCopy();

            //macroId: via ?.Id member access
            macroId = new AstGetSetMemberAccess(macroRef.File, macroRef.Line, macroRef.Column, PCall.Get,
                                                macroRef, PFunction.IdKey);

            //macroInterpretation: via infer_interpretation command
            macroInterpretation = context.CreateGetSetSymbol(SymbolInterpretations.Command,
                                                             PCall.Get,
                                                             InferInterpretation.Alias,
                                                             macroRefGetSet ?? macroRef);
        }

        private void _errorUsageFullRef(MacroContext context)
        {
            context.ReportMessage(ParseMessageSeverity.Error, "Used in this way, {0} has the form {0}([],macroRef,[justEffect?,call?],...).");
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
