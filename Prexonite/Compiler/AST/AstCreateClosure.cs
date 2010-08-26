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

using System.Diagnostics;

namespace Prexonite.Compiler.Ast
{
    [DebuggerNonUserCode]
    public class AstCreateClosure : AstNode,
                                    IAstExpression
    {
        public string FuncId;

        public AstCreateClosure(string file, int line, int column, string funcId)
            : base(file, line, column)
        {
            FuncId = funcId;
        }

        internal AstCreateClosure(Parser p, string funcId)
            : base(p)
        {
            FuncId = funcId;
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            target.Emit(this, OpCode.newclo, FuncId);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            return false;
        }

        #endregion
    }
}