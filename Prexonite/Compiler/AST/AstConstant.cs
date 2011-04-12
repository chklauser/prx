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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstConstant : AstNode,
                               IAstExpression
    {
        public object Constant;

        internal AstConstant(Parser p, object constant)
            : this(p.scanner.File, p.t.line, p.t.col, constant)
        {
        }

        public AstConstant(string file, int line, int column, object constant)
            : base(file, line, column)
        {
            Constant = constant;
        }

        public static bool TryCreateConstant(
            CompilerTarget target,
            ISourcePosition position,
            PValue value,
            out IAstExpression expr)
        {
            expr = null;
            if (value.Type is ObjectPType)
                target.Loader.Options.ParentEngine.CreateNativePValue(value.Value);
            if (value.Type is IntPType ||
                value.Type is RealPType ||
                value.Type is BoolPType ||
                value.Type is StringPType ||
                value.Type is NullPType)
                expr = new AstConstant(position.File, position.Line, position.Column, value.Value);
            else //Cannot represent value in a constant instruction
                return false;
            return expr != null;
        }

        public PValue ToPValue(CompilerTarget target)
        {
            return target.Loader.Options.ParentEngine.CreateNativePValue(Constant);
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            if (Constant == null)
                target.EmitNull(this);
            else
                switch (Type.GetTypeCode(Constant.GetType()))
                {
                    case TypeCode.Boolean:
                        target.EmitConstant(this, (bool)Constant);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Byte:
                    case TypeCode.Int32:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                        target.EmitConstant(this, (int)Constant);
                        break;
                    case TypeCode.Single:
                    case TypeCode.Double:
                        target.EmitConstant(this, (double)Constant);
                        break;
                    case TypeCode.String:
                        target.EmitConstant(this, (string)Constant);
                        break;
                    default:
                        throw new PrexoniteException(
                            "Prexonite does not support constants of type " +
                            Constant.GetType().Name + ".");
                }
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;
            return false;
        }

        #endregion

        public override string ToString()
        {
            string str;
            if (Constant != null) 
                if((str = Constant as string) != null)
                    return String.Concat("\"",StringPType.Escape(str),"\"");
                else
                    return Constant.ToString();
            else return "-null-";
        }
    }
}