using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Commands;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CallMacro : PartialMacroCommand
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

        public static KeyValuePair<string,CallMacroPerform> GetHelperCommands(Loader ldr)
        {
            return
                new KeyValuePair<string, CallMacroPerform>(CallMacroPerform.Alias, new CallMacroPerform(ldr));
        }

        #region Call\Macro\PrepareMacro

        public class CallMacroPerform : PCommand
        {
// ReSharper disable MemberHidesStaticFromOuterClass
            public const string Alias = @"call\macro\perform";
// ReSharper restore MemberHidesStaticFromOuterClass
            private readonly Loader _loader;

            public CallMacroPerform(Loader loader)
            {
                _loader = loader;
            }

            #region Overrides of PCommand

            private static SymbolInterpretations _inferInterpretation(PValue arg)
            {
                const string notRecognized = "{0} is not recognizable as a macro.";
                if (!(arg.Type is ObjectPType))
                    throw new PrexoniteException(string.Format(
                        notRecognized, arg));


                var raw = arg.Value;
                var cmd = raw as MacroCommand;
                var func = raw as PFunction;

                if (cmd != null)
                    return SymbolInterpretations.MacroCommand;
                else if (func != null && func.IsMacro)
                    return SymbolInterpretations.Function;
                else
                    throw new PrexoniteException(string.Format(
                        notRecognized, arg));
            }

            public const int CallingConventionArgumentsCount = 4;

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                if (args.Length < CallingConventionArgumentsCount)
                    throw new PrexoniteException("Id of macro implementation, effect flag, call type and/or context missing.");

                string id;
                SymbolInterpretations macroInterpretation;

                //Parse arguments
                _getMacro(sctx, args[0], out id, out macroInterpretation);
                var context = _getContext(args[1]);
                var call = _getCallType(args[2]);
                var justEffect = _getEffectFlag(args[3]);

                // Prepare macro
                var target = _loader.FunctionTargets[context.Function];
                var argList = Call.FlattenArguments(sctx, args, CallingConventionArgumentsCount);
                _detectRuntimeValues(argList);

                var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                                                 context.Invocation.Column, id,
                                                 macroInterpretation);
                inv.Arguments.AddRange(argList.Select(p => (IAstExpression) p.Value));
                inv.Call = call;

                //Execute the macro
                MacroSession macroSession = null;
                try
                {
                    macroSession = target.AcquireMacroSession();

                    return sctx.CreateNativePValue(
                        macroSession.ExpandMacro(inv, justEffect));
                }
                finally
                {
                    if(macroSession != null)
                        target.ReleaseMacroSession(macroSession);
                }
            
            }

            private static void _detectRuntimeValues(List<PValue> argList)
            {
                var offender =
                    argList.FirstOrDefault(
                        p => !(p.Type is ObjectPType) || !(p.Value is IAstExpression));
                if (offender != null)
                    throw new PrexoniteException(
                        string.Format(
                            "Macros cannot have runtime values as arguments in call to {1}. {0} is not an AST node.",
                            offender,Alias));
            }

            private static bool _getEffectFlag(PValue rawEffectFlag)
            {
                if(rawEffectFlag.Type != PType.Bool)
                    throw new PrexoniteException(string.Format("Effect flag is missing in call to {0}.", Alias));
                return (bool) rawEffectFlag.Value;
            }

            private static PCall _getCallType(PValue rawCallType)
            {
                if(!(rawCallType.Type is ObjectPType && rawCallType.Value is PCall))
                    throw new PrexoniteException(string.Format("Call type is missing in call to {0}.", Alias));
                return (PCall) rawCallType.Value;
            }

            private static MacroContext _getContext(PValue rawContext)
            {
                var context = rawContext.Value as MacroContext;
                if(!(rawContext.Type is ObjectPType) || context == null)
                    throw new PrexoniteException(string.Format("Macro context is missing in call to {0}.", Alias));
                return context;
            }

            private static void _getMacro(StackContext sctx, PValue rawMacro, out string id, out SymbolInterpretations macroInterpretation)
            {
                var list = rawMacro.Value as List<PValue>;
                if(rawMacro.Type == PType.List && list != null)
                {
                    if (list.Count < 2)
                        throw new PrexoniteException(
                            string.Format(
                                "First argument to {0} is a list, it must contain the macro id and its interpretation.",
                                CallMacro.Alias));

                    id = list[0].Value as string;
                    if (list[0].Type != PType.String || id == null)
                        throw new PrexoniteException(string.Format("First argument must be id in call to {0}.", Alias));

                    if (!(list[1].Type is ObjectPType) || !(list[1].Value is SymbolInterpretations))
                        throw new PrexoniteException(string.Format("Second argument must be symbol interpretation in call to {0}.", Alias));
                    macroInterpretation = (SymbolInterpretations)list[1].Value;
                }
                else
                {
                    id = rawMacro.DynamicCall(sctx, Cil.Runtime.EmptyPValueArray, PCall.Get,
                        PFunction.IdKey).ConvertTo<string>(sctx, false);
                    macroInterpretation = _inferInterpretation(rawMacro);
                }
            }

            #endregion

            private readonly PartialCallMacroPerform _partial = new PartialCallMacroPerform();

            public PartialCallMacroPerform Partial
            {
                [DebuggerStepThrough]
                get { return _partial; }
            }

            public class PartialCallMacroPerform : PartialCallWrapper
            {
// ReSharper disable MemberHidesStaticFromOuterClass
                public const string Alias = @"call\macro\impl";
// ReSharper restore MemberHidesStaticFromOuterClass

                public PartialCallMacroPerform()
                    : base(Alias,CallMacroPerform.Alias,SymbolInterpretations.Command)
                {
                    
                }

                protected override int GetPassThroughArguments(MacroContext context)
                {
                    return CallingConventionArgumentsCount + 1; //Take reference to call\macro\perform into account.
                }
            }
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            var prepareCall = _assembleCallPerform(context);

            //null indicates failure, error has already been reported.
            if(prepareCall == null)
                return;

            context.Block.Expression = prepareCall;
        }

        #region Helper routines

        /// <summary>
        /// Establishes macro context and parses arguments.
        /// </summary>
        /// <param name="context">The macro context.</param>
        /// <returns>The call to call\macro\perform expression on success; null otherwise.</returns>
        private AstMacroInvocation _assembleCallPerform(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    "call\\macro must be supplied a macro reference.");
                return null;
            }

            if (!context.CallerIsMacro())
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    string.Format(
                        "call\\macro called from {0}. " +
                            "call\\macro can only be called from a macro context, i.e., from a macro function or an " +
                                "inner function of a macro.", context.Function.LogicalId));
                return null;
            }

            context.EstablishMacroContext();

            IAstExpression call;
            IAstExpression justEffect;
            IAstExpression[] args;
            IAstExpression macroSpec;

            if (!_parseArguments(context, out call, out justEffect, out args, false, out macroSpec))
                return null;

            // [| call\macro\prepare_macro("$macroId", $macroInterpretation, context, $call, $justEffect, $args...) |]
            return _prepareMacro(context, macroSpec, call, justEffect, args);
        }

        private AstMacroInvocation _prepareMacro(MacroContext context, IAstExpression macroSpec, IAstExpression call, IAstExpression justEffect, IAstExpression[] args)
        {
            var getContext = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalReferenceVariable, PCall.Get, MacroAliases.ContextAlias);
            var prepareCall = context.CreateMacroInvocation(context.Call,
                CallMacroPerform.PartialCallMacroPerform.Alias, SymbolInterpretations.MacroCommand, macroSpec,
                getContext, call,
                justEffect);
            prepareCall.Arguments.AddRange(args);
            return prepareCall;
        }

        private bool _parseArguments(MacroContext context, out IAstExpression call, out IAstExpression justEffect, out IAstExpression[] args, bool isPartialApplication, out IAstExpression macroSpec)
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
             * call([proto(...1),je,c],...2) = call([],from_proto(proto),[je,c],[...1],...2);
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

                args = inv.Arguments.Skip(1).ToArray();
                macroSpec = invokeSpec;
                return _parseReference(context, inv.Arguments[0], isPartialApplication);
            }
            else if (listSpec.Elements.Count == 0)
            {
                // - Macro reference specified as expression that evaluates to an actual macro reference
                // - followed by a list of options

                macroSpec = invokeSpec;

                AstListLiteral optionsRaw;
                if (inv.Arguments.Count < 3 ||
                    (optionsRaw = inv.Arguments[2] as AstListLiteral) == null)
                {
                    _errorUsageFullRef(context, isPartialApplication);
                    args = null;
                    macroSpec = null;
                    return false;
                }

                //first option: justEffect
                if (optionsRaw.Elements.Count >= 1)
                    justEffect = optionsRaw.Elements[0];

                //second option: call type
                if (optionsRaw.Elements.Count >= 2)
                    call = optionsRaw.Elements[1];

                //args: except first 3
                args = inv.Arguments.Skip(3).ToArray();

                return _parseReference(context, inv.Arguments[1],
                    isPartialApplication);
            }
            else
            {
                // - Macro reference specified as a prototype
                // - includes list of options

                var proto = listSpec.Elements[0] as AstMacroInvocation;
                if (proto == null)
                {
                    _errorUsagePrototype(context, isPartialApplication);
                    args = null;
                    macroSpec = null;
                    return false;
                }

                macroSpec = _getMacroSpecExpr(context, proto);

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
                if (setArgs != null)
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

        private static IAstExpression _getMacroSpecExpr(MacroContext context, AstMacroInvocation proto)
        {
            IAstExpression macroSpec;
            //macroId: as a constant
            var macroId = context.CreateConstant(proto.MacroId);

            //macroInterpretation: as an expression
            var macroInterpretation = proto.Interpretation.EnumToExpression(proto);

            var listLit = new AstListLiteral(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column);
            listLit.Elements.Add(macroId);
            listLit.Elements.Add(macroInterpretation);

            macroSpec = listLit;
            return macroSpec;
        }

        private void _errorUsagePrototype(MacroContext context, bool isPartialApplication)
        {
            context.ReportMessage(ParseMessageSeverity.Error,
                string.Format(
                    "Used in this way, {0} has the form {0}([macroPrototype(...),justEffect?,call?],...).",
                    Alias));
        }

        private bool _parseReference(MacroContext context, IAstExpression macroRef, bool isPartialApplication)
        {
            if(macroRef.IsPlaceholder())
            {
                context.ReportMessage(ParseMessageSeverity.Error, "The macro prototype must be known at compile-time, it must not be a placeholder.");
                return false;
            }

            return true;
        }

        private void _errorUsageFullRef(MacroContext context, bool isPartialApplication)
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
        
        #endregion

        #region Overrides of PartialMacroCommand

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            var prepareCall = _assembleCallPerform(context);

            //null indicates failure, error has already been reported.
            if (prepareCall == null)
                return true;

            context.Block.Expression = prepareCall;

            return true;
        }

        #endregion
    }
}
