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
    public class AstReturn : AstNode
    {
        public ReturnVariant ReturnVariant;
        public IAstExpression Expression;

        public AstReturn(string file, int line, int column, ReturnVariant returnVariant)
            : base(file, line, column)
        {
            ReturnVariant = returnVariant;
        }

        internal AstReturn(Parser p, ReturnVariant returnVariant)
            : this(p.scanner.File, p.t.line, p.t.col, returnVariant)
        {
        }

        public override void EmitCode(CompilerTarget target)
        {
            if (Expression != null &&
                (ReturnVariant == ReturnVariant.Exit || ReturnVariant == ReturnVariant.Set))
            {
                OptimizeNode(target, ref Expression);
                Expression.EmitCode(target);
            }
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    target.Emit(Expression != null ? OpCode.ret_value : OpCode.ret_exit);
                    break;
                case ReturnVariant.Set:
                    if (Expression == null)
                        throw new PrexoniteException("Return assignment requires an expression.");
                    target.Emit(OpCode.ret_set);
                    break;
                case ReturnVariant.Continue:
                    if (Expression != null)
                    {
                        Expression.EmitCode(target);
                        target.Emit(OpCode.ret_set);
                    }
                    target.Emit(OpCode.ret_continue);
                    break;
                case ReturnVariant.Break:
                    target.Emit(OpCode.ret_break);
                    break;
            }
        }

        public override string ToString()
        {
            string format = "";
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    format = Expression != null ? "return {0};" : "return;";
                    break;
                case ReturnVariant.Set:
                    format = "return = {0};";
                    break;
                case ReturnVariant.Continue:
                    format = "continue;";
                    break;
                case ReturnVariant.Break:
                    format = "break;";
                    break;
            }
            return String.Format(format, Expression);
        }
    }

    public enum ReturnVariant
    {
        Exit,
        Set,
        Break,
        Continue
    }
}