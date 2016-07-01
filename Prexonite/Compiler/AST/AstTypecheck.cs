// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstTypecheck : AstExpr,
                                IAstHasExpressions,
                                IAstPartiallyApplicable
    {
        private AstExpr _subject;
        private AstTypeExpr _type;

        /// <summary>
        /// Indicates whether this typecheck is inverted (X is not Y).
        /// Does not, however, change the behaviour of AstTypecheck itself.
        /// It is used to distinguish between
        ///     not (X is Y)
        /// and
        ///     X is not Y
        /// 
        /// Both times, the typecheck is wrapped in a unary not, but the flag
        /// is only set in the second case.
        /// </summary>
        public bool IsInverted { get; set; }

        public AstTypecheck(
            string file, int line, int column, AstExpr subject, AstTypeExpr type)
            : base(file, line, column)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            _subject = subject;
            _type = type;
        }

        internal AstTypecheck(Parser p, AstExpr subject, AstTypeExpr type)
            : this(p.scanner.File, p.t.line, p.t.col, subject, type)
        {
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return new[] {_subject}; }
        }

        public AstExpr Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        public AstTypeExpr Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (stackSemantics == StackSemantics.Effect)
                return;

            _subject.EmitValueCode(target);
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
                    target.Emit(Position,OpCode.check_null);
                else
                    target.Emit(Position,OpCode.check_const, constType.TypeExpression);
            }
            else
            {
                _type.EmitValueCode(target);
                target.Emit(Position,OpCode.check_arg);
            }
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            _OptimizeNode(target, ref _subject);
            _type = (AstTypeExpr) _GetOptimizedNode(target, _type);

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
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(_subject.Singleton());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = _type as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(Position, constType.TypeExpression);
            else
                _type.EmitValueCode(target);

            target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialTypeCheckAlias);
        }
    }
}