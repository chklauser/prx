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
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetStatic : AstGetSetImplBase, IAstPartiallyApplicable
    {
        public AstTypeExpr TypeExpr { get; private set; }
        private readonly string _memberId;

        [DebuggerStepThrough]
        public AstGetSetStatic(
            string file, int line, int col, PCall call, AstTypeExpr typeExpr, string memberId)
            : base(file, line, col, call)
        {
            if (typeExpr == null)
                throw new ArgumentNullException(nameof(typeExpr));
            if (memberId == null)
                throw new ArgumentNullException(nameof(memberId));
            TypeExpr = typeExpr;
            _memberId = memberId;
        }

        [DebuggerStepThrough]
        internal AstGetSetStatic(Parser p, PCall call, AstTypeExpr typeExpr, string memberId)
            : this(p.scanner.File, p.t.line, p.t.col, call, typeExpr, memberId)
        {
        }

        public string MemberId
        {
            get { return _memberId; }
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            TypeExpr = (AstTypeExpr) _GetOptimizedNode(target, TypeExpr);
            return base.TryOptimize(target, out expr);
        }

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            var justEffect = stackSemantics == StackSemantics.Effect;
            if (constType != null)
            {
                EmitArguments(target);
                target.EmitStaticGetCall(Position, Arguments.Count, constType.TypeExpression, _memberId, justEffect);
            }
            else
            {
                TypeExpr.EmitValueCode(target);
                target.EmitConstant(Position, _memberId);
                EmitArguments(target);
                target.EmitGetCall(Position, Arguments.Count + 1, PType.StaticCallFromStackId, justEffect);
            }
        }


        /// <summary>
        /// Warning: cannot handle set-expressions, use <see cref="EmitSetCode(Prexonite.Compiler.CompilerTarget,Prexonite.Compiler.Ast.StackSemantics)"/> instead.
        /// </summary>
        /// <param name="target"></param>
        protected override void EmitSetCode(CompilerTarget target)
        {
            EmitSetCode(target, StackSemantics.Effect);
        }

        protected virtual void EmitSetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var constType = TypeExpr as AstConstantTypeExpression;
            var justEffect = stackSemantics == StackSemantics.Effect;
            if (constType != null)
            {
                EmitArguments(target, !justEffect, 0);
                target.EmitStaticSetCall(Position, Arguments.Count, constType.TypeExpression + "::" + _memberId);
            }
            else
            {
                TypeExpr.EmitValueCode(target);
                target.EmitConstant(Position, _memberId);
                EmitArguments(target, !justEffect, 2);
                //type.StaticCall\FromStack(memberId, args...)
                target.EmitSetCall(Position, Arguments.Count + 1, PType.StaticCallFromStackId);
            }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            //Do not yet emit arguments.
            if (Call == PCall.Get)
                EmitGetCode(target, stackSemantics);
            else
                EmitSetCode(target, stackSemantics);
        }

        public override AstGetSet GetCopy()
        {
            var copy = new AstGetSetStatic(File, Line, Column, Call, TypeExpr, _memberId);
            CopyBaseMembers(copy);
            return copy;
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv = AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments);
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constTypeExpr = TypeExpr as AstConstantTypeExpression;
            if (constTypeExpr != null)
                target.EmitConstant(constTypeExpr.Position, constTypeExpr.TypeExpression);
            else
                TypeExpr.EmitValueCode(target);
            target.EmitConstant(Position, (int) Call);
            target.EmitConstant(Position, _memberId);
            target.EmitCommandCall(Position, ctorArgc + 3, Engine.PartialStaticCallAlias);
        }

        public override string ToString()
        {
            string name = Enum.GetName(typeof (PCall), Call);
            return string.Format("{0} {1}::{2}({3})",
                name == null ? "-" : name.ToLowerInvariant(), TypeExpr, MemberId, Arguments);
        }
    }
}