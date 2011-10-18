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

namespace Prexonite.Compiler.Ast
{
    public class AstUsing : AstNode,
                            IAstHasBlocks,
                            IAstHasExpressions
    {
        private const string LabelPrefix = "using";

        internal AstUsing(Parser p)
            : base(p)
        {
            _block = new AstSubBlock(File, Line, Column, this);
        }

        public AstUsing(string file, int line, int column)
            : base(file, line, column)
        {
            _block = new AstSubBlock(File, Line, Column, this);
        }

        public IAstExpression Expression;
        private readonly AstSubBlock _block;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new[] {_block}; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {Expression}; }
        }

        public AstBlock Block
        {
            get { return _block; }
        }

        #endregion

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            if (Expression == null)
                throw new PrexoniteException("AstUsing requires Expression to be initialized.");

            var tryNode = new AstTryCatchFinally(File, Line, Column);
            var vContainer = _block.CreateLabel("container");
            target.Function.Variables.Add(vContainer);
            //Try block => Container = {Expression}; {Block};
            var setCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Set,
                    vContainer,
                    SymbolInterpretations.LocalObjectVariable);
            setCont.Arguments.Add(Expression);

            var getCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    vContainer,
                    SymbolInterpretations.LocalObjectVariable);

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
                    Engine.DisposeAlias,
                    SymbolInterpretations.Command);

            dispose.Arguments.Add(getCont);

            tryNode.FinallyBlock.Add(dispose);

            //Emit code!
            tryNode.EmitCode(target);
        }
    }
}