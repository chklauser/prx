// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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

using System.Diagnostics;
using Prexonite.Commands;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands;

public class CallMacro : PartialMacroCommand
{
    public const string Alias = @"call\macro";

    #region Singleton pattern

    public static CallMacro Instance { get; } = new();

    CallMacro() : base(Alias)
    {
    }

    #endregion

    public static KeyValuePair<string, CallMacroPerform> GetHelperCommands(Loader ldr)
    {
        return
            new(CallMacroPerform.Alias,
                new(ldr));
    }

    #region Call\Macro\Perform

    public class CallMacroPerform(Loader loader) : PCommand
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Alias = @"call\macro\perform";
        // ReSharper restore MemberHidesStaticFromOuterClass

        #region Overrides of PCommand

        const int CallingConventionArgumentsCount = 4;

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args.Length < CallingConventionArgumentsCount)
                throw new PrexoniteException(
                    "Id of macro implementation, effect flag, call type and/or context missing.");

            var entityRef = _getMacroRef(sctx, args[0]);

            //Parse arguments
            var context = _getContext(args[1]);
            var call = _getCallType(args[2]);
            var justEffect = _getEffectFlag(args[3]);

            // Prepare macro
            var target = loader.FunctionTargets[context.Function]!;
            var argList = Call.FlattenArguments(sctx, args, CallingConventionArgumentsCount);
            _detectRuntimeValues(argList);

            var inv = new AstExpand(context.Invocation.Position, entityRef, call);
            inv.Arguments.AddRange(argList.Select(p => p.Value).OfType<AstExpr>());

            //Execute the macro
            MacroSession? macroSession = null;
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

        static void _detectRuntimeValues(IEnumerable<PValue> argList)
        {
            var offender =
                argList.FirstOrDefault(
                    p => p.Type is not ObjectPType || p.Value is not AstExpr);
            if (offender != null)
                throw new PrexoniteException(
                    string.Format(
                        "Macros cannot have runtime values as arguments in call to {1}. {0} is not an AST node.",
                        offender, Alias));
        }

        static bool _getEffectFlag(PValue rawEffectFlag)
        {
            if (rawEffectFlag.Type != PType.Bool)
                throw new PrexoniteException(
                    $"Effect flag is missing in call to {Alias}.");
            return (bool) rawEffectFlag.Value!;
        }

        static PCall _getCallType(PValue rawCallType)
        {
            if (!(rawCallType.Type is ObjectPType && rawCallType.Value is PCall))
                throw new PrexoniteException(
                    $"Call type is missing in call to {Alias}.");
            return (PCall) rawCallType.Value;
        }

        static MacroContext _getContext(PValue rawContext)
        {
            if (rawContext.Type is not ObjectPType || rawContext.Value is not MacroContext context)
                throw new PrexoniteException(
                    $"Macro context is missing in call to {Alias}.");
            return context;
        }

        static EntityRef _getMacroRef(StackContext sctx, PValue rawMacro)
        {
            if (rawMacro.TryConvertTo(sctx, out PFunction? func))
                return EntityRef.Function.Create(func.Id, func.ParentApplication.Module.Name);
            else if (rawMacro.TryConvertTo(sctx, out MacroCommand? mcmd))
                return EntityRef.MacroCommand.Create(mcmd.Id);
            else
                return rawMacro.ConvertTo<EntityRef>(sctx);
        }

        #endregion

        public PartialCallMacroPerform Partial { [DebuggerStepThrough] get; } = new();

        public class PartialCallMacroPerform : PartialCallWrapper
        {
            // ReSharper disable MemberHidesStaticFromOuterClass
            public const string Alias = @"call\macro\impl";
            // ReSharper restore MemberHidesStaticFromOuterClass

            public PartialCallMacroPerform()
                : base(Alias, EntityRef.Command.Create(CallMacroPerform.Alias))
            {
            }

            protected override int GetPassThroughArguments(MacroContext context)
            {
                return CallingConventionArgumentsCount + 1;
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
    static AstGetSet? _assembleCallPerform(MacroContext context)
    {
        if (context.Invocation.Arguments.Count == 0)
        {
            context.ReportMessage(
                Message.Error(
                    Resources.CallMacro_call_macro_must_be_supplied_a_macro_reference, context.Invocation.Position,
                    MessageClasses.MacroReferenceForCallMacroMissing));
            return null;
        }

        if (!context.CallerIsMacro())
        {
            context.ReportMessage(
                Message.Error(
                    string.Format(
                        Resources.CallMacro_CalledFromNonMacro,
                        context.Function.LogicalId), context.Invocation.Position,
                    MessageClasses.CallMacroCalledFromNonMacro));
            return null;
        }

        context.EstablishMacroContext();

        if (!_parseArguments(context, out var call, out var justEffect, out var args, out var macroSpec))
            return null;

        // [| call\macro\prepare_macro($macroEntityRef, context, $call, $justEffect, $args...) |]
        return _prepareMacro(context, macroSpec, call, justEffect, args);
    }

    static AstGetSet _prepareMacro(MacroContext context,
        AstExpr macroSpec, AstExpr call, AstExpr justEffect,
        IEnumerable<AstExpr> args)
    {
        var getContext = context.Factory.IndirectCall(context.Invocation.Position,
            context.Factory.Call(context.Invocation.Position,
                EntityRef.Variable.Local.Create(
                    MacroAliases.ContextAlias)));
        var prepareCall =
            context.CreateExpand(EntityRef.MacroCommand.Create(CallMacroPerform.PartialCallMacroPerform.Alias));
        prepareCall.Arguments.Add(macroSpec);
        prepareCall.Arguments.Add(getContext);
        prepareCall.Arguments.Add(call);
        prepareCall.Arguments.Add(justEffect);
        prepareCall.Arguments.AddRange(args);
        return prepareCall;
    }

    static bool _parseArguments(MacroContext context, out AstExpr call,
        out AstExpr justEffect,
        [NotNullWhen(true)]
        out AstExpr[]? args,
        [NotNullWhen(true)]
        out AstExpr? macroSpec)
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
        call = PCall.Get.ToExpr(context.Invocation.Position);

        var invokeSpec = inv.Arguments[0];
        if (invokeSpec is not AstListLiteral listSpec)
        {
            // - Macro reference specified as expression that evaluates to an actual macro reference

            args = inv.Arguments.Skip(1).ToArray();
            macroSpec = invokeSpec;
            return _parseReference(context, inv.Arguments[0]);
        }
        else if (listSpec.Elements.Count == 0)
        {
            // - Macro reference specified as expression that evaluates to an actual macro reference
            // - followed by a list of options

            AstListLiteral? optionsRaw;
            if (inv.Arguments.Count < 3 ||
                (optionsRaw = inv.Arguments[2] as AstListLiteral) == null)
            {
                _errorUsageFullRef(context);
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

            return _parseReference(context, inv.Arguments[1]);
        }
        else
        {
            // - Macro reference specified as a prototype
            // - includes list of options

            var specProto = listSpec.Elements[0];
            if (
                !_parsePrototype(context, specProto, out var protoCall,
                    out var protoArguments, out macroSpec))
            {
                args = null;
                return false;
            }

            //first option: justEffect
            if (listSpec.Elements.Count >= 2)
                justEffect = listSpec.Elements[1];

            //second option: call type
            call = listSpec.Elements.Count >= 3
                ? listSpec.Elements[2]
                : protoCall.ToExpr(specProto.Position);

            //args: lift and pass prototype arguments, special care for set

            var setArgs = protoCall == PCall.Set
                ? protoArguments.Last()
                : null;
            var getArgs = protoCall == PCall.Set
                ? protoArguments.Take(protoArguments.Count - 1)
                : protoArguments;

            // ReSharper disable PossibleMultipleEnumeration 
            // enumerating getArgs multiple times is safe and efficient
            if (getArgs.Any(a => !_ensureExplicitPlaceholder(context, a)))

            {
                args = Array.Empty<AstExpr>();
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
            // ReSharper restore PossibleMultipleEnumeration
            else
            {
                getArgsLit = Enumerable.Empty<AstExpr>();
            }

            IEnumerable<AstExpr> setArgsLit;
            if (setArgs != null)
            {
                if (!_ensureExplicitPlaceholder(context, setArgs))
                {
                    args = Array.Empty<AstExpr>();
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

    static bool _ensureExplicitPlaceholder(MacroContext context, AstExpr arg)
    {
        if (arg is AstPlaceholder {Index: null} setPlaceholder)
        {
            context.ReportMessage(
                Message.Error(
                    string.Format(Resources.CallMacro_SpecifyPlaceholderIndexExplicitly, Alias),
                    setPlaceholder.Position, MessageClasses.SpecifyPlaceholderIndexExplicitly));
            return false;
        }
        return true;
    }

    static bool _parsePrototype(MacroContext context,
        AstExpr specProto, out PCall protoCall,
        [NotNullWhen(true)]
        out IList<AstExpr>? protoArguments,
        [NotNullWhen(true)]
        out AstExpr? macroSpec)
    {
        var proto2 = specProto as AstExpand;
        if (specProto.IsPlaceholder())
        {
            //As an exception, allow a placeholder here
            macroSpec = specProto;
            protoCall = PCall.Get;
            protoArguments = new List<AstExpr>();
        }
        else if (proto2 != null)
        {
            macroSpec = _getMacroSpecExpr(context, proto2);
            protoCall = proto2.Call;
            protoArguments = proto2.Arguments;
        }
        else
        {
            _errorUsagePrototype(context);
            macroSpec = null;
            protoCall = PCall.Get;
            protoArguments = null;
            return false;
        }
        return true;
    }

    static AstExpr _getMacroSpecExpr(MacroContext context,
        AstExpand proto)
    {
        return EntityRefTo.ToExpr(context.Factory, context.Invocation.Position, proto.Entity);
    }

    static void _errorUsagePrototype(MacroContext context)
    {
        context.ReportMessage(
            Message.Error(
                string.Format(Resources.CallMacro_errorUsagePrototype, Alias), context.Invocation.Position,
                MessageClasses.CallMacroUsage));
    }

    static bool _parseReference(MacroContext context, AstExpr macroRef)
    {
        if (macroRef.IsPlaceholder())
        {
            context.ReportMessage(
                Message.Error(
                    Resources.CallMacro_notOnPlaceholder, context.Invocation.Position,
                    MessageClasses.CallMacroNotOnPlaceholder));
            return false;
        }

        return true;
    }

    static void _errorUsageFullRef(MacroContext context)
    {
        context.ReportMessage(
            Message.Error(
                Resources.CallMacro_errorUsageFullRef, context.Invocation.Position,
                MessageClasses.CallMacroUsage));
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