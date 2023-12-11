﻿// Prexonite
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
using JetBrains.Annotations;
using Prexonite.Commands;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands;

public class Unpack : MacroCommand
{
    public const string Alias = @"macro\unpack";

    #region Singleton pattern

    public static Unpack Instance { get; } = new();

    Unpack() : base(Alias)
    {
    }

    public static IEnumerable<KeyValuePair<string, PCommand>> GetHelperCommands()
    {
        yield return
            new KeyValuePair<string, PCommand>(Impl.Alias, Impl.Instance);
    }

    #endregion

    #region Overrides of MacroCommand

    protected override void DoExpand(MacroContext context)
    {
        if (context.Invocation.Arguments.Count < 1)
        {
            context.ReportMessage(
                Message.Error(
                    $"{Alias} requires at least one argument, the id of the object to unpack.",
                    context.Invocation.Position, MessageClasses.UnpackUsage));
            return;
        }

        context.EstablishMacroContext();

        // [| macro\unpack\impl(context, $arg0) |]

        var getContext =
            context.CreateIndirectCall(context.CreateCall(EntityRef.Variable.Local.Create(MacroAliases.ContextAlias)));

        context.Block.Expression = context.CreateCall(EntityRef.Command.Create(Impl.Alias),
            PCall.Get, getContext, context.Invocation.Arguments[0]);
    }

    #endregion

    class Impl : PCommand
    {
// ReSharper disable MemberHidesStaticFromOuterClass // not an issue
        public const string Alias = @"macro\unpack\impl";
// ReSharper restore MemberHidesStaticFromOuterClass

        #region Singleton pattern

// ReSharper disable MemberHidesStaticFromOuterClass not an issue (singleton pattern)

        [NotNull]
        public static Impl Instance { get; } = new();

        Impl()
        {
        }

        #endregion

        #region Overrides of PCommand

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            MacroContext context;
            if (args.Length < 2 || args[0].Type is not ObjectPType ||
                (context = args[0].Value as MacroContext) == null)
                throw new PrexoniteException(_getUsage());

            if (args[1].TryConvertTo(sctx, true, out int id))
                return context.RetrieveFromTransport(id);

            AstConstant constant;
            if (args[1].Type is not ObjectPType ||
                (constant = args[1].Value as AstConstant) == null || constant.Constant is not int)
                throw new PrexoniteException(_getUsage());

            return context.RetrieveFromTransport((int) constant.Constant);
        }

        static string _getUsage()
        {
            return $"usage {Alias}(context, id)";
        }

        #endregion
    }
}