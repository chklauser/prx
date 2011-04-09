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
using System.Linq;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstListLiteral : AstNode,
                                  IAstExpression,
                                  IAstHasExpressions,
        IAstPartiallyApplicable
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
            foreach (IAstExpression arg in Elements.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in ListLiteral node (" + ToString() +
                        ") detected at position " + Elements.IndexOf(arg) + ".");
                IAstExpression oArg = _GetOptimizedNode(target, arg);
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

        protected override void DoEmitCode(CompilerTarget target)
        {
            var call = new AstGetSetSymbol(
                File, Line, Column, PCall.Get, Engine.ListAlias, SymbolInterpretations.Command);
            call.Arguments.AddRange(Elements);
            call.EmitCode(target);
        }

        #region Implementation of IAstPartiallyApplicable

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            DoEmitCode(target); //Code is the same. Partial application is handled by AstGetSetSymbol
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Elements.Any(AstPartiallyApplicable.IsPlaceholder);
        }

        #endregion
    }
}