// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Commands;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
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

        public static KeyValuePair<string, CallMacroPerform> GetHelperCommands(Loader ldr)
        {
            return
                new KeyValuePair<string, CallMacroPerform>(CallMacroPerform.Alias,
                    new CallMacroPerform(ldr));
        }

        #region Call\Macro\Perform

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

            private static SymbolInterpretations _inferInterpretationAndModule(PValue arg, out ModuleName moduleName)
            {
                const string notRecognized = "{0} is not recognizable as a macro.";
                if (!(arg.Type is ObjectPType))
                    throw new PrexoniteException(string.Format(
                        notRecognized, arg));


                var raw = arg.Value;
                var cmd = raw as MacroCommand;
                var func = raw as PFunction;

                if (cmd != null)
                {
                    moduleName = null;
                    return SymbolInterpretations.MacroCommand;
                }
                else if (func != null && func.IsMacro)
                {
                    throw new NotImplementedException("Cannot infer module name from function object " + func);
                    //return SymbolInterpretations.Function;
                }
                else
                    throw new PrexoniteException(string.Format(
                        notRecognized, arg));
            }

            private const int _callingConventionArgumentsCount = 4;

            public override PValue Run(StackContext sctx, PValue[] args)
            {
                if (args.Length < _callingConventionArgumentsCount)
                    throw new PrexoniteException(
                        "Id of macro implementation, effect flag, call type and/or context missing.");

                var sym = _getMacro(sctx, args[0]);

                //Parse arguments
                var context = _getContext(args[1]);
                var call = _getCallType(args[2]);
                var justEffect = _getEffectFlag(args[3]);

                // Prepare macro
                var target = _loader.FunctionTargets[context.Function];
                var argList = Call.FlattenArguments(sctx, args, _callingConventionArgumentsCount);
                _detectRuntimeValues(argList);

                var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                    context.Invocation.Column, sym);
                inv.Arguments.AddRange(argList.Select(p => (AstExpr) p.Value));
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
                    if (macroSession != null)
                        target.ReleaseMacroSession(macroSession);
                }
            }

            private static void _detectRuntimeValues(IEnumerable<PValue> argList)
            {
                var offender =
                    argList.FirstOrDefault(
                        p => !(p.Type is ObjectPType) || !(p.Value is AstExpr));
                if (offender != null)
                    throw new PrexoniteException(
                        string.Format(
                            "Macros cannot have runtime values as arguments in call to {1}. {0} is not an AST node.",
                            offender, Alias));
            }

            private static bool _getEffectFlag(PValue rawEffectFlag)
            {
                if (rawEffectFlag.Type != PType.Bool)
                    throw new PrexoniteException(
                        string.Format("Effect flag is missing in call to {0}.", Alias));
                return (bool) rawEffectFlag.Value;
            }

            private static PCall _getCallType(PValue rawCallType)
            {
                if (!(rawCallType.Type is ObjectPType && rawCallType.Value is PCall))
                    throw new PrexoniteException(
                        string.Format("Call type is missing in call to {0}.", Alias));
                return (PCall) rawCallType.Value;
            }

            private static MacroContext _getContext(PValue rawContext)
            {
                var context = rawContext.Value as MacroContext;
                if (!(rawContext.Type is ObjectPType) || context == null)
                    throw new PrexoniteException(
                        string.Format("Macro context is missing in call to {0}.", Alias));
                return context;
            }

            private static SymbolEntry _getMacro(StackContext sctx, PValue rawMacro)
            {
                var list = rawMacro.Value as List<PValue>;
                SymbolInterpretations macroInterpretation;
                ModuleName moduleName;
                string id;
                if (rawMacro.Type == PType.List && list != null)
                {
                    if (list.Count < 3)
                        throw new PrexoniteException(
                            string.Format(
                                "First argument to {0} is a list, it must contain the macro id, its interpretation and containing module name.",
                                CallMacro.Alias));

                    id = list[0].Value as string;
                    if (list[0].Type != PType.String || id == null)
                        throw new PrexoniteException(
                            string.Format("First argument must be id in call to {0}.", Alias));

                    if (!(list[1].Type is ObjectPType) || !(list[1].Value is SymbolInterpretations))
                        throw new PrexoniteException(
                            string.Format(
                                "Second argument must be symbol interpretation in call to {0}.",
                                Alias));
                    macroInterpretation = (SymbolInterpretations) list[1].Value;

                    //Read module name, first as Object<"Prexonite.Modular.Name"> then as String
                    string moduleNameRaw;
                    if((moduleName = list[2].Value as ModuleName) != null 
                        && list[2].Type == ModuleName.PType)
                    {
                        // moduleName already assigned
                    }
                    else if ((moduleNameRaw = list[2].Value as string) != null 
                        && list[2].Type == PType.String 
                        && ModuleName.TryParse(moduleNameRaw, out moduleName))
                    {
                        // moduleName already assigned
                    }
                    else
                    {
                        moduleName = null;
                    }
                }
                else
                {
                    id = rawMacro.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get,
                        PFunction.IdKey).ConvertTo<string>(sctx, false);
                    macroInterpretation = _inferInterpretationAndModule(rawMacro, out moduleName);
                }

                if (macroInterpretation.AssociatedWithModule() && moduleName == null)
                    throw new PrexoniteException(
                        string.Format("Missing module name for {0} macro with internal name {1}.",
                            Enum.GetName(typeof (SymbolInterpretations), macroInterpretation), id));

                return new SymbolEntry(macroInterpretation, id, moduleName);
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
                    : base(Alias, SymbolEntry.Command(CallMacroPerform.Alias))
                {
                }

                protected override int GetPassThroughArguments(MacroContext context)
                {
                    return _callingConventionArgumentsCount + 1;
                    //Take reference to call\macro\perform into account.
                }
            }
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            var prepareCall = _assembleCallPerform(context);

            //null indicates failure, error has already been reported.
            if (prepareCall == null)
                return;

            context.Block.Expression = prepareCall;
        }

        #region Helper routines

        /// <summary>
        ///     Establishes macro context and parses arguments.
        /// </summary>
        /// <param name = "context">The macro context.</param>
        /// <returns>The call to call\macro\perform expression on success; null otherwise.</returns>
        private static AstMacroInvocation _assembleCallPerform(MacroContext context)
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

            AstExpr call;
            AstExpr justEffect;
            AstExpr[] args;
            AstExpr macroSpec;

            if (!_parseArguments(context, out call, out justEffect, out args, false, out macroSpec))
                return null;

            // [| call\macro\prepare_macro("$macroId", $macroInterpretation, context, $call, $justEffect, $args...) |]
            return _prepareMacro(context, macroSpec, call, justEffect, args);
        }

        private static AstMacroInvocation _prepareMacro(MacroContext context,
            AstExpr macroSpec, AstExpr call, AstExpr justEffect,
            IEnumerable<AstExpr> args)
        {
            var getContext = context.CreateGetSetSymbol(
                SymbolEntry.LocalReferenceVariable(MacroAliases.ContextAlias), PCall.Get);
            var prepareCall = context.CreateMacroInvocation(context.Call,
                SymbolEntry.MacroCommand(CallMacroPerform.PartialCallMacroPerform.Alias),
                macroSpec,
                getContext, call,
                justEffect);
            prepareCall.Arguments.AddRange(args);
            return prepareCall;
        }

        private static bool _parseArguments(MacroContext context, out AstExpr call,
            out AstExpr justEffect, out AstExpr[] args, bool isPartialApplication,
            out AstExpr macroSpec)
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

                AstListLiteral optionsRaw;
                if (inv.Arguments.Count < 3 ||
                    (optionsRaw = inv.Arguments[2] as AstListLiteral) == null)
                {
                    _errorUsageFullRef(context, isPartialApplication);
                    args = null;
                    macroSpec = null;
                    return false;
                }

                macroSpec = inv.Arguments[1];

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

                var specProto = listSpec.Elements[0];
                PCall protoCall;
                IList<AstExpr> protoArguments;
                if (
                    !_parsePrototype(context, isPartialApplication, specProto, out protoCall,
                        out protoArguments, out macroSpec))
                {
                    args = null;
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
                    call = protoCall.EnumToExpression(specProto);
                }

                //args: lift and pass prototype arguments, special care for set

                var setArgs = protoCall == PCall.Set
                    ? protoArguments.Last()
                    : null;
                var getArgs = protoCall == PCall.Set
                    ? protoArguments.Take(protoArguments.Count - 1)
                    : protoArguments;

                if (getArgs.Any(a => !_ensureExplicitPlaceholder(context, a)))
                {
                    args = new AstExpr[] {};
                    return false;
                }

                IEnumerable<AstExpr> getArgsLit;
                if (getArgs.Any())
                {
                    var getArgsLitNode = new AstListLiteral(specProto.File, specProto.Line,
                        specProto.Column);
                    getArgsLitNode.Elements.AddRange(getArgs);
                    getArgsLit = getArgsLitNode.Singleton();
                }
                else
                {
                    getArgsLit = Enumerable.Empty<AstExpr>();
                }

                IEnumerable<AstExpr> setArgsLit;
                if (setArgs != null)
                {
                    if (!_ensureExplicitPlaceholder(context, setArgs))
                    {
                        args = new AstExpr[] {};
                        return false;
                    }
                    var lit = new AstListLiteral(setArgs.File, setArgs.Line, setArgs.Column);
                    lit.Elements.Add(setArgs);
                    setArgsLit = lit.Singleton();
                }
                else
                {
                    setArgsLit = Enumerable.Empty<AstExpr>();
                }

                args = getArgsLit
                    .Append(inv.Arguments.Skip(1))
                    .Append(setArgsLit).ToArray();

                return true;
            }
        }

        private static bool _ensureExplicitPlaceholder(MacroContext context, AstExpr arg)
        {
            var setPlaceholder = arg as AstPlaceholder;
            if (setPlaceholder != null && !setPlaceholder.Index.HasValue)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    string.Format(
                        "Due to an internal limitation, " +
                            "the index of a placeholder in the macro prototype's argument list inside {0} cannot be inferred. " +
                                "Specify the placeholders index explicitly (e.g.,  ?0, ?1, etc.).",
                        Alias), setPlaceholder);
                return false;
            }
            return true;
        }

        private static bool _parsePrototype(MacroContext context, bool isPartialApplication,
            AstExpr specProto, out PCall protoCall, out IList<AstExpr> protoArguments,
            out AstExpr macroSpec)
        {
            var proto = specProto as AstMacroInvocation;
            if (proto != null)
            {
                macroSpec = _getMacroSpecExpr(context, proto);
                protoCall = proto.Call;
                protoArguments = proto.Arguments;
            }
            else if (specProto.IsPlaceholder())
            {
                //As an exception, allow a placeholder here
                macroSpec = specProto;
                protoCall = PCall.Get;
                protoArguments = new List<AstExpr>();
            }
            else
            {
                _errorUsagePrototype(context, isPartialApplication);
                macroSpec = null;
                protoCall = PCall.Get;
                protoArguments = null;
                return false;
            }
            return true;
        }

        private static AstExpr _getMacroSpecExpr(MacroContext context,
            AstMacroInvocation proto)
        {
            //macroId: as a constant
            var macroId = context.CreateConstant(proto.Implementation.InternalId);

            //macroInterpretation: as an expression
            var macroInterpretation = proto.Implementation.Interpretation.EnumToExpression(proto);

            //macroModule: as a constant (string or null)
            var macroModule = context.CreateConstantOrNull(proto.Implementation.Module);

            var listLit = new AstListLiteral(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column);
            listLit.Elements.Add(macroId);
            listLit.Elements.Add(macroInterpretation);
            listLit.Elements.Add(macroModule);

            return listLit;
        }

        private static void _errorUsagePrototype(MacroContext context, bool isPartialApplication)
        {
            context.ReportMessage(ParseMessageSeverity.Error,
                string.Format(
                    "Used in this way, {0} has the form {0}([macroPrototype(...),justEffect?,call?],...).",
                    Alias));
        }

        private static bool _parseReference(MacroContext context, AstExpr macroRef,
            bool isPartialApplication)
        {
            if (macroRef.IsPlaceholder())
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    "The macro prototype must be known at compile-time, it must not be a placeholder.");
                return false;
            }

            return true;
        }

        private static void _errorUsageFullRef(MacroContext context, bool isPartialApplication)
        {
            context.ReportMessage(ParseMessageSeverity.Error,
                "Used in this way, {0} has the form {0}([],macroRef,[justEffect?,call?],...).");
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