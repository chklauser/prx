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

using System.Collections.Generic;
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
            if (context.Invocation.Arguments.Count < 1)
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
                if (args.Length < 2 || !(args[0].Type is ObjectPType) ||
                    (context = args[0].Value as MacroContext) == null)
                    throw new PrexoniteException(_getUsage());

                int id;
                if (args[1].TryConvertTo(sctx, true, out id))
                    return context.RetrieveFromTransport(id);

                AstConstant constant;
                if (!(args[1].Type is ObjectPType) ||
                    (constant = args[1].Value as AstConstant) == null || !(constant.Constant is int))
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