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

using System;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    ///     Indicates that the ast node can optionally skip emitting a value and just emit effect code. Called when an expression is used as a statement.
    /// 
    ///     <strong>Implement this interface explicitly, and DON'T CALL IT DIRECTLY, use <see cref = "AstEffect.EmitEffectCode{T}" /> instead (an extension method).</strong>
    /// </summary>
    public interface IAstEffect : IAstExpression
    {
        /// <summary>
        ///     For internal use only. Implement explicitly. Emits the effect code only. No value must be produced.
        /// </summary>
        /// <param name = "target">The function target to compile to.</param>
        void DoEmitEffectCode(CompilerTarget target);
    }

    /// <summary>
    ///     Extensions to the <see cref = "IAstEffect" /> interface.
    /// </summary>
    public static class AstEffect
    {
        public static void EmitEffectCode<T>(this T node, CompilerTarget target)
            where T : AstNode, IAstEffect
        {
            node._EmitEffectCode(target);
        }

        public static void EmitEffectCode(this IAstEffect effect, CompilerTarget target)
        {
            var node = effect as AstNode;
            if (node == null)
                throw new ArgumentException("Effect must be an AST node", "effect");
            node._EmitEffectCode(target);
        }
    }
}