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
    public class AstKeyValuePair : AstNode,
                                   IAstExpression,
                                   IAstHasExpressions, IAstPartiallyApplicable
    {
        public AstKeyValuePair(string file, int line, int column)
            : this(file, line, column, null, null)
        {
        }

        public AstKeyValuePair(
            string file, int line, int column, IAstExpression key, IAstExpression value)
            : base(file, line, column)
        {
            Key = key;
            Value = value;
        }

        internal AstKeyValuePair(Parser p)
            : this(p, null, null)
        {
        }

        internal AstKeyValuePair(Parser p, IAstExpression key, IAstExpression value)
            : base(p)
        {
            Key = key;
            Value = value;
        }

        public IAstExpression Key;
        public IAstExpression Value;

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {Key, Value}; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException("target");

            var call = new AstGetSetSymbol(
                File, Line, Column, PCall.Get, Engine.PairAlias, SymbolInterpretations.Command);
            call.Arguments.Add(Key);
            call.Arguments.Add(Value);
            call.EmitCode(target);
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException("target");

            _OptimizeNode(target, ref Key);
            _OptimizeNode(target, ref Value);

            expr = null;

            return false;
        }

        #endregion

        #region Implementation of IAstPartiallyApplicable

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            DoEmitCode(target); //Partial application is handled by AstGetSetSymbol. Code is the same
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Key is AstPlaceholder || Value is AstPlaceholder;
        }

        #endregion
    }
}