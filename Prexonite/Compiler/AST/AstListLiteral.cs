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

using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstListLiteral : AstNode,
                                  IAstExpression,
                                  IAstHasExpressions
    {
        public List<IAstExpression> Elements = new List<IAstExpression>();

        internal AstListLiteral(Parser p)
            : base(p)
        {
        }

        public AstListLiteral(string file, int line, int column)
            : base(file, line, column)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Elements.ToArray(); }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            IAstExpression oArg;
            foreach (IAstExpression arg in Elements.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in ListLiteral node (" + ToString() +
                        ") detected at position " + Elements.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = Elements.IndexOf(arg);
                    Elements.Insert(idx, oArg);
                    Elements.RemoveAt(idx + 1);
                }
            }
            expr = null;
            return false;
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (Elements.Count == 0)
            {
                target.Emit(OpCode.newobj, 0, "List");
            }
            else
            {
                foreach (IAstExpression element in Elements)
                    element.EmitCode(target);
                target.EmitStaticGetCall(Elements.Count, "List", "Create", false);
            }
        }
    }
}