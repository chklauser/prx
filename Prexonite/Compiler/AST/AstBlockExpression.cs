// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Prexonite.Compiler.Ast
{
    public class AstBlockExpression : AstBlock,
                                      IAstEffect,
                                      IAstHasExpressions
    {
        public IAstExpression Expression;

        public AstBlockExpression(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstBlockExpression(Parser p)
            : base(p)
        {
        }

        #region IAstExpression/IAstEffect Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Will be optimized after code generation, hopefully
            if (Expression != null)
                _OptimizeNode(target, ref Expression);

            expr = null;
            return false;
        }

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            base.DoEmitCode(target);
            var effect = Expression as IAstEffect;
            if (effect != null)
                effect.EmitEffectCode(target);
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            base.DoEmitCode(target);
            if (Expression != null)
                Expression.EmitCode(target);
        }

        #region Implementation of IAstHasExpressions

        public IAstExpression[] Expressions
        {
            get { return new[] {Expression}; }
        }

        #endregion

        public override string ToString()
        {
            if (Expression == null)
                return base.ToString();
            else
                return string.Format("{0} (return {1})", base.ToString(), Expression);
        }
    }
}