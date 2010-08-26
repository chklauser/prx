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
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstObjectCreation : AstNode,
                                     IAstExpression,
                                     IAstHasExpressions
    {
        public IAstType TypeExpr;
        private readonly ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        private readonly List<IAstExpression> _arguments = new List<IAstExpression>();

        [DebuggerStepThrough]
        public AstObjectCreation(string file, int line, int col, IAstType type)
            : base(file, line, col)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            TypeExpr = type;
            _proxy = new ArgumentsProxy(_arguments);
        }

        [DebuggerStepThrough]
        internal AstObjectCreation(Parser p, IAstType type)
            : this(p.scanner.File, p.t.line, p.t.col, type)
        {
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            TypeExpr = (IAstType) GetOptimizedNode(target, TypeExpr);

            //Optimize arguments
            foreach (var arg in _arguments.ToArray())
            {
                var oArg = GetOptimizedNode(target, arg);
                if (ReferenceEquals(oArg, arg))
                    continue;
                _arguments.Remove(arg);
                _arguments.Add(oArg);
            }

            return false;
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                foreach (var arg in _arguments)
                    arg.EmitCode(target);
                target.Emit(this, OpCode.newobj, _arguments.Count, constType.TypeExpression);
            }
            else
            {
                //Load type and call construct on it
                TypeExpr.EmitCode(target);
                foreach (var arg in _arguments)
                    arg.EmitCode(target);
                target.EmitGetCall(this, _arguments.Count, PType.ConstructFromStackId);
            }
        }

        #endregion
    }
}