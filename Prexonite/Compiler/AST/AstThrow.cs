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
    public class AstThrow : AstNode,
                            IAstExpression,
                            IAstHasExpressions,
                            IAstEffect
    {
        public IAstExpression Expression;

        public AstThrow(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstThrow(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {Expression}; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            return false;
        }

        #endregion

        public void EmitCode(CompilerTarget target, bool justEffect)
        {
            if (Expression == null)
                throw new PrexoniteException("Expression must be assigned.");

            Expression.EmitCode(target);
            target.Emit(this, OpCode.@throw);
            if (!justEffect)
                target.Emit(this, OpCode.ldc_null);
        }

        public override string ToString()
        {
            return "throw " + (Expression.ToString() ?? "");
        }

        #region IAstEffect Members

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }
    }
}