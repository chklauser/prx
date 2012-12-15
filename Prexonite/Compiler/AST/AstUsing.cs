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
using JetBrains.Annotations;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstUsing : AstScopedBlock,
                            IAstHasBlocks
    {
        private const string LabelPrefix = "using";

        public AstUsing([NotNull] ISourcePosition p, 
            [NotNull] AstBlock lexicalScope)
            : base(p, lexicalScope)
        {
            _block = new AstScopedBlock(p, this,prefix:LabelPrefix);
        }

        private AstExpr _resourceExpression;
        private readonly AstScopedBlock _block;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new AstBlock[] {_block}; }
        }

        #region IAstHasExpressions Members

        public override AstExpr[] Expressions
        {
            get 
            { 
                var b = base.Expressions;
                var r = new AstExpr[b.Length + 1];
                b.CopyTo(r,0);
                r[b.Length] = _resourceExpression;
                return r;
            }
        }

        [PublicAPI]
        public AstScopedBlock Block
        {
            get { return _block; }
        }

        [PublicAPI]
        public AstExpr ResourceExpression
        {
            get { return _resourceExpression; }
            set { _resourceExpression = value; }
        }

        #endregion

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics == StackSemantics.Value)
                throw new NotSupportedException("Using blocks do not produce values and can thus not be used as expressions.");

            if (_resourceExpression == null)
                throw new PrexoniteException("AstUsing requires Expression to be initialized.");

            var tryNode = new AstTryCatchFinally(Position, this);
            var vContainer = _block.CreateLabel("container");
            target.Function.Variables.Add(vContainer);
            //Try block => Container = {Expression}; {Block};
            var setCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Set,
                    new SymbolEntry(SymbolInterpretations.LocalObjectVariable, vContainer, null));
            setCont.Arguments.Add(_resourceExpression);

            var getCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    new SymbolEntry(SymbolInterpretations.LocalObjectVariable, vContainer, null));

            var tryBlock = tryNode.TryBlock;
            tryBlock.Add(setCont);
            tryBlock.AddRange(_block);

            //Finally block => dispose( Container );
            var dispose =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    new SymbolEntry(SymbolInterpretations.Command, Engine.DisposeAlias, null));

            dispose.Arguments.Add(getCont);

            tryNode.FinallyBlock.Add(dispose);

            //Emit code!
            tryNode.EmitEffectCode(target);
        }
    }
}