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
    public class AstTypecast : AstNode,
                               IAstExpression,
                               IAstHasExpressions,
                               IAstPartiallyApplicable
    {
        public IAstExpression Subject { get; private set; }
        public IAstType Type { get; private set; }

        public AstTypecast(string file, int line, int column, IAstExpression subject, IAstType type)
            : base(file, line, column)
        {
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (type == null)
                throw new ArgumentNullException("type");
            Subject = subject;
            Type = type;
        }

        internal AstTypecast(Parser p, IAstExpression subject, IAstType type)
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
                target.Emit(this, OpCode.cast_const, constType.TypeExpression);
            else
            {
                Type.EmitCode(target);
                target.Emit(this, OpCode.cast_arg);
            }
        }

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            Subject = _GetOptimizedNode(target, Subject);
            Type = (IAstType) _GetOptimizedNode(target, Type);

            expr = null;

            var constType = Type as AstConstantTypeExpression;
            if (constType == null)
                return false;

            //Constant cast
            var constSubject = Subject as AstConstant;
            if (constSubject != null)
                return _tryOptimizeConstCast(target, constSubject, constType, out expr);

            //Redundant cast
            AstTypecast castSubject;
            AstConstantTypeExpression sndCastType;
            if ((castSubject = Subject as AstTypecast) != null &&
                (sndCastType = castSubject.Type as AstConstantTypeExpression) != null)
            {
                if (Engine.StringsAreEqual(sndCastType.TypeExpression, constType.TypeExpression))
                {
                    //remove the outer cast.
                    expr = castSubject;
                    return true;
                }
            }

            return false;
        }

        private bool _tryOptimizeConstCast(CompilerTarget target, AstConstant constSubject,
            AstConstantTypeExpression constType, out IAstExpression expr)
        {
            expr = null;
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
            PValue result;
            if (constSubject.ToPValue(target).TryConvertTo(target.Loader, type, out result))
                return AstConstant.TryCreateConstant(target, this, result, out expr);
            else
                return false;
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Subject.IsPlaceholder();
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(Subject.Singleton());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = Type as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(this, constType.TypeExpression);
            else
                Type.EmitCode(target);

            target.EmitCommandCall(this, ctorArgc + 1, Engine.PartialTypeCastAlias);
        }
    }
}