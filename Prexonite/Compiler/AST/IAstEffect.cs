/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// Indicates that the ast node can optionally skip emitting a value and just emit effect code. Called when an expression is used as a statement.
    /// 
    /// <strong>Implement this interface explicitly, and DON'T CALL IT DIRECTLY, use <see cref="AstEffect.EmitEffectCode{T}"/> instead (an extension method).</strong>
    /// </summary>
    public interface IAstEffect : IAstExpression
    {
        /// <summary>
        /// For internal use only. Implement explicitly. Emits the effect code only. No value must be produced.
        /// </summary>
        /// <param name="target">The function target to compile to.</param>
        void DoEmitEffectCode(CompilerTarget target);
    }

    /// <summary>
    /// Extensions to the <see cref="IAstEffect"/> interface.
    /// </summary>
    public static class AstEffect
    {
        public static void EmitEffectCode<T>(this T node, CompilerTarget target) where T : AstNode, IAstEffect
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