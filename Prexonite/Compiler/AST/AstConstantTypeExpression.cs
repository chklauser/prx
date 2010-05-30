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
    public class AstConstantTypeExpression : AstNode,
                                             IAstType
    {
        #region IAstExpression Members

        public string TypeExpression;

        public AstConstantTypeExpression(string file, int line, int column, string typeExpression)
            : base(file, line, column)
        {
            if (typeExpression == null)
                throw new ArgumentNullException("typeExpression");
            TypeExpression = typeExpression;
        }

        internal AstConstantTypeExpression(Parser p, string typeExpression)
            : this(p.scanner.File, p.t.line, p.t.col, typeExpression)
        {
        }

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            return false;
        }

        public override void EmitCode(CompilerTarget target)
        {
            target.Emit(this, OpCode.ldr_type, TypeExpression);
        }

        #endregion
    }
}