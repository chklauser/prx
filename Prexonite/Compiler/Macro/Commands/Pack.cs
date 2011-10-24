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

using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class Pack : MacroCommand
    {
        public const string Alias = @"macro\pack";

        #region Singleton pattern

        private static readonly Pack _instance = new Pack();

        public static Pack Instance
        {
            get { return _instance; }
        }

        private Pack() : base(Alias)
        {
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count < 1)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    string.Format("Must supply an object to be transported to {0}.", Alias));
                return;
            }

            context.EstablishMacroContext();

            // [| context.StoreForTransport(boxed($arg0)) |]

            var getContext = context.CreateGetSetSymbol(
                SymbolEntry.LocalReferenceVariable(MacroAliases.ContextAlias), PCall.Get);
            var boxedArg0 = context.CreateGetSetSymbol(SymbolEntry.Command(Engine.BoxedAlias), PCall.Get, context.Invocation.Arguments[0]);
            context.Block.Expression = context.CreateGetSetMember(getContext, PCall.Get,
                "StoreForTransport", boxedArg0);
        }

        #endregion
    }
}