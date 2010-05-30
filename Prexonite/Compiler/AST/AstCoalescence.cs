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


using System.Collections.Generic;

namespace Prexonite.Compiler.Ast
{
    public class AstCoalescence : AstNode,
                                  IAstExpression,
                                  IAstHasExpressions
    {
        public AstCoalescence(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstCoalescence(Parser p)
            : base(p)
        {
        }

        public readonly List<IAstExpression> Expressions = new List<IAstExpression>(2);

        #region IAstHasExpressions Members

        IAstExpression[] IAstHasExpressions.Expressions
        {
            get { return Expressions.ToArray(); }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            //Optimize arguments
            IAstExpression oArg;
            foreach (var arg in Expressions.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + Expressions.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = Expressions.IndexOf(arg);
                    Expressions.Insert(idx, oArg);
                    Expressions.RemoveAt(idx + 1);
                }
            }

            foreach (var iexpr in Expressions.ToArray())
            {
                if (iexpr is AstNull ||
                    (iexpr is AstConstant && ((AstConstant) iexpr).Constant == null))
                    Expressions.Remove(iexpr);
            }

            if (Expressions.Count == 1)
            {
                expr = Expressions[0];
                return true;
            }
            else if (Expressions.Count == 0)
            {
                expr = new AstNull(File, Line, Column);
                return true;
            }
            else
                return false;
        }

        #endregion

        private static int _count = -1;

        public override void EmitCode(CompilerTarget target)
        {
            //Expressions contains at least two expressions

            _count++;
            var endOfExpressionLabel = "coal\\n" + _count + "\\end";
            for (var i = 0; i < Expressions.Count; i++)
            {
                var expr = Expressions[i];

                if (i > 0)
                {
                    target.EmitPop(this);
                }

                expr.EmitCode(target);

                if (i + 1 >= Expressions.Count)
                    continue;
                target.EmitDuplicate(this);
                target.Emit(this, OpCode.check_null);
                target.EmitJumpIfFalse(this, endOfExpressionLabel);
            }

            target.EmitLabel(this, endOfExpressionLabel);
        }
    }
}