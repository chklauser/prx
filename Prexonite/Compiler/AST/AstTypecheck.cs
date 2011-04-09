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
        private IAstExpression _subject;
        private IAstType _type;

        public bool IsInverted { get; set; }

        public AstTypecheck(
            string file, int line, int column, IAstExpression subject, IAstType type)
            : base(file, line, column)
        {
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (type == null)
                throw new ArgumentNullException("type");
            _subject = subject;
            _type = type;
        }

        internal AstTypecheck(Parser p, IAstExpression subject, IAstType type)
            : this(p.scanner.File, p.t.line, p.t.col, subject, type)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new[] {_subject}; }
        }

        public IAstExpression Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        public IAstType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            _subject.EmitCode(target);
            var constType = _type as AstConstantTypeExpression;
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
                _type.EmitCode(target);
                target.Emit(this, OpCode.check_arg);
            }
        }

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            _OptimizeNode(target, ref _subject);
            _type = (IAstType) _GetOptimizedNode(target, _type);

            expr = null;

            var constSubject = _subject as AstConstant;
            var constType = _type as AstConstantTypeExpression;
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
            return base.CheckForPlaceholders() || _subject.IsPlaceholder();
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(_subject.Singleton());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = _type as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(this, constType.TypeExpression);
            else
                _type.EmitCode(target);

            target.EmitCommandCall(this, ctorArgc + 1, Engine.PartialTypeCheckAlias);
        }
    }
}