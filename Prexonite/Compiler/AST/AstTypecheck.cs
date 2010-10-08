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
    public class AstTypecheck : AstNode,
                                IAstExpression,
                                IAstHasExpressions,
        IAstPartiallyApplicable
    {
        public IAstExpression Subject;
        public IAstType Type;

        public AstTypecheck(
            string file, int line, int column, IAstExpression subject, IAstType type)
            : base(file, line, column)
        {
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (type == null)
                throw new ArgumentNullException("type");
            Subject = subject;
            Type = type;
        }

        internal AstTypecheck(Parser p, IAstExpression subject, IAstType type)
            : this(p.scanner.File, p.t.line, p.t.col, subject, type)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {Subject}; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            Subject.EmitCode(target);
            var constType = Type as AstConstantTypeExpression;
            if (constType != null)
            {
                PType T = null;
                try
                {
                    T = target.Loader.ConstructPType(constType.TypeExpression);
                }
                catch (PrexoniteException)
                {
                    //ignore failures here
                }
                if ((object) T != null && T == PType.Null)
                    target.Emit(this, OpCode.check_null);
                else
                    target.Emit(this, OpCode.check_const, constType.TypeExpression);
            }
            else
            {
                Type.EmitCode(target);
                target.Emit(this, OpCode.check_arg);
            }
        }

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            OptimizeNode(target, ref Subject);
            Type = (IAstType) GetOptimizedNode(target, Type);

            expr = null;

            var constSubject = Subject as AstConstant;
            var constType = Type as AstConstantTypeExpression;
            if (constSubject == null || constType == null)
                return false;
            PType type;
            try
            {
                type = target.Loader.ConstructPType(constType.TypeExpression);
            }
            catch (PrexoniteException)
            {
                //ignore, cast failed. cannot be optimized
                return false;
            }
            expr =
                new AstConstant(File, Line, Column, constSubject.ToPValue(target).Type.Equals(type));
            return true;
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Subject.IsPlaceholder();
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(Subject.Singleton());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = Type as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(this, constType.TypeExpression);
            else
                Type.EmitCode(target);

            target.EmitCommandCall(this, ctorArgc + 1, Engine.PartialTypeCheckAlias);
        }
    }
}