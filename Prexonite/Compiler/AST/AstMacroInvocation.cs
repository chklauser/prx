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
using System.Diagnostics;
using Prexonite.Compiler.Macro;
using Prexonite.Types;
using NotSupportedException = Prexonite.Commands.Concurrency.NotSupportedException;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstMacroInvocation : AstGetSet
    {
        private readonly SymbolEntry _implementation;

        public AstMacroInvocation(string file, int line, int column, SymbolEntry implementation) : base(file, line, column, PCall.Get)
        {
            if (implementation == null)
                throw new ArgumentNullException("implementation");
            _implementation = implementation;
        }

        internal AstMacroInvocation(Parser p, SymbolEntry implementation)
            : base(p, PCall.Get)
        {
            _implementation = implementation;
        }

        public SymbolEntry Implementation
        {
            get { return _implementation; }
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            throw new NotSupportedException(
                "Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            throw new NotSupportedException(
                "Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            //instantiate macro for the current target
            MacroSession session = null;

            try
            {
                //Acquire current macro session
                session = target.AcquireMacroSession();

                //Expand macro
                var node = session.ExpandMacro(this, justEffect);

                //Emit generated code
                var effect = node as IAstEffect;
                if (justEffect)
                    effect.EmitEffectCode(target);
                else
                    node.EmitCode(target);
            }
            finally
            {
                if (session != null)
                    target.ReleaseMacroSession(session);
            }
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Do not optimize the macros arguments! They should be passed to the macro in their original form.
            //  the macro should decide whether or not to apply AST-optimization to the arguments or not.
            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            var macro = new AstMacroInvocation(File, Line, Column, Implementation);
            CopyBaseMembers(macro);
            return macro;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", base.ToString(), Implementation, ArgumentsToString());
        }
    }
}