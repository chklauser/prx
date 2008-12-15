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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstUnresolved : AstGetSet
    {
        public AstUnresolved(string file, int line, int column, string id) : base(file, line, column, PCall.Get)
        {
            _id = id;
        }

        internal AstUnresolved(Parser p, string id) : base(p, PCall.Get)
        {
            _id = id;
        }

        #region Overrides of AstGetSet

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            _reportUnresolved(target);
        }

        private void _reportUnresolved(CompilerTarget target)
        {
            target.Loader.ReportSemanticError(Line, Column, "The symbol " + Id + " has not been resolved.");
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if(value == null)
                    throw new  ArgumentNullException("value");
                _id = value;
            }
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            _reportUnresolved(target);
        }

        public override AstGetSet GetCopy()
        {
            return new AstUnresolved(File, Line, Column, _id);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            if(base.TryOptimize(target, out expr))
                return true;
            else
            {
                IAstExpression sol = this;
                do
                {
                    foreach (var resolver in target.Loader.CustomResolvers)
                    {
                        sol = resolver.Resolve(target, sol as AstUnresolved);
                        if(sol != null)
                            break;
                    }
                    expr = sol;
                } while ((sol != this) && (expr is AstUnresolved));
                if (sol == this)
                    return false;
                else
                    return expr != null;
            }
        }

        #endregion
    }
}
