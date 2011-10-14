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
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetStatic : AstGetSet, IAstPartiallyApplicable
    {
        public IAstType TypeExpr { get; private set; }
        private readonly string _memberId;

        [DebuggerStepThrough]
        public AstGetSetStatic(
            string file, int line, int col, PCall call, IAstType typeExpr, string memberId)
            : base(file, line, col, call)
        {
            if (typeExpr == null)
                throw new ArgumentNullException("typeExpr");
            if (memberId == null)
                throw new ArgumentNullException("memberId");
            TypeExpr = typeExpr;
            _memberId = memberId;
        }

        [DebuggerStepThrough]
        internal AstGetSetStatic(Parser p, PCall call, IAstType typeExpr, string memberId)
            : this(p.scanner.File, p.t.line, p.t.col, call, typeExpr, memberId)
        {
        }

        public string MemberId
        {
            get { return _memberId; }
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            TypeExpr = (IAstType) _GetOptimizedNode(target, TypeExpr);
            return base.TryOptimize(target, out expr);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                EmitArguments(target);
                target.EmitStaticGetCall(this,
                    Arguments.Count, constType.TypeExpression, _memberId, justEffect);
            }
            else
            {
                TypeExpr.EmitCode(target);
                target.EmitConstant(this, _memberId);
                EmitArguments(target);
                target.EmitGetCall(this, Arguments.Count + 1, PType.StaticCallFromStackId,
                    justEffect);
            }
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            EmitSetCode(target, true);
        }

        private void EmitSetCode(CompilerTarget target, bool justEffect)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                EmitArguments(target, !justEffect, 0);
                target.EmitStaticSetCall(this,
                    Arguments.Count, constType.TypeExpression + "::" + _memberId);
            }
            else
            {
                TypeExpr.EmitCode(target);
                target.EmitConstant(this, _memberId);
                EmitArguments(target, !justEffect, 2);
                    //type.StaticCall\FromStack(memberId, args...)
                target.EmitSetCall(this, Arguments.Count + 1, PType.StaticCallFromStackId);
            }
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            //Do not yet emit arguments.
            if (Call == PCall.Get)
                EmitGetCode(target, justEffect);
            else
                EmitSetCode(target, justEffect);
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstGetSetStatic(File, Line, Column, Call, TypeExpr, _memberId);
            CopyBaseMembers(copy);
            return copy;
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments);
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constTypeExpr = TypeExpr as AstConstantTypeExpression;
            if (constTypeExpr != null)
                target.EmitConstant(constTypeExpr, constTypeExpr.TypeExpression);
            else
                TypeExpr.EmitCode(target);
            target.EmitConstant(this, (int) Call);
            target.EmitConstant(this, _memberId);
            target.EmitCommandCall(this, ctorArgc + 3, Engine.PartialStaticCallAlias);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}::{2}({3})",
                Enum.GetName(typeof (PCall), Call).ToLowerInvariant(), TypeExpr, MemberId, Arguments);
        }
    }
}