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
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro.Commands
{
    public class PartialCallWrapper : PartialMacroCommand
    {
        private readonly SymbolEntry _callImplementation;

        public SymbolEntry CallImplementation
        {
            get { return _callImplementation; }
        }

        public PartialCallWrapper(string alias, SymbolEntry callImplementation)
            : base(alias)
        {
            if (callImplementation == null)
                throw new ArgumentNullException("callImplementation");

            _callImplementation = callImplementation;
        }

        #region Overrides of MacroCommand

        private static bool _hasPlaceholder(IAstExpression expr)
        {
            var lit = expr as AstListLiteral;
            return expr.IsPlaceholder() || (lit != null && lit.CheckForPlaceholders());
        }

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                // Call with no arguments returns null.
                // The macro system will supply that null.
                return;
            }

            var p = context.Invocation.Arguments[0] as AstPlaceholder;
            if (context.Invocation.Arguments.Count == 1
                && p != null
                    && (p.Index.GetValueOrDefault(0) == 0))
            {
                // call(?0) ⇒ call\perform(?0)

                var cp = GetTrivialPartialApplication(context);
                context.Block.Expression = cp;
                return;
            }

            if (!context.Invocation.Arguments.Any(_hasPlaceholder))
            {
                // no placeholders, invoke call\perform directly

                var call = context.CreateGetSetSymbol(_callImplementation,
                    context.Call, GetCallArguments(context).ToArray());
                context.Block.Expression = call;
                return;
            }

            var inv = new AstMacroInvocation(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column,
                CallStar.Instance.Id,
                SymbolInterpretations.MacroCommand);

            // Protected the first two arguments
            inv.Arguments.Add(context.CreateConstant(GetPassThroughArguments(context)));

            // Indicate the kind of call by passing `call(?)`, a partial application of call
            var paCall = context.CreateGetSetSymbol(_callImplementation,
                context.Invocation.Call,
                new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                    context.Invocation.Column, 0));
            inv.Arguments.Add(paCall);

            // Pass all the other arguments through
            inv.Arguments.AddRange(GetCallArguments(context));

            context.Block.Expression = inv;
        }

        protected virtual AstGetSetSymbol GetTrivialPartialApplication(MacroContext context)
        {
            var cp = new AstGetSetSymbol(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, context.Invocation.Call, _callImplementation);
            cp.Arguments.Add(new AstPlaceholder(context.Invocation.File, context.Invocation.Line,
                context.Invocation.Column, 0));
            return cp;
        }

        protected virtual IEnumerable<IAstExpression> GetCallArguments(MacroContext context)
        {
            return context.Invocation.Arguments;
        }

        protected virtual int GetPassThroughArguments(MacroContext context)
        {
            return 2;
        }

        #endregion

        #region Overrides of PartialMacroCommand

        protected override bool DoExpandPartialApplication(MacroContext context)
        {
            DoExpand(context);
            return true;
        }

        #endregion
    }
}