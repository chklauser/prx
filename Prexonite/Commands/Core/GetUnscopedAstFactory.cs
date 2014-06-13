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
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Commands.Core
{
    public class GetUnscopedAstFactory : PCommand, ICilCompilerAware
    {
        private class UnscopedFactory : AstFactoryBase
        {
            private readonly AstBlock _root;

            public UnscopedFactory(ModuleName compartment)
            {
                _root =
                    AstBlock.CreateRootBlock(NoSourcePosition.Instance, SymbolStore.Create(), compartment.ToString(),
                                             Guid.NewGuid().ToString("N"));
            }

            protected override AstBlock CurrentBlock
            {
                get { return _root; }
            }

            protected override AstGetSet CreateNullNode(ISourcePosition position)
            {
                return IndirectCall(position, Null(position));
            }

            protected override bool IsOuterVariable(string id)
            {
                return false;
            }

            protected override void RequireOuterVariable(string id)
            {
            }

            public override void ReportMessage(Message message)
            {
                // Yes, this also fails for info and warning messages, but we really have no other choice.
                // Unscoped factories should not be used except when you know exactly what you are doing.
                throw new ErrorMessageException(message);
            }

            protected override CompilerTarget CompileTimeExecutionContext
            {
                get { throw new InvalidOperationException("Unscoped AST factory does not have access to a compiler instance."); }
            }
        }

        #region Singleton

        private static readonly GetUnscopedAstFactory _instance = new GetUnscopedAstFactory();

        public static GetUnscopedAstFactory Instance
        {
            get { return _instance; }
        }

        private GetUnscopedAstFactory()
        {
        }

        public const string Alias = "get_unscoped_ast_factory";

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        private static readonly ConcurrentDictionary<ModuleName,UnscopedFactory> _unscopedFactories = new ConcurrentDictionary<ModuleName, UnscopedFactory>();

        [PublicAPI]
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            return
                sctx.CreateNativePValue(_unscopedFactories.GetOrAdd(sctx.ParentApplication.Module.Name,
                                                                    n => new UnscopedFactory(n)));
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
           return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException(Alias + " does not provide a custom CIL implementation.");
        }
    }
}