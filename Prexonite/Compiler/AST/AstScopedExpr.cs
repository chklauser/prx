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
using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstScopedExpr : AstExpr
    {
        [NotNull]
        private readonly AstBlock _lexicalScope;

        protected AstScopedExpr([NotNull] ISourcePosition position, [NotNull] AstBlock lexicalScope)
            : base(position)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException(nameof(lexicalScope));
            _lexicalScope = lexicalScope;
        }

        internal AstScopedExpr([NotNull] Parser p, [NotNull]  AstBlock lexicalScope)
            : base(p)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException(nameof(lexicalScope));
            _lexicalScope = lexicalScope;
        }

        protected AstScopedExpr([NotNull] string file, int line, int column, [NotNull] AstBlock lexicalScope)
            : base(file, line, column)
        {
            if (lexicalScope == null)
                throw new System.ArgumentNullException(nameof(lexicalScope));
            _lexicalScope = lexicalScope;
        }

        /// <summary>
        ///     The node this block is a part of.
        /// </summary>
        [NotNull]
        public AstBlock LexicalScope
        {
            [DebuggerStepThrough]
            get { return _lexicalScope; }
        }
    }
}