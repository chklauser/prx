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
using System.Collections.Generic;
using System.Linq;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands;

public class CallStar : PartialMacroCommand
{
    #region Singleton pattern

    public static CallStar Instance { get; } = new();

    CallStar()
        : base(@"call\star")
    {
    }

    #endregion

    #region Overrides of PartialMacroCommand

    protected override bool DoExpandPartialApplication(MacroContext context)
    {
        if (context.Invocation.Arguments.Count < 1)
        {
            context.ReportMessage(Message.Error(
                string.Format(Resources.CallStar_usage, Id), context.Invocation.Position, MessageClasses.CallStarUsage));
            return true;
        }

        _determinePassThrough(context, out var passThrough, out var arguments);

        _expandPartialApplication(context, passThrough, arguments);

        return true;
    }

    void _expandPartialApplication(MacroContext context, int passThrough,
        List<AstExpr> arguments)
    {
        var flatArgs = new List<AstExpr>(arguments.Count);
        var directives = new List<int>(arguments.Count);

        //The call target is a "non-argument" in partial application terms. Do not include it in the
        //  stream of directives.
        flatArgs.Add(arguments[0]);

        var opaqueSpan = 0;
        for (var i = 1; i < arguments.Count; i++)
        {
            var arg = arguments[i];
            if (i < passThrough || !_isPartialList(arg, out var lit))
            {
                flatArgs.Add(arg);
                opaqueSpan++;
            }
            else
            {
                flatArgs.AddRange(lit.Elements);
                if (opaqueSpan > 0)
                {
                    directives.Add(opaqueSpan);
                    opaqueSpan = 0;
                }
                directives.Add(-lit.Elements.Count);
            }
        }

        var ppArgv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(flatArgs);

        var argc = ppArgv.Count;
        var mappings8 = new int[argc + directives.Count + 1];
        var closedArguments = new List<AstExpr>(argc);

        AstPartiallyApplicable.GetMapping(ppArgv, mappings8, closedArguments);
        _mergeDirectivesIntoMappings(directives, mappings8, argc);
        var mappings32 = PartialApplicationCommandBase.PackMappings32(mappings8);

        var implCall = context.Factory.Call(context.Invocation.Position,
            EntityRef.Command.Create(PartialCallStarImplCommand.Alias), context.Call);
        implCall.Arguments.AddRange(closedArguments);

        implCall.Arguments.AddRange(mappings32.Select(m => context.CreateConstant(m)));

        context.Block.Expression = implCall;
    }

    static void _mergeDirectivesIntoMappings(List<int> directives, int[] mappings8,
        int argc)
    {
        var mi = argc;
        foreach (var directive in directives)
        {
            mappings8[mi++] = directive;
        }

        mappings8[^1] = directives.Count;
    }

    #endregion

    #region Overrides of MacroCommand

    static bool _isPartialList(AstExpr expr)
    {
        return _isPartialList(expr, out _);
    }

    static bool _isPartialList(AstExpr expr, out AstListLiteral lit)
    {
        lit = expr as AstListLiteral;
        return lit != null && lit.CheckForPlaceholders();
    }

    protected override void DoExpand(MacroContext context)
    {
        if (context.Invocation.Arguments.Count < 1)
        {
            context.ReportMessage(
                Message.Error(
                    string.Format(Resources.CallStar_usage, Id), context.Invocation.Position,
                    MessageClasses.CallStarUsage));
            return;
        }

        _determinePassThrough(context, out var passThrough, out var arguments);

        if (arguments.Skip(passThrough).Any(_isPartialList))
        {
            _expandPartialApplication(context, passThrough, arguments);
            return;
        }

        // "Fallback" direct invocation
        var ic = new AstIndirectCall(context.Invocation.File, context.Invocation.Line,
            context.Invocation.Column, context.Invocation.Call, arguments[0]);
        ic.Arguments.AddRange(arguments.Skip(1));
        context.Block.Expression = ic;
    }

    static void _determinePassThrough(MacroContext context, out int passThrough,
        out List<AstExpr> arguments)
    {
        var arg0 = context.Invocation.Arguments[0];
        var passThroughNode = arg0 as AstConstant;
        if (passThroughNode?.Constant is int constant)
        {
            arguments = new List<AstExpr>(context.Invocation.Arguments.Skip(1));
            passThrough = constant;
        }
        else
        {
            arguments = new List<AstExpr>(context.Invocation.Arguments);
            passThrough = 1;
        }

        if (passThrough < 1)
            context.ReportMessage(
                Message.Error(
                    string.Format(Resources.CallStar__invalid_PassThrough, passThrough),
                    passThroughNode?.Position ?? context.Invocation.Position,
                    MessageClasses.CallStarPassThrough));
    }

    #endregion
}