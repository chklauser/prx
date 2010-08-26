/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
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
                OptimizeNode(target, ref Expression);

            expr = null;
            return false;
        }

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            EmitCode(target);
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