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
using System;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    public static class AstFactoryExtensions
    {
        public static AstGetSet Call(this IAstFactory factory, ISourcePosition position, EntityRef entity,
                                     PCall call = PCall.Get, params AstExpr[] arguments)
        {
            var c = factory.IndirectCall(position, factory.Reference(position, entity), call);
            c.Arguments.AddRange(arguments);
            return c;
        }

        public static AstExpr ExprFor(this IAstFactory factory, ISourcePosition position, QualifiedId qualifiedId,
            ISymbolView<Symbol> scope)
        {
            var currentScope = scope;
            for (var i = 0; i < qualifiedId.Count; i++)
            {
                // Lookup name part
                if (!currentScope.TryGet(qualifiedId[i], out var sym))
                {
                    return new AstUnresolved(position, qualifiedId[i]);
                }

                var expr = factory.ExprFor(position, sym);

                // last part of qualified ID does not need to be a namespace, so we're done here
                if (i == qualifiedId.Count - 1)
                    return expr;

                if (!(expr is AstNamespaceUsage nsUsage))
                {
                    factory.ReportMessage(Message.Error(
                        string.Format(Resources.Parser_NamespaceExpected, qualifiedId[i], sym),
                        position, MessageClasses.NamespaceExcepted));
                    return factory.IndirectCall(position, factory.Null(position));
                }

                currentScope = nsUsage.Namespace;
            }

            throw new InvalidOperationException("Failed to resolve qualified Id (program control should never reach this point)");
        }
    }
}
