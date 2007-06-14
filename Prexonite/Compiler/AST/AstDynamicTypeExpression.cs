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
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstDynamicTypeExpression : AstNode,
                                            IAstType
    {
        public List<IAstExpression> Arguments = new List<IAstExpression>();
        public string TypeId;

        public AstDynamicTypeExpression(string file, int line, int column, string typeId)
            : base(file, line, column)
        {
            if (typeId == null)
                throw new ArgumentNullException("TypeId cannot be null");
            TypeId = typeId;
        }

        internal AstDynamicTypeExpression(Parser p, string typeId)
            : this(p.scanner.File, p.t.line, p.t.col, typeId)
        {
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            bool isConstant = true;
            StringBuilder buffer = new StringBuilder(TypeId);
            buffer.Append("(");

            //Optimize arguments
            IAstExpression oArg;
            foreach (IAstExpression arg in Arguments.ToArray())
            {
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    Arguments.Remove(arg);
                    Arguments.Add(oArg);
                }

                AstConstant constValue = oArg as AstConstant;
                AstConstantTypeExpression constType = oArg as AstConstantTypeExpression;

                if (constValue == null && constType == null)
                {
                    isConstant = false;
                }
                else if (isConstant)
                {
                    if (constValue != null)
                    {
                        buffer.Append('"');
                        buffer.Append(StringPType.Escape(constValue.ToPValue(target).CallToString(target.Loader)));
                        buffer.Append('"');
                    }
                    else //if(constType != null)
                        buffer.Append(constType.TypeExpression);
                    buffer.Append(",");
                }
            }
            if (!isConstant)
                return false;

            buffer.Remove(buffer.Length - 1, 1); //remove , or (
            if (Arguments.Count != 0)
                buffer.Append(")"); //Add ) if necessary

            expr = new AstConstantTypeExpression(File, Line, Column, buffer.ToString());
            return true;
        }

        public override void EmitCode(CompilerTarget target)
        {
            foreach (IAstExpression expr in Arguments)
                expr.EmitCode(target);
            target.Emit(OpCode.newtype, Arguments.Count, TypeId);
        }

        #endregion
    }
}