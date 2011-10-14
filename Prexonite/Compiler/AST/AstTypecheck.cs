// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(_subject.Singleton());
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